using System;
using System.Collections.Generic;
using System.Text;

namespace ESPNBot
{
    class Player : IComparable<Player>, IEquatable<Player>
    {
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
    }
}
