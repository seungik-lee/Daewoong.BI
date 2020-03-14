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
    public class PreviousGeneralCommentModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int scenarioID { get; set; } = 0;

        public string PageStatus { get; set; } = "";

        public BusinessAnalysis analysisObj { get; set; }

        public string businessCaption { get; set; }

        public string analysisUpdateDate { get; set; }

        public List<BusinessFile> businessFiles { get; set; }

        public IActionResult OnGet()
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();

                    string ba_read_sql = $@"
                        select analysis.analysis_id, analysis.txt, analysis.update_date, base.caption
                        from business_scenario scenario 
                        inner join business_analysis analysis
                        on scenario.scenario_id = analysis.scenario_id
                        inner join business_base base
                        on base.business_id = scenario.business_id
                        where scenario.scenario_id = {scenarioID}";

                    MySqlCommand cmd = new MySqlCommand(ba_read_sql, conn);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    analysisObj = new BusinessAnalysis();
                    analysisObj.AnalysisID = int.Parse(dt.Rows[0]["analysis_id"].ToString());
                    analysisObj.Txt = WebUtility.HtmlDecode(dt.Rows[0]["txt"].ToString());
                    analysisObj.UpdateDate = Convert.ToDateTime(dt.Rows[0]["update_date"].ToString());
                    analysisUpdateDate = analysisObj.UpdateDate.ToString("yyyy-MM-dd HH:mm");

                    businessCaption = dt.Rows[0]["caption"].ToString();

                    // 첨부 파일 조회
                    string bf_read_sql = $@"
                        select files.file_id, files.ref_id, files.file_url, files.file_name
                        from business_file files
                        where files.table_header = 'analysis' and files.ref_id = {analysisObj.AnalysisID}";

                    MySqlCommand cmdFile = new MySqlCommand(bf_read_sql, conn);
                    MySqlDataAdapter adapterFile = new MySqlDataAdapter(cmdFile);
                    DataTable dtFiles = new DataTable();
                    adapterFile.Fill(dtFiles);

                    if (dtFiles != null && dtFiles.Rows.Count != 0)
                    {
                        businessFiles = new List<BusinessFile>();

                        foreach (DataRow drAnalysis in dtFiles.Rows)
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