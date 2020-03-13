using Daewoong.BI.Datas;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daewoong.BI.Helper
{
    public class UserHelper
    {
        public void GetCurrentUser()
        {
        }

        public DWBIUser GetByKey(string id, HttpRequest Request)
        {
            return new DWBIUser
            {
                Name = "Test"
            };
            //try
            //{
            //    List<DWBIUser> list = new List<DWBIUser>();

            //    using (var db = new DWContext())
            //    {
            //        using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
            //        {
            //            conn.Open();
            //            MySqlCommand cmd = new MySqlCommand("get_userByID", conn);
            //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //            cmd.Parameters.Add(new MySqlParameter("@id", id));

            //            using (var reader = cmd.ExecuteReader())
            //            {
            //                if (!reader.HasRows)
            //                    return null;

            //                reader.Read();
            //                DWBIUser user = new DWBIUser();

            //                user.ID = Convert.ToInt32(reader["Id"]);
            //                user.Name = reader["Name"].ToString();
            //                user.CompanyID = Convert.ToInt32(reader["CompanyID"]);
            //                user.CompanyCode = Convert.ToInt32(reader["Code"]);
            //                if (user.CompanyCode == 2000)
            //                    user.CompanyCode = 1100;
            //                user.UserID = reader["UserID"].ToString();
            //                user.RoleID = reader["RoleID"].ToString();
            //                user.CompanyName = reader["CompanyName"].ToString();
            //                user.IsAdmin = Convert.ToBoolean(reader["IsAdmin"]);
            //                user.UserRole = (Role)Convert.ToInt32(reader["UserRole"]);

            //                if (user.UserRole == Role.Manager)
            //                {
            //                    if (Request != null && !string.IsNullOrEmpty(Request.Headers["company"]))
            //                        user.CompanyCode = int.Parse(Request.Headers["company"]);
            //                }

            //                return user;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message + ex.InnerException.Message);
            //}
        }
    }
}
