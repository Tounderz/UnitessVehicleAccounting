using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vehicleAccounting.Data.models;

namespace vehicleAccounting.Data.interfaces
{
    public interface IAuth
    {
        Task<UserModel> SignIn(string login);
        Task<UserModel> SignUp(UserModel user);
        Task<UserModel> GetByUserFromToken(JwtSecurityToken token);
        Task<IEnumerable<UserModel>> GetAllUsers();
    }
}
