using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using cloudscribe.Pagination.Models;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace Daewoong.BI
{
    public class ScenarioListModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int pageNo { get; set; } = 0;

        public cloudscribe.Pagination.Models.PagedResult<BusinessBase> PagedResult { get; set; }

        private DWBIUser DWUserInfo
        {
            get
            {
                return HttpContext.Session.GetObject<DWBIUser>("DWUserInfo");
            }
        }

        public IActionResult OnGet()
        {
            // 세션이 끊긴 상태
            if (DWUserInfo == null || DWUserInfo.ID == 0)
            {
                return Redirect("/login.html");
            }

            if (this.pageNo == 0)
            {
                return Redirect("/business/scenariolist/1");
            }

            List<BusinessBase> baseList = new List<BusinessBase>();
            int baseListCount = 0;
            int pageSize = 8;

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(@"
                        select business_id, dates, caption
                            , update_date, writer
                            , ispublish, ifnull(publish_date, '9999-12-31') as publish_date
                            , isscenario, ifnull(scenario_date, '9999-12-31') as scenario_date
                            , isanalysis, ifnull(analysis_date, '9999-12-31') as analysis_date 
                        from business_base 
                        order by business_id desc", conn);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    baseListCount = dt.Rows.Count;

                    foreach (DataRow dr in dt.Rows)
                    {
                        BusinessBase businessBase = new BusinessBase();
                        businessBase.BusinessID = Int32.Parse(dr["business_id"].ToString());
                        businessBase.Dates = DateTime.Parse(dr["dates"].ToString());
                        businessBase.Caption = dr["caption"].ToString();
                        businessBase.UpdateDate = DateTime.Parse(dr["update_date"].ToString());
                        businessBase.IsPublish = dr["ispublish"].ToString();
                        businessBase.PublishDate = DateTime.Parse(dr["publish_date"].ToString());
                        businessBase.Writer = dr["writer"].ToString();
                        businessBase.IsScenario = dr["isscenario"].ToString();
                        businessBase.ScenarioDate = DateTime.Parse(dr["scenario_date"].ToString());
                        businessBase.IsAnalysis = dr["isanalysis"].ToString();
                        businessBase.AnalysisDate = DateTime.Parse(dr["analysis_date"].ToString());

                        baseList.Add(businessBase);
                    }

                    // 페이지 개수보다 더 높은 pageNo가 들어오면 1로 초기화
                    if ((baseListCount % pageSize == 0))
                    {
                        if ((baseListCount / pageSize) < pageNo)
                        {
                            pageNo = 1;
                        }
                    }
                    else
                    {
                        if ((baseListCount / pageSize) + 1 < pageNo)
                        {
                            pageNo = 1;
                        }
                    }

                    int exclude_item_index = (pageNo * pageSize) - pageSize;

                    if (baseList.Count > 0)
                    {
                        baseList = baseList.Skip(exclude_item_index).Take(pageSize).ToList();
                    }

                    //cmd = new MySqlCommand(@"update menu set url = '/business/scenario' where id = 1114;update menu set url = '/business/causeanalysis' where id = 1115;update menu set url = '/business/managingmeeting' where id = 1116; ", conn);
                    //cmd.ExecuteNonQuery();

                }
            }

            PagedResult = new PagedResult<BusinessBase>
            {
                Data = baseList,
                TotalItems = baseListCount,
                PageNumber = pageNo,
                PageSize = pageSize
            };

            return Page();
        }

        [HttpGet]
        public JsonResult OnGetCheckScenario(string business_ids)
        {
            if (String.IsNullOrWhiteSpace(business_ids))
            {
                return new JsonResult("시나리오를 완료시킬 항목이 선택되지 않았습니다.");
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand($"update business_base set isScenario = 'Y', scenario_date = now() where business_id in ({business_ids.TrimEnd(',')})", conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return new JsonResult("정상적으로 처리되었습니다.");
        }

        [HttpGet]
        public JsonResult OnGetUnCheckScenario(string business_ids)
        {
            if (String.IsNullOrWhiteSpace(business_ids))
            {
                return new JsonResult("시나리오를 해제할 항목이 선택되지 않았습니다.");
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand($"update business_base set isScenario = 'N', scenario_date = null where business_id in ({business_ids.TrimEnd(',')})", conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return new JsonResult("정상적으로 처리되었습니다.");
        }

        [HttpGet]
        public JsonResult OnGetCheckAnalysis(string business_ids)
        {
            if (String.IsNullOrWhiteSpace(business_ids))
            {
                return new JsonResult("원인분석을 완료시킬 항목이 선택되지 않았습니다.");
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand($"update business_base set isAnalysis = 'Y', analysis_date = now() where business_id in ({business_ids.TrimEnd(',')})", conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return new JsonResult("정상적으로 처리되었습니다.");
        }

        [HttpGet]
        public JsonResult OnGetUnCheckAnalysis(string business_ids)
        {
            if (String.IsNullOrWhiteSpace(business_ids))
            {
                return new JsonResult("원인분석을 해제할 항목이 선택되지 않았습니다.");
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand($"update business_base set isAnalysis = 'N', analysis_date = null where business_id in ({business_ids.TrimEnd(',')})", conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return new JsonResult("정상적으로 처리되었습니다.");
        }

        [HttpGet]
        public JsonResult OnGetCheckPublish(string business_ids)
        {
            if (String.IsNullOrWhiteSpace(business_ids))
            {
                return new JsonResult("게시를 완료시킬 항목이 선택되지 않았습니다.");
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand($"update business_base set isPublish = 'N' where isPublish = 'Y';update business_base set isPublish = 'Y', publish_date = now() where business_id in ({business_ids.TrimEnd(',')});", conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return new JsonResult("정상적으로 처리되었습니다.");
        }

        [HttpGet]
        public JsonResult OnGetUnCheckPublish(string business_ids)
        {
            if (String.IsNullOrWhiteSpace(business_ids))
            {
                return new JsonResult("게시를 해제할 항목이 선택되지 않았습니다.");
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand($"update business_base set isPublish = 'N', publish_date = null where business_id in ({business_ids.TrimEnd(',')});", conn);
                    cmd.ExecuteNonQuery();
                }
            }

            return new JsonResult("정상적으로 처리되었습니다.");
        }
    }
}