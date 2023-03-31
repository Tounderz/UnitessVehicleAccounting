using Dapper;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using vehicleAccounting.Data.interfaces;
using vehicleAccounting.Data.models;
using vehicleAccounting.Data.utils;

#pragma warning disable CS8603

namespace vehicleAccounting.Api.data.repositories
{
    public class AuthRepository : IAuth
    {
        private readonly ConnectionStringConfiguration _connectionString;

        public AuthRepository(ConnectionStringConfiguration connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<UserModel> SignIn(string login)
        {
            var user = await GetUserById(login);
            return user;
        }

        public async Task<UserModel> SignUp(UserModel model)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var check = await CheckLoginAndEmail(model.Email, model.Login, connection);
            if (!check)
            {
                return null;
            }

            var user = new UserModel()
            {
                Name = !string.IsNullOrEmpty(model.Name) ? model.Name : string.Empty,
                Email = !string.IsNullOrEmpty(model.Email) ? model.Email : string.Empty,
                Login = model.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            await connection.ExecuteAsync(ConstAuth.INSERT_USER, user);
            return user;
        }

        public async Task<UserModel> GetByUserFromToken(JwtSecurityToken token)
        {
            var login = string.Empty;
            foreach (var item in token.Claims)
            {
                if (item.Type == ClaimTypes.NameIdentifier)
                {
                    login = item.Value;
                    break;
                }
            }

            var user = await GetUserById(login);
            return user;
        }

        private async Task<UserModel> GetUserById(string login)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var users = await connection.QueryAsync<UserModel>(ConstAuth.SELECT_ALL);
            if (users == null || users.Count() <= 0)
            {
                return null;
            }

            var user = users.FirstOrDefault(i => i.Login == login);
            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var users = await connection.QueryAsync<UserModel>(ConstAuth.SELECT_ALL);
            return users;
        }

        private async Task<bool> CheckLoginAndEmail(string email, string login, SqlConnection connection)
        {
            var users = await connection.QueryAsync<UserModel>(ConstAuth.SELECT_ALL);
            var user = !string.IsNullOrEmpty(email) ? users.FirstOrDefault(i => i.Login == login && i.Email == email) : users.FirstOrDefault(i => i.Login == login);
            if (user != null)
            {
                return false;
            }

            return true;
        }
    }
}
