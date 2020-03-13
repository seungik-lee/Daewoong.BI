using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]

    //20200109 김태규 수정 배포
    //public class UserController : ControllerBase
    public class UserController : Controller
    {
        string fineKey = "~gw-biadvancement-SecretKey";
        string fineIV = "~gw-biadvancement-InitVector";

        private DWBIUser DWUserInfo
        {
            get
            {
                return HttpContext.Session.GetObject<DWBIUser>("DWUserInfo");
            }
        }

        /// <summary>
        /// 모든사용자 취득
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize]
        public List<DWBIUser> Get()
        {
            List<DWBIUser> list = new List<DWBIUser>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_users", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DWBIUser()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                CompanyCode = Convert.ToInt32(reader["CompanyCode"]),
                                UserID = reader["UserID"].ToString(),
                                CompanyName = reader["CompanyName"].ToString()
                            });
                        }
                    }
                }
                return list;
            }
        }

        public DWBIUser GetByKey(string id, HttpRequest Request)
        {
            try
            {
                List<DWBIUser> list = new List<DWBIUser>();
                //2019-12-26 김태규 수정 배포
                DWBIUser user = new DWBIUser();

                using (var db = new DWContext())
                {
                    using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("get_userByID", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("@id", id));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                                return null;

                            reader.Read();
                            //2019-12-26 김태규 수정 배포
                            //DWBIUser user = new DWBIUser();

                            user.ID = Convert.ToInt32(reader["Id"]);
                            user.Name = reader["Name"].ToString();
                            user.CompanyCode = Convert.ToInt32(reader["Code"]);
                            if (user.CompanyCode == 2000)
                                user.CompanyCode = 1100;
                            user.UserID = reader["UserID"].ToString();
                            user.RoleID = reader["RoleID"].ToString();
                            user.CompanyName = reader["CompanyName"].ToString();
                            user.IsAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                            user.UserRole = (Role)Convert.ToInt32(reader["UserRole"]);

                            if (user.UserRole == Role.Manager)
                            {
                                if (Request != null && !string.IsNullOrEmpty(Request.Headers["company"]))
                                    user.CompanyCode = int.Parse(Request.Headers["company"]);
                            }

                            //return user;
                        }
                    }
                }
                //2019-12-26 김태규 수정 배포
                user.Companies = GetCompanies(user.ID);

                return user;

            }
            catch (MySqlProtocolException ex)
            {
                throw new Exception(ex.Message + ex.InnerException.Message);
            }
        }

        [HttpGet]
        [Route("GetCompanies")]
        public List<Company> GetCompanies(int id)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_CompaniesByUserID", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@id", id));

                    List<Company> companies = new List<Company>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            companies.Add(new Company()
                            {
                                CompanyName = reader["companyName"].ToString(),
                                Code = Convert.ToInt32(reader["code"]),
                            });
                        }
                    }
                    return companies;
                }
            }
        }

        public List<DWBIUser> GetUserByCompany(int companyID)
        {
            List<DWBIUser> list = new List<DWBIUser>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from user where companyID=" + companyID, conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DWBIUser()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                UserID = reader["UserID"].ToString()
                            });
                        }
                    }
                }
                return list;
            }

        }

        [HttpPost]
        [Route("")]
        public bool Post([FromBody] DWBIUser user)
        {
            return SaveUser(user);
        }

        public bool SaveUser(DWBIUser user)
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;

                return false;
            }

            try
            {
                using (var db = new DWContext())
                {
                    using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                    {
                        conn.Open();

                        Encryptor ec = new Encryptor(fineKey, fineIV);
                        string encrypt_password = ec.Encrypt(user.Password);

                        MySqlCommand cmd = new MySqlCommand(
                            $@" insert into user (Name, CompanyID, UserRole, isAdmin, UserID, PW) 
                                values('{user.Name}', {user.CompanyID}, {(int)user.UserRole}, 0, '{user.UserID}', '{encrypt_password}')", conn);

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        [HttpPut]
        [Route("")]
        public bool Update([FromBody] DWBIUser user)
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;

                return false;
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    string update_sql = $@" 
                            update user 
                            set name = '{user.Name}'
                                , companyID = {user.CompanyID}
                                , UserID = '{user.UserID}'
                                , UserRole= {(int)user.UserRole}
                            where id = '{user.ID}'";

                    if (!String.IsNullOrWhiteSpace(user.Password))
                    {
                        Encryptor ec = new Encryptor(fineKey, fineIV);
                        string encrypt_password = ec.Encrypt(user.Password);

                        update_sql = $@" 
                            update user 
                            set name = '{user.Name}'
                                , companyID = {user.CompanyID}
                                , UserID = '{user.UserID}'
                                , UserRole= {(int)user.UserRole}
                                , PW = '{encrypt_password}'
                            where id = '{user.ID}'";
                    }

                    MySqlCommand cmd = new MySqlCommand(update_sql, conn);

                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        [HttpPost]
        [Route("DeleteUser")]
        //20200109 김태규 수정 배포
        //public bool Delete()
        public bool DeleteUser([FromBody] DWBIUser user)
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;

                return false;
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("delete from user where id=" + user.ID, conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        // 세션 초기화
        [HttpGet]
        [Route("InitializeSessionUserInfo")]
        public void InitializeSessionUserInfo()
        {
            HttpContext.Session.SetObject("DWUserInfo", null);
        }

        [HttpGet]
        [Route("ChangeUserCompanyInfo")]
        public string ChangeUserCompanyInfo(string companyCode, string companyName)
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;

                return null;
            }

            DWBIUser dwbiUser = DWUserInfo;
            
            dwbiUser.CompanyCode = int.Parse(companyCode);
            dwbiUser.CompanyName = companyName;

            HttpContext.Session.SetObject("DWUserInfo", dwbiUser);

            return "success";
        }

        [HttpGet]
        [Route("ChangeUserPassword")]
        public string ChangeUserPassword(string oldPassword, string newPassword)
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;

                return null;
            }

            if (String.IsNullOrWhiteSpace(oldPassword) || String.IsNullOrWhiteSpace(newPassword))
            {
                return "fail:비밀번호 정보가 정상적으로 전달되지 않았습니다.";
            }

            Encryptor ec = new Encryptor(fineKey, fineIV);
            string old_encrypt_password = ec.Encrypt(oldPassword);
            string new_encrypt_password = ec.Encrypt(newPassword);

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    string read_sql = $"select * from user where userid = '{DWUserInfo.UserID}' and pw = '{old_encrypt_password}'";

                    MySqlCommand cmd = new MySqlCommand(read_sql, conn);
                    
                    DataTable dt = new DataTable();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dt);


                    if (dt == null || dt.Rows.Count != 1)
                    {
                        return "fail:이전 비밀번호가 일치하지 않습니다. 비밀번호를 다시 확인해 주세요.";
                    }

                    string update_sql = $"update user set pw = '{new_encrypt_password}' where userid = '{DWUserInfo.UserID}'";

                    cmd = new MySqlCommand(update_sql, conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return "success:비밀번호가 변경되었습니다. 다시 로그인해 주세요.";
        }
    }
}