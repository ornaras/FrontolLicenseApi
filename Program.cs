using FrontolLicenseApi.Configurations;
using FrontolLicenseApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FrontolConfiguration>(builder.Configuration.GetSection(FrontolConfiguration.SectionName));
builder.Services.Configure<LocalesConfiguration>(builder.Configuration.GetSection(LocalesConfiguration.SectionName));

var cfgStorage = new StorageConfiguration();
builder.Configuration.Bind(StorageConfiguration.SectionName, cfgStorage);
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
{
    switch (cfgStorage.Type)
    {
        case StorageConfiguration.DbType.Sqlite:
            {
                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = cfgStorage.Database!,
                };
                opts.UseSqlite(builder.ConnectionString);
                break;
            }
        case StorageConfiguration.DbType.MySql:
            {
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = cfgStorage.Host!,
                    Port = (uint)cfgStorage.Port!.Value,
                    Database = cfgStorage.Database!,
                    UserID = cfgStorage.Username!,
                    Password = cfgStorage.Password!,
                };
                opts.UseMySQL(builder.ConnectionString);
                break;
            }
        case StorageConfiguration.DbType.Postgres:
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = cfgStorage.Host!,
                    Port = cfgStorage.Port!.Value,
                    Database = cfgStorage.Database!,
                    Username = cfgStorage.Username!,
                    Password = cfgStorage.Password!,
                };
                opts.UseNpgsql(builder.ConnectionString);
                break;
            }
        default: throw new ArgumentException($"{StorageConfiguration.SectionName}.Type");
    }
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(FrontolLicenseFetcher.ClientKey, c =>
{
#pragma warning disable S1075
    c.BaseAddress = new Uri("https://lk.frontol.ru");
#pragma warning restore S1075
});
builder.Services.AddHostedService<FrontolLicenseFetcher>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapOpenApi();
app.MapSwagger();
app.MapSwaggerUI();
app.MapControllers();

await app.RunAsync();
