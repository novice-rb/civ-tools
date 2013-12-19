using MapEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor
{
    public abstract class MapOperation
    {
        public abstract MapState Execute(MapState state);
        public abstract string GetName();
    }

    public class CopyToNewLayerOperation : MapOperation
    {
        public override MapState Execute(MapState state)
        {
            var selection = state.ActiveLayer.Map.GetAllTiles().Where(t => t.Selected).ToList();
            var newLayer = new Layer() { LayerName = "New layer", Map = (Map)state.ActiveLayer.Map.Clone() };
            foreach (var tile in newLayer.Map.GetAllTiles())
            {
                if (!tile.Selected)
                {
                    tile.IsEmpty = true;
                }
            }
            state.ActiveLayer.Map.ClearSelection(new MapEngine.Parameters.SelectionParameters() { ClearSelection = true });
            state.Layers.Insert(0, newLayer);
            state.ActiveLayer = state.Layers[0];
            return state;
        }

        public override string GetName()
        {
            return "Copy to new layer";
        }
    }

}
