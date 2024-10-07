using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace InventorySystem.MVVM
{
    internal class DatabaseConnection
    {
        private MySqlConnection connection;
        public DatabaseConnection()
        {
            string connectionString = "server=127.0.0.1;uid=root;" +
            "pwd=-;database=invdb";
            connection = new MySqlConnection(connectionString);
        }

        public MySqlConnection GetConnection()
        {
            return connection;
        }
    }
}
