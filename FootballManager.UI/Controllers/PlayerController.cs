using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FootballManager.UI.Controllers
{
    public class PlayerController : Controller
    {
        private readonly IOptions<ProjectSettings> _projectSettings;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;
        private readonly ISeasonService _seasonService;

        public PlayerController(IOptions<ProjectSettings> projectSettings, IOptions<SeasonsSettings> seasonsSettings, ISeasonService seasonService)
        {
            _projectSettings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            _seasonsSettings = seasonsSettings ?? throw new ArgumentNullException(nameof(seasonsSettings));
            _seasonService = seasonService ?? throw new ArgumentNullException(nameof(seasonService));
        }


        [HttpGet("/Player")]
        public IActionResult Index()
        {
            return RedirectToAction("SeasonPlayerStandings", "Player");
        }

        [HttpGet("Player/Standings")]
        [HttpGet("Player/Standings/{seasonId}")]
        public async Task<IActionResult> SeasonPlayerStandings(int? seasonId)
        {
            var seasonPlayerStandings = await _seasonService.GetSeasonPlayerStandings(seasonId);
            return View(seasonPlayerStandings ?? new());
        }
    }
}
