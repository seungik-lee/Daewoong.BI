using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace Daewoong.BI
{
    public class ScenarioModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int businessID { get; set; } = 0;

        public List<string> DisplayYearList { get; set; }

        public BusinessBase businessBaseObj { get; set; }

        public DWBIUser DWUserInfo
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

            DisplayYearList = new List<string>();

            int nowYear = DateTime.Now.Year;
            int startYear = 2015;

            while (startYear <= nowYear)
            {
                DisplayYearList.Add($"{startYear.ToString()}년");

                startYear++;
            }

            if (this.businessID == 0)
            {
                return Page();
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    string bs_read_sql = $@"
                    select base.business_id, base.caption, base.dates, base.update_date
                        , scenario.scenario_id, scenario.types, scenario.sorting as scenario_sorting
                        , content.content_id, content.label as content_label, content.content_type, content.content_data, content.sorting as content_sorting
                        from business_base base 
                        inner join business_scenario scenario
                        on base.business_id = scenario.business_id
                        inner join business_content content 
                        on scenario.scenario_id = content.scenario_id
                        where base.business_id = {businessID}
                        and scenario.types <> 5
                        order by scenario.sorting asc, content.sorting asc";

                    MySqlCommand cmd = new MySqlCommand(bs_read_sql, conn);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        businessBaseObj = new BusinessBase();
                        businessBaseObj.BusinessID = int.Parse(dt.Rows[0]["business_id"].ToString());
                        businessBaseObj.Caption = dt.Rows[0]["caption"].ToString();
                        businessBaseObj.Dates = Convert.ToDateTime(dt.Rows[0]["dates"].ToString());
                        businessBaseObj.UpdateDate = Convert.ToDateTime(dt.Rows[0]["update_date"].ToString());
                        businessBaseObj.BusinessScenarios = new List<BusinessScenario>();

                        foreach (DataRow dr in dt.Rows)
                        {
                            if (businessBaseObj.BusinessScenarios.Where(x => x.ScenarioID == int.Parse(dr["scenario_id"].ToString())).Count() == 0)
                            {
                                BusinessScenario subScenario = new BusinessScenario();
                                subScenario.ScenarioID = int.Parse(dr["scenario_id"].ToString());
                                subScenario.Types = int.Parse(dr["types"].ToString());
                                subScenario.TypesName = subScenario.GetTypesName(subScenario.Types);
                                subScenario.Sorting = int.Parse(dr["scenario_sorting"].ToString());

                                businessBaseObj.BusinessScenarios.Add(subScenario);
                            }

                            BusinessScenario findScenario = businessBaseObj.BusinessScenarios.Single(x => x.ScenarioID == int.Parse(dr["scenario_id"].ToString()));

                            if (findScenario.BusinessContents == null || findScenario.BusinessContents.Count == 0)
                            {
                                findScenario.BusinessContents = new List<BusinessContent>();
                            }

                            findScenario.BusinessContents.Add(new BusinessContent()
                            {
                                ContentID = int.Parse(dr["content_id"].ToString()),
                                Label = dr["content_label"].ToString(),
                                ContentType = dr["content_type"].ToString(),
                                ContentData = dr["content_data"].ToString(),
                                Sorting = int.Parse(dr["content_sorting"].ToString())
                            });
                        }
                    }
                }
            }

            return Page();
        }
    }
}