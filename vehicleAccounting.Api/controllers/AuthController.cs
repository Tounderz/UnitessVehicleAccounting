using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using vehicleAccounting.Data.interfaces;
using vehicleAccounting.Data.models;
using vehicleAccounting.Data.utils;

#pragma warning disable CS8604

namespace vehicleAccounting.Api.controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _auth;
        private readonly IJwt _jwt;

        public AuthController(IAuth auth, IJwt jwt)
        {
            _auth = auth;
            _jwt = jwt;
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn(LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest( new { message = ConstMessages.COMPLETE_ALL_FIELDS });
            }

            var user = await _auth.SignIn(model.Login);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return Unauthorized();
            }

            var accessToken = _jwt.GenerateJwt(user);
            var refreshToken = _jwt.GenerateRefreshToken(user.Id);

            return Ok( new 
            { 
                accessToken = accessToken 
            });
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp(UserModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new {message = ConstMessages.COMPLETE_ALL_FIELDS });
            }

            var user = await _auth.SignUp(model);
            if (user == null)
            {
                return BadRequest(new { message = ConstMessages.ERROR_REGISTRATION });
            }

            return Ok(new { message = ConstMessages.SUCCESSFUL_REGISTRATION });
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _auth.GetAllUsers();
            if (users == null)
            {
                return BadRequest(new { message = ConstMessages.LIST_EMPTY });
            }

            return Ok(new { users = users });
        }

        [Authorize]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var jwtToken = Request.Headers[ConstAuth.AUTHORIZATION].FirstOrDefault()?.Split(" ").Last();
            var token = _jwt.Verify(jwtToken);
            if (token != null && !token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
            {
                return Unauthorized();
            }

            var user = await _auth.GetByUserFromToken(token);
            var refreshTokenModel = await _jwt.GetRefreshToken(user.Id);
            if (!refreshTokenModel.IsActive)
            {
                return BadRequest(new { isRefreshToken = refreshTokenModel.IsActive });
            }

            var accessToken = _jwt.GenerateJwt(user);
            var refreshToken = _jwt.GenerateRefreshToken(user.Id);

            return Ok(new
            {
                accessToken = accessToken
            });
        }
    }
}
