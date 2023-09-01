using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TextChannelMessageValidation
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public bool IsEnabled { get; set; }
        public int? CurrencyId { get; set; }
        public int AmountGainedPerMessage { get; set; }
        public int DelayBetweenAllowedMessageInMinutes { get; set; }

        //Validation
        public int MinimumCharacterCount { get; set; }
        public bool IsEnabledCharacterCountCheck { get; set; }
        public string? PhrasesThatShouldExist { get; set; }
        public bool IsEnabledPhraseCheck { get; set; }
        public bool ShouldContainURL { get; set; }
        public bool ShouldContainMediaURL { get; set; }
        public bool ShouldContainMedia { get; set; }
        public bool ShouldDeleteMessageOnSuccess { get; set; }
        public bool ShouldDeleteMessageOnFailure { get; set; }
        public string? MessageToSendSuccess { get; set; }
        public bool ShouldSendDMSuccess { get; set; }
        public string? MessageToSendFailure { get; set; }
        public bool ShouldSendDMFailure { get; set; }
        public ulong? RoleToGiveSuccess { get; set; }
        public ulong? RoleToGiveFailure { get; set; }
        public bool UseGPT { get; set; }
        public string? GPTCriteria { get; set; }
        public string? DMStyleSuccess { get; set; }
        public string? DMStyleFailure { get; set; }
    }
}
