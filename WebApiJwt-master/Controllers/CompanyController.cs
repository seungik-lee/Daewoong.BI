using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
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
        private DWBIUser DWUserInfo
        {
            get
            {
                return HttpContext.Session.GetObject<DWBIUser>("DWUserInfo");
            }
        }

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

        /// <summary>
        /// 접속한 회원이 가지고 있는 모든 회사 리스트 조회
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCompanies")]
        public List<Company> GetCompanies()
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;

                return null;
            }

            if (DWUserInfo.UserRole == Role.Member)
            {
                return null;
            }

            // 모든 회사 코드 조회해서 넘겨줌
            return Get();
        }

        /// <summary>
        /// 시나리오 작성 페이지에서 사용되는 회사 정보
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCompanyInScenario")]
        public List<Company> GetCompanyInScenario()
        {
            List<Company> list = new List<Company>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("select * from company where code in (1200, 1300, 1400, 1500) order by code asc", conn);

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