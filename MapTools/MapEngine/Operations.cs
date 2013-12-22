using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public abstract class MapLayerOperation
    {
        public abstract Map Execute(Map map, Selection selection);
        public virtual Selection CreateNewSelection(Selection selection)
        {
            return selection;
        }
        public abstract string GetName();
    }

    public abstract class MapSelectOperation
    {
        public abstract Selection Execute(Map map, Selection selection);
        public abstract string GetName();
    }

    public class RotateAroundCornerOperation : MapLayerOperation
    {
        public bool TopCorner { get; set; }
        public bool LeftCorner { get; set; }

        public override Map Execute(Map map, Selection selection)
        {
            return RotateAroundCorner(map, TopCorner, LeftCorner);
        }

        public static Map RotateAroundCorner(Map map, bool topCorner, bool leftCorner)
        {
            if (map.Width != map.Height) map = ResizeOperation.Resize(map, new ResizeOperation() 
            {
                Width = Math.Min(map.Width, map.Height),
                Height = Math.Min(map.Width, map.Height),
                TerrainType = TerrainTypes.TERRAIN_GRASS,
                PlotType = PlotTypes.FLAT,
                HorAlign = HorizontalNeighbour.None,
                VerAlign = VerticalNeighbour.None
            });
            Map topLeft = null, topRight = null, bottomLeft = null, bottomRight = null;
            if (leftCorner)
            {
                if (topCorner)
                {
                    // Rotate around top left corner
                    bottomRight = map;
                    bottomLeft = RotateOperation.RotateCW(bottomRight);
                    topLeft = RotateOperation.RotateCW(bottomLeft);
                    topRight = RotateOperation.RotateCW(topLeft);
                }
                else
                {
                    // Rotate around bottom left corner
                    topRight = map;
                    bottomRight = RotateOperation.RotateCW(topRight);
                    bottomLeft = RotateOperation.RotateCW(bottomRight);
                    topLeft = RotateOperation.RotateCW(bottomLeft);
                }
            }
            else
            {
                if (topCorner)
                {
                    // Rotate around top right corner
                    bottomLeft = map;
                    topLeft = RotateOperation.RotateCW(bottomLeft);
                    topRight = RotateOperation.RotateCW(topLeft);
                    bottomRight = RotateOperation.RotateCW(topRight);
                }
                else
                {
                    // Rotate around bottom right corner
                    topLeft = map;
                    topRight = RotateOperation.RotateCW(topLeft);
                    bottomRight = RotateOperation.RotateCW(topRight);
                    bottomLeft = RotateOperation.RotateCW(bottomRight);
                }
            }
            Map r = (Map)map.Clone();
            r.SetDimensions(map.Width * 2, map.Height * 2);
            r.SetTiles(bottomRight, map.Width, 0);
            r.SetTiles(bottomLeft, 0, 0);
            r.SetTiles(topLeft, 0, map.Height);
            r.SetTiles(topRight, map.Width, map.Height);
            return r;
        }

        public override string GetName()
        {
            return "Rotate around " + (TopCorner ? "top" : "bottom") + " " + (LeftCorner ? "left" : "right") + " corner";
        }
    }

    public class RotateOperation : MapLayerOperation
    {
        public bool Clockwise { get; set; }

        public override Map Execute(Map map, Selection selection)
        {
            if (Clockwise)
                return RotateCW(map);
            else
                return RotateCCW(map);
        }

        public static Map RotateCCW(Map thisMap)
        {
            Map map = new Map();
            map.SetDimensions(thisMap.Height, thisMap.Width);
            map.IsHorizontalWrap = thisMap.IsVerticalWrap;
            map.IsVerticalWrap = thisMap.IsHorizontalWrap;
            map.UnparsedData = new List<string>(thisMap.UnparsedData);
            foreach (Tile t in thisMap.GetAllTiles())
            {
                map.SetTile(map.Width - 1 - t.Y, t.X, (Tile)t.Clone());
            }
            Map map2 = (Map)map.Clone();
            foreach (Tile t in map2.GetAllTiles())
            {
                t.IsWOfRiver = false;
                t.IsNOfRiver = false;
            }
            foreach (Tile t in map.GetAllTiles())
            {
                if (t.IsNOfRiver)
                {
                    Tile tt = map2.GetTile(t.X, t.Y);
                    tt.IsWOfRiver = true;
                    if (t.RiverWEDirection == RiverDirection.WEST_TO_EAST)
                        tt.RiverNSDirection = RiverDirection.SOUTH_TO_NORTH;
                    else
                        tt.RiverNSDirection = RiverDirection.NORTH_TO_SOUTH;
                }
                if (t.IsWOfRiver)
                {
                    Tile n = map2.GetNeighbour(t.X, t.Y, HorizontalNeighbour.None, VerticalNeighbour.North);
                    if (n != null)
                    {
                        n.IsNOfRiver = true;
                        if (t.RiverNSDirection == RiverDirection.NORTH_TO_SOUTH)
                            n.RiverWEDirection = RiverDirection.WEST_TO_EAST;
                        else
                            n.RiverWEDirection = RiverDirection.EAST_TO_WEST;
                    }
                }
            }
            return map2;
        }

        public static Map RotateCW(Map thisMap)
        {
            Map map = new Map();
            map.SetDimensions(thisMap.Height, thisMap.Width);
            map.IsHorizontalWrap = thisMap.IsVerticalWrap;
            map.IsVerticalWrap = thisMap.IsHorizontalWrap;
            map.UnparsedData = new List<string>(thisMap.UnparsedData);
            foreach (Tile t in thisMap.GetAllTiles())
            {
                map.SetTile(t.Y, map.Height - 1 - t.X, (Tile)t.Clone());
            }
            Map map2 = (Map)map.Clone();
            foreach (Tile t in map2.GetAllTiles())
            {
                t.IsWOfRiver = false;
                t.IsNOfRiver = false;
            }
            foreach (Tile t in map.GetAllTiles())
            {
                if (t.IsNOfRiver)
                {
                    Tile w = map2.GetNeighbour(t.X, t.Y, HorizontalNeighbour.West, VerticalNeighbour.None);
                    if (w != null)
                    {
                        w.IsWOfRiver = true;
                        if (t.RiverWEDirection == RiverDirection.WEST_TO_EAST)
                            w.RiverNSDirection = RiverDirection.NORTH_TO_SOUTH;
                        else
                            w.RiverNSDirection = RiverDirection.SOUTH_TO_NORTH;
                    }
                }
                if (t.IsWOfRiver)
                {
                    Tile tt = map2.GetTile(t.X, t.Y);
                    tt.IsNOfRiver = true;
                    if (t.RiverNSDirection == RiverDirection.NORTH_TO_SOUTH)
                        tt.RiverWEDirection = RiverDirection.EAST_TO_WEST;
                    else
                        tt.RiverWEDirection = RiverDirection.WEST_TO_EAST;
                }
            }
            return map2;
        }

        public override string GetName()
        {
            return Clockwise ? "Rotate clockwise" : "Rotate counter-clockwise";
        }
    }

    public class ResizeOperation : MapLayerOperation
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public TerrainTypes TerrainType { get; set; }
        public PlotTypes PlotType { get; set; }
        public HorizontalNeighbour HorAlign { get; set; }
        public VerticalNeighbour VerAlign { get; set; }

        public override Map Execute(Map map, Selection selection)
        {
            return Resize(map, this);
        }

        public static Map Resize(Map thisMap, ResizeOperation parameters) {
            Map map = (Map)thisMap.Clone();
            map.SetDimensions(parameters.Width, parameters.Height);
            for (int x = 0; x < parameters.Width; x++)
            {
                for (int y = 0; y < parameters.Height; y++)
                {
                    Tile t = new Tile();
                    t.PlotType = parameters.PlotType;
                    t.Terrain = parameters.TerrainType;
                    map.SetTile(x, y, t);
                }
            }
            int minX = 0; // west alignment
            int minY = 0; // south alignment
            if (parameters.HorAlign == HorizontalNeighbour.None) // center alignment
                minX = (thisMap.Width - parameters.Width) / 2;
            else if (parameters.HorAlign == HorizontalNeighbour.East)
                minX = thisMap.Width - parameters.Width;
            if (parameters.VerAlign == VerticalNeighbour.None) // center alignment
                minY = (thisMap.Height - parameters.Height) / 2;
            else if (parameters.VerAlign == VerticalNeighbour.North)
                minY = thisMap.Height - parameters.Height;
            for (int x = minX; x < minX + parameters.Width; x++)
            {
                for (int y = minY; y < minY + parameters.Height; y++)
                {
                    Tile t = (Tile)thisMap.GetTileAcrossWrap(x, y, false, false);
                    if(t != null)
                        map.SetTile(x - minX, y - minY, t);
                }
            }
            return map;
        }

        public override string GetName()
        {
            return "Resize to " + Width + "x" + Height;
        }
    }

    public class MirrorOperation : MapLayerOperation {
        public enum MirrorEdge {
            West,
            North,
            East,
            South
        }
        public MirrorEdge MirrorAroundEdge { get; set; }

        public override Map Execute(Map map, Selection selection)
        {
            switch (MirrorAroundEdge)
            {
                case MirrorEdge.East:
                    return MirrorEast(map);
                case MirrorEdge.North:
                    return MirrorNorth(map);
                case MirrorEdge.West:
                    return MirrorWest(map);
                case MirrorEdge.South:
                    return MirrorSouth(map);
                default:
                    throw new Exception("Unknown mirror edge: " + MirrorAroundEdge);
            }
        }

        public static Map MirrorEast(Map thisMap)
        {
            Map map = (Map)thisMap.Clone();
            map.SetDimensions(thisMap.Width * 2, thisMap.Height);
            for (int x = 0; x < thisMap.Width; x++)
            {
                for (int y = 0; y < thisMap.Height; y++)
                {
                    Tile t = thisMap.GetTile(x, y);
                    map.SetTile(x, y, (Tile)t.Clone());
                    map.SetTile(map.Width - 1 - x, y, (Tile)t.Clone());
                }
            }
            map.FlipRiversHorizontally(thisMap.Width, map.Width - 1);
            return map;
        }

        public static Map MirrorWest(Map thisMap)
        {
            Map map = (Map)thisMap.Clone();
            map.SetDimensions(thisMap.Width * 2, thisMap.Height);
            for (int x = 0; x < thisMap.Width; x++)
            {
                for (int y = 0; y < thisMap.Height; y++)
                {
                    Tile t = thisMap.GetTile(x, y);
                    map.SetTile(thisMap.Width + x, y, (Tile)t.Clone());
                    map.SetTile(thisMap.Width - 1 - x, y, (Tile)t.Clone());
                }
            }
            map.FlipRiversHorizontally(0, thisMap.Width - 1);
            return map;
        }

        public static Map MirrorNorth(Map thisMap)
        {
            Map map = (Map)thisMap.Clone();
            map.SetDimensions(thisMap.Width, thisMap.Height * 2);
            for (int x = 0; x < thisMap.Width; x++)
            {
                for (int y = 0; y < thisMap.Height; y++)
                {
                    Tile t = thisMap.GetTile(x, y);
                    map.SetTile(x, y, (Tile)t.Clone());
                    map.SetTile(x, map.Height - 1 - y, (Tile)t.Clone());
                }
            }
            map.FlipRiversVertically(thisMap.Height, map.Height - 1);
            return map;
        }

        public static Map MirrorSouth(Map thisMap)
        {
            Map map = (Map)thisMap.Clone();
            map.SetDimensions(thisMap.Width, thisMap.Height * 2);
            for (int x = 0; x < thisMap.Width; x++)
            {
                for (int y = 0; y < thisMap.Height; y++)
                {
                    Tile t = thisMap.GetTile(x, y);
                    map.SetTile(x, thisMap.Height + y, (Tile)t.Clone());
                    map.SetTile(x, thisMap.Height - 1 - y, (Tile)t.Clone());
                }
            }
            map.FlipRiversVertically(0, thisMap.Height - 1);
            return map;
        }

        public override string GetName()
        {
            return "Mirror " + this.MirrorAroundEdge.ToString();
        }
    }

    public class RepeatOperation : MapLayerOperation
    {
        public int Times { get; set; }
        public bool Vertically { get; set; }

        public override Map Execute(Map map, Selection selection)
        {
            if (Vertically)
                return RepeatVertically(map, Times);
            else
                return RepeatHorizontally(map, Times);
        }

        public static Map RepeatHorizontally(Map thisMap, int times)
        {
            if (times < 2) return thisMap;
            Map map = (Map)thisMap.Clone();
            map.SetDimensions(thisMap.Width * times, thisMap.Height);
            for (int x = 0; x < thisMap.Width; x++)
            {
                for (int y = 0; y < thisMap.Height; y++)
                {
                    Tile t = thisMap.GetTile(x, y);
                    for (int r = 0; r < times; r++)
                        map.SetTile(x + r * thisMap.Width, y, (Tile)t.Clone());
                }
            }
            return map;
        }

        public static Map RepeatVertically(Map thisMap, int times)
        {
            if (times < 2) return thisMap;
            Map map = (Map)thisMap.Clone();
            map.SetDimensions(thisMap.Width, thisMap.Height * times);
            for (int x = 0; x < thisMap.Width; x++)
            {
                for (int y = 0; y < thisMap.Height; y++)
                {
                    Tile t = thisMap.GetTile(x, y);
                    for (int r = 0; r < times; r++)
                        map.SetTile(x, y + r * thisMap.Height, (Tile)t.Clone());
                }
            }
            return map;
        }

        public override string GetName()
        {
            return "Repeat " + (Vertically ? "vertically" : "horizontally");
        }
    }

    public class ScrambleOperation : MapLayerOperation
    {
        public int Distance { get; set; }
        public bool DontScrambleWater { get; set; }

        public override Map Execute(Map map, Selection selection)
        {
            return ScrambleSelection(map, selection, Distance, DontScrambleWater);
        }

        public static Map ScrambleSelection(Map thisMap, Selection selection, int distance, bool dontScrambleWater)
        {
            Map map = (Map)thisMap.Clone();
            var unswappedTiles = map.GetAllTiles().Where(t => selection.ContainsTile(t) && !(t.IsWater() && dontScrambleWater)).ToList();
            unswappedTiles = Utility.ShuffleList(unswappedTiles);
            Dictionary<Tile, Tile> swaps = new Dictionary<Tile, Tile>();
            while (unswappedTiles.Count > 0)
            {
                var tileToSwap = unswappedTiles[0];
                Tile tileToSwapWith = null;
                for (var i = 1; i < unswappedTiles.Count; i++)
                {
                    var t = unswappedTiles[i];
                    if (map.GetDistanceBetween(t.X, t.Y, tileToSwap.X, tileToSwap.Y) <= distance)
                    {
                        tileToSwapWith = t;
                        break;
                    }
                }
                if (tileToSwapWith != null)
                {
                    swaps[tileToSwap] = tileToSwapWith;
                    unswappedTiles.Remove(tileToSwapWith);
                }
                unswappedTiles.Remove(tileToSwap);
            }
            foreach (var tile in swaps.Keys)
            {
                Utility.SwapTiles(tile, swaps[tile]);
            }
            map.CalculateFreshWater();
            map.CalculateIrrigationStatus();
            map.AssignContinentIds();
            return map;
        }

        public override string GetName()
        {
            return "Scramble selected tiles";
        }
    }

    public class CropOperation : MapLayerOperation
    {
        public override Map Execute(Map map, Selection selection)
        {
            return CropToSelection(map, selection);
        }

        public static Map CropToSelection(Map thisMap, Selection selection)
        {
            int minX = int.MaxValue; int maxX = int.MinValue; int minY = int.MaxValue; int maxY = int.MinValue;
            foreach (var t in selection)
            {
                minX = Math.Min(minX, t.X);
                minY = Math.Min(minY, t.Y);
                maxX = Math.Max(maxX, t.X);
                maxY = Math.Max(maxY, t.Y);
            }
            Map map = (Map)thisMap.Clone();
            if (minX == int.MaxValue) return map; // return uncropped map if no selected tiles
            map.SetDimensions(maxX - minX + 1, maxY - minY + 1);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Tile t = (Tile)thisMap.GetTile(x, y).Clone();
                    map.SetTile(x - minX, y - minY, t);
                }
            }
            return map;
        }

        public override Selection CreateNewSelection(Selection selection)
        {
            return new Selection();
        }

        public override string GetName()
        {
            return "Crop to selection";
        }
    }

    public class SelectionOperation : MapSelectOperation
    {
        public bool Clear { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override Selection Execute(Map map, Selection selection)
        {
            if (Clear)
                return new Selection();
            else
                return SelectTiles(map, selection, Left, Top, Width, Height);
        }

        public static Selection SelectTiles(Map thisMap, Selection selection, int left, int top, int width, int height)
        {
            var result = new Selection();
            result.AddRange(selection);
            for (int x = left; x < left + width; x++)
                for (int y = top; y < top + height; y++)
                {
                    Tile t = thisMap.GetTileAcrossWrap(x, y);
                    if (t != null && !selection.ContainsTile(t))
                        result.Add(t.GetCoordinate());
                }
            return result;
        }

        public override string GetName()
        {
            if (Clear)
                return "Clear selection";
            else
                return "Select tiles";
        }
    }

}


