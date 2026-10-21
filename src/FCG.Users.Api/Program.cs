using FCG.Users.Api.Endpoints;
using FCG.Users.Api.Exceptions;
using FCG.Users.Application;
using FCG.Users.Domain.Enums;
using FCG.Users.Infrastructure;
using FCG.Users.Infrastructure.Persistence;
using FCG.Users.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddHealthChecks();

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer();

    // Mesma configuração que Catalog e Payments usam para validar o token emitido aqui:
    // claims com os nomes originais do JWT (sub, email, name, role), sem remapeamento.
    builder.Services
        .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        .Configure<JwtSettings>((options, jwt) =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                NameClaimType = "name",
                RoleClaimType = "role",
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(nameof(RoleUsuario.Administrador), policy => policy.RequireRole(nameof(RoleUsuario.Administrador)));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "FCG Users API",
            Version = "v1",
            Description = "Cadastro de usuários, autenticação e emissão do JWT da plataforma FIAP Cloud Games."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Informe apenas o token JWT, sem o prefixo \"Bearer\" (o Swagger adiciona sozinho).",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Users API v1"));

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
    app.MapHealthChecks("/health");

    app.MapAuthEndpoints();
    app.MapUsuarioEndpoints();

    await DatabaseInitializer.InicializarAsync(app.Services);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "fcg-users-api encerrada de forma inesperada durante a inicialização");
    // Código de saída diferente de zero: num container ou pod, é o que sinaliza a falha de subida.
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
