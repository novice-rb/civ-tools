using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine.Parameters
{
    public class ScrambleParameters
    {
        public int Distance { get; set; }
        public bool DontScrambleWater { get; set; }
    }

    public class ResizeParameters
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public TerrainTypes TerrainType { get; set; }
        public PlotTypes PlotType { get; set; }
        public HorizontalNeighbour HorAlign { get; set; }
        public VerticalNeighbour VerAlign { get; set; }
    }

    public class RotateAroundCornerParameters
    {
        public bool TopCorner { get; set; }
        public bool LeftCorner { get; set; }
    }

    public class RotateParameters
    {
        public bool Clockwise { get; set; }
    }

    public class MirrorParameters
    {
    }

    public class CropParameters
    {
    }

    public class RepeatParameters
    {
        public int Times { get; set; }
    }

    public class SelectionParameters
    {
        public bool ClearSelection { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

}
