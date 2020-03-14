using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daewoong.BI.Datas
{
    public class DWContext : IDisposable
    {
        public string ConnectionString = "server=localhost;port=3306;database=daewoongbi;uid=dwadmin;password=dwbi11!!;allow user variables=true";
        //public string ConnectionString = "server=175.207.12.61;port=3306;database=daewoongbi;uid=dwadmin;password=dwbi11!!;allow user variables=true";
        //public string ConnectionString = "server=10.1.20.120;port=3306;database=daewoongbi;uid=dwadmin;password=dwbi11!!;allow user variables=true";        

        public MySqlConnection Connection;

        public DWContext()
        {
            Connection = new MySqlConnection(ConnectionString);
        }

        public DWContext(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
