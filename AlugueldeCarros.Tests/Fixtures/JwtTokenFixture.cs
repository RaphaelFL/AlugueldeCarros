using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlugueldeCarros.Configurations;
using Microsoft.IdentityModel.Tokens;

namespace AlugueldeCarros.Tests.Fixtures;

/// <summary>
/// Fixture para gerar tokens JWT válidos e inválidos para testes
/// </summary>
public class JwtTokenFixture
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenFixture()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-min-32-chars-long-for-testing!!!!",
            ExpiryInMinutes = 60,
            Issuer = "AlugueldeCarros",
            Audience = "AlugueldeCarrosApi"
        };
    }

    /// <summary>
    /// Gera um token JWT válido com claims customizáveis
    /// </summary>
    public string GenerateValidToken(
        Guid? userId = null,
        string email = "user@test.com",
        string[] roles = null)
    {
        userId ??= Guid.NewGuid();
        roles ??= new[] { "USER" };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim("email", email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gera um token JWT válido com role ADMIN
    /// </summary>
    public string GenerateAdminToken(Guid? userId = null, string email = "admin@test.com")
    {
        return GenerateValidToken(userId, email, new[] { "ADMIN" });
    }

    /// <summary>
    /// Gera um token JWT válido com role USER
    /// </summary>
    public string GenerateUserToken(Guid? userId = null, string email = "user@test.com")
    {
        return GenerateValidToken(userId, email, new[] { "USER" });
    }

    /// <summary>
    /// Gera um token JWT válido com múltiplas roles
    /// </summary>
    public string GenerateTokenWithRoles(Guid? userId = null, params string[] roles)
    {
        return GenerateValidToken(userId, "user@test.com", roles);
    }

    /// <summary>
    /// Gera um token JWT expirado
    /// </summary>
    public string GenerateExpiredToken(Guid? userId = null)
    {
        userId ??= Guid.NewGuid();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim("email", "user@test.com"),
            new Claim(ClaimTypes.Role, "USER")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(-10), // Expirado há 10 segundos
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gera um token JWT com secret inválido (falha na validação)
    /// </summary>
    public string GenerateInvalidToken()
    {
        var wrongSecret = "wrong-secret-key-min-32-chars-wrong-secret-key!!!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(wrongSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("email", "user@test.com"),
            new Claim(ClaimTypes.Role, "USER")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Extrai claims de um token JWT para validação em testes
    /// </summary>
    public Dictionary<string, string> ExtractClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
    }

    /// <summary>
    /// Valida se um token é genuinamente válido com a secret correta
    /// </summary>
    public bool IsTokenValid(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
