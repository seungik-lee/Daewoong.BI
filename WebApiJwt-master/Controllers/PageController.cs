using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    public class PageController : ControllerBase
    {
        private DWBIUser DWUserInfo
        {
            get
            {
                return HttpContext.Session.GetObject<DWBIUser>("DWUserInfo");
            }
        }

        [HttpGet]
        public List<Page> Get()
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                Response.StatusCode = 600;

                return null;
            }

            try
            {
                using (var db = new DWContext())
                {
                    List<Page> list = new List<Page>();
                    KPIController kpi = new KPIController();

                    using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("select * from pages where userID = '" + DWUserInfo.UserID + "' and companyCode = '" + DWUserInfo.CompanyCode + "' order by seq asc", conn);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new Page()
                                {
                                    ID = Convert.ToInt32(reader["Id"]),
                                    Seq = Convert.ToInt32(reader["Seq"]),
                                    KPIs = kpi.GetKpiByPage(Convert.ToInt32(reader["Id"]), DWUserInfo.CompanyCode.ToString()),
                                    Title = reader["Title"].ToString(),
                                    Layout = reader["Layout"].ToString()
                                });
                            }
                        }
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                List<Page> p = new List<Page>();
                Page p1 = new Page();
                p1.Title = ex.Message;
                p.Add(p1);

                Page p2 = new Page();
                p2.Title = ex.InnerException.Message;
                p.Add(p2);
                return p;
            }
        }

        [HttpPost]
        [Route("{userID}")]
        public bool Post([FromBody]Page page, string userID)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("insert into pages(title, layout, userID, companyCode, seq) values('"
                        + page.Title + "', '"
                        + page.Layout + "', '"
                        + userID + "', '"
                        + page.CompanyCode + "', '"
                        + page.Seq + "')", conn);
                    cmd.ExecuteNonQuery();
                    long id = cmd.LastInsertedId;

                    SaveChildKPIs(page.KPIs, id, userID, conn);
                }
            }
            return true;
        }

        [HttpPut]
        [Route("{userID}")]
        public bool Put([FromBody]Page page, string userID)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("update pages set title= '" + page.Title + "', layout= '" + page.Layout + "', seq='" + page.Seq + "', companyCode='" + page.CompanyCode + "' where id='" + page.ID + "'", conn);
                    cmd.ExecuteNonQuery();

                    UpdateChildKPIs(page.KPIs, page.ID, userID, conn);
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
                    MySqlCommand cmd = new MySqlCommand("delete from Pages where id=" + id[0], conn);
                    cmd.ExecuteNonQuery();
                    MySqlCommand cmd2 = new MySqlCommand("delete from kpis where parentPageID=" + id[0], conn);
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        private void UpdateChildKPIs(List<KPI> kPIs, long id, string userID, MySqlConnection conn)
        {
            MySqlCommand cmd = new MySqlCommand("delete from kpis where parentPageID='" + id + "'", conn);
            cmd.ExecuteNonQuery();
            SaveChildKPIs(kPIs, id, userID, conn);
        }

        private void SaveChildKPIs(List<KPI> kPIs, long id, string userID, MySqlConnection conn)
        {
            foreach (KPI kpi in kPIs)
            {
                MySqlCommand cmd = new MySqlCommand("insert into kpis(parentPageID, UserID, seq, KPIBankID) values('"
                       + id + "', '"
                       + userID + "', '"
                       + kpi.Seq + "', '"
                       + kpi.KPIBankID + "')", conn);
                cmd.ExecuteNonQuery();
            }
        }

        private List<KPI> GetKPIs(int v, KPIController c, DWBIUser user, string companyCode)
        {
            return c.GetKpiByPage(v, companyCode);
        }
    }
}