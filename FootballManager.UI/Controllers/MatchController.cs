using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FootballManager.UI.Controllers
{
    public class MatchController : Controller
    {
        private readonly IOptions<ProjectSettings> _projectSettings;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;
        private readonly ISeasonService _seasonService;

        public MatchController(IOptions<ProjectSettings> projectSettings, IOptions<SeasonsSettings> seasonsSettings, ISeasonService seasonService)
        {
            _projectSettings = projectSettings;
            _seasonsSettings = seasonsSettings ?? throw new ArgumentNullException(nameof(seasonsSettings));
            _seasonService = seasonService;
        }

        [HttpGet("/Match")]
        public IActionResult Index()
        {
            return RedirectToAction("Fixture", "Match");
        }

        [HttpGet("/Match/Fixture")]
        [HttpGet("/Match/Fixture/{seasonId}")]
        public async Task<IActionResult> Fixture(int? seasonId)
        {
            var fixture = await _seasonService.GetSeasonMatchFixture(seasonId);
            return View(fixture ?? new());
        }
    }
}
