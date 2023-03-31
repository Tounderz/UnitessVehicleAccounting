using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vehicleAccounting.Data.utils
{
    public class ConstCar
    {
        public const string SELECT_ID = "select * from cars where id = @Id";
        public const string UPDATE_CAR_ID = "update Cars set Name = @Name, Description = @Description where Id=@Id";
        public const string INSERT_CAR = "insert into Cars (Name, Description) values (@Name, @Description)";
        public const string DELETE_CAR = "delete from Cars where Id = @Id";
        public const string SELECT_ALL_CAR = "select * from Cars";
    }
}
