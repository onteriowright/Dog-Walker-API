using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalkerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogWalkerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NeighborhoodController : ControllerBase
    {
        private readonly IConfiguration _config;

        public NeighborhoodController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        // Get all departments from the database
        [HttpGet]
        public async Task<IActionResult> GET()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM Neighborhood
                        ";

                    SqlDataReader reader = cmd.ExecuteReader();

                    var neighborhood = new List<Neighborhood>();

                    while (reader.Read())
                    {
                        var newNeighorhood = new Neighborhood
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        neighborhood.Add(newNeighorhood);
                    }
                    reader.Close();

                    return Ok(neighborhood);
                }
            }
        }


        // Get a single department by Id from database
        [HttpGet("{id}", Name = "GetNeighborhood")]
        public async Task<IActionResult> GET([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM Neighborhood
                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Neighborhood neighborhood = null;

                    if (reader.Read())
                    {
                        neighborhood = new Neighborhood
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))                        
                        };
                        reader.Close();

                        return Ok(neighborhood);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }


        // Create department and add it to database
        [HttpPost]
        public async Task<IActionResult> POST([FromBody] Neighborhood neighborhood)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Neighborhood (Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", neighborhood.Name));

                    int newId = (int)cmd.ExecuteScalar();
                    neighborhood.Id = newId;
                    return CreatedAtRoute("GetNeighborhood", new { id = newId }, neighborhood);
                }
            }
        }

        //Update single department by id in database
        [HttpPut("{id}")]
        public async Task<IActionResult> PUT([FromRoute] int id, [FromBody] Neighborhood neighborhood)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Neighborhood
                                            SET 
                                            Name = @name
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", neighborhood.Name));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // Delete single department by id from database
        [HttpDelete("{id}")]
        public async Task<IActionResult> DELETE([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE 
                                            FROM Neighborhood 
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // Check to see if department exist by id in database
        private bool DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM Neighborhood
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}