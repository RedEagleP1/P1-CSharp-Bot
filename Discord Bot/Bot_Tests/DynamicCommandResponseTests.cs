using Discord;
using Discord.WebSocket;
using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using Bot.Commands;

namespace Bot.Tests.Commands
{
    [TestFixture]
    public class DynamicCommandResponseTests
    {
        [Test]
        public async Task RespondAsync_ShouldCallRespondAsyncOnSocketInteraction()
        {
            // Arrange
            var mockSocketInteraction = new Mock<SocketInteraction>();
            var embedBuilder = new EmbedBuilder().WithTitle("Test Title").WithDescription("Test Description");
            var response = new DynamicCommandResponse(mockSocketInteraction.Object, embedBuilder, "Test Content", true, true);

            // Act
            await response.RespondAsync();

            // Assert
            //todo Fix this test
            // mockSocketInteraction.Verify(x => x.RespondAsync(It.IsAny<string>(), It.IsAny<Embed>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
