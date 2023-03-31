using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Xml.Linq;
using vehicleAccounting.Data.interfaces;
using vehicleAccounting.Data.models;
using vehicleAccounting.Data.utils;

#pragma warning disable CS8603

namespace vehicleAccounting.Api.data.repositories
{
    public class CarRepository : ICar
    {
        private readonly ConnectionStringConfiguration _connectionString;

        public CarRepository(ConnectionStringConfiguration connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<CarModel> GetItem(int id)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var car = await connection.QueryFirstAsync<CarModel>(ConstCar.SELECT_ID,
                    new { Id = id });

            return car;
        }

        public async Task<IEnumerable<CarModel>> GetItems(IndexModel model)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            IEnumerable<CarModel> cars = await AllCars(connection);
            if (model.StartIndex > cars.Count() && model.EndIndex > cars.Count())
            {
                return null;
            }

            var start = model.StartIndex > 0 ? model.StartIndex : 0;
            start = start <= cars.Count() ? start : cars.Count();
            var end = model.EndIndex > 0 ? model.EndIndex : 0;
            end = end <= cars.Count() ? end : cars.Count();
            switch (start)
            {
                case 0:
                    if (end > 0)
                    {
                        cars = cars.Where(i => i.Id >= start && i.Id <= end);
                    }
                    break;
                case > 0:
                    if (start < end)
                    {
                        cars = cars.Where(i => i.Id >= start && i.Id <= end);
                    }
                    else if (start > end)
                    {
                        cars = cars.Where(i => i.Id <= start && i.Id >= end);
                    }
                    else
                    {
                        cars = cars.Where(i => i.Id == start);
                    }
                    break;
                default:
                    break;
                
            }            

            return cars;
        }

        public async Task<bool> Create(CarModel model)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var check = string.IsNullOrEmpty(model.Name) || await CheckName(model.Name, connection);
            if (!check)
            {
                return false;
            }

            var car = new CarModel()
            {
                Name = !string.IsNullOrEmpty(model.Name) ? model.Name : string.Empty,
                Description = !string.IsNullOrEmpty(model.Description) ? model.Description : string.Empty
            };

            await connection.ExecuteAsync(ConstCar.INSERT_CAR, car);
            return true;
        }

        public async Task<bool> Update(CarModel model)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var check = string.IsNullOrEmpty(model.Name) || await CheckName(model.Name, connection);
            if (!check)
            {
                return false;
            }

            var car = await GetItem(model.Id);
            if (car == null)
            {
                return false;
            }

            car.Name = !string.IsNullOrEmpty(model.Name) ? model.Name : car.Name;
            car.Description = !string.IsNullOrEmpty(model.Description) ? model.Description : car.Description;
            await connection.ExecuteAsync(ConstCar.UPDATE_CAR_ID, car);
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString.ConnectionString);
            var car = await connection.QueryFirstAsync<CarModel>(ConstCar.SELECT_ID,
                    new { Id = id });
            if (car == null)
            {
                return false;
            }

            await connection.ExecuteAsync(ConstCar.DELETE_CAR, new { Id = id });

            return true;
        }

        private async Task<IEnumerable<CarModel>> AllCars(SqlConnection connection)
        {
            var cars =  await connection.QueryAsync<CarModel>(ConstCar.SELECT_ALL_CAR);
            if (cars == null || !cars.Any())
            {
                return null;
            }

            return cars;
        }

        private async Task<bool> CheckName(string name, SqlConnection connection)
        {
            var cars = await connection.QueryAsync<CarModel>(ConstCar.SELECT_ALL_CAR);
            var car = cars.FirstOrDefault(i => i.Name.ToLower() == name.ToLower());
            if (car == null)
            {
                return true;
            }

            return false;
        }
    }
}
