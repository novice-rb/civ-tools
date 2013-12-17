using MapEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor
{
    public class Layer
    {
        public string LayerName { get; set; }
        public Map Map { get; set; }
        public Layer ShallowClone()
        {
            var lyr = new Layer();
            lyr.LayerName = LayerName;
            lyr.Map = Map;
            return lyr;
        }
    }
}
