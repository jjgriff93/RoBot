using System.Threading.Tasks;
using Microsoft.Robots;
using System.Collections.Generic;

namespace CoreBotCLU
{
    public interface IRobotService
    {
        Task<bool> MoveRobotAsync(string objectToMove, string destination);
        Task<bool> StartSessionAsync(string robotId);
        Task<bool> StopSessionAsync(string robotId);
        Task<List<Robot>> GetAvailableRobotsAsync();
        Task<bool> MoveRobotbyIDAsync(string robotId, string key, string objectToMove, string destination);
        Task<bool> GetRobotHeartbeatAsync(string robotId, string key); 

    
    }
}
