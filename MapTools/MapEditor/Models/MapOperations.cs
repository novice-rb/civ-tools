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

}
