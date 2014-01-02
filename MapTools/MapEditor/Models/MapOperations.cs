using MapEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor
{
    public abstract class MapOperation
    {
        public abstract MapState Execute(MapState state, Selection selection);
        public abstract string GetName();
    }

    public class CopyToNewLayerOperation : MapOperation
    {
        public override MapState Execute(MapState state, Selection selection)
        {
            var newLayer = new Layer() { LayerName = "New layer", Map = (Map)state.ActiveLayer.Map.Clone(), Visible=true };
            foreach (var tile in newLayer.Map.GetAllTiles())
            {
                tile.IsEmpty = true;
                foreach (var c in selection)
                    if (c.X == tile.X && c.Y == tile.Y)
                        tile.IsEmpty = false;
            }
            state.Selection = new Selection();
            state.Layers.Insert(0, newLayer);
            //state.ActiveLayer.Visible = false;
            state.ActiveLayer = state.Layers[0];
            return state;
        }

        public override string GetName()
        {
            return "Copy to new layer";
        }
    }

    public class MoveLayerOperation : MapOperation
    {
        public int DeltaX { get; set; }
        public int DeltaY { get; set; }
        public bool KeepInside { get; set; }

        public override MapState Execute(MapState state, Selection selection)
        {
            if (KeepInside)
            {
                int minx = int.MaxValue, miny = int.MaxValue, maxx = int.MinValue, maxy = int.MinValue;
                foreach (var tile in state.ActiveLayer.Map.GetAllTiles())
                {
                    if (!tile.IsEmpty)
                    {
                        minx = Math.Min(minx, tile.X);
                        miny = Math.Min(miny, tile.Y);
                        maxx = Math.Max(maxx, tile.X);
                        maxy = Math.Max(maxy, tile.Y);
                    }
                }
                if (minx + DeltaX < 0) DeltaX = -minx;
                if (miny + DeltaY < 0) DeltaY = -miny;
                if (maxx + DeltaX >= state.ActiveLayer.Map.Width) DeltaX = state.ActiveLayer.Map.Width - 1 - maxx;
                if (maxy + DeltaY >= state.ActiveLayer.Map.Height) DeltaY = state.ActiveLayer.Map.Height - 1 - maxy;
            }
            if (DeltaX != 0 || DeltaY != 0)
            {
                Map m = new Map();
                m.SetDimensions(state.ActiveLayer.Map.Width, state.ActiveLayer.Map.Height);
                foreach (var t in state.ActiveLayer.Map.GetAllTiles())
                {
                    m.SetTile(t.X, t.Y, new Tile() { IsEmpty = true });
                }
                foreach (var t in state.ActiveLayer.Map.GetAllTiles())
                {
                    var x = t.X + DeltaX;
                    var y = t.Y + DeltaY;
                    if(x >= 0 && y >= 0 && x < m.Width && y < m.Height)
                        m.SetTile(x, y, t);
                }
                state.ActiveLayer.Map = m;
            }
            return state;
        }

        public override string GetName()
        {
            return "Move layer";
        }
    }

}
