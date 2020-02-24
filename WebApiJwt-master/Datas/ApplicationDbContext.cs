using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//using MySql.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Daewoong.BI.Controllers.AccountController;

namespace Daewoong.BI.Datas
{
    public class ApplicationDbContext : IdentityDbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           optionsBuilder.UseMySql(GetConnectionString());
        }

        private static string GetConnectionString()
        {
            //return "server=localhost;port=3306;database=webapijwt;uid=dwadmin;password=dwbi11!!;allow user variables=true;";
            return "server=175.207.12.61;port=3306;database=webapijwt;uid=dwadmin;password=dwbi11!!;allow user variables=true;";
            //return "server=10.1.20.120;port=3306;database=webapijwt;uid=dwadmin;password=dwbi11!!;allow user variables=true;";
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Iterate over every DbSet<> found in the current DbContext
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Iterate over each property found on the Entity class
                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    if (property.PropertyInfo == null)
                    {
                        continue;
                    }

                    if (property.IsPrimaryKey() && IsPrimaryKey(property.PropertyInfo))
                    {
                        // At this point we know that the property is a primary key
                        // let's set it to AutoIncrement on insert.
                        modelBuilder.Entity(entityType.ClrType)
                                    .Property(property.Name)
                                    .ValueGeneratedOnAdd()
                                    .Metadata.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                    }
                    else if (property.PropertyInfo.PropertyType.IsBoolean())
                    {
                        // Since MySQL stores bool as tinyint, let's add a converter so the tinyint is treated as boolean
                        modelBuilder.Entity(entityType.ClrType)
                                    .Property(property.Name)
                                    .HasConversion(new BoolToZeroOneConverter<short>());
                    }
                }

            };
        }

        private static bool IsPrimaryKey(PropertyInfo property)
        {
            var identityTypes = new List<Type> {
            typeof(short),
            typeof(int),
            typeof(long)
        };

            return property.Name.Equals("Id", StringComparison.CurrentCultureIgnoreCase) && identityTypes.Contains(property.PropertyType);
        }
    }

    public static class TypeExtensions
    {
        public static bool IsBoolean(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return t == typeof(bool);
        }

        public static bool IsTrueEnum(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return t.IsEnum;
        }
    }
}
