using FrontolLicenseApi.Configurations;
using FrontolLicenseApi.Extensions;
using FrontolLicenseApi.Models;
using FrontolLicenseApi.Models.Request;
using FrontolLicenseApi.Models.Response;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
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
                        var detail = await GetDetailLicenseAsync(client, license.Id, token, stoppingToken);
                        var product = await GetOrAddProductAsync(context, detail.Data.ProductId, detail.Data.ProductDisplayName, stoppingToken);

                        context.UpsertLicense(new License
                        {
                            Code = detail.Data.LicenseId,
                            Product = product,
                            ExpiredAt = detail.Data.SubscriptionDate ?? (detail.Data.LeftDays is null ? null : DateOnly.FromDateTime(DateTime.Today.AddDays(detail.Data.LeftDays.Value)))
                        });

                        AddBaseLicenses(context, detail.Data.LicenseId, detail.Data.BaseFor);
                        AddSubscriptions(context, detail.Data.LicenseId, product, detail.Data.Subscriptions);
                        AddProlongLicenses(context, detail.Data.LicenseId, product, detail.Data.ProlongLicenses);

                        await context.SaveChangesAsync(stoppingToken);
                    }
                } while (page is { });
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }

    private static async Task<Product> GetOrAddProductAsync(ApplicationDbContext context, string id, string displayName, CancellationToken ct)
    {
        var product = await context.Products.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (product is null)
        {
            product = new Product
            {
                Id = id,
                DisplayName = displayName
            };
            context.Products.Add(product);
        }
        return product;
    }

    private static void AddProlongLicenses(ApplicationDbContext context, string parentCode, Product product,
        FrontolDetailLicenseResponse.FrontolDetailLicense.FrontolProlongLicense[]? array)
    {
        foreach (var prolong in array ?? [])
        {
            context.UpsertLicense(new License
            {
                Code = prolong.LicenseId,
                Product = product,
                ExpiredAt = DateOnly.FromDateTime(prolong.OperationDateTime.AddDays(prolong.LeftDays)),
                ParentCode = parentCode,
                ParentType = License.LicenseRelationshipType.Prolong
            });
        }
    }

    private static void AddSubscriptions(ApplicationDbContext context, string parentCode, Product product,
        FrontolDetailLicenseResponse.FrontolDetailLicense.FrontolSubscription[]? array)
    {
        foreach (var sub in array ?? [])
        {
            context.UpsertLicense(new License
            {
                Code = sub.LicenseId,
                Product = product,
                ExpiredAt = sub.FinalDate,
                ParentCode = parentCode,
                ParentType = License.LicenseRelationshipType.Subscription
            });
        }
    }

    private static void AddBaseLicenses(ApplicationDbContext context, string parentCode, 
        FrontolDetailLicenseResponse.FrontolDetailLicense.FrontolBaseForItem[]? array)
    {
        foreach (var baseFor in array ?? [])
        {
            context.UpsertLicense(new License
            {
                Code = baseFor.LicenseId,
                ParentCode = parentCode,
                ParentType = License.LicenseRelationshipType.Converted
            });
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
        (HttpClient client, int id, string authToken, CancellationToken ct)
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
