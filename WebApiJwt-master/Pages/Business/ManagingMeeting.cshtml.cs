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
    public class ManagingMeetingModel : PageModel
    {
        public BusinessBase businessBaseObj { get; set; }

        public List<BusinessBase> prevBusinessBaseObjs { get; set; }

        public int businessBaseObjCount { get; set; }

        public int prevBusinessBaseObjCount { get; set; }

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

            // 이전 게시된 정보 조회
            SetPublishedBusinessInfo();

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
                        where base.isPublish = 'Y'
                        order by scenario.sorting asc, content.sorting asc";

                    MySqlCommand cmd = new MySqlCommand(bs_read_sql, conn);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    businessBaseObj = new BusinessBase();

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        businessBaseObj.BusinessScenarios = new List<BusinessScenario>();

                        return Page();
                    }
                    else
                    {
                        businessBaseObjCount = 1;
                    }

                    businessBaseObj.BusinessID = int.Parse(dt.Rows[0]["business_id"].ToString());
                    businessBaseObj.Caption = dt.Rows[0]["caption"].ToString();
                    businessBaseObj.Dates = Convert.ToDateTime(dt.Rows[0]["dates"].ToString());
                    businessBaseObj.UpdateDate = Convert.ToDateTime(dt.Rows[0]["update_date"].ToString());
                    businessBaseObj.BusinessScenarios = new List<BusinessScenario>();

                    // 원인 분석 및 첨부 파일 조회
                    string bf_sql = $@"
                        select analysis.analysis_id, analysis.content_id, files.file_id, files.ref_id, files.file_url, files.file_name
                        from business_base base 
                        inner join business_scenario scenario
                        on base.business_id = scenario.business_id
                        inner join business_analysis analysis
                        on scenario.scenario_id = analysis.scenario_id
                        inner join business_file files
                        on files.table_header = 'analysis' and files.ref_id = analysis.analysis_id
                        where base.business_id = {businessBaseObj.BusinessID}";

                    MySqlCommand cmdAnalysis = new MySqlCommand(bf_sql, conn);
                    MySqlDataAdapter adapterAnalysis = new MySqlDataAdapter(cmdAnalysis);
                    DataTable dtAnalysisFiles = new DataTable();
                    adapterAnalysis.Fill(dtAnalysisFiles);

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

                        if (dtAnalysisFiles != null && dtAnalysisFiles.Rows.Count != 0)
                        {
                            DataRow[] drAnalysisFiles = dtAnalysisFiles.Select("ref_id=" + businessAnalysis.AnalysisID);

                            if (drAnalysisFiles != null && drAnalysisFiles.Length > 0)
                            {
                                businessAnalysis.BusinessFiles = new List<BusinessFile>();

                                foreach (DataRow drAnalysisFile in drAnalysisFiles)
                                {
                                    businessAnalysis.BusinessFiles.Add(new BusinessFile()
                                    {
                                        FileID = int.Parse(drAnalysisFile["file_id"].ToString()),
                                        RefID = int.Parse(drAnalysisFile["ref_id"].ToString()),
                                        FileName = drAnalysisFile["file_name"].ToString(),
                                        FileURL = drAnalysisFile["file_url"].ToString()
                                    });
                                }
                            }
                        }

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
                }
            }

            return Page();
        }

        /// <summary>
        /// 이전 게시된 정보 조회
        /// </summary>
        private void SetPublishedBusinessInfo()
        {
            prevBusinessBaseObjs = new List<BusinessBase>();

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
                        where base.isPublish = 'N' and base.publish_date is not null
                        order by base.publish_date desc, scenario.sorting asc, content.sorting asc";

                    MySqlCommand cmd = new MySqlCommand(bs_read_sql, conn);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return;
                    }

                    string analysis_ids = "";

                    foreach (DataRow dr in dt.Rows)
                    {
                        analysis_ids += dr["analysis_id"].ToString() + ",";
                    }

                    // 원인 분석 및 첨부 파일 조회
                    string bf_sql = $@"
                        select files.file_id, files.ref_id, files.file_url, files.file_name
                        from business_file files
                        where files.table_header = 'analysis' and files.ref_id in ({analysis_ids.TrimEnd(',')})";

                    MySqlCommand cmdAnalysis = new MySqlCommand(bf_sql, conn);
                    MySqlDataAdapter adapterAnalysis = new MySqlDataAdapter(cmdAnalysis);
                    DataTable dtAnalysisFiles = new DataTable();
                    adapterAnalysis.Fill(dtAnalysisFiles);

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (prevBusinessBaseObjs.Where(x => x.BusinessID == int.Parse(dr["business_id"].ToString())).Count() == 0)
                        {
                            BusinessBase businessBase = new BusinessBase();
                            businessBase.BusinessID = int.Parse(dr["business_id"].ToString());
                            businessBase.Caption = dr["caption"].ToString();
                            businessBase.Dates = Convert.ToDateTime(dr["dates"].ToString());
                            businessBase.UpdateDate = Convert.ToDateTime(dr["update_date"].ToString());
                            businessBase.BusinessScenarios = new List<BusinessScenario>();

                            prevBusinessBaseObjs.Add(businessBase);
                        }   
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        BusinessBase businessBase = prevBusinessBaseObjs.First(x => x.BusinessID == int.Parse(dr["business_id"].ToString()));

                        if (businessBase.BusinessScenarios.Where(x => x.ScenarioID == int.Parse(dr["scenario_id"].ToString())).Count() == 0)
                        {
                            BusinessScenario subScenario = new BusinessScenario();
                            subScenario.ScenarioID = int.Parse(dr["scenario_id"].ToString());
                            subScenario.Types = int.Parse(dr["types"].ToString());
                            subScenario.Title = dr["scenario_title"].ToString();
                            subScenario.Sorting = int.Parse(dr["scenario_sorting"].ToString());

                            businessBase.BusinessScenarios.Add(subScenario);
                        }

                        BusinessScenario findScenario = businessBase.BusinessScenarios.Single(x => x.ScenarioID == int.Parse(dr["scenario_id"].ToString()));

                        if (findScenario.BusinessContents == null || findScenario.BusinessContents.Count == 0)
                        {
                            findScenario.BusinessContents = new List<BusinessContent>();
                        }

                        BusinessAnalysis businessAnalysis = new BusinessAnalysis();
                        businessAnalysis.AnalysisID = int.Parse(dr["analysis_id"].ToString());
                        businessAnalysis.Txt = WebUtility.HtmlDecode(dr["txt"].ToString());

                        if (dtAnalysisFiles != null && dtAnalysisFiles.Rows.Count != 0)
                        {
                            DataRow[] drAnalysisFiles = dtAnalysisFiles.Select("ref_id=" + businessAnalysis.AnalysisID);

                            if (drAnalysisFiles != null && drAnalysisFiles.Length > 0)
                            {
                                businessAnalysis.BusinessFiles = new List<BusinessFile>();

                                foreach (DataRow drAnalysisFile in drAnalysisFiles)
                                {
                                    businessAnalysis.BusinessFiles.Add(new BusinessFile()
                                    {
                                        FileID = int.Parse(drAnalysisFile["file_id"].ToString()),
                                        RefID = int.Parse(drAnalysisFile["ref_id"].ToString()),
                                        FileName = drAnalysisFile["file_name"].ToString(),
                                        FileURL = drAnalysisFile["file_url"].ToString()
                                    });
                                }
                            }
                        }

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

                    prevBusinessBaseObjCount = prevBusinessBaseObjs.Count;
                }
            }
        }
    }
}