
using System.Data;
using System.Data.SqlClient;
using System.Net;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace bloodconnect.Model
{
    public class Dal : IDisposable
    {
        private readonly SqlConnection _connection;

        public Dal(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        // register the user
        public Response Register(Registration registration)
        {
            using (_connection)
            {
                _connection.Open();

                // Check if the username already exists
                string checkQuery = "SELECT COUNT(*) FROM Registration WHERE Username = @Username";
                using (var checkCmd = new SqlCommand(checkQuery, _connection))
                {
                    checkCmd.Parameters.AddWithValue("@Username", registration.Username);
                    int existingUsersCount = (int)checkCmd.ExecuteScalar();

                    if (existingUsersCount > 0)
                    {
                        return new Response { StatusCode = 100, StatusMessage = "User already registered", Success = false };
                    }
                }

                //hide or hash the password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registration.Password);
                // insert new username 
                string sqlQuery = "INSERT INTO Registration (Username, Password, IsActive) VALUES (@Username, @Password, @IsActive)";

                using (var cmd = new SqlCommand(sqlQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@Username", registration.Username);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.Parameters.AddWithValue("@IsActive", registration.IsActive);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return new Response { StatusCode = 200, StatusMessage = "Registration successful", Success = true };
                    }
                    else
                    {
                        return new Response { StatusCode = 100, StatusMessage = "Registration failed", Success = false };
                    }
                }

            }
        }

        // login of the user
        public Response Login(Registration registration)
        {
            using (_connection)
            {
                try
                {
                    if (_connection.State != ConnectionState.Open)
                    {
                        _connection.Open();
                    }

                    string checkQuery = "SELECT Password FROM Registration WHERE Username = @Username";
                    using (var checkCmd = new SqlCommand(checkQuery, _connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", registration.Username);

                        string hashedPassword = (string)checkCmd.ExecuteScalar();
                        if (!string.IsNullOrEmpty(hashedPassword))
                        {
                            // Compare hashed passwords
                            bool passwordMatch = BCrypt.Net.BCrypt.Verify(registration.Password, hashedPassword);
                            if (passwordMatch)
                            {
                                return new Response { StatusCode = 200, StatusMessage = "Login Successful", Success = true };
                            }
                        }
                        return new Response { StatusCode = 401, StatusMessage = "Invalid username or password", Success = false };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during login: {ex.Message}");
                    return new Response { StatusCode = 500, StatusMessage = "An error occurred during login", Success = false };
                }
                finally
                {
                    _connection.Close();
                }
            }
        }


        // profile created
       public Response Profile(Profile profile) 
{
    using (_connection)
    {
        _connection.Open();

        // Check if the username already exists
        string checkQuery = "SELECT COUNT(*) FROM Profile WHERE Contact = @Contact";
        using (var checkCmd = new SqlCommand(checkQuery, _connection))
        {
            checkCmd.Parameters.AddWithValue("@Contact", profile.Contact);
            int existingUsersCount = (int)checkCmd.ExecuteScalar();

            if (existingUsersCount > 0)
            {
                return new Response { StatusCode = 100, StatusMessage = "Profile already found", Success = false };
            }
        }

        // Insert new profile
        string sqlQuery = "INSERT INTO Profile (Name, District, Contact, Bgroup, Gender, DoB) OUTPUT INSERTED.ID VALUES (@Name, @District, @Contact, @Bgroup, @Gender, @DoB)";

        using (var cmd = new SqlCommand(sqlQuery, _connection))
        {
            cmd.Parameters.AddWithValue("@Name", profile.Name);
            cmd.Parameters.AddWithValue("@District", profile.District);
            cmd.Parameters.AddWithValue("@Contact", profile.Contact);
            cmd.Parameters.AddWithValue("@Bgroup", profile.Bgroup);
            cmd.Parameters.AddWithValue("@Gender", profile.Gender);
            cmd.Parameters.AddWithValue("@DoB", profile.DoB);

            // Execute query and get the ID of the newly inserted profile
            int newProfileId = (int)cmd.ExecuteScalar();

            // Fetch the newly created profile data
            string selectQuery = "SELECT * FROM Profile WHERE ID = @Id";
            using (var selectCmd = new SqlCommand(selectQuery, _connection))
            {
                selectCmd.Parameters.AddWithValue("@Id", newProfileId);
                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Create a new Profile object with the fetched data
                        Profile newProfile = new Profile
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            District = reader.GetString(reader.GetOrdinal("District")),
                            Contact = reader.GetString(reader.GetOrdinal("Contact")),
                            Bgroup = reader.GetString(reader.GetOrdinal("Bgroup")),
                            Gender = reader.GetString(reader.GetOrdinal("Gender")),
                            DoB = reader.GetString(reader.GetOrdinal("DoB")),
                        };

                        return new Response { StatusCode = 200, StatusMessage = "Profile created successfully", Success = true, Data = newProfile };
                    }
                }
            }
        }
    }

    // Return error response if something goes wrong
    return new Response { StatusCode = 100, StatusMessage = "Profile creation unsuccessful", Success = false };
}

        // edit the profile
        public Response UpdateProfile(int id, Profile profile)
        {
            using (_connection)
            {
                _connection.Open();

                // Check if the profile exists
                string checkQuery = "SELECT COUNT(*) FROM Profile WHERE ID = @ID";
                using (var checkCmd = new SqlCommand(checkQuery, _connection))
                {
                    checkCmd.Parameters.AddWithValue("@ID", id);
                    int existingProfilesCount = (int)checkCmd.ExecuteScalar();

                    if (existingProfilesCount == 0)
                    {
                        return new Response { StatusCode = 404, StatusMessage = "Profile not found", Success = false };
                    }
                }

                string sqlQuery = "UPDATE Profile SET Name = @Name, District = @District, Contact = @Contact, Bgroup = @Bgroup, Gender = @Gender, DoB = @DoB WHERE ID = @ID";

                using (var cmd = new SqlCommand(sqlQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Name", profile.Name);
                    cmd.Parameters.AddWithValue("@District", profile.District);
                    cmd.Parameters.AddWithValue("@Contact", profile.Contact);
                    cmd.Parameters.AddWithValue("@Bgroup", profile.Bgroup);
                    cmd.Parameters.AddWithValue("@Gender", profile.Gender);
                    cmd.Parameters.AddWithValue("@DoB", profile.DoB);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        // No rows were updated
                        return new Response { StatusCode = 404, StatusMessage = "Profile not found or no changes made", Success = false };
                    }

                    // Rows were updated, continue fetching the updated profile ID
                    int updateProfileId = id;

                    string selectQuery = "SELECT * FROM Profile WHERE ID = @Id";
                    using (var selectCmd = new SqlCommand(selectQuery, _connection))
                    {
                        selectCmd.Parameters.AddWithValue("@Id", updateProfileId);
                        using (var reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Create a new Profile object with the fetched data
                                Profile updateProfile = new Profile
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    District = reader.GetString(reader.GetOrdinal("District")),
                                    Contact = reader.GetString(reader.GetOrdinal("Contact")),
                                    Bgroup = reader.GetString(reader.GetOrdinal("Bgroup")),
                                    Gender = reader.GetString(reader.GetOrdinal("Gender")),
                                    DoB = reader.GetString(reader.GetOrdinal("DoB")),
                                };

                                return new Response { StatusCode = 200, StatusMessage = "Profile updated successfully", Success = true, Data = updateProfile };
                            }
                        }
                    }
                }
            }

            // Return error response if something goes wrong
            return new Response { StatusCode = 100, StatusMessage = "Profile update unsuccessful", Success = false };
        }


        // profile get
        public Profile GetProfile(int id)
        {
            using (_connection)
            {
                _connection.Open();

                string selectQuery = "SELECT * FROM Profile WHERE ID = @Id";
                using (var cmd = new SqlCommand(selectQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Create a new Profile object with the fetched data
                            Profile profile = new Profile
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                District = reader.GetString(reader.GetOrdinal("District")),
                                Contact = reader.GetString(reader.GetOrdinal("Contact")),
                                Bgroup = reader.GetString(reader.GetOrdinal("Bgroup")),
                                Gender = reader.GetString(reader.GetOrdinal("Gender")),
                                DoB = reader.GetString(reader.GetOrdinal("DoB")),
                            };

                            return profile;
                        }
                    }
                }
            }

            return null; // Return null if profile with given ID is not found
        }


        // delete the profile
        public Response DeleteProfile(int id)
        {
            using (_connection)
            {
                _connection.Open();

                // Check if the profile exists
                string checkQuery = "SELECT COUNT(*) FROM Profile WHERE ID = @ID";
                using (var checkCmd = new SqlCommand(checkQuery, _connection))
                {
                    checkCmd.Parameters.AddWithValue("@ID", id);
                    int existingProfilesCount = (int)checkCmd.ExecuteScalar();

                    if (existingProfilesCount == 0)
                    {
                        return new Response { StatusCode = 404, StatusMessage = "Profile not found", Success = false };
                    }
                }

                string sqlQuery = "DELETE FROM Profile WHERE ID = @ID";

                using (var cmd = new SqlCommand(sqlQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@ID", id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return new Response { StatusCode = 200, StatusMessage = "Profile deleted successfully", Success = true };
                    }
                    else
                    {
                        return new Response { StatusCode = 100, StatusMessage = "Profile deletion unsuccessful", Success = false };
                    }
                }
            }
        }


        // request blood

        public List<Profile> RetrieveProfiles(string District, string Bgroup)
        {
            List<Profile> profiles = new List<Profile>();
            using (_connection)
            {
                _connection.Open();
                string sqlQuery = "SELECT Name, District, Contact, Bgroup FROM Profile WHERE District = @District AND Bgroup = @Bgroup";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@District", District);
                    cmd.Parameters.AddWithValue("@Bgroup", Bgroup);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Map each retrieved data to a Profile object and add it to the list
                            Profile profile = new Profile
                            {
                                Name = reader["Name"].ToString(),
                                Contact = reader["Contact"].ToString(),
                                // Add other properties as needed
                            };
                            profiles.Add(profile);
                        }
                    }
                }
            }
            return profiles;
        }


        public void Dispose()
        {
            _connection?.Dispose();
        }

      
    }
}
