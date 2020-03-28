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
    public class DogController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DogController(IConfiguration config)
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
                        SELECT d.Id, d.Name, d.Breed, d.Notes, d.OwnerId, o.Name AS OwnerName, o.NeighborhoodId, o.Address, o.Phone, n.Name AS NeighborhoodName
                        FROM Dog d
                        LEFT JOIN Owner o
                        ON d.OwnerId = o.Id
                        LEFT JOIN Neighborhood n
                        ON o.NeighborhoodId = n.Id
                        ";

                    SqlDataReader reader = cmd.ExecuteReader();
                    var dog = new List<Dog>();

                    while (reader.Read())
                    {
                        var newDog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Owner = new Owner()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("OwnerName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Neighborhood = new Neighborhood()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("NeighborhoodName"))
                                }
                            }
                        };

                        dog.Add(newDog);
                    }
                    reader.Close();

                    return Ok(dog);
                }
            }
        }


        // Get a single department by Id from database
        [HttpGet("{id}", Name = "GetDog")]
        public async Task<IActionResult> GET([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.Id, d.Name, d.Breed, d.Notes, d.OwnerId, o.Name AS OwnerName, o.NeighborhoodId, o.Address, o.Phone, n.Name AS NeighborhoodName
                        FROM Dog d
                        LEFT JOIN Owner o
                        ON d.OwnerId = o.Id
                        LEFT JOIN Neighborhood n
                        ON o.NeighborhoodId = n.Id
                        WHERE d.Id = @id
                        ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dog dog = null;

                    if (reader.Read())
                    {
                        dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Owner = new Owner()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("OwnerName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Neighborhood = new Neighborhood()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("NeighborhoodName"))
                                }
                            }
                        };
                        reader.Close();

                        return Ok(dog);
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
        public async Task<IActionResult> POST([FromBody] Dog dog)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Dog (Name, Breed, Notes, OwnerId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @breed, @notes, @ownerId)";

                    cmd.Parameters.Add(new SqlParameter("@name", dog.Name));
                    cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                    cmd.Parameters.Add(new SqlParameter("@notes", (object)dog.Notes ?? DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@ownerId", dog.OwnerId));

                    int newId = (int)cmd.ExecuteScalar();
                    dog.Id = newId;
                    return CreatedAtRoute("GetDog", new { id = newId }, dog);
                }
            }
        }

        //Update single department by id in database
        [HttpPut("{id}")]
        public async Task<IActionResult> PUT([FromRoute] int id, [FromBody] Dog dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Dog
                                            SET 
                                            Name = @name,
                                            Breed = @breed,
                                            Notes = @notes,
                                            OwnerId = @ownerId
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", dog.Name));
                        cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));
                        cmd.Parameters.Add(new SqlParameter("@ownerId", dog.OwnerId));
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
                        cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";

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
                        SELECT Id, Name, Breed, Notes, OwnerId
                        FROM Dog
                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}