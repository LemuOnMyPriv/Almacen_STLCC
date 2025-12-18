using Almacen_STLCC.Data;
using Almacen_STLCC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Minio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) => (IStringLocalizer?)null!;
    });

builder.Services.AddHttpContextAccessor();

// Configurar MySQL
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Registrar servicio LDAP
builder.Services.AddScoped<LdapAuthenticationService>();

// Registrar servicio de auditoría
builder.Services.AddScoped<AuditoriaService>();

// Configurar MinIO
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var config = builder.Configuration.GetSection("MinIO");
    var endpoint = config["Endpoint"];
    var accessKey = config["AccessKey"];
    var secretKey = config["SecretKey"];
    var useSSL = bool.Parse(config["UseSSL"] ?? "true");
    var bucketName = config["BucketName"]?.Trim();

    if (string.IsNullOrWhiteSpace(bucketName))
        bucketName = "almacen";

    Console.WriteLine($"[MinIO] Configurando cliente:");
    Console.WriteLine($"  Endpoint: {endpoint}");
    Console.WriteLine($"  UseSSL: {useSSL}");
    Console.WriteLine($"  Bucket: {config["BucketName"]}");

    var clientBuilder = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey);

    if (useSSL)
        clientBuilder = clientBuilder.WithSSL();

    return clientBuilder.Build();
});

// Registrar MinioService
builder.Services.AddScoped<MinioService>();

// Registrar el servicio de limpieza automática de auditorías
builder.Services.AddHostedService<AuditoriaLimpiezaService>();

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();