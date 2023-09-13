using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Robots;
using Newtonsoft.Json;

namespace CoreBotCLU
{
    public class RobotService : IRobotService
    {
        private readonly string _robotServiceUrl;
        private readonly string _robotMultiMoveServiceUrl;
        private HttpClientService _httpClientService;
        private HttpClientService _httpClientService2;
        public RobotService(IConfiguration configuration)
        {
            _robotServiceUrl =$"{configuration["RobotAPIEndpoint"]}";
            _robotMultiMoveServiceUrl = $"{configuration["RobotMultiMoveEndpoint"]}";
            _httpClientService = new HttpClientService(_robotServiceUrl);
            _httpClientService2 = new HttpClientService(_robotMultiMoveServiceUrl);
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

        public async Task<bool> MoveRobotbyIDAsync(string robotId, string key, string objectToMove, string destination)
        {
             var queryParameters = new Dictionary<string, string>
             {
                { "key", key }
             };
              var data = new MoveCommand
                {
                    x = -1,
                    y = 0,
                    z = 0,
                    rx = 0,
                    ry = 0,
                    rz = 0,
                    velocity = 0.2,
                    acceleration = 0.2
                };
            string jsonData = JsonConvert.SerializeObject(data);
            var response = await  _httpClientService.PutAsync($"/robot/{robotId}/move", jsonData, queryParameters);
            if(response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> MultiMoveRobotAsync(string robotId, string key, string moveCommand)
        {
            var queryParameters = new Dictionary<string, string>
             {
                { "key", key },
                { "moveCommand", moveCommand }
             };
            var response = await _httpClientService2.PutAsync($"/robot/{robotId}/multimove", "", queryParameters);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> StartSessionAsync(string robotId)
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

        public async Task<bool> StopSessionAsync(string robotId)
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

        public async Task<List<Robot>> GetAvailableRobotsAsync()
        {
            var response = await  _httpClientService.GetAsync("/robot");
            
            //Check if the response is null or empty
            if(!string.IsNullOrEmpty(response))
            {
                List<Robot> robots = JsonConvert.DeserializeObject<List<Robot>>(response);
                return robots;
            }
            return null;    
        }

        public async Task<bool> GetRobotHeartbeatAsync(string robotId, string key)
        {
            var queryParameters = new Dictionary<string, string>
             {
                    { "robotId", robotId },
                    { "key", key}
             };
             var response = await  _httpClientService.PostAsync("Robot/HeartBeat", "", queryParameters);
            if(response.IsSuccessStatusCode){
                return true;
            }
            return false;
        }
    }
}
