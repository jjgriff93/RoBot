using System.Threading.Tasks;

namespace CoreBotCLU
{
    public interface IRobotService
    {
        Task<bool> MoveRobotAsync(string objectToMove, string destination);
        Task<bool> StartSessionAsync(int robotId);
        Task<bool> StopSessionAsync(int robotId);
    }
}
