using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotInfrastructure.HttpClients;
using Bot.Commands;

namespace Bot.Tests.Commands
{
    [TestFixture]
    public class CreateDynamicCommandsTests
    {
        [Test]
        public async Task BuildCommandAsync_ShouldReturnListOfSlashCommandProperties()
        {
            // Arrange
            var mockHttpClient = new Mock<IHttpClient>();
            var createDynamicCommands = new CreateDynamicCommands(mockHttpClient.Object);
            string name = "Test Command";
            string description = "Test Description";
            string endpoint = "http://test.endpoint";
            var options = new List<DiscordCommandOption>();

            var commandModels = new List<CreateCommandModel>
            {
                new CreateCommandModel { Name = name, Description = description, Options = options }
            };

            mockHttpClient.Setup(x => x.GetAsync<CreateCommandModel[]>(endpoint)).ReturnsAsync(commandModels.ToArray());

            // Act
            var commands = await createDynamicCommands.BuildCommandAsync(name, description, endpoint, options);

            // Assert
            Assert.IsNotNull(commands);
            Assert.IsNotEmpty(commands);
        }
    }
}
