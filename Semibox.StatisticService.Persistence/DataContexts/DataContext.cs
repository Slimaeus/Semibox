using Microsoft.EntityFrameworkCore;
using Semibox.StatisticService.Domain.Entities;

namespace Semibox.StatisticService.Persistence.DataContexts
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Statistic> Statistics { get; set; }
    }
}
