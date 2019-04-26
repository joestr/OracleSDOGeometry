using System;
using System.Collections.Generic;
using System.Drawing;
using Oracle.ManagedDataAccess.Client;

namespace Data
{
    public class Database
    {
        SortedList<int, Building> sortedListBuildings = new SortedList<int, Building>();
        SortedList<int, Visitor> sortedListVisitors = new SortedList<int, Visitor>();
        static Database database = null;
        static OracleConnection connection = null;
        readonly string ip = "192.168.128.152"; //"212.152.179.117" "192.168.128.152"

        private Database()
        {
            connection = new OracleConnection(@"user id=d4b23;password=d4b;data source=" +
                                                     "(description=(address=(protocol=tcp)" +
                                                     "(host=" + ip + ")(port=1521))(connect_data=" +
                                                     "(service_name=ora11g)))");
            connection.Open();
        }

        public static Database GetInstance()
        {
            if (database == null) database = new Database();

            return database;
        }

        private void ReadBuildingsFromDatabase()
        {
            string sqlString = "SELECT v.building_id as bId, v.name as bName, v.visitors as visitors, t.X as xCoordinate, t.Y as yCoordinate, t.id as cId FROM village v, TABLE(SDO_UTIL.GETVERTICES(v.building)) t";
            OracleCommand command = new OracleCommand(sqlString, connection);
            OracleDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (!sortedListBuildings.ContainsKey(Convert.ToInt32(reader["bId"].ToString())))
                    {
                        Building building = new Building(Convert.ToInt32(reader["bId"].ToString()), reader["bName"].ToString(), Convert.ToInt32(reader["visitors"].ToString()));
                        sortedListBuildings.Add(building.ID, building);
                    }
                    sortedListBuildings[Convert.ToInt32(reader["bId"].ToString())].AddPoint(new Point(Convert.ToInt32(reader["xCoordinate"].ToString()), Convert.ToInt32(reader["yCoordinate"].ToString())));
                }
            }
        }

        private void ReadVisitorsFromDatabase()
        {
            string sqlString = "select v.v_id id, v.v_name name, t.X x, t.Y y from visitors v, TABLE(SDO_UTIL.GETVERTICES(v.POSITION)) t";
            OracleCommand command = new OracleCommand(sqlString, connection);
            OracleDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    sortedListVisitors.Add(Convert.ToInt32(reader["id"]), new Visitor(Convert.ToInt32(reader["id"]), reader["name"].ToString(), new Point(Convert.ToInt32(reader["x"]), Convert.ToInt32(reader["y"]))));                    
                }
            }
        }

        public IList<Visitor> GetVisitors()
        {
            ReadVisitorsFromDatabase();
            return sortedListVisitors.Values;
        }

        public IList<Building> GetBuildings()
        {
            ReadBuildingsFromDatabase();
            return sortedListBuildings.Values;
        }
    }
}
