using System;
using System.Collections.Generic;
using System.Text;

namespace ESPNBot
{
    class Player : IComparable<Player>, IEquatable<Player>
    {
        private static Dictionary<string, Position> positionMap = new Dictionary<string, Position>() {
            { "QB", Position.Quarterback },
            { "RB", Position.RunningBack },
            { "WR", Position.WideReceiver },
            { "TE", Position.TightEnd },
            { "K", Position.Kicker },
            { "D/ST", Position.Defense }
        };

        private static Dictionary<string, Eligibility> eligibilityMap = new Dictionary<string, Eligibility>()
        {
            { "AOK", Eligibility.AOK },
            { "Questionable", Eligibility.Questionable },
            { "Injured", Eligibility.Injured },
            { "Out", Eligibility.Out },
            { "Suspended", Eligibility.Suspended }
        };

        public string name;
        public string team;
        public Eligibility eligibility;
        public int byeWeek;
        public Position position;
        public double projected;

        public static Player NullPlayer(Position p)
        {
            return new Player()
            {
                position = p
            };
        }

        public Player() { }

        public Player(string name, string team, Eligibility eligibility, Position position, double projected)
        {
            this.name = name;
            this.team = team;
            this.eligibility = eligibility;
            this.position = position;
            this.projected = projected;
            byeWeek = Team.GetByeWeek(team);
        }

        public Player(string name, string team, string eligibility, string position, double projected)
        {
            this.name = name;
            this.team = team;
            this.eligibility = GetEligibility(eligibility);
            this.position = GetPosition(position);
            this.projected = projected;
            byeWeek = Team.GetByeWeek(team);
        }

        public int CompareTo(Player other)
        {
            return (int)((projected - other.projected) * 10);
        }

        public bool Equals(Player other)
        {
            if (other == null) return false;
            bool n = (name != null && name.Equals(other.name)) || (name == null && other.name == null);
            bool t = (team != null && team.Equals(other.team)) || (team == null && other.team == null);
            bool e = eligibility.Equals(other.eligibility);
            bool b = byeWeek - other.byeWeek == 0;
            bool p = projected - other.projected < 0.1;
            bool pos = position.Equals(other.position);
            return n && t && e && b && p && pos;
        }

        public bool IsNull()
        {
            return name == default && team == default;
        }

        public bool IsPlaying(int currWeek)
        {
            if (byeWeek == currWeek)
            {
                return false;
            }

            if (!eligibility.Equals(Eligibility.AOK))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return name + ": " + position.ToString() + "(" + team +")";
        }

        private Position GetPosition(string pos)
        {
            if (positionMap.TryGetValue(pos, out Position position))
            {
                return position;
            }
            else
            {
                throw new ArgumentException(pos + " is not a valid position");
            }
        }

        private Eligibility GetEligibility(string elg)
        {
            if (eligibilityMap.TryGetValue(elg, out Eligibility eligibility))
            {
                return eligibility;
            }
            else
            {
                throw new ArgumentException(elg + " is not a valid eligibility");
            }
        }
    }
}
