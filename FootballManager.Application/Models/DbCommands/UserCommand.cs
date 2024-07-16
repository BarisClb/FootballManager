﻿namespace FootballManager.Application.Models.DbCommands
{
    public class UserCommand : BaseCommand
    {
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string Groups { get; set; }
        public string Roles { get; set; }
    }
}