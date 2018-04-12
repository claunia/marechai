using MySql.Data.MySqlClient;

namespace cicm_web.Database
{
    public class Mysql
    {
        MySqlConnection connection;

        public Mysql(string server, string user, string database, ushort port, string password)
        {
            string connectionString =
                $"server={server};user={user};database={database};port={port};password={password}";
            
            connection = new MySqlConnection(connectionString);
        }

        ~Mysql()
        {
            connection?.Close();
        }
    }
}