using FrontolLicenseApi.Configurations;
using FrontolLicenseApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FrontolConfiguration>(builder.Configuration.GetSection("Frontol"));

var cfgStorage = new StorageConfiguration();
builder.Configuration.Bind("Storage", cfgStorage);
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
                if(cfgStorage.Password is { } passwd)
                    builder.Password = passwd;
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
        default: throw new ArgumentOutOfRangeException();
    }
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(FrontolLicenseFetcher.ClientKey, c =>
{
    c.BaseAddress = new Uri("https://lk.frontol.ru");
});
builder.Services.AddHostedService<FrontolLicenseFetcher>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapOpenApi();
app.MapSwagger();
app.MapSwaggerUI();
app.MapControllers();

app.Run();
