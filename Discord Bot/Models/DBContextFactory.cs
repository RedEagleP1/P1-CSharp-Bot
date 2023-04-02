using Microsoft.EntityFrameworkCore;

namespace Models
{
    public static class DBContextFactory
    {
        static DbContextOptions<ApplicationDbContext> options;
        public static void Init(DbContextOptions<ApplicationDbContext> options)
        {
            DBContextFactory.options = options;
        }

        public static ApplicationDbContext GetNewContext()
        {
            return new ApplicationDbContext(options);
        }
    }
}
