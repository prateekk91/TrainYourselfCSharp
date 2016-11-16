using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectMvvm
{
    public class DBManager
    {
        DBConnection dbConnection = DBConnection.Instance();
        public bool initializeDatabase()
        {
            dbConnection.DatabaseName = "kinect";
            dbConnection.Password = "password";
            return dbConnection.IsConnect();
        }

        public string getData()
        {
            if (dbConnection.Open())
            {
                //suppose col0 and col1 are defined as VARCHAR in the DB
                string query = "SELECT value FROM kinectfeedback";
                var cmd = new MySqlCommand(query, dbConnection.Connection);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string feedback = reader.GetString(0);
                    Console.WriteLine(feedback);
                    dbConnection.Close();
                    return feedback;
                }
            }
            return "";
        }

        public void setData(string data)
        {
            if (dbConnection.Open())
            {
                string query = string.Format("Update kinectfeedback set value=@value");
                var cmd = new MySqlCommand(query, dbConnection.Connection);
                cmd.Parameters.AddWithValue("@value", data);
                cmd.ExecuteNonQuery();
                dbConnection.Close();
            }
        }
    }
}
