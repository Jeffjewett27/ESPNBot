using System;
using System.Collections.Generic;
using System.Text;

namespace ESPNBot
{
    class Roster
    {
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
        public List<Player> droppedPlayers;

        public Roster(Player[] players)
        {
            if (players.Length < 16) //check if insufficient players
            {
                throw new ArgumentException("Not enough players in the lineup");
            }
            SetStarters(players);
            SetBench(players);
        }

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

        private void SetBench(Player[] players)
        {
            for (int i = 9; i < players.Length; i++)
            {
                bench[i - 9] = players[i];
            }
        }

        public Player SubOut(int startingSpot, int currWeek)
        {
            if (startingSpot < 0 || startingSpot > 8)
            {
                throw new ArgumentOutOfRangeException("startingSpot must be between 0 and 8, inclusive");
            }
            Position sPosition = starters[startingSpot].position;
            Position fPosition1 = sPosition;
            Position fPosition2 = sPosition;

            if (sPosition.Equals(Position.RunningBack) && starters[6].position.Equals(Position.RunningBack))
            {
                fPosition1 = Position.WideReceiver;
                fPosition2 = Position.TightEnd;
            } else if (sPosition.Equals(Position.WideReceiver) && starters[6].position.Equals(Position.WideReceiver))
            {
                fPosition1 = Position.RunningBack;
                fPosition2 = Position.TightEnd;
            } else if (sPosition.Equals(Position.TightEnd) && starters[6].position.Equals(Position.TightEnd))
            {
                fPosition1 = Position.WideReceiver;
                fPosition2 = Position.RunningBack;
            }

            List<int> potential = new List<int>();
            for (int i = 0; i < bench.Length; i++)
            {
                if (bench[i].position.Equals(sPosition) || bench[i].position.Equals(fPosition1) || bench[i].position.Equals(fPosition2))
                {
                    if (isPlaying(bench[i], currWeek))
                    {
                        potential.Add(i);
                    }
                }
            }
            return null;
        }

        public bool isPlaying(Player p, int currWeek)
        {
            if (p.byeWeek == currWeek)
            {
                return false;
            }

            if (!p.eligibility.Equals(Eligibility.AOK))
            {
                return true;
            }

            return true;
        }
    }
}
