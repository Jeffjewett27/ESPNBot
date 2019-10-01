using System;

namespace ESPNBot
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            Logger.Configure();
            DateTime firstSeptember = new DateTime(DateTime.Today.Year, 9, 1, 7, 0, 0, DateTimeKind.Utc);
            int dayOffset = (DayOfWeek.Thursday - firstSeptember.DayOfWeek + 7) % 7 - (DayOfWeek.Thursday - DayOfWeek.Tuesday);
            DateTime startDay = firstSeptember.AddDays(dayOffset);
            int currWeek = (int)((DateTime.Now - startDay).TotalDays / 7 + 1);
            logger.Info("ESPN session starting for week " + currWeek);
            using (var espnTeam = new ESPNTeam())
            {
                Player[] players = espnTeam.GetPlayers();
                Roster roster = new Roster(players);
                int count = players.Length;
                RosterManager manager = new RosterManager(roster, espnTeam);
                manager.ManageTeam(currWeek);
            }
            logger.Info("ESPN session for week " + currWeek + " was successful");
        }
    }
}
