using MySql.Data;
using MySql.Data.MySqlClient;
using System;

namespace KinectMvvm
{
    public class DBConnection
    {
        private DBConnection()
        {
        }

        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }

        public string Password { get; set; }
        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            bool result = true;
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(databaseName))
                    result = false;
                string connstring = string.Format("Server=localhost; database={0}; UID=username; password=password", databaseName);
                connection = new MySqlConnection(connstring);
                result = true;
            }

            return result;
        }

        public bool Open()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
                return true;
            }
            else
                return false;
        }

        public void Close()
        {
            if (connection.State != System.Data.ConnectionState.Closed)
                connection.Close();
        }
    }
}