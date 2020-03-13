using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    public class CategoryController : ControllerBase
    {
        [HttpGet]
        public List<Category> Get()
        {
            List<Category> list = new List<Category>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from category", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Category()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                            });
                        }
                    }
                }
                return list;
            }
        }
    }
}