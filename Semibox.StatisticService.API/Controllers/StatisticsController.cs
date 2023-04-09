using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semibox.StatisticService.Application.Statistics;
using Semibox.StatisticService.Domain.Entities;
using Semibox.StatisticService.Persistence.DataContexts;
using System.Security.Claims;

namespace Semibox.StatisticService.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public StatisticsController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult<IQueryable<Statistic>> Get()
        {
            var statistics = _dataContext.Statistics;
            return Ok(statistics);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<IQueryable<Statistic>>> GetAsync(Guid id)
        {
            var statistic = await _dataContext.Statistics
                .FirstOrDefaultAsync(s => s.Id == id);
            return Ok(statistic);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, EditStatisticDTO input)
        {
            if (!id.Equals(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))))
            {
                ModelState.AddModelError(nameof(id), "You can not edit another user's statistic");
                return ValidationProblem();
            }
            var statistic = await _dataContext.Statistics
                .FirstOrDefaultAsync(s => s.Id == id);

            if (statistic == null) return NotFound();

            statistic.Attack = input.Attack;
            statistic.Defence = input.Defence;
            statistic.Speed = input.Speed;

            var success = await _dataContext.SaveChangesAsync() > 0;

            if (success) return NoContent();

            return BadRequest();
        }

        [HttpPost]
        [ProducesDefaultResponseType(typeof(Statistic))]
        public async Task<IActionResult> PostAsync(CreateStatisticDTO input)
        {
            if (await _dataContext.Statistics.AnyAsync(s => s.Id == Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))))
            {
                ModelState.AddModelError(nameof(User), "You already have statistic");
                return ValidationProblem();
            }
            var statistic = new Statistic
            {
                Id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Attack = input.Attack,
                Defence = input.Defence,
                Speed = input.Speed,
            };

            _dataContext.Statistics.Add(statistic);

            var success = await _dataContext.SaveChangesAsync() > 0;

            if (success) return CreatedAtAction(nameof(Get), statistic);

            return BadRequest();
        }

        [HttpDelete("{id}")]
        [ProducesDefaultResponseType(typeof(Statistic))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (!id.Equals(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))))
            {
                ModelState.AddModelError(nameof(id), "You can not delete another user's statistic");
                return ValidationProblem();
            }
            var statistic = await _dataContext.Statistics
                .FirstOrDefaultAsync(s => s.Id == id);

            if (statistic == null) return NotFound();

            _dataContext.Statistics.Remove(statistic);

            var success = await _dataContext.SaveChangesAsync() > 0;

            if (success) return NoContent();

            return BadRequest();
        }
    }
}
