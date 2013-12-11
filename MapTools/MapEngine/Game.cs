using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public class Game
    {
        public const int BarbarianPlayerId = 18;

        public Game() {
            Teams = new List<Team>();
            Players = new List<Player>();
            UnparsedData = new List<string>();
            UnparsedOuterData = new List<string>();
        }

        public int Version { get; set; }
        public Map Map { get; set; }
        public List<Team> Teams { get; set; }
        public List<Player> Players { get; set; }
        public List<string> UnparsedData { get; set; }
        public List<string> UnparsedOuterData { get; set; }

        public Team GetTeamById(int teamId)
        {
            foreach (Team t in Teams)
                if (t.TeamId == teamId)
                    return t;
            return null;
        }
    }
}
