using System.Threading.Tasks;

namespace CoreBotCLU
{
    public interface IRobotService
    {
        Task<string> MoveRobotAsync(string objectToMove, string destination);
        Task<bool> StartSessionAsync(int robotId);
        Task<string> StopSessionAsync(int robotId);
    }
}
