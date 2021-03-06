﻿using MapEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MapEditor
{
    class MapEditorModel
    {
        public MapState State { get; set; }
        public ObservableCollection<EditOperation> UndoHistory { get; set; }
        public int UndoIndex { get; set; }
        private Game _Game;
        public Game Game {
            get
            {
                return _Game;
            }
            set
            {
                _Game = value;
                State.Selection = new Selection();
                State.Layers = new ObservableCollection<Layer>();
                State.ActiveLayer = new Layer();
                State.ActiveLayer.LayerName = "Map";
                State.ActiveLayer.Visible = true;
                State.ActiveLayer.Map = _Game.Map;
                State.Layers.Add(State.ActiveLayer);
                UndoHistory = new ObservableCollection<EditOperation>();
                UndoHistory.Add(new EditOperation() { Name = "Open map", StateAfterOperation = State });
                UndoIndex = 0;
            }
        }
        public string Title { get; set; }
        public string TitleWithSize {
            get
            {
                if (Game == null || Game.Map == null) return Title;
                return Title + string.Format(" - ({0}x{1} tiles)", Game.Map.Width, Game.Map.Height);
            }
        }
        public StartingLocationPlacer Placer { get; set; }

        public MapEditorModel() {
            State = new MapState();
            State.Layers = new ObservableCollection<Layer>();
            UndoHistory = new ObservableCollection<EditOperation>();
            Placer = null;
            System.Reflection.Assembly asm = this.GetType().Assembly;
            System.Reflection.AssemblyName name = asm.GetName();
            Title = "MapTuner build " + name.Version.ToString();
        }

        private Map MergeLayers()
        {
            Map map = (Map)State.Layers.Last().Map.Clone();
            foreach (var tile in map.GetAllTiles())
                tile.IsEmpty = true;
            foreach (var lyr in State.Layers)
            {
                if (!lyr.Visible) continue;
                foreach (var tile in lyr.Map.GetAllTiles())
                {
                    if (!tile.IsEmpty && map.GetTile(tile.X, tile.Y).IsEmpty)
                        map.SetTile(tile.X, tile.Y, tile);
                }
            }
            foreach (var tile in map.GetAllTiles())
            {
                if (tile.IsEmpty)
                {
                    tile.IsEmpty = false;
                    tile.Terrain = TerrainTypes.TERRAIN_SNOW;
                    tile.Units = new List<Unit>();
                    tile.PlotType = PlotTypes.FLAT;
                    tile.FeatureType = FeatureTypes.FEATURE_NONE;
                    tile.FeatureVariety = 0;
                    tile.BonusType = BonusTypes.BONUS_NONE;
                    tile.IsNOfRiver = false;
                    tile.IsWOfRiver = false;
                }
            }
            return map;
        }

        public void SaveGame(string filename)
        {
            Game.Map = MergeLayers();
            WorldBuilderParser.ToWorldbuilderFile(Game, filename);
        }

        public void PerformMapOperation(MapOperation operation)
        {
            UndoHistory[UndoIndex].StateAfterOperation = State.Clone();
            State = operation.Execute(State, State.Selection);
            AddEditHistoryEntry(operation.GetName());
        }

        public void PerformLayerOperation(MapLayerOperation operation)
        {
            UndoHistory[UndoIndex].StateAfterOperation = State.Clone();
            foreach (var lyr in State.Layers)
            {
                lyr.Map = operation.Execute(lyr.Map, State.Selection);
            }
            State.Selection = operation.CreateNewSelection(State.Selection);
            AddEditHistoryEntry(operation.GetName());
        }

        public void PerformSelectionOperation(MapSelectOperation operation)
        {
            UndoHistory[UndoIndex].StateAfterOperation = State.Clone();
            State.Selection = operation.Execute(State.ActiveLayer.Map, State.Selection);
            AddEditHistoryEntry(operation.GetName());
        }

        private void AddEditHistoryEntry(string operationName)
        {
            EditOperation op = new EditOperation();
            op.StateAfterOperation = State;
            op.Name = operationName;
            UndoIndex++;
            while (UndoHistory.Count > UndoIndex) UndoHistory.RemoveAt(UndoIndex);
            UndoHistory.Add(op);
        }

        public bool UndoTo(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException();
            if (index >= UndoHistory.Count) throw new IndexOutOfRangeException("Undo history is messed up");
            if (UndoIndex == index) return false;
            UndoIndex = index;
            State = UndoHistory[UndoIndex].StateAfterOperation;
            return true;
        }
    }
}
