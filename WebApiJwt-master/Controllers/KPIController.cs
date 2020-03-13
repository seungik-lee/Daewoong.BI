using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    
    public class KPIController : ControllerBase
    {
        private DWBIUser DWUserInfo
        {
            get
            {
                return HttpContext.Session.GetObject<DWBIUser>("DWUserInfo");
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
        public List<KPI> Get()
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;
                
                return null;
            }

            List<KPI> list = new List<KPI>();

            int mainCompanyCode = DWUserInfo.CompanyCode;

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_AllKPIs", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@companyCode", mainCompanyCode));

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

                if (DWUserInfo.UserRole == Role.Member) {
                    return list.Where(o => o.CompanyCode == mainCompanyCode.ToString()).ToList();
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