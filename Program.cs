using System;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using TANA.Data;
using TANA.Helpers;
using TANA.Models;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8020";
builder.WebHost.UseUrls($"http://*:{port}");
builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyFintech API", Version = "v1" });

    // Resolve conflicting actions (duplicate method/path) by selecting the first one
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    // JWT Bearer auth in Swagger
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapping());
});

IMapper mapper = mapperConfig.CreateMapper();

builder.Services.AddSingleton(mapper);
builder.Services.AddTransient<EmailHelper>();
builder.Services.AddTransient<SmsHelper>();

builder.Services.AddTransient<Rbac>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
        };
    });

string ConStr = builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseMySql(ConStr, new MySqlServerVersion(new Version(8, 0, 21)))
);

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ─── Manual seeder (Run with --seed) ──────────────────────────────────────────
if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
        Console.WriteLine("Applying migrations...");
        await db.Database.MigrateAsync();
        Console.WriteLine("Navigating to seeding database...");
        await DatabaseSeeder.SeedAsync(db);
        Console.WriteLine("Seeding completed successfully.");
    }
    return;
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Application started. Listening on: " + string.Join(", ", app.Urls));
});
app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is stopping...");
});
app.Lifetime.ApplicationStopped.Register(() =>
{
    Console.WriteLine("Application stopped.");
});

Console.WriteLine("Starting web host...");

// In Docker/Production, we often let the proxy handle HTTPS.
// Only redirect if not in a container or explicitly required.
if (!builder.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", " MyFintech App API V1");
    c.RoutePrefix = "swagger"; // serve at /swagger
});

app.UseAuthentication();
app.UseAuthorization();

// convention base routing
app.MapControllerRoute(name: "default", pattern: "api/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "listfilter",
    pattern: "api/{controller=Home}/{action=Index}/{fieldname}/{fieldvalue}"
);

// Map default route to /api/test
// app.MapGet("/", () => Results.Redirect("/api/test"));

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("Host terminated unexpectedly: " + ex.Message);
    throw;
}
