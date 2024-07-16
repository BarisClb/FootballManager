using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.Requests;
using FootballManager.UI.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.UI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISeasonService _seasonService;
        private readonly IMatchService _matchService;
        private readonly ITeamService _teamService;
        private readonly ISeasonPassService _seasonPass;
        private readonly IJobService _jobService; // TODO DELETE

        public AdminController(IUserService userService, ISeasonService seasonService, IMatchService matchService, ITeamService teamService, ISeasonPassService seasonPass, IJobService jobService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _seasonService = seasonService ?? throw new ArgumentNullException(nameof(seasonService));
            _matchService = matchService ?? throw new ArgumentNullException(nameof(matchService));
            _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
            _seasonPass = seasonPass ?? throw new ArgumentNullException(nameof(seasonPass));
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        }


        [AdminAuthorizationHandler]
        [HttpPost("/createUser")]
        public async Task<IActionResult> CreateUser(CreateUserRequest createUserRequest)
        {
            return Ok(await _userService.CreateUser(createUserRequest));
        }

        [AdminAuthorizationHandler]
        [HttpPost("/createSeason")]
        public async Task<IActionResult> CreateSeason(CreateSeasonRequest createSeasonRequest)
        {
            return Ok(await _seasonService.CreateSeason(createSeasonRequest));
        }

        [AdminAuthorizationHandler]
        [HttpPost("/createSeasonPass")]
        public async Task<IActionResult> CreateSeasonPass(CreateSeasonPassRequest createSeasonPassRequest)
        {
            return Ok(await _seasonPass.CreateSeasonPass(createSeasonPassRequest));
        }

        [AdminAuthorizationHandler]
        [HttpPost("/createTeam")]
        public async Task<IActionResult> CreateTeam(CreateTeamRequest createTeamRequest)
        {
            return Ok(await _teamService.CreateTeam(createTeamRequest));
        }

        [AdminAuthorizationHandler]
        [HttpPost("/scheduleSeason")]
        public async Task<IActionResult> ScheduleSeason(ScheduleSeasonRequest scheduleSeasonRequest)
        {
            return Ok(await _seasonService.ScheduleSeason(scheduleSeasonRequest));
        }

        [AdminAuthorizationHandler]
        [HttpPost("/simulateMatch")]
        public async Task<IActionResult> PlayMatch(int matchId)
        {
            return Ok(await _matchService.SimulateMatch(matchId));
        }

        [AdminAuthorizationHandler]
        [HttpPost("/concludeLeague")]
        public async Task<IActionResult> ConcludeLeague(int seasonId)
        {
            return Ok(await _seasonService.ConcludeLeague(seasonId));
        }

        [AdminAuthorizationHandler]
        [HttpPost("/createOrUpdateSimulateMatchJob")]
        public async Task<IActionResult> CreateOrUpdateSimulateMatchJob(string cronExpression = "*/5 * * * *")
        {
            Hangfire.RecurringJob.AddOrUpdate<IJobService>(job => job.SimulateMatchesJob(), cronExpression);
            return Ok($"Successfully Created/Updated job: 'SimulateMatch'. CronTimer: {cronExpression}");
        }

        [AdminAuthorizationHandler]
        [HttpPost("/createOrUpdateScheduleWeeklyLeagueJob")]
        public async Task<IActionResult> CreateOrUpdateScheduleWeeklyLeagueJob(string cronExpression = "0 20 * * MON")
        {
            Hangfire.RecurringJob.AddOrUpdate<IJobService>(job => job.SimulateMatchesJob(), cronExpression);
            return Ok($"Successfully Created/Updated job: 'ScheduleWeeklyLeague'. CronTimer: {cronExpression}");
        }

        [AdminAuthorizationHandler]
        [HttpPost("/createOrUpdateScheduleWeeklyTournamentJob")]
        public async Task<IActionResult> CreateOrUpdateScheduleWeeklyTournamentJob(string cronExpression = "0 21 * * MON")
        {
            Hangfire.RecurringJob.AddOrUpdate<IJobService>(job => job.SimulateMatchesJob(), cronExpression);
            return Ok($"Successfully Created/Updated job: 'ScheduleWeeklyTournament'. CronTimer: {cronExpression}");
        }

        [HttpGet("scheduleWeeklyLeague")]
        public async Task<IActionResult> ScheduleWeeklyLeague()
        {
            await _jobService.ScheduleWeeklyLeague();
            return Ok();
        }

        [HttpGet("scheduleWeeklyTournament")]
        public async Task<IActionResult> ScheduleWeeklyTournament()
        {
            await _jobService.ScheduleWeeklyTournament();
            return Ok();
        }
        
        [HttpGet("proceedTournaments")]
        public async Task<IActionResult> ProcedeTournament()
        {
            await _jobService.ProceedTournaments();
            return Ok();
        }
    }
}
