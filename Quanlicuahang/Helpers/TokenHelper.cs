using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Quanlicuahang.Helpers
{
    public interface ITokenHelper
    {
        Task<string?> GetUserIdFromTokenAsync();
    }

    public class TokenHelper : ITokenHelper
    {
        private readonly IHttpContextAccessor _httpContext;

        public TokenHelper(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

   
        public async Task<string?> GetUserIdFromTokenAsync()
        {
            try
            {
                var userId = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                    return userId;
                var authHeader = _httpContext.HttpContext?.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader))
                    return null;
                var token = authHeader.Replace("Bearer ", "").Trim();
                if (string.IsNullOrEmpty(token))
                    return null;
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                return userIdClaim;
            }
            catch
            {
                return null;
            }
        }
    }
}