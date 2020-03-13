using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Daewoong.BI.Models
{
    [DataContract]
    public class Company
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string CompanyName { get; set; }

        [DataMember]
        public string Logo { get; set; }

        /// <summary>
        /// 회사코드
        /// </summary>
        [DataMember]
        public int Code { get; set; }
    }
}
