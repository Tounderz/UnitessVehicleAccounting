using Dapper;
using System.Data.SqlClient;
using System.Reflection;
using vehicleAccounting.Data.models;

namespace vehicleAccounting.Api.data
{
    public class DBObjects
    {
        public static async Task Initial(SqlConnection connection)
        {
             var cars = await connection.QueryAsync<CarModel>("select * from Cars");
             var sql = "insert into Cars (Name, Description) values (@Name, @Description)";
             var carsCount = cars.Count();
             if (!cars.Any() || carsCount < 10)
             {
                 while(carsCount < 10)
                 { 
                     var car = new CarModel
                     {
                         Name = Faker.Company.Name(),
                         Description = Faker.Company.BS()
                     };

                     await connection.ExecuteAsync(sql, car);
                     carsCount++;
                 }
             }
        }
    }
}
