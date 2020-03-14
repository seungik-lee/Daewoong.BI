//using System;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Extensions;
////using MySql.Data.EntityFrameworkCore.Extensions;
//namespace Daewoong.BI.Entities
//{
//    public class ApplicationDbContext : IdentityDbContext
//    {
//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            //optionsBuilder.UseMySQL(GetConnectionString());
//        }
        
//        private static string GetConnectionString()
//        {
//            const string databaseName = "webapijwt";
//            const string databaseUser = "root";
//            const string databasePass = "1";
//        return "server=localhost;port=3306;database=webapijwt;uid=dwadmin;password=dwbi11!!;persistsecurityinfo=True;";

//        }
//    }
//}