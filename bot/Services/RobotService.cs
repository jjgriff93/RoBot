using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreBotCLU
{
    public class RobotService : IRobotService
    {
        private readonly string _robotServiceUrl;
        private HttpClientService _httpClientService;
        public RobotService(IConfiguration configuration)
        {
            _robotServiceUrl =$"{configuration["RobotAPIEndpoint"]}";
            _httpClientService = new HttpClientService(_robotServiceUrl);
        }
        public async Task<bool> MoveRobotAsync(string objectToMove, string destination)
        {
             var queryParameters = new Dictionary<string, string>
             {
                { "action", "move" }
             };
            var response = await  _httpClientService.PutAsync("/robot/move", "", queryParameters);
            if(response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> StartSessionAsync(int robotId)
        {
            // Create a dictionary of query parameters
            var queryParameters = new Dictionary<string, string>
             {
                    { "robotId", robotId.ToString() }
             };
            var response = await  _httpClientService.PostAsync("/robot/startsession", "", queryParameters);
            if(response.IsSuccessStatusCode)
            {
                return true;
            }   
            return false;
        }

        public async Task<bool> StopSessionAsync(int robotId)
        {
           // Create a dictionary of query parameters
            var queryParameters = new Dictionary<string, string>
             {
                    { "robotId", robotId.ToString() }
             };
            var response = await  _httpClientService.PostAsync("Robot/EndSession", "", queryParameters);
            if(response.IsSuccessStatusCode)
            {
                return true;
            }   
            return false;    
        }
    }
}
