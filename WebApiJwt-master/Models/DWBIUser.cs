using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Daewoong.BI.Models
{
    [DataContract]
    public class DWBIUser
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public int CompanyID { get; set; }

        [DataMember]
        public int CompanyCode { get; set; }

        [DataMember]
        public string CompanyName { get; set; }

        [DataMember]
        public object Token { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public bool IsAdmin { get; set; }

        [DataMember]
        public Role UserRole { get; set; }

        [DataMember]
        public string key { get; set; }

        [DataMember]
        public string RoleIDKey { get; set; }

        [DataMember]
        public List<Company> Companies { get; set; }

        [DataMember]
        public string RoleID { get; set; }

        [DataMember]
        public IQueryable<IdentityUser> Users { get; set; }
}

    public enum Role
    {
        Admin = 0,
        Manager = 1,
        Member = 2
    }

}
