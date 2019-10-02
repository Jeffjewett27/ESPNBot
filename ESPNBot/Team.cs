using System;
using System.Collections.Generic;
using System.Text;

namespace ESPNBot
{
    static class Team
    {
        //Team bye weeks collected from fantasypros.com
        static Dictionary<string, int> byeWeeks = new Dictionary<string, int>
        {
            { "ARI", 12 },
            { "ATL", 9 },
            { "BAL", 8 },
            { "BUF", 6 },
            { "CAR", 7 },
            { "CHI", 6 },
            { "CIN", 9 },
            { "CLE", 7 },
            { "DAL", 8 },
            { "DEN", 10 },
            { "DET", 5 },
            { "GB", 11 },
            { "HOU", 10 },
            { "IND", 6 },
            { "JAX", 10 },
            { "KC", 12 },
            { "LAC", 12 },
            { "LAR", 9 },
            { "MIA", 5 },
            { "MIN", 12 },
            { "NE", 10 },
            { "NO", 9 },
            { "NYG", 11 },
            { "NYJ", 4 },
            { "OAK", 6 },
            { "PHI", 10 },
            { "PIT", 7 },
            { "SF", 4 },
            { "SEA", 11 },
            { "TB", 7 },
            { "TEN", 11 },
            { "WSH", 10 },
            { "FA", 0 }
        };

        /// <summary>
        /// Gets the week a team is on bye
        /// </summary>
        /// <param name="team">The team</param>
        /// <returns></returns>
        public static int GetByeWeek(string team)
        {
            if (byeWeeks.TryGetValue(team.ToUpper(), out int week))
            {
                return week;
            } else
            {
                throw new ArgumentException(team + " is not a valid team name");
            }
        }
    }
}
