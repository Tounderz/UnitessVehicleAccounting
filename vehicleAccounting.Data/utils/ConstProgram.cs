using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vehicleAccounting.Data.utils
{
    public class ConstProgram
    {
        public const string NAME_CONNECTION_STRING = "Default";
        public const string VALID_ISSUER = "Jwt:Issuer";
        public const string VALID_AUDIENCE = "Jwt:Audience";
        public const string ISSUER_SIGNING_KEY = "Jwt:Key";
        public const string SECTION_JWT = "Jwt";
        public const string SECTION_CONNECTION_STRING = "ConnectionStrings";
        public static readonly string[] WITH_ORIGINS = new[] { "http://localhost:3000", "http://localhost:8080", "http://localhost:4200" };
    }
}
