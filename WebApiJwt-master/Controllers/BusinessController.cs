using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        [HttpPost]
        [Route("CreateBusinessScenario")]
        public string CreateBusinessScenario([FromBody] BusinessBase businessBaseObject)
        {
            if (businessBaseObject == null)
            {
                Response.StatusCode = 500;
                return "시스템 오류가 발생하였습니다. 잠시 후에 다시 시도해 주세요.";
            }

            string writer_id = "yangnest@daewoong-bio.co.kr";

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    try
                    {
                        conn.Open();

                        string bb_sql = $@" insert into business_base (dates, caption, ispublish, update_date, writer, isScenario, isAnalysis) 
                                            values('{businessBaseObject.Dates.ToString("yyyy-MM-dd")}', '{businessBaseObject.Caption}', 'N', now(), '{writer_id}', 'N', 'N');";

                        if (businessBaseObject.BusinessID > 0)
                        {
                            bb_sql = $@"update business_base 
                                        set dates = '{businessBaseObject.Dates.ToString("yyyy-MM-dd")}', caption = '{businessBaseObject.Caption}', update_date = now() 
                                        where business_id = {businessBaseObject.BusinessID}";
                        }

                        MySqlCommand cmd = new MySqlCommand(bb_sql, conn);
                        cmd.ExecuteNonQuery();

                        if (businessBaseObject.BusinessID == 0)
                        {
                            string bb_read_sql = "select LAST_INSERT_ID() as business_base_id";

                            cmd = new MySqlCommand(bb_read_sql, conn);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    businessBaseObject.BusinessID = Convert.ToInt32(reader["business_base_id"]);
                                }
                            }
                        }

                        // 시나리오 정보 저장
                        foreach (BusinessScenario scenario in businessBaseObject.BusinessScenarios)
                        {
                            string bs_sql = $@" insert into business_scenario (business_id, sorting, types, writer, update_date) 
                                                values ({businessBaseObject.BusinessID}, {scenario.Sorting}, {scenario.Types}, '{writer_id}', now())";

                            // 기존에 등록된 시나리오
                            if (scenario.ScenarioID > 0)
                            {
                                // 삭제된 시나리오 처리
                                if (scenario.Status == "deleted")
                                {
                                    bs_sql = $@"delete from business_content 
                                                where scenario_id = {scenario.ScenarioID}; 
                                                delete from business_scenario 
                                                where scenario_id = {scenario.ScenarioID}";
                                }
                                else
                                {
                                    bs_sql = $@"update business_scenario
                                                set sorting = {scenario.Sorting}, update_date = now() 
                                                where scenario_id = {scenario.ScenarioID}";
                                }
                            }

                            cmd = new MySqlCommand(bs_sql, conn);
                            cmd.ExecuteNonQuery();

                            if (scenario.ScenarioID == 0)
                            {
                                string bs_read_sql = "select LAST_INSERT_ID() as business_scenario_id";

                                cmd = new MySqlCommand(bs_read_sql, conn);

                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        scenario.ScenarioID = Convert.ToInt32(reader["business_scenario_id"]);
                                    }
                                }
                            }

                            if (scenario.BusinessContents != null && scenario.BusinessContents.Count > 0)
                            {
                                // 컨텐츠 정보 저장
                                foreach (BusinessContent content in scenario.BusinessContents)
                                {
                                    string bc_sql = $@" insert into business_content (scenario_id, label, sorting, content_type, content_data, writer, update_date) 
                                                        values ({scenario.ScenarioID}, '{content.Label}', {content.Sorting}, '{content.ContentType}', '{WebUtility.HtmlDecode(content.ContentData)}', '{writer_id}', now())";

                                    // 기존에 등록된 컨텐츠
                                    if (content.ContentID > 0)
                                    {
                                        // 삭제된 컨텐츠 처리
                                        if (content.Status == "deleted")
                                        {
                                            bc_sql = $@"delete from business_analysis 
                                                        where content_id = {content.ContentID};
                                                        delete from business_content 
                                                        where content_id = {content.ContentID};";
                                        }
                                        else
                                        {
                                            bc_sql = $@"update business_content
                                                        set sorting = {content.Sorting}, update_date = now() 
                                                        where content_id = {content.ContentID};";
                                        }
                                    }

                                    cmd = new MySqlCommand(bc_sql, conn);
                                    cmd.ExecuteNonQuery();

                                    if (content.ContentID == 0)
                                    {
                                        string bc_read_sql = "select LAST_INSERT_ID() as business_content_id";

                                        cmd = new MySqlCommand(bc_read_sql, conn);

                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                content.ContentID = Convert.ToInt32(reader["business_content_id"]);
                                            }
                                        }

                                        // 컨텐츠별 원인분석 항목 추가
                                        string ba_ins_sql = $@" insert into business_analysis (scenario_id, content_id, txt, writer, update_date) 
                                                            values({scenario.ScenarioID}, {content.ContentID}, '', '{writer_id}', now());";

                                        cmd = new MySqlCommand(ba_ins_sql, conn);
                                        cmd.ExecuteNonQuery();

                                        if (content.BusinessFile != null)
                                        {
                                            // 파일 정보 저장
                                            string bf_ins_sql = $@" insert into business_file (ref_id, table_header, file_name, file_size, file_url, writer, update_date) 
                                                                values ({content.ContentID}, 'content', '{content.BusinessFile.FileName}', {content.BusinessFile.FileSize}, '{content.BusinessFile.FileURL}', '{writer_id}', now())";

                                            cmd = new MySqlCommand(bf_ins_sql, conn);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.StatusCode = 500;
                        return ex.ToString();
                    }
                }
            }

            Response.StatusCode = 200;
            return "정상적으로 처리되었습니다.";
        }

        [HttpPost]
        [Route("SetBusinessAnalysis")]
        public string SetBusinessAnalysis([FromBody] List<BusinessAnalysis> businessAnalysises)
        {
            if (businessAnalysises == null || businessAnalysises.Count == 0)
            {
                Response.StatusCode = 500;
                return "시스템 오류가 발생하였습니다. 잠시 후에 다시 시도해 주세요.";
            }

            string writer_id = "yangnest@daewoong-bio.co.kr";

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    try
                    {
                        conn.Open();

                        foreach (BusinessAnalysis businessAnalysis in businessAnalysises)
                        {
                            string ba_upd_sql = $@"update business_analysis 
                                    set txt = '{businessAnalysis.Txt}'
                                        , update_date = now()
                                    where analysis_id = {businessAnalysis.AnalysisID};";

                            MySqlCommand cmd = new MySqlCommand(ba_upd_sql, conn);
                            cmd.ExecuteNonQuery();

                            if (businessAnalysis.BusinessFiles != null && businessAnalysis.BusinessFiles.Count > 0)
                            {
                                foreach (BusinessFile businessFile in businessAnalysis.BusinessFiles)
                                {
                                    string bf_sql = "";

                                    if (businessFile.Status == "deleted")
                                    {
                                        // 업로드된 파일정보 삭제
                                        bf_sql = $@"delete from business_file where file_id = {businessFile.FileID}";
                                    }
                                    else
                                    {
                                        // 파일 정보 저장
                                        bf_sql = $@"insert into business_file (ref_id, table_header, file_name, file_size, file_url, writer, update_date) values ({businessAnalysis.AnalysisID}, 'analysis', '{businessFile.FileName}', {businessFile.FileSize}, '{businessFile.FileURL}', '{writer_id}', now())";
                                    }

                                    cmd = new MySqlCommand(bf_sql, conn);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //tran.Rollback();

                        Response.StatusCode = 500;
                        return ex.ToString();
                    }
                }
            }

            Response.StatusCode = 200;
            return "정상적으로 처리되었습니다.";
        }
    }
}