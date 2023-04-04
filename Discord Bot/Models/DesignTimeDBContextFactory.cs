using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Models
{
    //This class only exists to make changes to database schema during development.
    //Just set the connection string below for the database you want to alter.
    //Make sure to remove that after done as the connection string is sensitive data and shouldn't exist in the code.
    internal class DesignTimeDBContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var jsonText = File.ReadAllText("appsettings.json");
            var settings = JsonSerializer.Deserialize<AppsettingsJson>(jsonText);
            optionsBuilder.UseMySql(settings.ConnectionString, ServerVersion.AutoDetect(settings.ConnectionString));

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private class AppsettingsJson
        {
            public string ConnectionString { get; set; }
        }
    }
}
