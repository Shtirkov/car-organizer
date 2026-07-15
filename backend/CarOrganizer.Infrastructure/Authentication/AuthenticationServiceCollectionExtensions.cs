using System.Text;
using CarOrganizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CarOrganizer.Infrastructure.Authentication;

/// <summary>
/// Registers JWT bearer authentication — i.e. the rules by which every incoming access token is
/// validated (signature, issuer, audience, lifetime). This is the framework's built-in handler;
/// we only configure it. The pipeline step that runs it is <c>app.UseAuthentication()</c>.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim types as-is ("sub", "email") instead of remapping them to long URIs.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection[nameof(JwtSettings.Issuer)],
                    ValidateAudience = true,
                    ValidAudience = jwtSection[nameof(JwtSettings.Audience)],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection[nameof(JwtSettings.Key)]!)),
                    ValidateLifetime = true,
                    // No grace period beyond the token's own exp (default is 5 minutes).
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
