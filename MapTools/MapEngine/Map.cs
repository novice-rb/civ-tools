using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MapEngine
{
    public abstract class MapOperation
    {
        public abstract Map Execute(Map map);
        public abstract string GetName();
    }

    public class NamedMapOperation : MapOperation
    {
        string name;

        public NamedMapOperation(string operationName)
        {
            this.name = operationName;
        }

        public override string GetName()
        {
            return name;
        }

        public override Map Execute(Map map)
        {
            switch(name) {
                case "RotateCW":
                    return map.RotateCW();
                case "RotateCCW":
                    return map.RotateCCW();
                case "CropToSelection":
                    return map.CropToSelection();
            }
            throw new NotImplementedException("Operation " + name + " not implemented.");
        }
    }

    [Serializable]
    public class Map : ICloneable
    {
        public int Width { get; set; }
        public int Height { get; set; }
        private Tile[,] _Tiles { get; set; }
        public bool IsVerticalWrap { get; set; }
        public bool IsHorizontalWrap { get; set; }
        public List<string> UnparsedData { get; set; }

        public Map()
        {
            UnparsedData = new List<string>();
        }

        public int GetDistanceBetween(int x1, int y1, int x2, int y2)
        {
            int xDist = 0, yDist = 0;
            if (IsHorizontalWrap)
            {
                if (x1 > x2)
                    xDist = Math.Min(x1 - x2, (Width - x1) + x2);
                else
                    xDist = Math.Min(x2 - x1, (Width - x2) + x1);
            }
            else
                xDist = Math.Max(x1, x2) - Math.Min(x1, x2);
            if (IsVerticalWrap)
            {
                if (y1 > y2)
                    yDist = Math.Min(y1 - y2, (Height - y1) + y2);
                else
                    yDist = Math.Min(y2 - y1, (Height - y2) + y1);
            }
            else
                yDist = Math.Max(y1, y2) - Math.Min(y1, y2);
            int Dist = Math.Max(xDist, yDist) + (int)(Math.Min(xDist, yDist) / 2);
            return Dist;
        }

        public Map RotateCCW()
        {
            Map map = new Map();
            map.SetDimensions(this.Height, this.Width);
            map.IsHorizontalWrap = this.IsVerticalWrap;
            map.IsVerticalWrap = this.IsHorizontalWrap;
            map.UnparsedData = new List<string>(this.UnparsedData);
            foreach (Tile t in this.GetAllTiles())
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

        public Map RotateCW()
        {
            Map map = new Map();
            map.SetDimensions(this.Height, this.Width);
            map.IsHorizontalWrap = this.IsVerticalWrap;
            map.IsVerticalWrap = this.IsHorizontalWrap;
            map.UnparsedData = new List<string>(this.UnparsedData);
            foreach (Tile t in this.GetAllTiles())
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

        public Map ScrambleSelection(int maxSwapDistance, bool dontScrambleWater)
        {
            var unswappedTiles = this.GetAllTiles().Where(t => t.Selected && !(t.IsWater() && dontScrambleWater)).ToList();
            unswappedTiles = Utility.ShuffleList(unswappedTiles);
            Dictionary<Tile, Tile> swaps = new Dictionary<Tile, Tile>();
            while (unswappedTiles.Count > 0)
            {
                var tileToSwap = unswappedTiles[0];
                Tile tileToSwapWith = null;
                for (var i = 1; i < unswappedTiles.Count; i++)
                {
                    var t = unswappedTiles[i];
                    if (GetDistanceBetween(t.X, t.Y, tileToSwap.X, tileToSwap.Y) <= maxSwapDistance)
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
                _SwapTiles(tile, swaps[tile]);
            }
            this.CalculateFreshWater();
            this.CalculateIrrigationStatus();
            this.AssignContinentIds();
            return this;
        }

        private void _SwapTiles(Tile t1, Tile t2)
        {
            Tile tmp = (Tile)t1.Clone();
            _CopyTileInfo(t1, t2);
            _CopyTileInfo(t2, tmp);
        }

        private void _CopyTileInfo(Tile to, Tile from)
        {
            to.BonusType =  from.BonusType;
            to.FeatureType = from.FeatureType;
            to.FeatureVariety = from.FeatureVariety;
            to.FreshWaterType = from.FreshWaterType;
            to.PlotType = from.PlotType;
            to.Terrain = from.Terrain;
        }

        public void ClearSelection()
        {
            foreach (var t in GetAllTiles())
                t.Selected = false;
        }

        public Map CropToSelection()
        {
            int minX = int.MaxValue; int maxX = int.MinValue; int minY = int.MaxValue; int maxY = int.MinValue;
            foreach (Tile t in this.GetAllTiles())
            {
                if (t.Selected)
                {
                    minX = Math.Min(minX, t.X);
                    minY = Math.Min(minY, t.Y);
                    maxX = Math.Max(maxX, t.X);
                    maxY = Math.Max(maxY, t.Y);
                }
            }
            Map map = (Map)this.Clone();
            if (minX == int.MaxValue) return map; // return uncropped map if no selected tiles
            map.SetDimensions(maxX - minX + 1, maxY - minY + 1);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Tile t = (Tile)this.GetTile(x, y).Clone();
                    t.Selected = false;
                    map.SetTile(x - minX, y - minY, t);
                }
            }
            return map;
        }

        public Map RotateAroundCorner(bool leftCorner, bool topCorner)
        {
            Map map = this;
            if (map.Width != map.Height) map = map.ExpandToSize(Math.Min(map.Width, map.Height), Math.Min(map.Width, map.Height), TerrainTypes.TERRAIN_GRASS, PlotTypes.FLAT, HorizontalNeighbour.None, VerticalNeighbour.None);
            Map topLeft = null, topRight = null, bottomLeft = null, bottomRight = null;
            if (leftCorner)
            {
                if (topCorner)
                {
                    // Rotate around top left corner
                    bottomRight = map;
                    bottomLeft = bottomRight.RotateCW();
                    topLeft = bottomLeft.RotateCW();
                    topRight = topLeft.RotateCW();
                }
                else
                {
                    // Rotate around bottom left corner
                    topRight = map;
                    bottomRight = topRight.RotateCW();
                    bottomLeft = bottomRight.RotateCW();
                    topLeft = bottomLeft.RotateCW();
                }
            }
            else
            {
                if (topCorner)
                {
                    // Rotate around top right corner
                    bottomLeft = map;
                    topLeft = bottomLeft.RotateCW();
                    topRight = topLeft.RotateCW();
                    bottomRight = topRight.RotateCW();
                }
                else
                {
                    // Rotate around bottom right corner
                    topLeft = map;
                    topRight = topLeft.RotateCW();
                    bottomRight = topRight.RotateCW();
                    bottomLeft = bottomRight.RotateCW();
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

        public void SetTiles(Map map, int minX, int minY)
        {
            for (int x = minX; x < minX + map.Width; x++)
                for (int y = minY; y < minY + map.Height; y++)
                    this.SetTile(x, y, (Tile)map.GetTile(x - minX, y - minY).Clone());
        }

        public Map MirrorEast()
        {
            Map map = (Map)this.Clone();
            map.SetDimensions(this.Width * 2, this.Height);
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    Tile t = this.GetTile(x, y);
                    map.SetTile(x, y, (Tile)t.Clone());
                    map.SetTile(map.Width-1-x, y, (Tile)t.Clone());
                }
            }
            map.FlipRiversHorizontally(this.Width, map.Width - 1);
            return map;
        }

        public Map MirrorWest()
        {
            Map map = (Map)this.Clone();
            map.SetDimensions(this.Width * 2, this.Height);
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    Tile t = this.GetTile(x, y);
                    map.SetTile(this.Width + x, y, (Tile)t.Clone());
                    map.SetTile(this.Width - 1 - x, y, (Tile)t.Clone());
                }
            }
            map.FlipRiversHorizontally(0, this.Width - 1);
            return map;
        }

        public Map RepeatHorizontally(int times)
        {
            if (times < 2) return this;
            Map map = (Map)this.Clone();
            map.SetDimensions(this.Width * times, this.Height);
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    Tile t = this.GetTile(x, y);
                    for(int r = 0; r < times; r++)
                        map.SetTile(x + r*this.Width, y, (Tile)t.Clone());
                }
            }
            return map;
        }

        public Map RepeatVertically(int times)
        {
            if (times < 2) return this;
            Map map = (Map)this.Clone();
            map.SetDimensions(this.Width, this.Height * times);
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    Tile t = this.GetTile(x, y);
                    for (int r = 0; r < times; r++)
                        map.SetTile(x, y + r * this.Height, (Tile)t.Clone());
                }
            }
            return map;
        }

        public Map MirrorNorth()
        {
            Map map = (Map)this.Clone();
            map.SetDimensions(this.Width, this.Height * 2);
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    Tile t = this.GetTile(x, y);
                    map.SetTile(x, y, (Tile)t.Clone());
                    map.SetTile(x, map.Height - 1 - y, (Tile)t.Clone());
                }
            }
            map.FlipRiversVertically(this.Height, map.Height - 1);
            return map;
        }

        public Map MirrorSouth()
        {
            Map map = (Map)this.Clone();
            map.SetDimensions(this.Width, this.Height * 2);
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    Tile t = this.GetTile(x, y);
                    map.SetTile(x, this.Height + y, (Tile)t.Clone());
                    map.SetTile(x, this.Height - 1 - y, (Tile)t.Clone());
                }
            }
            map.FlipRiversVertically(0, this.Height - 1);
            return map;
        }

        public void FlipRiversVertically(int minY, int maxY)
        {
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = maxY; y >= minY; y--)
                {
                    if (this.GetTile(x, y).IsNOfRiver)
                    {
                        if (y + 1 < this.Height)
                        {
                            this.GetTile(x, y + 1).IsNOfRiver = true;
                            this.GetTile(x, y + 1).RiverWEDirection = this.GetTile(x, y).RiverWEDirection;
                        }
                        this.GetTile(x, y).IsNOfRiver = false;
                        this.GetTile(x, y).RiverWEDirection = 0;
                    }
                    if (this.GetTile(x, y).IsWOfRiver)
                        this.GetTile(x, y).RiverNSDirection = ReverseDirection(this.GetTile(x, y).RiverNSDirection);
                }
            }
        }

        public void FlipRiversHorizontally(int minX, int maxX)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    if (this.GetTile(x, y).IsWOfRiver)
                    {
                        if (x > 0)
                        {
                            this.GetTile(x - 1, y).IsWOfRiver = true;
                            this.GetTile(x - 1, y).RiverNSDirection = this.GetTile(x, y).RiverNSDirection;
                        }
                        this.GetTile(x, y).IsWOfRiver = false;
                        this.GetTile(x, y).RiverNSDirection = 0;
                    }
                    if (this.GetTile(x, y).IsNOfRiver)
                        this.GetTile(x, y).RiverWEDirection = ReverseDirection(this.GetTile(x, y).RiverWEDirection);
                }
            }
        }

        public static RiverDirection ReverseDirection(RiverDirection r)
        {
            switch (r)
            {
                case RiverDirection.EAST_TO_WEST: return RiverDirection.WEST_TO_EAST;
                case RiverDirection.WEST_TO_EAST: return RiverDirection.EAST_TO_WEST;
                case RiverDirection.NORTH_TO_SOUTH: return RiverDirection.SOUTH_TO_NORTH;
                case RiverDirection.SOUTH_TO_NORTH: return RiverDirection.NORTH_TO_SOUTH;
                default: return 0;
            }
        }

        public Map ExpandToSize(int newWidth, int newHeight, TerrainTypes fillWithTerrain, PlotTypes fillWithPlotType, HorizontalNeighbour horAlignment, VerticalNeighbour verAlignment)
        {
            Map map = (Map)this.Clone();
            map.SetDimensions(newWidth, newHeight);
            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    Tile t = new Tile();
                    t.PlotType = fillWithPlotType;
                    t.Terrain = fillWithTerrain;
                    map.SetTile(x, y, t);
                }
            }
            int minX = 0; // west alignment
            int minY = 0; // south alignment
            if (horAlignment == HorizontalNeighbour.None) // center alignment
                minX = (this.Width - newWidth) / 2;
            else if (horAlignment == HorizontalNeighbour.East)
                minX = this.Width - newWidth;
            if (verAlignment == VerticalNeighbour.None) // center alignment
                minY = (this.Height - newHeight) / 2;
            else if (verAlignment == VerticalNeighbour.North)
                minY = this.Height - newHeight;
            for (int x = minX; x < minX + newWidth; x++)
            {
                for (int y = minY; y < minY + newHeight; y++)
                {
                    Tile t = (Tile)this.GetTileAcrossWrap(x, y, false, false);
                    if(t != null)
                        map.SetTile(x - minX, y - minY, t);
                }
            }
            return map;
        }

        public bool IsRiverSide(int x, int y)
        {
            return IsRiverSide(GetTile(x, y));
        }

        public bool IsRiverSide(Tile t)
        {
            if (t.IsWater()) return false;
            if (t.IsNOfRiver || t.IsWOfRiver) return true;
            Tile w = GetNeighbour(t, HorizontalNeighbour.West, VerticalNeighbour.None);
            if (w != null && w.IsWOfRiver) return true;
            Tile n = GetNeighbour(t, HorizontalNeighbour.None, VerticalNeighbour.North); ;
            if (n != null && n.IsNOfRiver) return true;
            // Check diagonal adjacency to a river turn
            Tile s = GetNeighbour(t, HorizontalNeighbour.None, VerticalNeighbour.South);
            Tile e = GetNeighbour(t, HorizontalNeighbour.East, VerticalNeighbour.None);
            Tile sw = GetNeighbour(t, HorizontalNeighbour.West, VerticalNeighbour.South);
            Tile ne = GetNeighbour(t, HorizontalNeighbour.East, VerticalNeighbour.North);
            Tile nw = GetNeighbour(t, HorizontalNeighbour.West, VerticalNeighbour.North);
            if (w != null && w.IsNOfRiver && sw != null && sw.IsWOfRiver) return true; // southwestern tile is a river bend
            if (n != null && n.IsWOfRiver && ne != null && ne.IsNOfRiver) return true; // northeastern tile is a river bend
            if (nw != null && nw.IsNOfRiver && nw.IsWOfRiver) return true; // northwestern tile is a river bend
            if (s != null && s.IsWOfRiver && e != null && e.IsNOfRiver) return true; // southeastern tile is a river bend
            // Check diagonal adjacency to a river mouth
            if (e != null && e.IsNOfRiver && s != null && s.IsWater()) return true;
            if (ne != null && ne.IsNOfRiver && n != null && n.IsWater()) return true;
            if (w != null && w.IsNOfRiver && s != null && s.IsWater()) return true;
            if (nw != null && nw.IsNOfRiver && n != null && n.IsWater()) return true;
            if (n != null && n.IsWOfRiver && e != null && e.IsWater()) return true;
            if (nw != null && nw.IsWOfRiver && w != null && w.IsWater()) return true;
            if (s != null && s.IsWOfRiver && e != null && e.IsWater()) return true;
            if (sw != null && sw.IsWOfRiver && w != null && w.IsWater()) return true;
            return false;
        }

        public Tile GetTile(int x, int y)
        {
            return _Tiles[x, y];
        }

        public void SetTile(int x, int y, Tile t)
        {
            _Tiles[x, y] = t;
            t.X = x;
            t.Y = y;
        }

        public List<Tile> GetAllTiles()
        {
            List<Tile> tiles = new List<Tile>();
            var en = _Tiles.GetEnumerator();
            while (en.MoveNext())
            {
                tiles.Add((Tile)en.Current);
            }
            return tiles;
        }

        public void SetDimensions(int width, int height)
        {
            _Tiles = new Tile[width, height];
            Width = width;
            Height = height;
        }

        private Tile GetNeighbour(Tile t, HorizontalNeighbour h, VerticalNeighbour v)
        {
            return GetNeighbour(t.X, t.Y, h, v);
        }

        private Tile GetNeighbour(int x, int y, HorizontalNeighbour h, VerticalNeighbour v)
        {
            return GetTileAcrossWrap(x + (int)h, y + (int)v);
        }

        private Tile GetTileAcrossWrap(int x, int y)
        {
            return GetTileAcrossWrap(x, y, IsHorizontalWrap, IsVerticalWrap);
        }

        private Tile GetTileAcrossWrap(int x, int y, bool isHorizontalWrap, bool isVerticalWrap)
        {
            while (x < 0)
            {
                if (isHorizontalWrap)
                    x += Width;
                else
                    return null;
            }
            while (x >= Width)
            {
                if (isHorizontalWrap)
                    x -= Width;
                else
                    return null;
            }
            while (y < 0)
            {
                if (isVerticalWrap)
                    y += Height;
                else
                    return null;
            }
            while (y >= Height)
            {
                if (isVerticalWrap)
                    y -= Height;
                else
                    return null;
            }
            return GetTile(x, y);
        }

        public void CalculateFreshWater()
        {
            foreach (Tile t in this.GetAllTiles())
                t.FreshWaterType = FreshWaterTypes.NOT_CALCULATED;
            foreach(Tile t in this.GetAllTiles())
            {
                if (t.IsWater() && t.FreshWaterType == FreshWaterTypes.NOT_CALCULATED)
                {
                    List<Tile> waterBody = GetConnectedTiles(t, false, true);
                    t.WaterBodySize = waterBody.Count;
                    t.FreshWaterType = t.WaterBodySize <= 9 ? FreshWaterTypes.FRESHWATER : FreshWaterTypes.SALTWATER;
                    t.WaterBodySupportsShips = t.WaterBodySize >= 20;
                    t.WaterBodyContainsResources = false;
                    foreach (Tile s in waterBody)
                        if (s.IsResource())
                            t.WaterBodyContainsResources = true;
                    foreach (Tile s in waterBody)
                    {
                        s.WaterBodySize = t.WaterBodySize;
                        s.FreshWaterType = t.FreshWaterType;
                        s.WaterBodySupportsShips = t.WaterBodySupportsShips;
                        s.WaterBodyContainsResources = t.WaterBodyContainsResources;
                    }
                }
            }
        }

        public void CalculateIrrigationStatus()
        {
            foreach (Tile t in this.GetAllTiles())
                t.IrrigationStatus = IrrigationStatus.NOT_CALCULATED;
            foreach (Tile t in this.GetAllTiles())
            {
                if (IsRiverSide(t))
                {
                    t.IrrigationStatus = IrrigationStatus.IRRIGATED;
                    continue;
                }
                List<Tile> neighbours = GetNeighbours(t, true);
                foreach (Tile n in neighbours)
                {
                    if (n.IsWater() && n.IsFreshWater())
                    {
                        t.IrrigationStatus = IrrigationStatus.IRRIGATED;
                        break;
                    }
                }
                if (t.IrrigationStatus == IrrigationStatus.NOT_CALCULATED)
                    t.IrrigationStatus = IrrigationStatus.NOT_IRRIGATED;
            }
        }

        public void AssignContinentIds()
        {
            int currentId = 1;
            foreach (Tile t in this.GetAllTiles())
                t.ContinentId = 0;
            foreach (Tile t in this.GetAllTiles())
            {
                if (!t.IsWater() && t.ContinentId == 0)
                {
                    List<Tile> continent = GetConnectedTiles(t, true, false);
                    foreach (Tile c in continent)
                        c.ContinentId = currentId;
                    currentId++;
                }
            }
        }

        public List<Tile> GetConnectedTiles(Tile start, bool traverseImpassableLandTiles, bool traverseImpassableSeaTiles)
        {
            List<Tile> result = new List<Tile>();
            foreach (Tile t in this.GetAllTiles())
                t.TraversalFlag = -1;
            Queue<Tile> tilesToVisit = new Queue<Tile>();
            tilesToVisit.Enqueue(start);
            start.TraversalFlag = 0;
            while (tilesToVisit.Count > 0)
            {
                Tile t = tilesToVisit.Dequeue();
                result.Add(t);
                foreach (Tile n in this.GetNeighbours(t, !start.IsWater()))
                {
                    if (n.TraversalFlag == -1)
                    {
                        bool traversable = false;
                        if (start.IsWater())
                            traversable = n.IsWater() && (traverseImpassableSeaTiles || n.FeatureType != FeatureTypes.FEATURE_ICE);
                        else if (n.IsTraversableByLand() || (traverseImpassableLandTiles && !n.IsWater()))
                            traversable = true;
                        if(traversable)
                        {
                            n.TraversalFlag = t.TraversalFlag;
                            tilesToVisit.Enqueue(n);
                        }
                    }
                }
            }
            foreach (Tile t in this.GetAllTiles())
                t.TraversalFlag = -1;
            return result;
        }

        public List<Tile> GetNeighbours(Tile t, bool includeDiagonals)
        {
            int left = t.X - 1;
            int right = t.X + 1;
            int top = t.Y - 1;
            int bottom = t.Y + 1;
            List<Tile> n = new List<Tile>();
            AddTileIfNotNull(n, this.GetTileAcrossWrap(left, t.Y));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(t.X, top));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(t.X, bottom));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(right, t.Y));
            if (includeDiagonals)
            {
                AddTileIfNotNull(n, this.GetTileAcrossWrap(left, top));
                AddTileIfNotNull(n, this.GetTileAcrossWrap(left, bottom));
                AddTileIfNotNull(n, this.GetTileAcrossWrap(right, top));
                AddTileIfNotNull(n, this.GetTileAcrossWrap(right, bottom));
            }
            return n;
        }

        public List<Tile> GetBigFatCross(Tile t)
        {
            int left = t.X - 1;
            int farLeft = t.X - 2;
            int right = t.X + 1;
            int farRight = t.X + 2;
            int top = t.Y - 1;
            int farTop = t.Y - 2;
            int bottom = t.Y + 1;
            int farBottom = t.Y + 2;
            List<Tile> n = new List<Tile>();
            AddTileIfNotNull(n, t);
            n.AddRange(this.GetNeighbours(t, true));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(farLeft, t.Y));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(farLeft, top));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(farLeft, bottom));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(farRight, t.Y));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(farRight, top));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(farRight, bottom));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(t.X, farTop));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(left, farTop));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(right, farTop));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(t.X, farBottom));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(left, farBottom));
            AddTileIfNotNull(n, this.GetTileAcrossWrap(right, farBottom));
            return n;
        }

        private void AddTileIfNotNull(List<Tile> tiles, Tile t)
        {
            if (t != null) tiles.Add(t);
        }

        public void SelectTiles(int left, int top, int width, int height)
        {
            for(int x = left; x < left+width; x++)
                for(int y = top; y < top+height; y++)
                {
                    Tile t = GetTileAcrossWrap(x, y);
                    if (t != null) t.Selected = true;
                }
        }

        #region ICloneable Members

        public object Clone()
        {
            Map map = (Map)MemberwiseClone();
            map.SetDimensions(this.Width, this.Height);
            map.UnparsedData = new List<string>(this.UnparsedData);
            foreach (Tile t in this.GetAllTiles())
            {
                map.SetTile(t.X, t.Y, (Tile)t.Clone());
            }
            return map;
        }

        #endregion
    }

    public enum HorizontalNeighbour
    {
        West = -1,
        None = 0,
        East = 1
    }

    public enum VerticalNeighbour
    {
        South = -1,
        None = 0,
        North = 1
    }
}
