using System;
using System.IO;

namespace ServieNowTableGeneration
{
    class Program
    {

        private static string _servername = "servicenowsqldev";
        private static string _database = "ServiceNow";
        private static string _query = "SELECT [Table], [Parameters] FROM Common.DataSets";
        static void Main(string[] args)
        {
            DBHandler db = new DBHandler();
            var data = db.ExecuteQuery(_servername, _database, _query);
            string result = db.ContructQuery(data);
            Console.WriteLine(result);

            string path = $@"{AppContext.BaseDirectory}CreateTables.sql";
            Console.WriteLine($"File written to {path}");
            File.WriteAllText(path, result);

            Console.ReadKey();
        }
    }
}
