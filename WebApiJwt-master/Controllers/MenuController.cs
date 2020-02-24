using System;
using System.Collections.Generic;
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
    //[ApiController]
    public class MenuController : ControllerBase
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

        private string UserID {
            get {
                return User.FindFirst("sub")?.Value;
            }
        }
        [Authorize]
        [HttpGet]
        public List<MenuSet> Get(string companyID)
        {
            var user = UserController.GetByKey(UserID, Request);
            using (var db = new DWContext())
            {
                List<Menu> list = new List<Menu>();
                KPIController kpi = new KPIController();
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {   
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_menusByCompany", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@roleID", user.RoleID));
                    cmd.Parameters.Add(new MySqlParameter("@CompanyCode", companyID));


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //2020-01-23 임병규 수정 배포 : 사장님 FineReport 메뉴 사용안함 처리
                            if (user.UserID == "younjc@daewoong.co.kr" && reader["Category"].ToString() == "FineReport") { }                          
                            else {
                                list.Add(new Menu()
                                {
                                    ID = Convert.ToInt32(reader["Id"]),
                                    Category = reader["Category"].ToString(),
                                    Close = reader["Close"].ToString(),
                                    CompanyCode = Convert.ToInt32(reader["Id"]),
                                    Title = reader["Title"].ToString(),
                                    URL = reader["Url"].ToString(),
                                    Level = reader["Level"].ToString(),
                                    ParentID = reader["ParentID"].ToString(),
                                });
                            }
                        }
                    }
                }
                var results = list.GroupBy(o=>o.Category);

                List<MenuSet> menuSet = new List<MenuSet>();
                foreach (var item in results)
                {
                    MenuSet set = new MenuSet();
                    set.Category = item.Key;
                    set.Menus = getMenu(item);
                    menuSet.Add(set);
                }

                return menuSet;

            }

        }

        private List<Menu> getMenu(IGrouping<string, Menu> item)
        {
            List<Menu> menus = new List<Menu>();
            foreach (Menu menu in item)
            {
                menus.Add(menu);
            }
            return menus;
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
                    MySqlCommand cmd = new MySqlCommand("insert into pages(title, layout, userID, seq) values('"
                        + page.Title + "', '"
                        + page.Layout + "', '"
                        + userID + "', '"
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
                    MySqlCommand cmd = new MySqlCommand("update pages set title= '" + page.Title + "', layout= '" + page.Layout + "', seq='" + page.Seq + "' where id='" + page.ID + "'", conn);
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

        //private List<KPI> GetKPIs(int v, KPIController c)
        //{
        //    return c.GetKpiByPage(v);
        //}
    }
}