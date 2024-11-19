using Discord;
using NUnit.Framework;
using System.Collections.Generic;
using Bot.Commands;

namespace Bot.Tests.Commands
{
    [TestFixture]
    public class DynamicCommandOptionBuilderTests
    {
        [Test]
        public void CreateOption_ShouldReturnSlashCommandOptionBuilder()
        {
            // Arrange
            string name = "Test Option";
            string description = "Test Description";
            ApplicationCommandOptionType type = ApplicationCommandOptionType.String;
            bool required = true;
            var options = new List<DiscordCommandOption>();

            // Act
            var optionBuilder = DynamicCommandOptionBuilder.CreateOption(name, description, type, required, options);

            // Assert
            Assert.IsNotNull(optionBuilder);
            //todo fix this test
            // Assert.AreEqual(name, optionBuilder.Name);
            // Assert.AreEqual(description, optionBuilder.Description);
            // Assert.AreEqual(type, optionBuilder.Type);
            // Assert.AreEqual(required, optionBuilder.IsRequired);
        }
    }
}
