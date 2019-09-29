using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESPNBot
{
    class ESPNTeamTest : IESPNTeam
    {
        private Player[] freeAgents;
        private Roster roster;

        public ESPNTeamTest(Roster roster, Player[] freeAgents)
        {
            Player[] players = roster.GetPlayers();
            this.roster = new Roster(players);
            this.freeAgents = freeAgents;
        }

        public void AddFreeAgent(Position pos, Player p)
        {
            throw new NotImplementedException("Under maintenance");
            List<Player> agents = new List<Player>();
            foreach (Player f in freeAgents)
            {
                if (f.position.Equals(pos))
                {
                    agents.Add(f);
                }
            }
            agents.Sort();
            Player picked = agents[agents.Count - 1];
            //roster.SwapPlayers(roster.GetPlayer(slot), picked);
        }

        public Player GetPlayer(int slot)
        {
            return roster.GetPlayer(slot);
        }

        public Player[] GetPlayers()
        {
            return (from p in roster.GetPlayers() select new Player()
            {
                byeWeek = p.byeWeek,
                eligibility = p.eligibility,
                position = p.position,
                name = p.name,
                projected = p.projected,
                team = p.team
            }).ToArray();
        }

        public void SwapPlayers(int s1, int s2)
        {
            roster.SwapPlayers(roster.GetPlayer(s1), roster.GetPlayer(s2));
        }

        public void UpdatePlayer(Player player)
        {
            
        }
    }
}
