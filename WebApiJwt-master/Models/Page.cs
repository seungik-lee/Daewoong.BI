using Daewoong.BI.Datas;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Daewoong.BI.Models
{
    [DataContract]
    public class Page
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public int Seq { get; set; }

        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Layout { get; set; }

        [DataMember]
        public List<KPI> KPIs { get; set; }

        [DataMember]
        public string CompanyCode { get; set; }

    }
}
