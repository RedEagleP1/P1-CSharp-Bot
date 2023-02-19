using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleSurvey> RolesSurvey { get; set; }
        public DbSet<CurrencyOwner> CurrencyOwners { get; set; }
        public DbSet<CurrencyAwarder> CurrencyAwarders { get; set; }
        public DbSet<TaskCompletionRecord> TaskCompletionRecords { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(role => role.RoleSurveys)
                .WithOne(roleSurvey => roleSurvey.ParentRole);

            modelBuilder.Entity<RoleSurvey>()
                .HasMany(roleSurvey => roleSurvey.ChildSurveys)
                .WithOne(roleSurvey => roleSurvey.ParentSurvey);

            modelBuilder.Entity<RoleSurvey>()
                .HasMany(roleSurvey => roleSurvey.SurveyTriggerOptions)
                .WithMany(roleSurveyOption => roleSurveyOption.RoleSurveysThatHaveThisOptionAsTrigger);

            modelBuilder.Entity<RoleSurvey>()
                .HasMany(roleSurvey => roleSurvey.SurveyOptions)
                .WithOne(roleSurevyOption => roleSurevyOption.RoleSurvey);

            base.OnModelCreating(modelBuilder);
        }
    }
}
