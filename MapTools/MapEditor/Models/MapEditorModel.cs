using MapEngine;
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
                State.Layers = new ObservableCollection<Layer>();
                State.ActiveLayer = new Layer();
                State.ActiveLayer.LayerName = "Map";
                State.ActiveLayer.Map = value.Map;
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

        public void PerformMapOperation(MapOperation operation)
        {
            UndoHistory[UndoIndex].StateAfterOperation = State.Clone();
            State = operation.Execute(State);
            Game.Map = State.ActiveLayer.Map;
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

        public void PerformLayerOperation(MapLayerOperation operation)
        {
            UndoHistory[UndoIndex].StateAfterOperation = State.Clone();
            foreach (var lyr in State.Layers)
            {
                lyr.Map = operation.Execute(lyr.Map);
            }
            Game.Map = State.ActiveLayer.Map;
            AddEditHistoryEntry(operation.GetName());
        }

        public bool UndoTo(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException();
            if (index >= UndoHistory.Count) throw new IndexOutOfRangeException("Undo history is messed up");
            if (UndoIndex == index) return false;
            UndoIndex = index;
            State = UndoHistory[UndoIndex].StateAfterOperation;
            Game.Map = State.ActiveLayer.Map;
            return true;
        }
    }
}
