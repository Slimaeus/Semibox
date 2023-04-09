using System.ComponentModel.DataAnnotations;

namespace Semibox.StatisticService.Application.Statistics
{
    public class CreateStatisticDTO
    {
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Speed { get; set; }
    }
}
