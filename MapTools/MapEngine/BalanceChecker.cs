using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MapEngine
{
    public class BalanceCheckerSettings
    {
        public bool IncludeWater { get; set; }
        public bool TraverseMainlandFirst { get; set; }
        public bool IncludeIslands { get; set; }
        public bool UseTraversalCosts { get; set; }
        public double FoodWeight { get; set; }
        public double HammerWeight { get; set; }
        public double BaseCost { get; set; }
        public double WaterCost { get; set; }
        public double JunglePenalizer { get; set; }

        public BalanceCheckerSettings()
        {
            IncludeWater = true;
            IncludeIslands = true;
            TraverseMainlandFirst = true;
            UseTraversalCosts = true;
            FoodWeight = 3;
            HammerWeight = 1;
            BaseCost = 100;
            WaterCost = 100;
            JunglePenalizer = 3;
        }
    }

    public class BalanceChecker
    {
        private class TileTraversal : IComparable<TileTraversal>
        {
            public Tile Tile { get; set; }
            public double AccumulatedCost { get; set; }

            public TileTraversal(Tile t, double c)
            {
                Tile = t;
                AccumulatedCost = c;
            }

            public int CompareTo(TileTraversal other)
            {
                return this.AccumulatedCost.CompareTo(other.AccumulatedCost);
            }
        }

        public static BalanceReport CheckBalance(Map map, BalanceCheckerSettings settings, MapState mapState)
        {
            PrioQueue<TileTraversal, double> mainlandQueue = new PrioQueue<TileTraversal, double>();
            PrioQueue<TileTraversal, double> offshoreQueue = new PrioQueue<TileTraversal, double>();
            if (!settings.TraverseMainlandFirst) offshoreQueue = mainlandQueue;
            
            foreach (Tile tt in map.GetAllTiles())
            {
                tt.ClosestPlayer = -1;
                tt.AccumulatedTraversalCost = 0;
                if (!settings.UseTraversalCosts)
                    tt.TraversalCost = 1;
                else
                {
                    tt.TraversalCost = settings.WaterCost;
                    if (tt.IsTraversableByLand())
                    {
                        foreach (Tile t in map.GetBigFatCross(tt))
                        {
                            double c = t.GetLandQuality(map);
                            //c = t.GetFoodPotential(map) * settings.FoodWeight + t.GetHammerPotential() * settings.HammerWeight;
                            //if (t.FeatureType == FeatureTypes.FEATURE_JUNGLE) c /= settings.JunglePenalizer;
                            tt.TraversalCost += c;
                        }
                        if (tt.TraversalCost == 0)
                            tt.TraversalCost = settings.BaseCost;
                        else
                            tt.TraversalCost = settings.BaseCost / tt.TraversalCost;
                    }
                }
                if (tt.IsTraversableByLand())
                {
                    if (mapState != null)
                    {
                        foreach (var ps in mapState.PlayerStarts)
                            if (ps.StartX == tt.X && ps.StartY == tt.Y)
                                tt.ClosestPlayer = ps.PlayerId;
                    }
                    else
                    {
                        foreach (Unit u in tt.Units)
                            if (u.UnitOwner != Game.BarbarianPlayerId)
                                tt.ClosestPlayer = u.UnitOwner;
                    }
                    if (tt.ClosestPlayer >= 0)
                        mainlandQueue.Enqueue(new TileTraversal(tt, 0), 0);
                }
            }
            while(!mainlandQueue.IsEmpty())
            {
                TileTraversal t = mainlandQueue.Dequeue();
                VisitNeighbours(map, t, settings.IncludeWater, true, mainlandQueue, offshoreQueue);
            }
            while (!offshoreQueue.IsEmpty())
            {
                TileTraversal t = offshoreQueue.Dequeue();
                VisitNeighbours(map, t, true, settings.IncludeIslands, offshoreQueue, offshoreQueue);
            }
            return CreateReport(map);
        }

        private static void VisitNeighbours(Map map, TileTraversal tt, bool includeWater, bool includeLand, PrioQueue<TileTraversal, double> landQueue, PrioQueue<TileTraversal, double> waterQueue)
        {
            Tile t = tt.Tile;
            foreach (Tile n in map.GetNeighbours(t, !t.IsWater()))
            {
                if (n.ClosestPlayer != -1) continue;
                if (includeLand && n.IsTraversableByLand())
                {
                    n.ClosestPlayer = t.ClosestPlayer;
                    TileTraversal nt = new TileTraversal(n, tt.AccumulatedCost + n.TraversalCost);
                    n.AccumulatedTraversalCost = nt.AccumulatedCost;
                    landQueue.Enqueue(nt, nt.AccumulatedCost);
                }
                else if (includeWater && n.IsTraversableByWater())
                {
                    n.ClosestPlayer = t.ClosestPlayer;
                    TileTraversal nt = new TileTraversal(n, tt.AccumulatedCost + n.TraversalCost);
                    n.AccumulatedTraversalCost = nt.AccumulatedCost;
                    waterQueue.Enqueue(nt, nt.AccumulatedCost);
                }
            }
        }

        private static BalanceReport CreateReport(Map map)
        {
            BalanceReport report = new BalanceReport();
            double maxTraversalCost = 0;
            foreach(Tile p in map.GetAllTiles())
                if(p.IsTraversableByLand() && p.AccumulatedTraversalCost > maxTraversalCost)
                    maxTraversalCost = p.AccumulatedTraversalCost;
            if(maxTraversalCost == 0) throw new Exception("Max traversal cost is 0 - logical error in Balance Report calculation.");
            foreach (Tile p in map.GetAllTiles())
            {
                if (p.ClosestPlayer >= 0)
                {
                    while (report.PlayerData.Count <= p.ClosestPlayer)
                        report.PlayerData.Add(new BalanceReportItem() { Player = report.PlayerData.Count });
                    BalanceReportItem reportItem = report.PlayerData[p.ClosestPlayer];

                    TileYield y = p.GetPotentialYield(map);
                    reportItem.TotalFoodPotential += y.Food;
                    reportItem.TotalHammerPotential += y.Hammers;
                    reportItem.TotalCommercePotential += y.Commerce;
                    double distanceFactor = ((maxTraversalCost * 1.125) / (p.AccumulatedTraversalCost + maxTraversalCost * 0.125));
                    reportItem.TotalLandQuality += p.GetLandQuality(map)*distanceFactor;

                    if (p.Terrain == TerrainTypes.TERRAIN_COAST)
                        reportItem.CoastalTileCount++;
                    else if (p.Terrain == TerrainTypes.TERRAIN_OCEAN)
                        reportItem.OceanTileCount++;
                    else
                        reportItem.LandTileCount++;

                    if (p.Terrain == TerrainTypes.TERRAIN_GRASS)
                        reportItem.GrasslandCount++;
                    else if (p.Terrain == TerrainTypes.TERRAIN_DESERT)
                        reportItem.DesertCount++;
                    else if (p.Terrain == TerrainTypes.TERRAIN_PLAINS)
                        reportItem.PlainsCount++;
                    else if (p.Terrain == TerrainTypes.TERRAIN_TUNDRA)
                        reportItem.TundraCount++;
                    else if (p.Terrain == TerrainTypes.TERRAIN_SNOW)
                        reportItem.SnowCount++;

                    if (p.PlotType == PlotTypes.HILL)
                        reportItem.HillsCount++;
                    if (p.FeatureType == FeatureTypes.FEATURE_FLOOD_PLAINS)
                        reportItem.FloodPlainsCount++;
                    else if (p.FeatureType == FeatureTypes.FEATURE_OASIS)
                        reportItem.OasisCount++;
                    else if (p.FeatureType == FeatureTypes.FEATURE_JUNGLE)
                        reportItem.JungleCount++;
                    else if (p.FeatureType == FeatureTypes.FEATURE_FOREST)
                        reportItem.ForestCount++;

                    if (p.IsFoodBonus()) reportItem.CountBonus(p.BonusType, reportItem.FoodResourceCount);
                    if (p.IsHappyBonus()) reportItem.CountBonus(p.BonusType, reportItem.HappyResourceCount);
                    if (p.IsHealthBonus()) reportItem.CountBonus(p.BonusType, reportItem.HealthResourceCount);
                    if (p.IsStrategicBonus()) reportItem.CountBonus(p.BonusType, reportItem.StrategicResourceCount);
                }
            }
            // Calculate unfairness as standard deviation - root of sum of squares of deviation from average land quality.
            report.Unfairness = 0;
            if (report.PlayerData.Count > 0)
            {
                double sumOfSquares = 0;
                double avgQuality = 0;
                foreach (var player in report.PlayerData)
                    avgQuality += player.TotalLandQuality;
                avgQuality /= report.PlayerData.Count;
                foreach (var player in report.PlayerData)
                    sumOfSquares += (player.TotalLandQuality - avgQuality) * (player.TotalLandQuality - avgQuality);
                report.Unfairness = Math.Sqrt(sumOfSquares);
            }
            return report;
        }

    }
}
