using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kuafor.Web.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JwtService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration configuration, ApplicationDbContext context, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            
            var jwtSettings = _configuration.GetSection("JwtSettings");
            _secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not found");
            _issuer = jwtSettings["Issuer"] ?? "KuaforApp";
            _audience = jwtSettings["Audience"] ?? "KuaforUsers";
            _expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "120");
        }

        public string GenerateToken(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return GenerateToken(userId ?? "", email ?? "", roles);
        }

        public string GenerateToken(string userId, string email, IList<string> roles)
        {
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, email),
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Rolleri ekle
            foreach (var role in roles)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = Encoding.ASCII.GetBytes(_secretKey);
                var tokenHandler = new JwtSecurityTokenHandler();
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed");
                return null;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract user ID from token");
                return null;
            }
        }

        public IList<string> GetRolesFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                return principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract roles from token");
                return new List<string>();
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check token expiration");
                return true;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                {
                    throw new SecurityTokenException("Invalid refresh token");
                }

                // Eski refresh token'ı iptal et
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;

                // Kullanıcı bilgilerini al
                var user = await _context.Users.FindAsync(storedToken.UserId);
                if (user == null)
                {
                    throw new SecurityTokenException("User not found");
                }

                // Yeni JWT token oluştur
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                var newJwtToken = GenerateToken(user.Id, user.Email ?? "", roles.Where(r => r != null).Cast<string>().ToList());

                // Yeni refresh token oluştur
                var newRefreshToken = GenerateRefreshToken();
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 gün geçerli
                    ReplacedByToken = newRefreshToken
                };

                // Eski token'ı yeni token ile değiştir
                storedToken.ReplacedByToken = newRefreshToken;

                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                return newJwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token failed");
                throw;
            }
        }

        public async Task<string> CreateRefreshTokenAsync(string userId)
        {
            try
            {
                // Kullanıcının eski refresh token'larını iptal et
                var oldTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var oldToken in oldTokens)
                {
                    oldToken.IsRevoked = true;
                    oldToken.RevokedAt = DateTime.UtcNow;
                }

                // Yeni refresh token oluştur
                var refreshToken = GenerateRefreshToken();
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // 7 gün geçerli
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create refresh token failed");
                throw;
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    storedToken.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Revoke refresh token failed");
                throw;
            }
        }
    }
}
