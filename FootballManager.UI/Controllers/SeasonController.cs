using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FootballManager.UI.Controllers
{
    public class SeasonController : Controller
    {
        private readonly IOptions<ProjectSettings> _projectSettings;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;
        private readonly ISeasonService _seasonService;

        public SeasonController(IOptions<ProjectSettings> projectSettings, IOptions<SeasonsSettings> seasonsSettings, ISeasonService seasonService)
        {
            _projectSettings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            _seasonsSettings = seasonsSettings ?? throw new ArgumentNullException(nameof(seasonsSettings));
            _seasonService = seasonService ?? throw new ArgumentNullException(nameof(seasonService));
        }

        [HttpGet("/Season")]
        public IActionResult Index()
        {
            return RedirectToAction("Standings", "Season");
        }

        [HttpGet("/Season/Standings")]
        [HttpGet("/Season/Standings/{seasonId}")]
        public async Task<IActionResult> SeasonTeamStandings(int? seasonId)
        {
            var teamStandings = await _seasonService.GetSeasonTeamStandings(seasonId);
            return View(teamStandings ?? new());
        }
    }
}
