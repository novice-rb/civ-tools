using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public class MapState
    {
        public List<PlayerStart> PlayerStarts { get; set; }

        public MapState()
        {
            PlayerStarts = new List<PlayerStart>();
        }

        public override string ToString()
        {
            string s = "";
            foreach (PlayerStart ps in PlayerStarts)
                s += ps.StartX.ToString() + "," + ps.StartY.ToString() + ";";
            return s;
        }

        public MapState Clone()
        {
            MapState ms = new MapState();
            foreach (var ps in PlayerStarts)
                ms.PlayerStarts.Add(ps.Clone());
            return ms;
        }

        public static MapState FromMap(Map map)
        {
            MapState ms = new MapState();
            foreach(Tile t in map.GetAllTiles())
                foreach (Unit u in t.Units)
                    if (u.UnitOwner != Game.BarbarianPlayerId)
                    {
                        while (ms.PlayerStarts.Count <= u.UnitOwner)
                            ms.PlayerStarts.Add(new PlayerStart() { PlayerId = ms.PlayerStarts.Count });
                        ms.PlayerStarts[u.UnitOwner].StartX = t.X;
                        ms.PlayerStarts[u.UnitOwner].StartY = t.Y;
                    }
            return ms;
        }

        public void ApplyToMap(Map map)
        {
            Dictionary<int, List<Unit>> playerUnits = new Dictionary<int, List<Unit>>();
            foreach (Tile t in map.GetAllTiles())
            {
                foreach (Unit u in t.Units)
                {
                    if (!playerUnits.ContainsKey(u.UnitOwner))
                        playerUnits[u.UnitOwner] = new List<Unit>();
                    playerUnits[u.UnitOwner].Add(u);
                }
                t.Units.Clear();
            }
            foreach (PlayerStart ps in PlayerStarts)
                map.GetTile(ps.StartX, ps.StartY).Units.AddRange(playerUnits[ps.PlayerId]);
        }

        public bool RespectsMinimumDistance(Map map, int minimumDistanceBetweenCapitals)
        {
            foreach (var ps in PlayerStarts)
                foreach (var ps2 in PlayerStarts)
                    if (ps != ps2 && map.GetDistanceBetween(ps.StartX, ps.StartY, ps2.StartX, ps2.StartY) < minimumDistanceBetweenCapitals)
                        return false;
            return true;
        }

        public List<MapState> GetNeighbouringStates(Map map, List<int> fixedStarts, int minimumDistanceBetweenCapitals)
        {
            List<MapState> list = new List<MapState>();
            foreach (var startToMove in PlayerStarts)
            {
                if (fixedStarts.Contains(startToMove.PlayerId)) continue;
                foreach (Tile t in map.GetNeighbours(map.GetTile(startToMove.StartX, startToMove.StartY), true))
                {
                    if (t.IsTraversableByLand())
                    {
                        MapState clone = this.Clone();
                        clone.PlayerStarts[startToMove.PlayerId].StartX = t.X;
                        clone.PlayerStarts[startToMove.PlayerId].StartY = t.Y;
                        if(clone.RespectsMinimumDistance(map, minimumDistanceBetweenCapitals))
                            list.Add(clone);
                    }
                }
            }
            return list;
        }
    }

    public class PlayerStart
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int PlayerId { get; set; }

        public PlayerStart Clone()
        {
            return new PlayerStart() { StartX = this.StartX, StartY = this.StartY, PlayerId = this.PlayerId };
        }
    }
}
