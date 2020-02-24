using Daewoong.BI.Datas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    
    public class SettingController : ControllerBase
    {
        public SettingController()
        {
        }
        /// <summary>
        /// 모든사용자 취득
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize]
        public string Get()
        {
            var key = Request.Query["key"];
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from setting where keyName ='" + key + "'", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();
                        return reader["Value"].ToString();
                    }
                }
                
            }
        }

        [HttpPost]
        [Route("")]
        public bool Post([FromBody] KeyValuePair<string, string> item)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    if (IsExist(item.Key, conn))
                    {
                       // MySqlCommand cmd = new MySqlCommand("update setting set value='"
                       //+ item.Value + "'"
                       //+ "' where KeyName='" + item.Key + "'", conn);

                        MySqlCommand cmd = new MySqlCommand("update setting set value='"+ item.Value + "' where keyName='" + item.Key + "'", conn);

                        cmd.ExecuteNonQuery();
                    }
                    else
                    {

                        MySqlCommand cmd = new MySqlCommand("insert into setting(keyName, value) values('"
                            + item.Key + "', '"
                            + item.Value + "')", conn);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return true;
        }

        private bool IsExist(string key, MySqlConnection conn)
        {
            using (var db = new DWContext())
            {
                MySqlCommand cmd = new MySqlCommand("select * from setting where keyName ='" + key + "'", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return false;
                    return true;
                }
            }
        }

        [HttpPut]
        [Route("")]
        public bool Update([FromBody]  KeyValuePair<string, string> item)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("update setting set value='"
                        + item.Value + "'"
                        + "' where KeyName=" + item.Key, conn);

                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        [HttpDelete]
        [Route("")]
        public bool Delete()
        {
            var id = HttpContext.Request.Query["key"];
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("delete from user where key=" + id[0], conn);
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }
    }
}