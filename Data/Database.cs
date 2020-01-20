using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using Data;

namespace Data
{
    public class Database
    {
        SortedList<int, Building> sortedListBuildings = new SortedList<int, Building>();
        SortedList<int, Visitor> sortedListVisitors = new SortedList<int, Visitor>();
        static Database database = null;
        static OracleConnection connection = null;
        readonly string ip = "192.168.128.152"; //"212.152.179.117" "192.168.128.152"
        private readonly string buildings_select= "SELECT v.building_id as bId, v.name as bName, v.visitors as visitors, t.X as xCoordinate, t.Y as yCoordinate, t.id as cId FROM village v, TABLE(SDO_UTIL.GETVERTICES(v.building)) t";
        private readonly string visitors_select= "select v.v_id id, v.v_name name, t.X x, t.Y y from visitors v, TABLE(SDO_UTIL.GETVERTICES(v.POSITION)) t";
        private readonly string visitor_insert = "INSERT INTO visitors VALUES(visitors_seq.nextval, :name, SDO_GEOMETRY(2001, NULL, SDO_POINT_TYPE(:x, :y, NULL), NULL, NULL))";
        private readonly string visitors_of_building_select= "Select v.v_id id, v.v_name name, t.X x, t.Y y from visitors v , table (SDO_UTIL.GETVERTICES(v.position))t WHERE v.v_id IN (SELECT v2.v_id FROM visitors v2 INNER JOIN village b ON SDO_CONTAINS(b.BUILDING, v2.POSITION) = 'TRUE', TABLE(SDO_UTIL.GETVERTICES(v2.POSITION)) t where building_id = :buildingId )";
        private readonly string buildings_of_visitors_select = "SELECT  b.name FROM visitors v INNER JOIN village b ON SDO_CONTAINS(b.BUILDING, v.POSITION) = 'TRUE', TABLE(SDO_UTIL.GETVERTICES(v.POSITION)) t where v.v_name = :name";

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
            OracleCommand command = new OracleCommand(buildings_select, connection);
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
            OracleCommand command = new OracleCommand(visitors_select, connection);
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

        public void AddVisitor(Visitor newVisitor)
        {
            OracleCommand cmd = new OracleCommand(visitor_insert, connection);
            cmd.Parameters.Add("name", newVisitor.Name);
            cmd.Parameters.Add("x", newVisitor.Position.X);
            cmd.Parameters.Add("y", newVisitor.Position.Y);
            cmd.ExecuteNonQuery();
        }

        public IList<Visitor> ReadVisitorOfBuilding(Building building)
        {
            OracleCommand cmd = new OracleCommand(visitors_of_building_select, connection);
            cmd.Parameters.Add("buildingId", building.ID);
            OracleDataReader reader = cmd.ExecuteReader();
            List<Visitor> collVisitors = new List<Visitor>();
            if (reader.HasRows)
                while (reader.Read())
                    collVisitors.Add(new Visitor(Convert.ToInt32(reader["id"]), reader["name"].ToString(), new Point(Convert.ToInt32(reader["x"]), Convert.ToInt32(reader["y"]))));
            return collVisitors;
        }

        public string ReadBuildingWhereVisitorOccurs(Visitor selectedItem)
        {
            string rgw = "";
            OracleCommand cmd = new OracleCommand(buildings_of_visitors_select, connection);
            cmd.Parameters.Add(new OracleParameter("name", selectedItem.Name));
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                    rgw += reader["name"].ToString();
            return rgw;
        }
    }
}