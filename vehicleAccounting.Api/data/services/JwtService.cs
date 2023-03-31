using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using vehicleAccounting.Data.interfaces;
using vehicleAccounting.Data.models;

#pragma warning disable CS8603

namespace vehicleAccounting.Api.data.services
{
    public class JwtService : IJwt
    {
        private readonly JWTConfiguration _jwtConfig;
        private readonly ConnectionStringConfiguration _connectionString;

        public JwtService(JWTConfiguration jwtConfig, ConnectionStringConfiguration connectionString)
        {
            _jwtConfig = jwtConfig;
            _connectionString = connectionString;
        }

        public string GenerateJwt(UserModel model)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, model.Login),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var now = DateTime.Now;
            var securityToken = new JwtSecurityToken
                (
                    issuer: _jwtConfig.Issuer,
                    audience: _jwtConfig.Audience,
                    notBefore: now,
                    claims: claims,
                    expires: now.AddMinutes(10),
                    signingCredentials: credentials
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return accessToken;
        }

        public JwtSecurityToken Verify(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtConfig.Key);
            tokenHandler.ValidateToken(jwtToken, new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
            },
                out SecurityToken securityToken);

            var token = securityToken as JwtSecurityToken;

            return token;
        }

        public async Task<string> GenerateRefreshToken(int userId)
        {
            var randomNumber = new byte[64];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var refreshTokenModel = await GetRefreshToken(userId);
            if (refreshTokenModel != null)
            {
                var comparisonResult = DateTime.Compare(refreshTokenModel.TokenExpires, DateTime.Now);
                if (comparisonResult < 1)
                {
                    refreshTokenModel.RefreshToken = refreshToken;
                    refreshTokenModel.TokenExpires = DateTime.Now.AddDays(1);
                    refreshTokenModel.IsActive = true;

                    await connection.ExecuteAsync("update RefreshTokens set RefreshToken = @RefreshToken, TokenExpires = @TokenExpires, IsActive = @IsActive where Id=@Id", refreshTokenModel);
                }
            }
            else
            {
                refreshTokenModel = new RefreshTokenModel
                {
                    UserId = userId,
                    RefreshToken = refreshToken,
                    TokenExpires = DateTime.Now.AddDays(5),
                    IsActive = true,
                };

                await connection.ExecuteAsync("insert into RefreshTokens (UserId, RefreshToken, TokenExpires, IsActive) values (@UserId, @RefreshToken, @TokenExpires, @IsActive)", refreshTokenModel);
            }

            return refreshTokenModel.RefreshToken;
        }

        public async Task<RefreshTokenModel> GetRefreshToken(int userId)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var refreshTokens = await connection.QueryAsync<RefreshTokenModel>("select * from RefreshTokens");
            var model = refreshTokens.FirstOrDefault(i => i.UserId == userId);
            if (model == null)
            {
                return null;
            }

            var comparisonResult = DateTime.Compare(model.TokenExpires, DateTime.Now);
            if (comparisonResult < 1)
            {
                model.IsActive = false;
            }

            return model;
        }
    }
}
