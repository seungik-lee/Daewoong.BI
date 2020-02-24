using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Daewoong.BI.Models
{
    [DataContract]

    public class KPI
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public string DetailURL { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public string KPIBankID { get; set; }

        [DataMember]
        /// <summary>
        /// 회사코드
        /// </summary>
        public string CompanyCode { get; set; }

        [DataMember]
        public string CompanyName { get; set; }

        [DataMember]
        public int Seq { get; set; }

        /// <summary>
        /// 마감기준
        /// </summary>
        [DataMember]
        public string Close { get; set; }

        [DataMember]
        public string ChartType { get; set; }
    }
}
