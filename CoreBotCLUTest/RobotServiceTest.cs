using CoreBotCLU.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.ComponentModel.Design;

namespace CoreBotCLUTest
{
    public class RobotServiceTest
    {
        private readonly IRobotService _robotService;
        public RobotServiceTest() {

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x["RobotAPIEndpoint"]).Returns("https://robot-t1000-api--0ngcgmn.lemonfield-0e199130.eastus.azurecontainerapps.io");

            _robotService = new RobotService(mockConfiguration.Object);
        }
        [Fact]
        public async Task Robot_StartSession_returnsuccess()
        {

            //Act
            var actualResult = await _robotService.StartSessionAsync(1);
            
            //assert           
           Assert.True(actualResult);
        }
    }
}