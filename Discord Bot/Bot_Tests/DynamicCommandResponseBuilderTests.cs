using Discord;
using Discord.WebSocket;
using NUnit.Framework;
using Moq;
using Bot.Commands;

namespace Bot.Tests.Commands
{
    [TestFixture]
    public class DynamicCommandResponseBuilderTests
    {
        [Test]
        public void CreateResponse_ShouldReturnDynamicCommandResponse()
        {
            // Arrange
            var mockSocketInteraction = new Mock<SocketInteraction>();
            string title = "Test Title";
            string description = "Test Description";
            string content = "Test Content";
            Color color = Color.Blue;
            bool isEphemeral = true;
            bool isTTS = true;

            // Act
            var response = DynamicCommandResponseBuilder.CreateResponse(mockSocketInteraction.Object, title, description, content, color, isEphemeral, isTTS);

            // Assert
            Assert.IsNotNull(response);
            //todo Fix this test
            // Assert.AreEqual(content, response.Content);
            // Assert.AreEqual(isEphemeral, response.IsEphemeral);
            // Assert.AreEqual(isTTS, response.IsTTS);
        }
    }
}
