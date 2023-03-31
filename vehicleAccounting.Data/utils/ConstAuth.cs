using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vehicleAccounting.Data.utils
{
    public class ConstAuth
    {
        public const string INSERT_USER = "insert into Users (Name, Email, Login, Password) values (@Name, @Email, @Login, @Password)";
        public const string SELECT_ALL = "select * from Users";
        public const string AUTHORIZATION = "Authorization";
    }
}
