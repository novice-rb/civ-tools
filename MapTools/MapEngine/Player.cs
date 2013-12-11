using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public class Player
    {
        public PlayerColors Color { get; set; }
        public int PlayerId { get; set; }
	    public int TeamId { get; set; }
	    public string LeaderType { get; set; }
	    public string CivType { get; set; }
        public List<string> UnparsedData { get; set; }

        public bool IsActive() { return LeaderType != "NONE"; }

        public Player()
        {
            UnparsedData = new List<string>();
        }

        public override string ToString()
        {
            return ToCamelCase(LeaderType.Replace("LEADER_", "")) + " of " + ToCamelCase(CivType.Replace("CIVILIZATION_", ""));
        }

        public static string ToCamelCase(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }
    }

    public class Team
    {
        public Team()
        {
            UnparsedData = new List<string>();
        }

        public int TeamId { get; set; }
        public List<string> UnparsedData { get; set; }
    }

    public enum PlayerColors
    {
        NONE = 0,
        PLAYERCOLOR_BLACK,
        PLAYERCOLOR_BLUE,
        PLAYERCOLOR_LIGHT_BROWN,
        PLAYERCOLOR_BROWN,
        PLAYERCOLOR_CYAN,
        PLAYERCOLOR_MIDDLE_CYAN,
        PLAYERCOLOR_DARK_BLUE,
        PLAYERCOLOR_DARK_CYAN,
        PLAYERCOLOR_DARK_GREEN,
        PLAYERCOLOR_DARK_DARK_GREEN,
        PLAYERCOLOR_DARK_PINK,
        PLAYERCOLOR_LIGHT_PURPLE,
        PLAYERCOLOR_MIDDLE_PURPLE,
        PLAYERCOLOR_DARK_PURPLE,
        PLAYERCOLOR_DARK_INDIGO,
        PLAYERCOLOR_DARK_RED,
        PLAYERCOLOR_DARK_YELLOW,
        PLAYERCOLOR_DARK_GRAY,
        PLAYERCOLOR_GRAY,
        PLAYERCOLOR_LIGHT_GREEN,
        PLAYERCOLOR_MIDDLE_GREEN,
        PLAYERCOLOR_GREEN,
        PLAYERCOLOR_DARK_ORANGE,
        PLAYERCOLOR_ORANGE,
        PLAYERCOLOR_LIGHT_ORANGE,
        PLAYERCOLOR_PEACH,
        PLAYERCOLOR_PINK,
        PLAYERCOLOR_PURPLE,
        PLAYERCOLOR_RED,
        PLAYERCOLOR_WHITE,
        PLAYERCOLOR_LIGHT_BLUE,
        PLAYERCOLOR_LIGHT_YELLOW,
        PLAYERCOLOR_YELLOW,
        PLAYERCOLOR_DARK_LEMON,
        PLAYERCOLOR_GOLDENROD,
        PLAYERCOLOR_MIDDLE_BLUE,
        PLAYERCOLOR_MAROON,
        PLAYERCOLOR_PALE_RED,
        PLAYERCOLOR_PALE_ORANGE,
        PLAYERCOLOR_LIGHT_BLACK,
    }
}
