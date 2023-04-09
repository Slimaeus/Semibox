namespace Semibox.StatisticService.Domain.Entities
{
    public class Statistic
    {
        public Guid Id { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Speed { get; set; }
    }
}
