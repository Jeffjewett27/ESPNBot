using System;
using System.Collections.Generic;
using System.Text;

namespace ESPNBot
{
    /// <summary>
    /// Represents a fantasy football player
    /// </summary>
    class Player : IComparable<Player>, IEquatable<Player>
    {
        //Maps the espn abbrieviations to positions
        private static Dictionary<string, Position> positionMap = new Dictionary<string, Position>() {
            { "QB", Position.Quarterback },
            { "RB", Position.RunningBack },
            { "WR", Position.WideReceiver },
            { "TE", Position.TightEnd },
            { "K", Position.Kicker },
            { "D/ST", Position.Defense }
        };
        //Maps the eligibility strings to their eligibilities
        private static Dictionary<string, Eligibility> eligibilityMap = new Dictionary<string, Eligibility>()
        {
            { "AOK", Eligibility.AOK },
            { "Questionable", Eligibility.Questionable },
            { "Injured Reserve", Eligibility.Injured },
            { "Out", Eligibility.Out },
            { "Suspended", Eligibility.Suspended }
        };

        public string name;
        public string team;
        public Eligibility eligibility;
        public int byeWeek;
        public Position position;
        public double projected;
        public bool isMovable;

        /// <summary>
        /// Represents a player object that consists only of a position
        /// </summary>
        /// <param name="p">The position</param>
        /// <returns></returns>
        public static Player NullPlayer(Position p)
        {
            return new Player()
            {
                position = p
            };
        }

        private Player() { }

        public Player(string name, string team, Eligibility eligibility, Position position, double projected, bool isMovable)
        {
            this.name = name;
            this.team = team;
            this.eligibility = eligibility;
            this.position = position;
            this.projected = projected;
            this.isMovable = isMovable;
            byeWeek = Team.GetByeWeek(team);
        }

        public Player(string name, string team, string eligibility, string position, double projected, bool isMovable)
        {
            this.name = name;
            this.team = team;
            this.eligibility = GetEligibility(eligibility);
            this.position = GetPosition(position);
            this.projected = projected;
            this.isMovable = isMovable;
            byeWeek = Team.GetByeWeek(team);
        }

        /// <summary>
        /// Compares whether a player is better than the other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Player other)
        {
            return (int)((projected - other.projected) * 10);
        }

        /// <summary>
        /// Checks whether two players are equal in their information
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks whether a player is null
        /// </summary>
        /// <returns></returns>
        public bool IsNull()
        {
            return name == default && team == default;
        }

        /// <summary>
        /// Checks whether a player is playing in the current week
        /// </summary>
        /// <param name="currWeek">The current week</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the player position represented by an abbrieviation
        /// </summary>
        /// <param name="pos">The position abbrieviation</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the eligibility value of an abbrieviation
        /// </summary>
        /// <param name="elg">The eligibility abbrieviation</param>
        /// <returns></returns>
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
