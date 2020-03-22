using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace Daewoong.BI
{
    public class CauseAnalysisModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int businessID { get; set; } = 0;

        public BusinessBase businessBaseObj { get; set; }

        public List<BusinessFile> businessFiles { get; set; }

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

            if (this.businessID == 0)
            {
                return Redirect("/business/scenariolist/1");
            }

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    string bs_read_sql = $@"
                        select base.business_id, base.caption, base.dates, base.update_date
	                        , scenario.scenario_id, scenario.types, scenario.sorting as scenario_sorting, scenario.title as scenario_title
	                        , content.content_id, content.label as content_label, content.content_type, content.content_data, content.sorting as content_sorting
                            , analysis.analysis_id, analysis.txt
                        from business_base base 
                        inner join business_scenario scenario
                        on base.business_id = scenario.business_id
                        inner join business_content content 
                        on scenario.scenario_id = content.scenario_id
                        inner join business_analysis analysis
                        on content.content_id = analysis.content_id
                        where base.business_id = {businessID}
                        and scenario.types <> 5
                        order by scenario.sorting asc, content.sorting asc";

                    MySqlCommand cmd = new MySqlCommand(bs_read_sql, conn);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return Redirect("/business/scenariolist/1");
                    }

                    businessBaseObj = new BusinessBase();
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
                            subScenario.Title = dr["scenario_title"].ToString();
                            subScenario.Sorting = int.Parse(dr["scenario_sorting"].ToString());

                            businessBaseObj.BusinessScenarios.Add(subScenario);
                        }

                        BusinessScenario findScenario = businessBaseObj.BusinessScenarios.Single(x => x.ScenarioID == int.Parse(dr["scenario_id"].ToString()));

                        if (findScenario.BusinessContents == null || findScenario.BusinessContents.Count == 0)
                        {
                            findScenario.BusinessContents = new List<BusinessContent>();
                        }

                        BusinessAnalysis businessAnalysis = new BusinessAnalysis();
                        businessAnalysis.AnalysisID = int.Parse(dr["analysis_id"].ToString());
                        businessAnalysis.Txt = WebUtility.HtmlDecode(dr["txt"].ToString());

                        BusinessContent businessContent = new BusinessContent()
                        {
                            ContentID = int.Parse(dr["content_id"].ToString()),
                            Label = dr["content_label"].ToString(),
                            ContentType = dr["content_type"].ToString(),
                            ContentData = dr["content_data"].ToString(),
                            Sorting = int.Parse(dr["content_sorting"].ToString()),
                            BusinessAnalysis = businessAnalysis
                        };

                        findScenario.BusinessContents.Add(businessContent);
                    }

                    // 원인 분석 및 첨부 파일 조회
                    string ba_read_sql = $@"
                        select analysis.content_id, files.file_id, files.ref_id, files.file_url, files.file_name
                        from business_base base 
                        inner join business_scenario scenario
                        on base.business_id = scenario.business_id
                        inner join business_analysis analysis
                        on scenario.scenario_id = analysis.scenario_id
                        inner join business_file files
                        on files.table_header = 'analysis' and files.ref_id = analysis.analysis_id
                        where base.business_id = {businessID}";

                    MySqlCommand cmdAnalysis = new MySqlCommand(ba_read_sql, conn);
                    MySqlDataAdapter adapterAnalysis = new MySqlDataAdapter(cmdAnalysis);
                    DataTable dtAnalysis = new DataTable();
                    adapterAnalysis.Fill(dtAnalysis);

                    if (dtAnalysis != null && dtAnalysis.Rows.Count != 0)
                    {
                        businessFiles = new List<BusinessFile>();

                        foreach (DataRow drAnalysis in dtAnalysis.Rows)
                        {
                            businessFiles.Add(new BusinessFile()
                            {
                                FileID = int.Parse(drAnalysis["file_id"].ToString()),
                                RefID = int.Parse(drAnalysis["ref_id"].ToString()),
                                FileName = drAnalysis["file_name"].ToString(),
                                FileURL = drAnalysis["file_url"].ToString()
                            });
                        }
                    }
                }
            }

            return Page();
        }
    }
}