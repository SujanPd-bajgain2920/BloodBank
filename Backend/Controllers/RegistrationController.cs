using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using bloodconnect.Model;
using System;

namespace bloodconnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }



        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration([FromBody] Registration registration)
        {
            if (registration == null)
            {
                return BadRequest("Invalid registration data");
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("Blood");
                using (var dal = new Dal(connectionString))
                {
                    Response response = dal.Register(registration);
                    if (response.StatusCode == 409) // User already exists
                    {
                        return Conflict(response.StatusMessage); // 409 Conflict status code
                    }
                    else if (response.StatusCode == 200) // Registration successful
                    {
                        return Ok(response); // 200 OK status code
                    }
                    else
                    {
                        return Ok(response);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] Registration loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Invalid login request");
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("Blood");
                using (var dal = new Dal(connectionString))
                {
                    Response response = dal.Login(new Registration { Username = loginRequest.Username, Password = loginRequest.Password });
                    if (response.Success)
                    {
                        return Ok(response); // 200 OK status code
                    }
                    else
                    {
                        return Unauthorized(response.StatusMessage); // 401 Unauthorized status code
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}
