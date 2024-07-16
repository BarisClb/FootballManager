﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballManager.Domain.Entities
{
    public class SeasonSettings : BaseEntity
    {
        public int SeasonId { get; set; }
        public Season Season { get; set; }

        public string? MatchTimes { get; set; }
        public int? MatchIntervalMinutes { get; set; }
        public int? LeagueHalvesMinumumIntervalHours { get; set; }
        public int? HomeTeamExtraGoalChance { get; set; }
        public int? TiebreakerMatchHoursAfter { get; set; }
        public bool? StartNextRoundNextDay { get; set; }
        public bool? AllowNoAssistGoals { get; set; }
        public int? NumberOfTeamsToParticipate { get; set; }
        public int? SeasonStartDay { get; set; }
    }
}
