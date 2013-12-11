using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public class BalanceReport
    {
        public BalanceReport() { PlayerData = new List<BalanceReportItem>(); }

        public List<BalanceReportItem> PlayerData { get; set; }
        public double Unfairness { get; set; }
    }

    public class BalanceReportItem
    {
        public BalanceReportItem()
        {
            HealthResourceCount = new Dictionary<string, int>();
            FoodResourceCount = new Dictionary<string, int>();
            StrategicResourceCount = new Dictionary<string, int>();
            HappyResourceCount = new Dictionary<string, int>();
        }

        public int Player { get; set; }
        public int LandTileCount { get; set; }
        public int CoastalTileCount { get; set; }
        public int OceanTileCount { get; set; }
        public int GrasslandCount { get; set; }
        public int PlainsCount { get; set; }
        public int DesertCount { get; set; }
        public int TundraCount { get; set; }
        public int SnowCount { get; set; }
        public int ForestCount { get; set; }
        public int JungleCount { get; set; }
        public int OasisCount { get; set; }
        public int FloodPlainsCount { get; set; }
        public int HillsCount { get; set; }
        public double TotalLandQuality { get; set; }
        public double TotalFoodPotential { get; set; }
        public double TotalCommercePotential { get; set; }
        public double TotalHammerPotential { get; set; }
        public Dictionary<string, int> HealthResourceCount { get; set; }
        public Dictionary<string, int> FoodResourceCount { get; set; }
        public Dictionary<string, int> StrategicResourceCount { get; set; }
        public Dictionary<string, int> HappyResourceCount { get; set; }
        public void CountBonus(BonusTypes bonus, Dictionary<string, int> counterSet)
        {
            string bonusName = bonus.ToString();
            if (counterSet.ContainsKey(bonusName))
                counterSet[bonusName] = counterSet[bonusName] + 1;
            else
                counterSet[bonusName] = 1;
        }
        public string ToString(Game game)
        {
            Player p = game.Players[Player];
            return string.Format("Player {0}\n{9}\n{1} land tiles.\n({10} grass, {11} plains, {12} deserts, {13} tundra, {14} snow. {15} forests, {16} jungles, {17} flood plains, {18} oasis. {22} hills.)\n{23:0.0} total land quality.\n{25:0.00} average land quality.\n{4:0.0} total food potential.\n{19:0.00} food per non-ocean tile.\n{20:0.0} total hammer potential.\n{21:0.00} hammers per non-ocean tile.\n{24:0.0} total commerce potential.\n{2} coastal tiles.\n{3} ocean tiles.\n\n{5}\n{6}\n{7}\n{8}",
                this.Player, this.LandTileCount, this.CoastalTileCount, this.OceanTileCount, this.TotalFoodPotential,
                GetCounterSetString("strategic resources", StrategicResourceCount),
                GetCounterSetString("happy resources", HappyResourceCount),
                GetCounterSetString("food resources", FoodResourceCount),
                GetCounterSetString("health resources", HealthResourceCount),
                p.ToString(),
                GrasslandCount, PlainsCount, DesertCount, TundraCount, SnowCount, ForestCount, JungleCount, FloodPlainsCount, OasisCount,
                (double)TotalFoodPotential / (double)(LandTileCount + CoastalTileCount),
                TotalHammerPotential,
                (double)TotalHammerPotential / (double)(LandTileCount + CoastalTileCount),
                HillsCount, TotalLandQuality, TotalCommercePotential, (double)TotalLandQuality / (double)(LandTileCount + CoastalTileCount));
        }
        private string GetCounterSetString(string name, Dictionary<string, int> counterSet)
        {
            int totalCount = 0;
            string result = "";
            foreach (string bonusName in counterSet.Keys)
            {
                int c = counterSet[bonusName];
                totalCount += c;
                if (result.Length > 0) result += ", ";
                result += c + " " + bonusName.Replace("BONUS_", "").ToLower();
            }
            if(totalCount > 0)
                result = totalCount + " " + name + " (" + result + ").";
            else
                result = totalCount + " " + name + ".";
            return result;
        }
    }
}
