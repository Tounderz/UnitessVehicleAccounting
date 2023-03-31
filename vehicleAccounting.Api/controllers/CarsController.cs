using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vehicleAccounting.Data.interfaces;
using vehicleAccounting.Data.models;
using vehicleAccounting.Data.utils;

namespace vehicleAccounting.Api.controllers
{
    [Route("cars")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ICar _car;

        public CarsController(ICar car)
        {
            _car = car;
        }

        [HttpGet("item")]
        public IActionResult GetCar(int id) 
        {
            if (id <= 0)
            {
                return BadRequest(new { message = ConstMessages.SELECT_CAR });
            }

            var car = _car.GetItem(id);
            if (car == null)
            {
                return BadRequest( new { message = ConstMessages.ERROR_ITEM_CAR });
            }

            return Ok( new { car = car });
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(IndexModel model)
        {
            var cars = await _car.GetItems(model);
            if (cars == null || !cars.Any())
            {
                return BadRequest(new { message = ConstMessages.LIST_EMPTY });
            }

            return Ok( new { cars = cars });
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CarModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name))
            {
                return BadRequest( new { message = ConstMessages.COMPLETE_ALL_FIELDS });
            }

            var create = await _car.Create(model);
            if (!create)
            {
                return BadRequest(new { message = ConstMessages.ERROR_NAME_CAR });
            }


            return Ok( new { message = ConstMessages.CREATE_CAR });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update(CarModel model)
        {
            if (model == null || (string.IsNullOrEmpty(model.Name) && string.IsNullOrEmpty(model.Description)))
            {
                return BadRequest(new { message = ConstMessages.COMPLETE_ALL_FIELDS });
            }

            var update = await _car.Update(model);
            if (!update)
            {
                return BadRequest(new { message = ConstMessages.ERROR_NAME_CAR });
            }

            return Ok(new { message = ConstMessages.UPDATE_CAR });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = ConstMessages.SELECT_CAR });
            }

            var delete = await _car.Delete(id);
            if (!delete)
            {
                return BadRequest(new { message = ConstMessages.ERROR_ITEM_CAR });
            }

            return Ok(new { message = ConstMessages.DELETE_CAR });
        }
    }
}
