using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        string fineKey = "~gw-biadvancement-SecretKey";
        string fineIV = "~gw-biadvancement-InitVector";

        string biKey = "~gw-biportal-SecretKey";
        string biIV = "~gw-biportal-InitVector";

        [HttpPost]
        public DWBIUser Login([FromBody] LoginDto model)
        {
            if (!model.Email.Contains("@"))
            {
                //string dEmail = HttpUtility.UrlDecode(model.Email);
                model.Email = model.Email;
                string loginID = Encryptor.Decrypt(model.Email, biKey, biIV);
                if (string.IsNullOrEmpty(loginID))
                    return null;

                model.Email = loginID;
                model.Password = "dwbi11!!";
            }

            DWBIUser dwbiUser = new DWBIUser();

            try
            {
                using (var db = new DWContext())
                {
                    using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                    {
                        conn.Open();

                        Encryptor ec = new Encryptor(fineKey, fineIV);
                        string encrypt_password = ec.Encrypt(model.Password);

                        MySqlCommand cmd = new MySqlCommand("get_userInfo", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("@userID", model.Email));
                        cmd.Parameters.Add(new MySqlParameter("@pw", encrypt_password));

                        DataTable dt = new DataTable();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        adapter.Fill(dt);

                        if (dt != null && dt.Rows.Count == 1)
                        {
                            dwbiUser.ID = Convert.ToInt32(dt.Rows[0]["ID"].ToString());
                            dwbiUser.Name = dt.Rows[0]["Name"].ToString();
                            dwbiUser.UserID = dt.Rows[0]["UserID"].ToString();
                            dwbiUser.IsAdmin = (dt.Rows[0]["IsAdmin"].ToString() == "0") ? false : true;
                            dwbiUser.RoleID = dt.Rows[0]["RoleID"].ToString();
                            dwbiUser.UserRole = (Role)int.Parse(dt.Rows[0]["UserRole"].ToString());
                            dwbiUser.CompanyID = Convert.ToInt32(dt.Rows[0]["CompanyID"].ToString());
                            dwbiUser.CompanyCode = Convert.ToInt32(dt.Rows[0]["code"].ToString());
                            dwbiUser.CompanyName = dt.Rows[0]["companyName"].ToString();
                            dwbiUser.Companies = GetCompanies(dwbiUser.ID);
                            dwbiUser.key = ec.Encrypt(dwbiUser.UserID.Split('@')[0]);
                            dwbiUser.RoleIDKey = ec.Encrypt(dwbiUser.RoleID);

                            // Admin, Manager인 경우 회사 코드를 변경해 주고, 회사 정보를 설정
                            if (dwbiUser.UserRole != Role.Member)
                            {
                                dwbiUser.CompanyCode = 1200;
                                // 모든 회사 코드 조회해서 넘겨줌
                                //dwbiUser.Companies = GetAllCompanies();
                            }

                            HttpContext.Session.SetObject("DWUserInfo", dwbiUser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dwbiUser.Token = ex.InnerException.Message + ex.Message;
            }

            return dwbiUser;
        }

        /// <summary>
        /// ID가 속해 있는 회사 코드 조회
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<Company> GetCompanies(int id)
        {
            List<Company> companies = new List<Company>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_CompaniesByUserID", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@id", id));
                    
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
                }
            }

            return companies;
        }

        /// <summary>
        /// 모든 회사 코드 조회
        /// </summary>
        /// <returns></returns>
        private List<Company> GetAllCompanies()
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
            }

            return list;
        }

        public String AESEncrypt128(String Input, String key)
        {

            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(Input);
            byte[] Salt = Encoding.ASCII.GetBytes(key.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(PlainText, 0, PlainText.Length);
            cryptoStream.FlushFinalBlock();

            byte[] CipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string EncryptedData = Convert.ToBase64String(CipherBytes);

            return EncryptedData;
        }

        [DataContract]

        public class LoginDto
        {
            [Required]
            [DataMember]
            public string Email { get; set; }

            [Required]
            [DataMember]
            public string Password { get; set; }

        }

        [DataContract]
        public class RegisterDto
        {
            [Required]
            [DataMember]
            public string Email { get; set; }

            [Required]
            [DataMember]
            [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
            public string Password { get; set; }
        }


    }
}