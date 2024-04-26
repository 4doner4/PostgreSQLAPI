using Npgsql;
using Microsoft.Extensions.Configuration;

namespace PostgreSqlAPI.Models
{
    public class SearchPostgreSQL
    {
        private string _connection;
        private readonly IConfiguration _configuration;
        public SearchPostgreSQL(IConfiguration config)
        {
            _configuration = config;
            string DB = "DBProd";
            string dbHost = _configuration.GetValue($"{DB}:host", "");
            string dbPort = _configuration.GetValue("DB:port", "");
            string dbName = _configuration.GetValue("DB:name", "");
            string dbUsername = _configuration.GetValue("DB:username", "");
            string dbPassword = _configuration.GetValue("DB:password", "");
            _connection = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;", dbHost, dbPort, dbUsername, dbPassword, dbName);
        }

        public async Task<Dictionary<string, string>[]> GetRows(string cmdText, Dictionary<string, string> dictionary)
        {
            NpgsqlConnection sqlConnection = new(_connection);

            await sqlConnection.OpenAsync();

            NpgsqlCommand cmd = new(cmdText, sqlConnection);

            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

            Dictionary<string, string>[] json = ConvertDataReaderToDataSet(reader, dictionary);

            await sqlConnection.CloseAsync();

            return json;
        }

        private static Dictionary<string, string>[] ConvertDataReaderToDataSet(NpgsqlDataReader reader, Dictionary<string, string> dictionary)
        {
            List<Dictionary<string, string>> rows = new();

            var array = dictionary.Keys;

            while (reader.Read())
            {
                int i = 0;

                Dictionary<string, string> currentDictionary = new();

                foreach (var item in array)
                {
                    currentDictionary.Add(item, reader.GetValue(i).ToString());
                    i++;
                }

                rows.Add(currentDictionary);
            }

            return rows.ToArray();
        }
    }
}
