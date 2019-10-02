using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESPNBot
{
    /// <summary>
    /// Represents a fantasy football roster
    /// </summary>
    class Roster
    {
        /// <summary>
        /// The mappings of positions to their allowable slots
        /// </summary>
        static Dictionary<Position, int[]> positionMap = new Dictionary<Position, int[]>
        {
            { Position.Quarterback, new int[] { 0 } },
            { Position.RunningBack, new int[] { 1, 2, 6 } },
            { Position.WideReceiver, new int[] { 3, 4, 6 } },
            { Position.TightEnd, new int[] { 5, 6 } },
            { Position.Defense, new int[] { 7 } },
            { Position.Kicker, new int[] { 8 } }
        };

        public Player[] starters = new Player[9];
        public Player[] bench = new Player[7];

        public Roster(Player[] players)
        {
            if (players.Length < 16) //check if insufficient players
            {
                throw new ArgumentException("Not enough players in the lineup");
            }
            SetStarters(players);
            SetBench(players);
        }

        public Roster(Roster roster) : this(roster.GetPlayers()) {  }

        /// <summary>
        /// From a list of players, set the starting lineup
        /// </summary>
        /// <param name="players">The list of players</param>
        private void SetStarters(Player[] players)
        {
            for (int i = 0; i < 9; i++)
            {
                //Check if positional slot is correct
                bool canFit = false;
                positionMap.TryGetValue(players[i].position, out int[] slots);
                foreach (int slot in slots)
                {
                    if (slot == i)
                    {
                        canFit = true;
                    }
                }
                if (!canFit)
                {
                    throw new ArgumentException(players[i].name + " is in the wrong slot");
                }
                //Add to the starting roster 
                starters[i] = players[i];
            }
        }

        /// <summary>
        /// From a list of players, set the bench lineup
        /// </summary>
        /// <param name="players">The list of players</param>
        private void SetBench(Player[] players)
        {
            for (int i = 9; i < players.Length; i++)
            {
                bench[i - 9] = players[i];
            }
        }

        /// <summary>
        /// Returns a list of candidates from the bench that can potentially sub for a starter
        /// </summary>
        /// <param name="sPosition">The position of the starter</param>
        /// <param name="currWeek">The current week for byes</param>
        /// <returns></returns>
        public List<int> BenchCandidates(Position sPosition, int currWeek, bool canUseFlex)
        {
            bool canFlex = sPosition.Equals(starters[6].position);

            List<int> potential = new List<int>();
            for (int i = 0; i < bench.Length; i++)
            {
                if (bench[i].position.Equals(sPosition) || (canFlex && canUseFlex && IsFlexPosition(bench[i].position)))
                {
                    if (bench[i].IsPlaying(currWeek))
                    {
                        potential.Add(i);
                    }
                }
            }

            return potential;
        }

        private bool IsFlexPosition(Position p)
        {
            return p.Equals(Position.RunningBack) || p.Equals(Position.WideReceiver) ||
                p.Equals(Position.TightEnd);
        }

        /// <summary>
        /// Swap Player a out for Player b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void SwapPlayers(Player a, Player b)
        {
            int pa = GetPlayerSlot(a);
            int pb = GetPlayerSlot(b);
            
            if (pa < -1)
            {
                throw new ArgumentOutOfRangeException("Player a must be on the roster");
            }
            if (pa < 9)
            {
                starters[pa] = b;
            } else
            {
                bench[pa - 9] = b;
            }
            if (!(pb < 0))
            {
                if (pb < 9)
                {
                    starters[pb] = a;
                } else
                {
                    bench[pb - 9] = a;
                }
            }
        }

        /// <summary>
        /// Gets the slot that a player is in the roster
        /// </summary>
        /// <param name="p">A player</param>
        /// <returns>The slot from 0-15 of the player, or -1 if not found</returns>
        public int GetPlayerSlot(Player p)
        {
            int i = 0;
            while (i < 9)
            {
                if (starters[i].Equals(p))
                {
                    return i;
                }
                i++;
            }
            while (i < 16)
            {
                if (bench[i - 9].Equals(p))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Gets the player of a certain slot ID
        /// </summary>
        /// <param name="slot">The slot to retrieve</param>
        /// <returns></returns>
        public Player GetPlayer(int slot)
        {
            if (slot < 0 || slot > 15)
            {
                throw new ArgumentOutOfRangeException("Slot must be between 0 and 15, inclusive");
            }
            if (slot < 9)
            {
                return starters[slot];
            } else
            {
                return bench[slot - 9];
            }
        }

        /// <summary>
        /// Gets a list of starters that cannot play, whether due to injury or bye
        /// </summary>
        /// <param name="currWeek">The current week</param>
        /// <returns></returns>
        public List<int> IneligibleStarters(int currWeek)
        {
            var players = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                var starter = starters[i];
                if (starter.byeWeek == currWeek || starter.eligibility != Eligibility.AOK)
                {
                    players.Add(i);
                }
            }
            return players;
        }

        /// <summary>
        /// Gets the players contained by this roster
        /// </summary>
        /// <returns></returns>
        public Player[] GetPlayers()
        {
            Player[] players = new Player[16];
            Array.Copy(starters, players, 9);
            Array.Copy(bench, 0, players, 9, 7);
            return players;
        }

        /// <summary>
        /// Replaces the slot of a player with a new player
        /// </summary>
        /// <param name="slot">The slot to replace</param>
        /// <param name="p">The new player to add</param>
        public void ReplacePlayer(int slot, Player p)
        {
            if (slot < 0 || slot > 15)
            {
                throw new ArgumentOutOfRangeException("Slot " + slot + " is not in the bounds of [0,15]");
            }
            if (slot < 9)
            {
                starters[slot] = p;
            } else
            {
                bench[slot] = p;
            }
        }
    }
}
