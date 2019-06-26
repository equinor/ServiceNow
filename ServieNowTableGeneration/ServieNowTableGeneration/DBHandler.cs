using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Dynamic;
using System.ComponentModel;
using System.Data.Common;
using Microsoft.Azure.Services.AppAuthentication;
using System.Text.RegularExpressions;

namespace ServieNowTableGeneration
{
    public class DBHandler
    {

        public DataTable ExecuteQuery(string servername, string database, string query)
        {
            string token = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
            string connectionString = $"Server={servername}.database.windows.net;Initial Catalog={database};";
            DataTable table;
           
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.AccessToken = token;
                Console.WriteLine($"Connecting to database");
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                Console.WriteLine("Reading table");
                SqlDataReader reader = command.ExecuteReader();
                table = CreateDataTable(reader);
            }
            return table;
        }

        private DataTable CreateDataTable(SqlDataReader reader)
        {
            var columns = reader.GetColumnSchema().ToList();
            DataTable table = new DataTable();
            foreach(var column in columns)
            {
                DataColumn dataColumn = new DataColumn()
                {
                    DataType = column.DataType,
                    ColumnName = column.ColumnName
                };
                table.Columns.Add(dataColumn);
            }
            table = PopulateDataTable(table, reader);
            return table;
        }

        private DataTable PopulateDataTable(DataTable table, SqlDataReader reader)
        {
            while (reader.Read())
            {
                DataRow row = table.NewRow();
                var columns = table.Columns;
                foreach (var column in columns)
                {
                    string columnName = column.ToString();
                    var columnValue = reader[columnName];
                    row[columnName] = columnValue;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        public string ContructQuery(DataTable table)
        {
            string result = "";
            var reader = table.CreateDataReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string tablename = reader.GetString(0);
                    var parameters = reader.GetString(1).Split(',').ToList();
                    List<string> tables = new List<string>()
                    {
                        $"[SN_Staging].[{tablename}]",
                         $"[SN].[{tablename}]"
                    };
                    foreach(string item in tables)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine($"\nCREATE TABLE {item}");
                        builder.AppendLine("(");
                        foreach (string parameter in parameters)
                            builder.AppendLine($"\t[{parameter.TrimEnd()}] {GetDataType(parameter)},");
                           
                        string query = builder.ToString();
                        query = RemoveLast(query, ",");
                        query += "\n);\nGO\n";
                        result += query;

                    }
                }
            }
            return result;

        }
        private string RemoveLast(string text, string character)
        {
            if (text.Length < 1) return text;
            return text.Remove(text.ToString().LastIndexOf(character), character.Length);
        }

        private string GetDataType(string column)
        {
            if (column.EndsWith("sys_id"))
                return "CHAR(32)";
            else if (column.Equals("sys_created_on"))
                return "DATETIME";
            else if (column.Equals("sys_updated_on"))
                return "DATETIME";
            else if (column.Equals("due_date"))
                return "DATETIME";
            else if (column.Contains("_has_"))
                return "BIT";
            else
                return "NVARCHAR(4000)";
        }



    }


}
