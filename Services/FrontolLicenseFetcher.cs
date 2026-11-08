using FrontolLicenseApi.Configurations;
using FrontolLicenseApi.Extensions;
using FrontolLicenseApi.Models;
using FrontolLicenseApi.Models.Request;
using FrontolLicenseApi.Models.Response;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace FrontolLicenseApi.Services;

public sealed class FrontolLicenseFetcher
    (IServiceScopeFactory scopeFactory,
    IHttpClientFactory clientFactory, 
    IOptions<FrontolConfiguration> opts) : BackgroundService
{
    public const string ClientKey = "FrontolClient";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var client = clientFactory.CreateClient(ClientKey);
                var token = await LoginAsync(client, opts.Value.Login, opts.Value.Password, stoppingToken);

                int? page = 1;
                do
                {
                    var resp = await GetLicensesPageAsync(client, page.Value, opts.Value.PageSize, token, stoppingToken);
                    page = resp.NextPage;
                    foreach (var license in resp.Data)
                    {
                        LicenseType? type;
                        var detail = await GetDetailLicenseAsync(client, license.Id, license.Type, token, stoppingToken);

                        type = context.Types.FirstOrDefault(t => t.Type == detail.Data.Type);
                        if (type is null)
                            context.Types.Add(type = new LicenseType
                            {
                                Type = detail.Data.Type,
                                DisplayName = detail.Data.TypeDisplayName
                            });

                        context.UpsertLicense(new License
                        {
                            Id = detail.Data.LicenseId,
                            Type = type,
                            ExpiredAt = detail.Data.SubscriptionDate ?? (detail.Data.LeftDays is null ? null : DateOnly.FromDateTime(DateTime.Today.AddDays(detail.Data.LeftDays.Value)))
                        });

                        foreach (var baseFor in detail.Data.BaseFor ?? [])
                        {
                            context.UpsertLicense(new License
                            {
                                Id = baseFor.LicenseId,
                                Parent = detail.Data.LicenseId,
                                ParentType = License.LicenseRelationshipType.Converted
                            });
                        }

                        foreach(var prolong in detail.Data.ProlongLicenses ?? [])
                        {
                            context.UpsertLicense(new License
                            {
                                Id = prolong.LicenseId,
                                Type = type,
                                ExpiredAt = DateOnly.FromDateTime(prolong.OperationDateTime.AddDays(prolong.LeftDays)),
                                Parent = detail.Data.LicenseId,
                                ParentType = License.LicenseRelationshipType.Prolong
                            });
                        }

                        foreach(var sub in detail.Data.Subscriptions ?? [])
                        {
                            context.UpsertLicense(new License
                            {
                                Id = sub.LicenseId,
                                Type = type,
                                ExpiredAt = sub.FinalDate,
                                Parent = detail.Data.LicenseId,
                                ParentType = License.LicenseRelationshipType.Subscription
                            });
                        }

                        context.SaveChanges();
                    }
                } while (page is { });
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }

    private static async Task<string> LoginAsync
        (HttpClient client, string login, string password, CancellationToken ct)
    {
        using var reqMessage = new HttpRequestMessage(HttpMethod.Post, "/api/lk/auth/login");
        reqMessage.Content = JsonContent.Create(
            new FrontolAuthRequest(login, password), 
            new MediaTypeHeaderValue("application/json", "UTF-8"));

        using var respMessage = await client.SendAsync(reqMessage, ct);
        respMessage.EnsureSuccessStatusCode();
        var resp = await respMessage.Content.ReadFromJsonAsync<FrontolAuthResponse>(ct);
        return resp.Data.AuthToken;
    }

    private static async Task<FrontolLicensesPageResponse> GetLicensesPageAsync
        (HttpClient client, int page, int sizePage, string authToken, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizePage);
        ArgumentException.ThrowIfNullOrWhiteSpace(authToken);

        using var reqMessage = new HttpRequestMessage(HttpMethod.Post, 
            $"/api/lk/lk_lic/lic_list?page={page}&count={sizePage}");
        reqMessage.Headers.Add("Cookie", $"auth_token={authToken}");
        reqMessage.Headers.Add("Authorization", authToken);

        using var respMessage = await client.SendAsync(reqMessage, ct);
        respMessage.EnsureSuccessStatusCode();
        return await respMessage.Content.ReadFromJsonAsync<FrontolLicensesPageResponse>(ct);
    }

    private static async Task<FrontolDetailLicenseResponse> GetDetailLicenseAsync
        (HttpClient client, int id, string type, string authToken, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authToken);

        using var reqMessage = new HttpRequestMessage(HttpMethod.Post, 
            "/api/lk/lk_lic/get_lic_info");
        reqMessage.Content = JsonContent.Create(
            new FrontolDetailLicenseRequest(id),
            new MediaTypeHeaderValue("application/json", "UTF-8"));
        reqMessage.Headers.Add("Cookie", $"auth_token={authToken}");
        reqMessage.Headers.Add("Authorization", authToken);

        using var respMessage = await client.SendAsync(reqMessage, ct);
        respMessage.EnsureSuccessStatusCode();
        return await respMessage.Content.ReadFromJsonAsync<FrontolDetailLicenseResponse>(ct);
    }
}
