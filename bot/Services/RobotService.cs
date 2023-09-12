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
        public Task<string> MoveRobotAsync(string objectToMove, string destination)
        {

            throw new System.NotImplementedException();
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

        public Task<string> StopSessionAsync(int robotId)
        {
            throw new System.NotImplementedException();
        }
    }
}
