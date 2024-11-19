using Discord;
using NUnit.Framework;
using System.Collections.Generic;
using Bot.Commands;

namespace Bot.Tests.Commands
{
    [TestFixture]
    public class DynamicCommandBuilderTests
    {
        [Test]
        public void CreateCommand_ShouldReturnSlashCommandBuilder()
        {
            // Arrange
            string name = "Test Command";
            string description = "Test Description";
            var options = new List<DiscordCommandOption>();

            // Act
            var commandBuilder = DynamicCommandBuilder.CreateCommand(name, description, options);

            // Assert
            Assert.IsNotNull(commandBuilder);
            //todo fix this test
            // Assert.AreEqual(name, commandBuilder.Name);
            // Assert.AreEqual(description, commandBuilder.Description);
        }
    }
}
