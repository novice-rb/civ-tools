using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    [Serializable]
    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    [Serializable]
    public class Selection : List<Coordinate>
    {
        public bool ContainsTile(Tile t)
        {
            foreach (var c in this)
                if (c.X == t.X && c.Y == t.Y)
                    return true;
            return false;
        }
    }

    [Serializable]
    public class Tile : ICloneable
    {
        public Tile()
        {
            Units = new List<Unit>();
            UnparsedData = new List<string>();
        }

        public Coordinate GetCoordinate()
        {
            return new Coordinate() { X = this.X, Y = this.Y };
        }

        public List<string> UnparsedData { get; set; }
        public int TraversalFlag { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public TerrainTypes Terrain { get; set; }
        public FeatureTypes FeatureType { get; set; }
        public int FeatureVariety { get; set; }
        public PlotTypes PlotType { get; set; }
        public BonusTypes BonusType { get; set; }
        public List<Unit> Units { get; set; }
        public int ClosestPlayer { get; set; }
        public double TraversalCost { get; set; }
        public double AccumulatedTraversalCost { get; set; }
        public int ContinentId { get; set; }
        public FreshWaterTypes FreshWaterType { get; set; }
        public IrrigationStatus IrrigationStatus { get; set; }
        //public bool Selected { get; set; }
        public bool IsEmpty { get; set; }
        public bool WaterBodyContainsResources { get; set; }
        public bool WaterBodySupportsShips { get; set; }
        public int WaterBodySize { get; set; }

        public RiverDirection RiverWEDirection { get; set; }
        public RiverDirection RiverNSDirection { get; set; }
        public bool IsNOfRiver { get; set; }
        public bool IsWOfRiver { get; set; }

        public bool IsResource()
        {
            return BonusType != BonusTypes.BONUS_NONE;
        }

        public bool IsRiverSide(Map map)
        {
            return map.IsRiverSide(X, Y);
        }

        public bool IsIrrigated(Map map)
        {
            return IrrigationStatus == IrrigationStatus.IRRIGATED || IsRiverSide(map);
        }

        public bool IsFreshWater()
        {
            return FreshWaterType == FreshWaterTypes.FRESHWATER;
        }

        public bool IsWater()
        {
            return (Terrain == TerrainTypes.TERRAIN_COAST || Terrain == TerrainTypes.TERRAIN_OCEAN);
        }

        public bool IsTraversableByLand()
        {
            return (PlotType != PlotTypes.PEAK && PlotType != PlotTypes.WATER && !IsWater());
        }

        public bool IsTraversableByWater()
        {
            return (IsWater() && FeatureType != FeatureTypes.FEATURE_ICE);
        }

        private static BonusTypes[] _FoodBonusTypes = { BonusTypes.BONUS_BANANA, BonusTypes.BONUS_CORN, BonusTypes.BONUS_COW, BonusTypes.BONUS_CLAM, BonusTypes.BONUS_CRAB, BonusTypes.BONUS_DEER, BonusTypes.BONUS_FISH, BonusTypes.BONUS_PIG, BonusTypes.BONUS_RICE, BonusTypes.BONUS_SHEEP, BonusTypes.BONUS_SUGAR, BonusTypes.BONUS_WHALE, BonusTypes.BONUS_WHEAT };
        public bool IsFoodBonus()
        {
            return _FoodBonusTypes.Contains(BonusType);
        }

        private static BonusTypes[] _HappyBonusTypes = { BonusTypes.BONUS_DRAMA, BonusTypes.BONUS_DYE, BonusTypes.BONUS_FUR, BonusTypes.BONUS_GEMS, BonusTypes.BONUS_GOLD, BonusTypes.BONUS_INCENSE, BonusTypes.BONUS_IVORY, BonusTypes.BONUS_MOVIES, BonusTypes.BONUS_MUSIC, BonusTypes.BONUS_SILK, BonusTypes.BONUS_SILVER, BonusTypes.BONUS_SPICES, BonusTypes.BONUS_SUGAR, BonusTypes.BONUS_WHALE, BonusTypes.BONUS_WINE };
        public bool IsHappyBonus()
        {
            return _HappyBonusTypes.Contains(BonusType);
        }

        private static BonusTypes[] _StrategicBonusTypes = { BonusTypes.BONUS_ALUMINUM, BonusTypes.BONUS_COAL, BonusTypes.BONUS_COPPER, BonusTypes.BONUS_HORSE, BonusTypes.BONUS_IRON, BonusTypes.BONUS_IVORY, BonusTypes.BONUS_MARBLE, BonusTypes.BONUS_OIL, BonusTypes.BONUS_STONE, BonusTypes.BONUS_URANIUM };
        public bool IsStrategicBonus()
        {
            return _StrategicBonusTypes.Contains(BonusType);
        }

        private static BonusTypes[] _HealthBonusTypes = { BonusTypes.BONUS_BANANA, BonusTypes.BONUS_CLAM, BonusTypes.BONUS_CORN, BonusTypes.BONUS_COW, BonusTypes.BONUS_CRAB, BonusTypes.BONUS_DEER, BonusTypes.BONUS_FISH, BonusTypes.BONUS_PIG, BonusTypes.BONUS_RICE, BonusTypes.BONUS_SHEEP, BonusTypes.BONUS_SPICES, BonusTypes.BONUS_SUGAR, BonusTypes.BONUS_WHEAT, BonusTypes.BONUS_WINE };
        public bool IsHealthBonus()
        {
            return _HealthBonusTypes.Contains(BonusType);
        }

        public double GetLandQuality(Map map)
        {
            TileYield y = GetPotentialYield(map);
            double q = y.Hammers * 5 + y.Food * 8 + y.Commerce * 3;
            if (FeatureType == FeatureTypes.FEATURE_FOREST)
                q += 3;
            if (FeatureType == FeatureTypes.FEATURE_JUNGLE)
                q -= 3;
            q = Math.Max(0, q - 16);
            return q;
        }

        public TileYield GetPotentialYield(Map map)
        {
            TileYield y = new TileYield();
            if (Terrain == TerrainTypes.TERRAIN_SNOW) return y;
            if (FeatureType == FeatureTypes.FEATURE_FALLOUT) return y;
            if (FeatureType == FeatureTypes.FEATURE_ICE) return y;
            if (PlotType == PlotTypes.PEAK) return y;
            switch (Terrain)
            {
                case TerrainTypes.TERRAIN_COAST:
                    y.Commerce = 2;
                    if (IsFreshWater())
                        y.Food = 2;
                    else
                        y.Food = 1.5; // Assume half a lighthouse
                    break;
                case TerrainTypes.TERRAIN_OCEAN:
                    y.Commerce = 1;
                    if (IsFreshWater())
                        y.Food = 2;
                    else
                        y.Food = 1.5; // Assume half a lighthouse
                    break;
                case TerrainTypes.TERRAIN_PLAINS:
                    y.Hammers = 1;
                    y.Food = 1;
                    break;
                case TerrainTypes.TERRAIN_TUNDRA:
                    y.Food = 1;
                    break;
                case TerrainTypes.TERRAIN_GRASS:
                    y.Food = 2;
                    break;
            }
            if (IsRiverSide(map))
                y.Commerce += 1;
            if ((Terrain == TerrainTypes.TERRAIN_PLAINS || Terrain == TerrainTypes.TERRAIN_TUNDRA || Terrain == TerrainTypes.TERRAIN_GRASS)
                && PlotType == PlotTypes.FLAT && IsIrrigated(map) &&
                (BonusType == BonusTypes.BONUS_NONE || BonusType == BonusTypes.BONUS_CORN || BonusType == BonusTypes.BONUS_RICE || BonusType == BonusTypes.BONUS_WHEAT))
                y.Food += 1; // Extra food from irrigated farm
            if ((Terrain == TerrainTypes.TERRAIN_PLAINS || Terrain == TerrainTypes.TERRAIN_GRASS)
                && PlotType == PlotTypes.FLAT && !IsIrrigated(map) && BonusType == BonusTypes.BONUS_NONE)
                y.Commerce += 2.5; // Commerce from cottage
            if (PlotType == PlotTypes.HILL)
            {
                y.Food = Math.Max(0, y.Food - 1);
                y.Hammers += 3; // Hammers from hill + mine
                if ((Terrain == TerrainTypes.TERRAIN_SNOW) || (Terrain == TerrainTypes.TERRAIN_TUNDRA) || (Terrain == TerrainTypes.TERRAIN_DESERT))
                    y.Hammers -= 1;
            }
            if (FeatureType == FeatureTypes.FEATURE_FLOOD_PLAINS || FeatureType == FeatureTypes.FEATURE_OASIS)
                y.Food += 3;
            switch (BonusType)
            {
                case BonusTypes.BONUS_DYE:
                    y.Commerce += 5;
                    break;
                case BonusTypes.BONUS_FUR:
                case BonusTypes.BONUS_SILK:
                    y.Commerce += 4;
                    break;
                case BonusTypes.BONUS_INCENSE:
                    y.Commerce += 6;
                    break;
                case BonusTypes.BONUS_SPICES:
                    y.Commerce += 3;
                    y.Food += 1;
                    break;
                case BonusTypes.BONUS_ALUMINUM:
                case BonusTypes.BONUS_COAL:
                case BonusTypes.BONUS_COPPER:
                case BonusTypes.BONUS_IRON:
                    if (PlotType == PlotTypes.HILL)
                        y.Hammers += 2;
                    else
                        y.Hammers += 4;
                    break;
                case BonusTypes.BONUS_SILVER:
                    y.Commerce += 5;
                    if (PlotType == PlotTypes.HILL)
                        y.Hammers -= 1;
                    else
                        y.Hammers += 1;
                    break;
                case BonusTypes.BONUS_GEMS:
                    y.Commerce += 6;
                    if (PlotType == PlotTypes.HILL)
                        y.Hammers -= 1;
                    else
                        y.Hammers += 1;
                    break;
                case BonusTypes.BONUS_GOLD:
                    y.Commerce += 7;
                    if (PlotType == PlotTypes.HILL)
                        y.Hammers -= 1;
                    else
                        y.Hammers += 1;
                    break;
                case BonusTypes.BONUS_BANANA:
                    y.Food += 3;
                    break;
                case BonusTypes.BONUS_CRAB:
                case BonusTypes.BONUS_CLAM:
                    y.Food += 3;
                    break;
                case BonusTypes.BONUS_FISH:
                    y.Food += 4;
                    break;
                case BonusTypes.BONUS_WHEAT:
                case BonusTypes.BONUS_CORN:
                    y.Food += 3;
                    break;
                case BonusTypes.BONUS_COW:
                    y.Food += 2;
                    y.Hammers += 2;
                    break;
                case BonusTypes.BONUS_IVORY:
                    y.Hammers += 2;
                    y.Commerce += 1;
                    break;
                case BonusTypes.BONUS_MARBLE:
                    y.Hammers += 2;
                    y.Commerce += 2;
                    break;
                case BonusTypes.BONUS_URANIUM:
                    y.Hammers += 2;
                    y.Commerce += 3;
                    break;
                case BonusTypes.BONUS_SHEEP:
                    y.Food += 3;
                    y.Commerce += 1;
                    break;
                case BonusTypes.BONUS_DEER:
                    y.Food += 3;
                    break;
                case BonusTypes.BONUS_PIG:
                    y.Food += 4;
                    break;
                case BonusTypes.BONUS_RICE:
                    y.Food += 2;
                    break;
                case BonusTypes.BONUS_SUGAR:
                    y.Food += 2;
                    y.Commerce += 1;
                    break;
                case BonusTypes.BONUS_WHALE:
                    y.Hammers += 1;
                    y.Food += 1;
                    y.Commerce += 2;
                    break;
                case BonusTypes.BONUS_HORSE:
                case BonusTypes.BONUS_OIL:
                    y.Commerce += 1;
                    y.Hammers += 3;
                    break;
                case BonusTypes.BONUS_STONE:
                    y.Hammers += 3;
                    break;
                default:
                    y.Food += 0;
                    break;
            }
            return y;
        }

        #region ICloneable Members

        public object Clone()
        {
            Tile t = (Tile)MemberwiseClone();
            t.Units = new List<Unit>();
            foreach (Unit u in this.Units)
                t.Units.Add((Unit)u.Clone());
            t.UnparsedData = new List<string>(this.UnparsedData);
            return t;
        }

        #endregion
    }

    public enum FreshWaterTypes
    {
        NOT_CALCULATED = 0,
        FRESHWATER = 1,
        SALTWATER = 2
    }

    public enum IrrigationStatus
    {
        NOT_CALCULATED = 0,
        IRRIGATED = 1,
        NOT_IRRIGATED = 2
    }

    public enum PlotTypes
    {
        PEAK = 0,
        HILL = 1,
        FLAT = 2,
        WATER = 3
    }

    public enum TerrainTypes
    {
        TERRAIN_NONE = 0,
        TERRAIN_TUNDRA = 1,
        TERRAIN_DESERT = 2,
        TERRAIN_GRASS = 3,
        TERRAIN_PLAINS = 4,
        TERRAIN_COAST = 5,
        TERRAIN_OCEAN = 6,
        TERRAIN_SNOW = 7
    }

    public enum FeatureTypes
    {
        FEATURE_NONE = 0,
        FEATURE_FOREST = 1,
        FEATURE_JUNGLE = 2,
        FEATURE_OASIS = 3,
        FEATURE_FLOOD_PLAINS = 4,
        FEATURE_ICE = 5,
        FEATURE_FALLOUT = 6
    }

    public enum BonusTypes
    {
        BONUS_NONE = 0,
        BONUS_HORSE = 1,
        BONUS_FISH = 2,
        BONUS_CLAM = 3,
        BONUS_CRAB = 4,
        BONUS_CORN = 5,
        BONUS_WHEAT = 6,
        BONUS_RICE = 7,
        BONUS_COPPER = 8,
        BONUS_IRON = 9,
        BONUS_ALUMINUM = 10,
        BONUS_COAL = 11,
        BONUS_MARBLE = 12,
        BONUS_OIL = 13,
        BONUS_STONE = 14,
        BONUS_URANIUM = 15,
        BONUS_BANANA = 16,
        BONUS_COW = 17,
        BONUS_DEER = 18,
        BONUS_PIG = 19,
        BONUS_SHEEP = 20,
        BONUS_DYE = 21,
        BONUS_FUR = 22,
        BONUS_GEMS = 23,
        BONUS_GOLD = 24,
        BONUS_INCENSE = 25,
        BONUS_IVORY = 26,
        BONUS_SILK = 27,
        BONUS_SILVER = 28,
        BONUS_SPICES = 29,
        BONUS_SUGAR = 30,
        BONUS_WINE = 31,
        BONUS_WHALE = 32,
        BONUS_DRAMA = 33,
        BONUS_MUSIC = 34,
        BONUS_MOVIES = 35
    }

    public enum RiverDirection
    {
        NORTH_TO_SOUTH = 0,
        EAST_TO_WEST= 1,
        SOUTH_TO_NORTH = 2,
        WEST_TO_EAST = 3
    }

    public class TileYield
    {
        public double Food { get; set; }
        public double Hammers { get; set; }
        public double Commerce { get; set; }
    }
}
