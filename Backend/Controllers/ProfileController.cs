using bloodconnect.Model;
using Microsoft.AspNetCore.Mvc;

namespace bloodconnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProfileController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        // api to create profile
        /*[HttpPost]
        [Route("Profile")]
        public IActionResult Profile([FromBody] Profile profile)
        {
            if (profile == null)
            {
                return BadRequest("Invalid data");
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("Blood");
                using (var dal = new Dal(connectionString))
                {
                    Response response = dal.Profile(profile);
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
        */


        [HttpPost]
        [Route("Profile")]
        public IActionResult Profile([FromBody] Profile profile)
        {
            if (profile == null)
            {
                return BadRequest("Invalid data");
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("Blood");
                using (var dal = new Dal(connectionString))
                {
                    Response response = dal.Profile(profile);
                    if (response.StatusCode == 409) // Profile already exists for this user
                    {
                        return Conflict(response.StatusMessage); // 409 Conflict status code
                    }
                    else if (response.StatusCode == 200) // Profile creation successful
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

        // to get the data
        [HttpGet]
        [Route("Profile/{id}")]
        public IActionResult GetProfile(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("Blood");
                using (var dal = new Dal(connectionString))
                {
                    Profile profile = dal.GetProfile(id);
                    if (profile != null)
                    {
                        return Ok(profile); // 200 OK status code
                    }
                    else
                    {
                        return NotFound(); // 404 Not Found status code
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }





        // to edit the profile
        [HttpPut]
        [Route("EditProfile/{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] Profile profile)
         {
             if (profile == null)
             {
                 return BadRequest("Invalid data");
             }

             try
             {
                 string connectionString = _configuration.GetConnectionString("Blood");
                 using (var dal = new Dal(connectionString))
                 {
                     Response response = dal.UpdateProfile(id, profile);
                     if (response.StatusCode == 404) // Profile not found
                     {
                         return NotFound(response.StatusMessage); // 404 Not Found status code
                     }
                     else if (response.StatusCode == 200) // Profile updated successfully
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

       





        // to delete the profile
        [HttpDelete]
        [Route("Profile/{id}")]
        public IActionResult DeleteProfile(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("Blood");
                using (var dal = new Dal(connectionString))
                {
                    Response response = dal.DeleteProfile(id);
                    if (response.StatusCode == 404) // Profile not found
                    {
                        return NotFound(response.StatusMessage); // 404 Not Found status code
                    }
                    else if (response.StatusCode == 200) // Profile deleted successfully
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



        // to  search blood
        [HttpGet]
        [Route("Retriveprofile")]
        public IActionResult RetrieveProfiles([FromQuery] string district, [FromQuery] string requiredBloodGroup)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("Blood");
                using (var dal = new Dal(connectionString))
                {
                    // Call the RetrieveProfiles method from DAL to fetch profile data
                    List<Profile> profiles = dal.RetrieveProfiles(district, requiredBloodGroup);

                    if (profiles.Count > 0)
                    {
                        return Ok(profiles); // Return the fetched profile data
                    }
                    else
                    {
                        return NotFound(); // Profiles not found
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