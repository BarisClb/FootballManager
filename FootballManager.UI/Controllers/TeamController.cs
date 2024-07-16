using FootballManager.Application.Helpers;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FootballManager.UI.Controllers
{
    public class TeamController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;

        public TeamController(ITeamService teamService, IOptions<SeasonsSettings> seasonsSettings)
        {
            _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
            _seasonsSettings = seasonsSettings ?? throw new ArgumentNullException(nameof(seasonsSettings));
        }


        [HttpGet("/Team")]
        public IActionResult Index()
        {
            return RedirectToAction("TeamStats", "Team");
        }


        [HttpGet("/Team/{teamId}")]
        public async Task<IActionResult> TeamStats(int? teamId)
        {
            var teamStats = await _teamService.GetTeamStats(teamId ?? 0);
            return View(teamStats);
        }

        [HttpGet("/Team/CreateTeam")]
        [HttpGet("/Team/CreateTeam/{seasonId}")]
        [HttpGet("/Team/CreateTeam/{seasonId}/{premadeTeamId}")]
        public async Task<IActionResult> CreateTeam(int? seasonId, int? premadeTeamId)
        {
            var createTeamRequest = await _teamService.GetCreateTeamRequest(seasonId, premadeTeamId);
            ViewBag.PremadeTeams = PreMadeTeams.Teams.ToList();
            ViewBag.CreateTeamRequest = createTeamRequest;
            ModelState.Clear();
            return View(createTeamRequest);
        }

        [HttpPost("/Team/CreateTeamForm")]
        public async Task<IActionResult> CreateTeamRequest(CreateTeamRequestForm createTeamRequestForm)
        {
            var result = await _teamService.CreateTeamWithForm(createTeamRequestForm);
            if (!result.IsSuccess)
                return BadRequest(result);
            return RedirectToAction("Standings", "Season", new { seasonId = createTeamRequestForm.SeasonId ?? 0 });
        }
    }
}
