using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Daewoong.BI.Models
{
    [DataContract]
    public class Menu
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public int CompanyCode { get; set; }

        [DataMember]
        public string Close { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public string Level { get; set; }

        [DataMember]
        public string ParentID{ get; set; }
    }
}
