using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class DBContextFactory
    {
        DbContextOptions<ApplicationDbContext> options;
        public DBContextFactory(DbContextOptions<ApplicationDbContext> options)
        {
            this.options = options;
        }

        public ApplicationDbContext GetNewContext()
        {
            return new ApplicationDbContext(options);
        }
    }
}
