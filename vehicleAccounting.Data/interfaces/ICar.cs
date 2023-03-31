using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vehicleAccounting.Data.models;

namespace vehicleAccounting.Data.interfaces
{
    public interface ICar
    {
        Task<CarModel> GetItem(int id);
        Task<IEnumerable<CarModel>> GetItems(IndexModel model);
        Task<bool> Create(CarModel model);
        Task<bool> Update(CarModel model);
        Task<bool> Delete(int id);
    }
}
