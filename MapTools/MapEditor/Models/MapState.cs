using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MapEngine;

namespace MapEditor
{
    public class MapState
    {
        public Selection Selection { get; set; }
        public ObservableCollection<Layer> Layers { get; set; }
        public Layer ActiveLayer { get; set; }
        public MapState Clone()
        {
            MapState s = new MapState();
            s.Selection = new Selection();
            foreach(var c in Selection)
                s.Selection.Add(c);
            s.Layers = new ObservableCollection<Layer>();
            foreach(var l in Layers)
            {
                var c = l.Clone();
                s.Layers.Add(c);
                if (ActiveLayer == l)
                    s.ActiveLayer = c;
            }
            return s;
        }
    }
}
