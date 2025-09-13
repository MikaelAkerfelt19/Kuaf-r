using System.Security.Claims;

namespace Kuafor.Web.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(ClaimsPrincipal user);
        string GenerateToken(string userId, string email, IList<string> roles);
        ClaimsPrincipal? ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        IList<string> GetRolesFromToken(string token);
        bool IsTokenExpired(string token);
        string GenerateRefreshToken();
        Task<string> RefreshTokenAsync(string refreshToken);
        Task<string> CreateRefreshTokenAsync(string userId);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}
