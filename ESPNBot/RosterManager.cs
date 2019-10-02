using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace ESPNBot
{
    class RosterManager
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
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
            logger.Info("Managing team for week " + currWeek);
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
                            logger.Info(starter + "requires a free agent. " + starter + " is being dropped from roster");
                            action = () => espnTeam.AddFreeAgent(starter.position, starter, false);
                            actions.Enqueue(action);
                            roster.SwapPlayers(starter, Player.NullPlayer(starter.position));
                        } else
                        {
                            logger.Info(starter + "is being benched. " + worst + " is being dropped from roster for free agent.");
                            int slot = roster.GetPlayerSlot(worst);
                            action = () => espnTeam.AddFreeAgent(sub.position, worst, false);
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
                            logger.Info(sub + " is swapping with flex " + starter);
                            roster.SwapPlayers(roster.starters[6], sub);
                            action = () => espnTeam.SwapPlayers(6, slot);
                            actions.Enqueue(action);
                        }
                        logger.Info(starter + " is swapping with " + roster.GetPlayer(slot));
                        roster.SwapPlayers(roster.starters[s], roster.GetPlayer(slot));
                        action = () => espnTeam.SwapPlayers(s, slot);
                        actions.Enqueue(action);
                    }
                }
            }

            int failThreshold = 2;
            int fails = 0;
            while (actions.Any())
            {
                try
                {
                    Action action = actions.Dequeue();
                    action.Invoke();
                } catch
                {
                    fails++;
                    if (fails == failThreshold)
                    {
                        logger.Fatal(fails + " actions produced errors, exceeding the allowable quantity");
                        throw new Exception(failThreshold + " actions could not be performed");
                    } else
                    {
                        logger.Error("A roster action could not be performed. This is allowable error " + fails + "/" + (failThreshold - 1));
                    }
                }
            }
            roster = new Roster(espnTeam.GetPlayers());
        }
    }
}
