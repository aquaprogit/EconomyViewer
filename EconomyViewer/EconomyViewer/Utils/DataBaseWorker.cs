using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace EconomyViewer.Utils
{
    internal static class DataBaseWorker
    {
        private static readonly string connectionString = @"Data Source="
            + Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
            + "\\economy.db; Version=3;";
        private static SQLiteConnection connection;

        public delegate void DataChangedHandler();
        public static event DataChangedHandler DataChanged;
        public static void DeleteData(string serverName, Item toDelete)
        {
            ExecuteCommand($"DELETE FROM {serverName} " +
                $"WHERE i_id={toDelete.ID}");
            DataChanged?.Invoke();
        }

        public static void CreateDataBase()
        {
            SQLiteConnection.CreateFile(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\economy.db");
            List<string> servers = new List<string>() { "Classic", "Fantasy", "Galaxy", "HiTech", "Industrial", "MagicRPG", "Pixelmon", "SkyFactory", "TechnoMagic" };
            foreach (var server in servers)
            {
                string query = $"CREATE TABLE {server} (\"i_id\" INTEGER NOT NULL DEFAULT 0 UNIQUE,\"i_header\"  TEXT NOT NULL,\"i_count\"   INTEGER NOT NULL DEFAULT 1,\"i_price\"   INTEGER NOT NULL DEFAULT 1,\"i_mod\" TEXT NOT NULL,PRIMARY KEY(\"i_id\"))";
                ExecuteCommand(query);
            }
        }
        public static void DeleteAllData(string serverName)
        {
            ExecuteCommand($"DELETE FROM {serverName}");
        }
        public static List<string> GetAllTables()
        {
            DataTable userTables = null;
            using (connection = new SQLiteConnection(connectionString))
            {
                string[] restrictions = new string[4];
                restrictions[3] = "Table";
                connection.Open();
                userTables = connection.GetSchema("Tables", restrictions);
            }
            List<string> tableNames = new List<string>();
            for (int i = 0; i < userTables.Rows.Count; i++)
                tableNames.Add(userTables.Rows[i][2].ToString());
            return tableNames;
        }
        public static List<Item> GetData(string serverName)
        {
            using (connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                List<Item> result = new List<Item>();

                string query = $"SELECT * FROM {serverName} ORDER BY i_mod, i_header";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Item item = new Item(reader.GetInt32(0),
                        reader.GetString(1),
                        Convert.ToUInt32(reader.GetInt32(2)),
                        Convert.ToUInt32(reader.GetInt32(3)),
                        reader.GetString(4));
                    result.Add(item);
                }
                return result;
            }
        }
        public static List<string> GetOnlyColumnList(string serverName, string collumnName)
        {
            using (connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                List<string> result = new List<string>();

                string query = $"SELECT DISTINCT {collumnName} FROM {serverName}";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
                return result;
            }
        }
        public static void InsertData(string serverName, Item newItem)
        {
            ExecuteCommand($"INSERT INTO {serverName} (i_header, i_count, i_price, i_mod)" +
                $"VALUES ('{newItem.Header}', {newItem.Count}, {newItem.Price}, '{newItem.Mod}')");
            DataChanged?.Invoke();
        }
        public static void UpdateData(string serverName, Item updatedItem, int id)
        {
            ExecuteCommand($"UPDATE {serverName} " +
                $"SET i_header='{updatedItem.Header}', i_count={updatedItem.Count}, i_price={updatedItem.Price}, i_mod='{updatedItem.Mod}' " +
                $"WHERE i_id={id}");
            DataChanged?.Invoke();
        }
        public static void ExecuteCommand(string query)
        {
            using (connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
