using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    
    public class KPIController : ControllerBase
    {
        #region UserController
        private UserController _userController;
        public UserController UserController
        {
            get
            {
                if (_userController == null)
                {
                    //20200109 김태규 수정 배포
                    //_userController = new UserController();
                    _userController = new UserController(null, null, null);
                }
                return _userController;
            }

        }

        #endregion

        private string UserID
        {
            get
            {
                return User.FindFirst("sub")?.Value;
            }
        }

        public KPIController()
        {
        }

        /// <summary>
        /// 회사별 지표를 취득하여 반환한다.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public List<KPI> Get()
        {
            List<KPI> list = new List<KPI>();
            var user = UserController.GetByKey(UserID, Request);
			//2019-12-26 김태규 수정 배포
			var id = HttpContext.Request.Headers["company"];
            var company = "";
            if (id.Count > 0)
                company = id.FirstOrDefault();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_AllKPIs", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    //2019-12-26 김태규 수정 배포
                    //cmd.Parameters.Add(new MySqlParameter("@companyCode", user.CompanyCode));
                    cmd.Parameters.Add(new MySqlParameter("@companyCode", company));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new KPI()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                Category = reader["Category"].ToString(),
                                URL = reader["URL"].ToString(),
                                DetailURL = reader["DetailURL"].ToString(),
                                Title = reader["Title"].ToString(),
                                KPIBankID = reader["Id"].ToString(),
                                CompanyCode = reader["code"].ToString(),
                                CompanyName = reader["CompanyName"].ToString(),
                                Close = reader["Close"].ToString(),
                                ChartType = reader["ChartType"].ToString()
                            });
                        }
                    }
                }
                if (user.UserRole == Role.Member) {
                    //return list.Where(o => o.CompanyCode == user.CompanyCode.ToString()).ToList();
                    //2019-12-26 김태규 수정 배포
                    return list.Where(o => o.CompanyCode == company).ToList();
                } else {
                    return list;
              	}
            }

        }

        public List<KPI> GetKpiByPage(int pageID, string companyCode)
        {
            List<KPI> list = new List<KPI>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_KPIs", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@id", pageID));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new KPI()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Seq = Convert.ToInt32(reader["Seq"]),
                                Category = reader["Category"].ToString(),
                                URL = reader["URL"].ToString(),
                                CompanyCode = reader["CompanyCode"].ToString(),
                                DetailURL = reader["DetailURL"].ToString(),
                                KPIBankID = reader["BankID"].ToString(),
                                ChartType = reader["ChartType"].ToString(),
                                Close = reader["close"].ToString()
                            });
                        }
                    }
                  
                }
                return list.Where(o=>o.CompanyCode == companyCode).ToList();
            }

        }

        [HttpPost]
        [Route("")]
        public bool Post([FromBody] KPI kpi)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("insert into kpibank(category, url, detailURL, close, companyCode, chartType, title) values('"
                        + kpi.Category + "', '"
                        + kpi.URL + "', '"
                        + kpi.DetailURL + "', '"
                        + kpi.Close + "', '"
                        + kpi.CompanyCode + "', '"
                        + kpi.ChartType + "', '"
                        + kpi.Title + "')", conn);
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        [HttpPut]
        [Route("")]
        public bool Update([FromBody] KPI kpi)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("update kpibank set category='" 
                        + kpi.Category + "', url= '"  + kpi.URL 
                        + "', title= '" + kpi.Title 
                        + "', category= '" + kpi.Category
                        + "', close= '" + kpi.Close
                        + "', companyCode= '" + kpi.CompanyCode
                        + "', chartType= '" + int.Parse(kpi.ChartType)
                        + "', detailURL= '" + kpi.DetailURL
                        + "' where id=" + kpi.ID, conn);

                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        [HttpDelete]
        [Route("")]
        public bool Delete()
        {
            var id = HttpContext.Request.Query["id"];
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("delete from kpibank where id=" + id[0], conn);
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }
    }
}