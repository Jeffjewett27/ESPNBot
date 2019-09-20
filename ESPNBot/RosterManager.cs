using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESPNBot
{
    class RosterManager
    {
        private Roster roster;
        private IESPNTeam espnTeam;
        private Queue<Action> actions;

        public Roster Roster { get => roster; }

        public RosterManager(Roster roster, IESPNTeam espnTeam)
        {
            this.roster = roster;
            this.espnTeam = espnTeam;
            actions = new Queue<Action>();
        }

        public Player GetSub(int startingSpot, int currWeek)
        {
            if (startingSpot < 0 || startingSpot > 8)
            {
                throw new ArgumentOutOfRangeException("startingSpot must be between 0 and 8, inclusive");
            }
            Position sPosition = roster.starters[startingSpot].position;
            var candidates = roster.BenchCandidates(sPosition, currWeek, startingSpot != 6);

            if (candidates.Count > 0)
            {
                IEnumerable<Player> benchCandidates = from p in candidates select roster.bench[p];
                return BestPlayer(benchCandidates);
            }
            else
            {
                return Player.NullPlayer(sPosition);
            }
        }

        private Player BestPlayer(IEnumerable<Player> players)
        {
            Player best = null;

            foreach (Player p in players)
            {
                if (best == null || p.CompareTo(best) > 0)
                {
                    best = p;
                }
            }

            return best;
        }

        private Player WorstPlayer(IEnumerable<Player> players)
        {
            Player worst = null;

            foreach (Player p in players)
            {
                if (worst == null || p.CompareTo(worst) < 0)
                {
                    worst = p;
                }
            }
            return worst;
        }

        public void ManageTeam(int currWeek)
        {
            var inelligible = roster.IneligibleStarters(currWeek);

            if (inelligible.Count > 0)
            {
                foreach (int s in inelligible)
                {
                    Player starter = roster.starters[s];
                    Player sub = GetSub(s, currWeek);

                    if (sub.IsNull())
                    {
                        Player worst = WorstPlayer(roster.bench);
                        Action action;
                        if (starter.CompareTo(worst) < 0)
                        {
                            action = () => espnTeam.AddFreeAgent(starter.position, s);
                            actions.Enqueue(action);
                            roster.SwapPlayers(starter, Player.NullPlayer(starter.position));
                        } else
                        {
                            int slot = roster.GetPlayerSlot(worst);
                            action = () => espnTeam.AddFreeAgent(sub.position, slot);
                            actions.Enqueue(action);
                            Player nullP = Player.NullPlayer(sub.position);
                            roster.SwapPlayers(worst, nullP);
                            action = () => espnTeam.SwapPlayers(s, slot);
                            actions.Enqueue(action);
                            roster.SwapPlayers(starter, nullP);
                        }
                        
                    } else
                    {
                        Action action;
                        int slot = roster.GetPlayerSlot(sub);
                        if (!sub.position.Equals(starter.position))
                        {
                            roster.SwapPlayers(roster.starters[6], sub);
                            action = () => espnTeam.SwapPlayers(6, slot);
                            actions.Enqueue(action);
                        }
                        roster.SwapPlayers(roster.starters[s], roster.GetPlayer(slot));
                        action = () => espnTeam.SwapPlayers(s, slot);
                        actions.Enqueue(action);
                    }
                }
            }

            while (actions.Any())
            {
                actions.Dequeue().Invoke();
            }
            roster = new Roster(espnTeam.GetPlayers());
        }
    }
}
