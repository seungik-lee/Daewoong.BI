using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daewoong.BI.Models
{
	public class BusinessBase
	{
		public int BusinessID { get; set; }

		public DateTime Dates { get; set; }

		public string Caption { get; set; }

		public DateTime UpdateDate { get; set; }

		public string IsPublish { get; set; }

		public DateTime PublishDate { get; set; }

		public string Writer { get; set; }

		public string IsScenario { get; set; }

		public DateTime ScenarioDate { get; set; }

		public string IsAnalysis { get; set; }

		public DateTime AnalysisDate { get; set; }

		public List<BusinessScenario> BusinessScenarios { get; set; }
	}

	public class BusinessScenario
	{
		public int ScenarioID { get; set; }

		public int BusinessID { get; set; }

		public int Sorting { get; set; }

		public int Types { get; set; }

		public string TypesName { get; set; }

		public string Writer { get; set; }

		public DateTime UpdateDate { get; set; }

		public string Status { get; set; }

		public List<BusinessContent> BusinessContents { get; set; }

		public string GetTypesName(int type_no)
		{
			string typesName = "총평/지시사항";

			switch (type_no)
			{
				case 1:
					typesName = "매출현황";
					break;
				case 2:
					typesName = "손익현황";
					break;
				case 3:
					typesName = "재고현황";
					break;
				case 4:
					typesName = "생산현황";
					break;
			}

			return typesName;
		}
	}

	public class BusinessContent
	{
		public int ContentID { get; set; }

		public int ScenarioID { get; set; }

		public int FileID { get; set; }

		public string Label { get; set; }

		public int Sorting { get; set; }

		public string ContentType { get; set; }

		public string ContentData { get; set; }

		public string Writer { get; set; }

		public DateTime UpdateDate { get; set; }

		public string Status { get; set; }

		public BusinessFile BusinessFile { get; set; }

		public BusinessAnalysis BusinessAnalysis { get; set; }
	}

	public class BusinessFile
	{
		public int FileID { get; set; }

		public int RefID { get; set; }

		public string TableHeader { get; set; }

		public string FileName { get; set; }

		public long FileSize { get; set; }

		public string FileURL { get; set; }

		public string Writer { get; set; }

		public string Status { get; set; }

		public DateTime UpdateDate { get; set; }

		public string ResultMessage { get; set; }
	}

	public class BusinessAnalysis
	{
		public int AnalysisID { get; set; }

		public int ScenarioID { get; set; }

		public int ContentID { get; set; }

		public string Txt { get; set; }

		public string Writer { get; set; }

		public DateTime UpdateDate { get; set; }

		public List<BusinessFile> BusinessFiles { get; set; }
	}
}
