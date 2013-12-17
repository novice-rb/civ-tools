using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MapEditor
{
    public class MapState
    {
        public ObservableCollection<Layer> Layers { get; set; }
        public Layer ActiveLayer { get; set; }
        public MapState ShallowClone()
        {
            MapState s = new MapState();
            s.Layers = new ObservableCollection<Layer>();
            foreach(var l in Layers)
            {
                var c = l.ShallowClone();
                s.Layers.Add(c);
                if (ActiveLayer == l)
                    s.ActiveLayer = c;
            }
            return s;
        }
    }
}
