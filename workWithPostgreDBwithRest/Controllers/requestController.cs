
using Microsoft.AspNetCore.Mvc;
using PostgreSqlAPI.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace PostgreSqlAPI.Controllers
{
    [ApiController]
    [Route("api/database")]
    public class RequestController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RequestController> _logger;
        public RequestController(ILogger<RequestController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("/setdatabase")]
        public string SetDataBase([FromBody] DB authorization)
        {
            if (authorization == null) return "Error";
            if (authorization.access == null || authorization.access != "VKRlu7wwnVdp") return "Not access";


            string path = @"appsettings.json";
            string text =
                "{" +
                    "\"Logging\": {" +
                        "\"LogLevel\": {" +
                            "\"Default\": \"Information\"," +
                            "\"Microsoft\":\"Warning\"," +
                            "\"Microsoft.Hosting.Lifetime\": \"Information\"" +
                        "}" +
                    "}," +
                    "\"AllowedHosts\": \"*\"," +
            "\"DBProd\":{" +
                        $"\"host\":\" {authorization.host}\"," +
                        $"\"port\":\"{authorization.port}\"," +
                        $"\"name\":\"{authorization.name}\"," +
                        $"\"username\":\"{authorization.username}\"," +
                        $"\"password\":\"{authorization.password}\"" +
                    "}" +
                "}";

            System.IO.File.WriteAllText(path, string.Empty);
            FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

            StreamWriter sw = new StreamWriter(file);

            sw.Write(text);

            sw.Close();
            sw.Dispose();
            file.Close();
            file.Dispose();

            return "Complete";
        }

        [HttpPost("/select")]
        async public Task<IActionResult> Select([FromBody] InputCmd inputCmd)
        {
            if (inputCmd == null)
            {
                _logger.LogError(string.Format("{0} Got request GetRequiredInformation, but input is invalid: {1}", DateTime.Now.ToString("F"), inputCmd.ToString()));
                return BadRequest("Empty input data.");
            }

            static bool AccessCheck(string cmdText)
            {
                bool flagAccess = true;

                string[] allWord = cmdText.Split(' ');

                for (int i = 0; i < allWord.Length - 1; i++)
                {
                    if (allWord[i].ToUpper() == "DELETE" || allWord[i].ToUpper() == "DROP" ||
                        allWord[i].ToUpper() == "UPDATE" || allWord[i].ToUpper() == "ALTER" ||
                        allWord[i].ToUpper() == "CREATE" || allWord[i].ToUpper() == "INSERT")
                    {
                        flagAccess = false;
                        break;
                    }
                }

                return flagAccess;
            }

            if (AccessCheck(inputCmd.CmdText) == false) return BadRequest("No access to surgery.");

            _logger.LogInformation(string.Format("{0} Making return request.", DateTime.Now.ToString("F")));

            Dictionary<string, string>? dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputCmd.Row?.ToString() ?? "");
            string cmdText = inputCmd.CmdText;

            SearchPostgreSQL search = new(_configuration);

            Dictionary<string, string>[] json = await search.GetRows(cmdText, dictionary);

            return Ok(json);
        }
    }
}
