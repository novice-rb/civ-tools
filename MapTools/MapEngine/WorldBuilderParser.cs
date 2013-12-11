using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MapEngine
{
    public class WorldBuilderParser
    {
        public static void ToWorldbuilderFile(Game game, string path)
        {
            TextWriter tw = null;
            try
            {
                tw = new StreamWriter(path, false);
                tw.WriteLine("Version=" + game.Version);
                tw.WriteLine("BeginGame");
                foreach (string line in game.UnparsedData)
                    tw.WriteLine("\t" + line);
                tw.WriteLine("EndGame");
                foreach (Team t in game.Teams)
                {
                    tw.WriteLine("BeginTeam");
                    tw.WriteLine("\tTeamID=" + t.TeamId);
                    foreach (string line in t.UnparsedData)
                        tw.WriteLine("\t" + line);
                    tw.WriteLine("EndTeam");
                }
                foreach (Player p in game.Players)
                {
                    if (p.PlayerId == Game.BarbarianPlayerId) continue; // Barbarian players, don't save
                    tw.WriteLine("BeginPlayer");
                    tw.WriteLine("\tTeam=" + p.TeamId);
                    tw.WriteLine("\tLeaderType=" + p.LeaderType);
                    tw.WriteLine("\tCivType=" + p.CivType);
                    tw.WriteLine("\tColor=" + p.Color.ToString());
                    foreach (string line in p.UnparsedData)
                        tw.WriteLine("\t" + line);
                    tw.WriteLine("EndPlayer");
                }
                tw.WriteLine("BeginMap");
                tw.WriteLine("\tgrid width=" + game.Map.Width);
                tw.WriteLine("\tgrid height=" + game.Map.Height);
                tw.WriteLine("\twrap X=" + (game.Map.IsHorizontalWrap ? "1" : "0"));
                tw.WriteLine("\twrap Y=" + (game.Map.IsVerticalWrap ? "1" : "0"));
                tw.WriteLine("\tnum plots written=" + game.Map.Width * game.Map.Height);
                tw.WriteLine("\tnum signs written=0"); // Signs are not persisted, not implemented.
                foreach (string line in game.Map.UnparsedData)
                    tw.WriteLine("\t" + line);
                tw.WriteLine("EndMap");
                tw.WriteLine();
                tw.WriteLine("### Plot Info ###");
                for (int x = 0; x < game.Map.Width; x++)
                {
                    for (int y = 0; y < game.Map.Height; y++)
                    {
                        Tile t = game.Map.GetTile(x, y);
                        tw.WriteLine("BeginPlot");
                        tw.WriteLine("\tx=" + x + ",y=" + y);
                        if (t.IsNOfRiver)
                        {
                            tw.WriteLine("\tisNOfRiver");
                            tw.WriteLine("\tRiverWEDirection=" + ((int)t.RiverWEDirection).ToString());
                        }
                        if (t.IsWOfRiver)
                        {
                            tw.WriteLine("\tisWOfRiver");
                            tw.WriteLine("\tRiverNSDirection=" + ((int)t.RiverNSDirection).ToString());
                        }
                        if (t.BonusType != BonusTypes.BONUS_NONE)
                            tw.WriteLine("\tBonusType=" + t.BonusType.ToString());
                        if(t.FeatureType != FeatureTypes.FEATURE_NONE)
                        {
                            if(t.FeatureVariety > -1)
                                tw.WriteLine("\tFeatureType=" + t.FeatureType.ToString() + ", FeatureVariety=" + t.FeatureVariety);
                            else
                                tw.WriteLine("\tFeatureType=" + t.FeatureType.ToString());
                        }
                        tw.WriteLine("\tTerrainType=" + t.Terrain.ToString());
                        tw.WriteLine("\tPlotType=" + ((int)t.PlotType).ToString());
                        foreach (string line in t.UnparsedData)
                            tw.WriteLine("\t" + line);
                        foreach (Unit u in t.Units)
                        {
                            tw.WriteLine("\tBeginUnit");
                            tw.WriteLine("\t\tUnitType=" + u.UnitType + ", UnitOwner=" + u.UnitOwner);
                            foreach (string line in t.UnparsedData)
                                tw.WriteLine("\t\t" + line);
                            tw.WriteLine("\tEndUnit");
                        }
                        tw.WriteLine("EndPlot");
                    }
                }
                tw.WriteLine();
                tw.WriteLine("### Sign Info ###");
            }
            finally
            {
                if (tw != null) tw.Close();
            }
        }

        public static Game FromWorldbuilderFile(string path)
        {
            Game game;
            if (Path.GetExtension(path) == ".bic")
                game = ParseBicFile(path);
            else
                game = ParseWorldbuilderFile(path);
            game.Map.CalculateFreshWater();
            game.Map.AssignContinentIds();
            game.Map.CalculateIrrigationStatus();
            return game;
        }

        private static Game ParseBicFile(string path)
        {
            throw new Exception("Parsing .bic files is not yet fully implemented. (The map dimensions are hardcoded. Try asking Novice nicely for help.)");
            BinaryReader reader = null;
            try
            {
                Game game = new Game();
                reader = new BinaryReader(new FileStream(path, FileMode.Open));
                int i = 0;
                try
                {
                    while (true)
                    {
                        byte b = reader.ReadByte();
                        if ("TILE".IndexOf((char)b) == i)
                            i++;
                        else
                            i = 0;
                        if (i > 3)
                        {
                            i = 0;
                            int tileCount = reader.ReadInt32();
                            Map map = new Map();
                            map.Width = 200;
                            map.Height = 100;
                            map.SetDimensions(map.Width, map.Height);
                            game.Map = map;
                            for (int y = map.Height-1; y >= 0; y--)
                            {
                                for (int xx = 0; xx < map.Width; xx++)
                                {
                                    Tile tile = new Tile();
                                    if (xx * 2 >= map.Width)
                                        tile.X = xx * 2 - map.Width + 1;
                                    else
                                        tile.X = xx * 2;
                                    tile.Y = y;
                                    tile.PlotType = PlotTypes.FLAT;
                                    int tileByteCount = reader.ReadInt32();
                                    int riverInfo = reader.ReadInt16();
                                    int resource = reader.ReadInt32();
                                    byte image = reader.ReadByte();
                                    byte file = reader.ReadByte();
                                    int unknown = reader.ReadInt16();
                                    byte overlays = reader.ReadByte();
                                    byte terrain = reader.ReadByte();
                                    /*
                                        0a byte binary flags as indicated in overview
                                        0b nibble terrain: 0 = desert, 1 = plains, 2 = grassland,
                                        3 = tundra, 4 = floodplain, 5 = hills,
                                        6 = mountain, 7 = forest, 8 = jungle,
                                        9 = coast, a = sea, b = ocean
                                        nibble basic terrain: only 0,1,2,3,9,a,b
                                     */
                                    int terrainType = (int)(terrain / 16);
                                    int basicTerrain = terrain % 16;
                                    switch (terrainType)
                                    {
                                        case 0:
                                            tile.Terrain = TerrainTypes.TERRAIN_DESERT;
                                            break;
                                        case 1:
                                            tile.Terrain = TerrainTypes.TERRAIN_PLAINS;
                                            break;
                                        case 2:
                                            tile.Terrain = TerrainTypes.TERRAIN_GRASS;
                                            break;
                                        case 3:
                                            tile.Terrain = TerrainTypes.TERRAIN_TUNDRA;
                                            break;
                                        case 4:
                                            tile.Terrain = TerrainTypes.TERRAIN_DESERT;
                                            tile.FeatureType = FeatureTypes.FEATURE_FLOOD_PLAINS;
                                            break;
                                        case 5:
                                            tile.PlotType = PlotTypes.HILL;
                                            tile.Terrain = TerrainTypes.TERRAIN_GRASS;
                                            break;
                                        case 6:
                                            tile.PlotType = PlotTypes.PEAK;
                                            tile.Terrain = TerrainTypes.TERRAIN_GRASS;
                                            break;
                                        case 7:
                                            tile.FeatureType = FeatureTypes.FEATURE_FOREST;
                                            tile.Terrain = TerrainTypes.TERRAIN_GRASS;
                                            break;
                                        case 8:
                                            tile.FeatureType = FeatureTypes.FEATURE_JUNGLE;
                                            tile.Terrain = TerrainTypes.TERRAIN_GRASS;
                                            break;
                                        case 9:
                                            tile.Terrain = TerrainTypes.TERRAIN_COAST;
                                            break;
                                        case 10:
                                        case 11:
                                            tile.Terrain = TerrainTypes.TERRAIN_OCEAN;
                                            break;
                                        default:
                                            tile.Terrain = TerrainTypes.TERRAIN_GRASS;
                                            break;
                                    }
                                    switch (basicTerrain)
                                    {
                                        case 0:
                                            tile.Terrain = TerrainTypes.TERRAIN_DESERT;
                                            break;
                                        case 1:
                                            tile.Terrain = TerrainTypes.TERRAIN_PLAINS;
                                            break;
                                        case 2:
                                            tile.Terrain = TerrainTypes.TERRAIN_GRASS;
                                            break;
                                        case 3:
                                            tile.Terrain = TerrainTypes.TERRAIN_TUNDRA;
                                            break;
                                        case 9:
                                            tile.Terrain = TerrainTypes.TERRAIN_COAST;
                                            break;
                                        case 10:
                                        case 11:
                                            tile.Terrain = TerrainTypes.TERRAIN_OCEAN;
                                            break;
                                        default:
                                            break;
                                    }
                                    int bonuses = reader.ReadInt16();
                                    int barbCamp = reader.ReadInt16();
                                    int unknown2 = reader.ReadInt32();
                                    int continent = reader.ReadInt16();
                                    //byte unknown3 = reader.ReadByte();
                                    game.Map.SetTile(tile.X, tile.Y, tile);
                                }
                            }
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                }
                return game;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception reading map from file {0}. Error was: {1}", path, e.Message), e);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }


        private static Game ParseWorldbuilderFile(string path)
        {
            TextReader tr = null;
            int lineNumber = 0;
            string line = null;
            try
            {
                Game game = new Game();
                bool inGame = false;
                Map map = null;
                Tile tile = null;
                Unit unit = null;
                Team team = null;
                Player player = null;
                tr = new StreamReader(new FileStream(path, FileMode.Open));
                line = tr.ReadLine();
                while (line != null)
                {
                    lineNumber++;
                    line = line.Trim();
                    string tag = line.ToLower();
                    string value = null;
                    if (inGame)
                    {
                        if (tag.Equals("endgame"))
                            inGame = false;
                        else
                            game.UnparsedData.Add(line);
                    }
                    else if (map != null)
                    {
                        if (tag.Equals("endmap"))
                        {
                            map.SetDimensions(map.Width, map.Height);
                            game.Map = map;
                            map = null;
                        }
                        else if (ExtractValue(line, "grid width=", ref value))
                            map.Width = Int32.Parse(value);
                        else if (ExtractValue(line, "grid height=", ref value))
                            map.Height = Int32.Parse(value);
                        else if (ExtractValue(line, "wrap X=", ref value))
                            map.IsHorizontalWrap = (Int32.Parse(value) == 1);
                        else if (ExtractValue(line, "wrap Y=", ref value))
                            map.IsVerticalWrap = (Int32.Parse(value) == 1);
                        else if (ExtractValue(line, "num plots written=", ref value))
                            ;// Ignore this value, overwrite it with width*height when saving map
                        else if (ExtractValue(line, "num signs written=", ref value))
                            ;// Ignore this value, overwrite it with #of signs when saving map
                        else
                            map.UnparsedData.Add(line);
                    }
                    else if (team != null)
                    {
                        if (tag.Equals("endteam"))
                        {
                            game.Teams.Add(team);
                            team = null;
                        }
                        else if (ExtractValue(line, "TeamID=", ref value))
                            team.TeamId = Int32.Parse(value);
                        else
                            team.UnparsedData.Add(line);
                    }
                    else if (player != null)
                    {
                        if (tag.Equals("endplayer"))
                        {
                            Team t = game.GetTeamById(player.TeamId);
                            if (t == null) throw new Exception("Player object referring to unknown TeamId " + player.TeamId);
                            game.Players.Add(player);
                            player.PlayerId = game.Players.Count - 1;
                            player = null;
                        }
                        else if (ExtractValue(line, "Team=", ref value))
                            player.TeamId = Int32.Parse(value);
                        else if (ExtractValue(line, "LeaderType=", ref value))
                            player.LeaderType = value;
                        else if (ExtractValue(line, "CivType=", ref value))
                            player.CivType = value;
                        else if (ExtractValue(line, "Color=", ref value))
                            player.Color = (PlayerColors)Enum.Parse(typeof(PlayerColors), value);
                        else
                            player.UnparsedData.Add(line);
                    }
                    else if (tile != null)
                    {
                        if (unit != null)
                        {
                            //BeginUnit
                            //    UnitType=UNIT_SETTLER, UnitOwner=1
                            //    Damage=0
                            //    Level=1, Experience=0
                            //    FacingDirection=4
                            //    UnitAIType=UNITAI_SETTLE
                            //EndUnit
                            if (tag.Equals("endunit"))
                            {
                                tile.Units.Add(unit);
                                unit = null;
                            }
                            else if (ExtractValue(line, "UnitType=", ref value))
                            {
                                int i = value.IndexOf(", UnitOwner=");
                                if (i > 0)
                                {
                                    unit.UnitType = value.Substring(0, i).Trim();
                                    unit.UnitOwner = Int32.Parse(value.Substring(i + ", UnitOwner=".Length).Trim());
                                }
                                else
                                    unit.UnitType = value.Trim();
                            }
                            else
                                unit.UnparsedData.Add(line);
                        }
                        else if (tag.Equals("beginunit"))
                        {
                            unit = new Unit();
                        }
                        else if (tag.Equals("endplot"))
                        {
                            game.Map.SetTile(tile.X, tile.Y, tile);
                            tile = null;
                        }
                        else if (ExtractValue(line, "x=", ref value))
                        {
                            int i = value.IndexOf(",y=");
                            if (i > 0)
                            {
                                tile.X = Int32.Parse(value.Substring(0, i).Trim());
                                tile.Y = Int32.Parse(value.Substring(i + 3).Trim());
                            }
                        }
                        else if (ExtractValue(line, "TerrainType=", ref value))
                            tile.Terrain = (TerrainTypes)Enum.Parse(typeof(TerrainTypes), value);
                        else if (ExtractValue(line, "BonusType=", ref value))
                            tile.BonusType = (BonusTypes)Enum.Parse(typeof(BonusTypes), value);
                        else if (ExtractValue(line, "PlotType=", ref value))
                            tile.PlotType = (PlotTypes)Int32.Parse(value);
                        else if (ExtractValue(line, "RiverWEDirection=", ref value))
                            tile.RiverWEDirection = (RiverDirection)Int32.Parse(value);
                        else if (ExtractValue(line, "RiverNSDirection=", ref value))
                            tile.RiverNSDirection = (RiverDirection)Int32.Parse(value);
                        else if (tag.Equals("isnofriver"))
                            tile.IsNOfRiver = true;
                        else if (tag.Equals("iswofriver"))
                            tile.IsWOfRiver = true;
                        else if (ExtractValue(line, "FeatureType=", ref value))
                        {
                            int i = value.IndexOf(", FeatureVariety=");
                            if (i > 0)
                            {
                                tile.FeatureType = (FeatureTypes)Enum.Parse(typeof(FeatureTypes), value.Substring(0, i).Trim());
                                tile.FeatureVariety = Int32.Parse(value.Substring(i + ", FeatureVariety=".Length).Trim());
                            }
                            else
                            {
                                tile.FeatureType = (FeatureTypes)Enum.Parse(typeof(FeatureTypes), value);
                                tile.FeatureVariety = -1;
                            }
                        }
                        else
                            tile.UnparsedData.Add(line);
                    }
                    else if (tag.Equals("begingame"))
                        inGame = true;
                    else if (tag.Equals("beginteam"))
                        team = new Team();
                    else if (tag.Equals("beginplayer"))
                        player = new Player();
                    else if (tag.Equals("beginmap"))
                        map = new Map();
                    else if (tag.Equals("beginplot"))
                        tile = new Tile();
                    else if (ExtractValue(line, "Version=", ref value))
                        game.Version = Int32.Parse(value);
                    else
                        game.UnparsedOuterData.Add(line);
                    line = tr.ReadLine();
                }
                //BeginPlot
                //    x=1,y=2
                //    BonusType=BONUS_CLAM
                //    FeatureType=FEATURE_FOREST, FeatureVariety=1
                //    TerrainType=TERRAIN_GRASS
                //    PlotType=1
                //EndPlot
                //BeginMap
                //    grid width=50
                //    grid height=50
                //    top latitude=90
                //    bottom latitude=-90
                //    wrap X=1
                //    wrap Y=1
                //    world size=WORLDSIZE_STANDARD
                //    climate=CLIMATE_TEMPERATE
                //    sealevel=SEALEVEL_MEDIUM
                //    num plots written=2500
                //    num signs written=0
                //    Randomize Resources=false
                //EndMap
                game.Players.Add(new Player() { TeamId = -1, PlayerId = Game.BarbarianPlayerId, CivType = "BARBARIANS", Color = PlayerColors.PLAYERCOLOR_BLACK, LeaderType = "CONAN" });
                return game;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception reading map from file {0} (line {1}: {2}). Error was: {3}", path, lineNumber, line, e.Message), e);
            }
            finally
            {
                if (tr != null) tr.Close();
            }
        }

        private static bool ExtractValue(string line, string key, ref string value)
        {
            if (line.StartsWith(key))
            {
                value = line.Substring(key.Length).Trim();
                return true;
            }
            return false;
        }

    }
}
