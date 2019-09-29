using System;
using System.Collections.Generic;
using System.Text;

namespace ESPNBot
{
    interface IESPNTeam
    {
        Player[] GetPlayers();
        Player GetPlayer(int slot);
        void UpdatePlayer(Player player);
        void SwapPlayers(int s1, int s2);
        void AddFreeAgent(Position pos, Player player);
    }
}
