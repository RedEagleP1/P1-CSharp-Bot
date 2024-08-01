using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleMessage> RoleMessages { get; set; }
        public DbSet<RoleSurvey> RolesSurvey { get; set; }
        public DbSet<RoleSurveyOption> RoleSurveyOptions { get; set; }
        public DbSet<RoleSurveyRoleSurveyTrigger> RoleSurveyRoleSurveyTriggers { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CurrencyOwned> CurrenciesOwned { get; set; }
        public DbSet<CurrencyAwardLimit> CurrencyAwardLimits { get; set; }
        public DbSet<RoleCostAndReward> RolesCostAndReward { get; set; }
        public DbSet<RoleMessageAndSurveyRepeat> RoleMessageAndSurveyRepeats { get; set; }
        public DbSet<RoleMessageAndSurveyRepeated> RoleMessagesAndSurveysRepeated { get; set; }
        public DbSet<RoleForSale> RolesForSale { get; set; }
        public DbSet<TaskCompletionRecord> TaskCompletionRecords { get; set; }
        public DbSet<VoiceChannelCurrencyGain> VoiceChannelCurrencyGains { get; set; }
        public DbSet<GlobalVoiceCurrencyGain> GlobalVoiceCurrencyGains { get; set; }
        public DbSet<CurrencyReset> CurrencyResets { get; set; }
        public DbSet<VoiceChannelTrack> VoiceChannelTracks { get; set; }
        public DbSet<TextChannelMessageValidation> TextChannelMessageValidation { get; set; }
        public DbSet<MessageValidationSuccessTrack> MessageValidationSuccessTracks { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guild>()
                .HasMany<Role>()
                .WithOne()
                .HasForeignKey(r => r.GuildId);

            modelBuilder.Entity<Role>()
                .HasOne<RoleMessage>()
                .WithOne()
                .HasForeignKey<RoleMessage>(rm => rm.RoleId);

            modelBuilder.Entity<Role>()
                .HasMany<RoleSurvey>()
                .WithOne()
                .HasForeignKey(rs => rs.RoleId);

            modelBuilder.Entity<RoleSurvey>()
                .HasOne<RoleSurvey>()
                .WithMany()
                .HasForeignKey(rs => rs.ParentSurveyId)
                .IsRequired(false);

            modelBuilder.Entity<RoleSurvey>()
                .HasMany<RoleSurveyOption>()
                .WithOne()
                .HasForeignKey(o => o.RoleSurveyId);
            //configure many to many relation start
            modelBuilder.Entity<RoleSurveyRoleSurveyTrigger>()
                .HasKey(rsrst => new { rsrst.RoleSurveyId, rsrst.RoleSurveyOptionId });

            modelBuilder.Entity<RoleSurvey>()
                .HasMany<RoleSurveyRoleSurveyTrigger>()
                .WithOne()
                .HasForeignKey(rsrst => rsrst.RoleSurveyId);

            modelBuilder.Entity<RoleSurveyOption>()
                .HasMany<RoleSurveyRoleSurveyTrigger>()
                .WithOne()
                .HasForeignKey(rsrst => rsrst.RoleSurveyOptionId);
            //configure many to many relation end

            modelBuilder.Entity<Role>()
                .HasOne<RoleCostAndReward>()
                .WithOne()
                .HasForeignKey<RoleCostAndReward>(rcar => rcar.RoleId);

            modelBuilder.Entity<RoleCostAndReward>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(rcar => rcar.CostCurrencyId)
                .IsRequired(false);

            modelBuilder.Entity<RoleCostAndReward>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(rcar => rcar.RewardCurrencyId)
                .IsRequired(false);

            modelBuilder.Entity<Role>()
                .HasOne<RoleMessageAndSurveyRepeat>()
                .WithOne()
                .HasForeignKey<RoleMessageAndSurveyRepeat>(repeat => repeat.RoleId);

            modelBuilder.Entity<CurrencyOwned>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(co => co.CurrencyId);

            modelBuilder.Entity<CurrencyAwardLimit>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(cal => cal.CurrencyId);

            modelBuilder.Entity<RoleMessageAndSurveyRepeated>()
                .HasOne<Role>()
                .WithOne()
                .HasForeignKey<RoleMessageAndSurveyRepeated>(r => r.RoleId);

            modelBuilder.Entity<RoleForSale>()
                .HasOne<Role>()
                .WithOne()
                .HasForeignKey<RoleForSale>(r => r.RoleId);

            modelBuilder.Entity<VoiceChannelCurrencyGain>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(v => v.CurrencyId)
                .IsRequired(false);

            modelBuilder.Entity<VoiceChannelCurrencyGain>()
                .HasOne<Guild>()
                .WithMany()
                .HasForeignKey(v => v.GuildId);

            modelBuilder.Entity<GlobalVoiceCurrencyGain>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(v => v.CurrencyId)
                .IsRequired(false);

            modelBuilder.Entity<GlobalVoiceCurrencyGain>()
                .HasOne<Guild>()
                .WithMany()
                .HasForeignKey(v => v.GuildId);

            modelBuilder.Entity<Automation>()
                .HasOne<Guild>()
                .WithMany()
                .HasForeignKey(v => v.GuildId);

			modelBuilder.Entity<IdAuto>()
				.HasOne<Guild>()
				.WithMany()
				.HasForeignKey(v => v.AutomationId);

			modelBuilder.Entity<InfoAuto>()
				.HasOne<Guild>()
				.WithMany()
				.HasForeignKey(v => v.AutomationId);

			modelBuilder.Entity<CurrencyReset>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(v => v.CurrencyId)
                .IsRequired(false);

            modelBuilder.Entity<CurrencyReset>()
                .HasOne<Guild>()
                .WithMany()
                .HasForeignKey(v => v.GuildId);

            modelBuilder.Entity<TextChannelMessageValidation>()
                .HasOne<Guild>()
                .WithMany()
                .HasForeignKey(mv => mv.GuildId);

            modelBuilder.Entity<TextChannelMessageValidation>()
                .HasOne<Currency>()
                .WithMany()
                .HasForeignKey(mv => mv.CurrencyId)
                .IsRequired(false);

            modelBuilder.Entity<TextChannelMessageValidation>()
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey(mv => mv.RoleToGiveSuccess)
                .IsRequired(false);

            modelBuilder.Entity<TextChannelMessageValidation>()
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey(mv => mv.RoleToGiveFailure)
                .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
