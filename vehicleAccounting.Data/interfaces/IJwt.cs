using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vehicleAccounting.Data.models;

namespace vehicleAccounting.Data.interfaces
{
    public interface IJwt
    {
        string GenerateJwt(UserModel model);
        JwtSecurityToken Verify(string jwtToken);
        Task<string> GenerateRefreshToken(int userId);
        Task<RefreshTokenModel> GetRefreshToken(int userId);
    }
}
