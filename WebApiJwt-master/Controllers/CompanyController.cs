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
    public class CompanyController : ControllerBase
    {
        [HttpGet]
        public List<Company> Get()
        {
            List<Company> list = new List<Company>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from company order by (case id when 2 then 0 else 1 end) asc, id asc", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Company()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                CompanyName = reader["CompanyName"].ToString(),
                                Logo = reader["Logo"].ToString(),
                                Code = Convert.ToInt32(reader["Code"]),
                            });
                        }
                    }
                }
                return list;
            }
        }
    }
}