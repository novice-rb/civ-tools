using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public class StartingLocationPlacer
    {
        private Dictionary<string, double> _FairnessDict = new Dictionary<string, double>();
        private PrioQueue<MapState, double> _States = new PrioQueue<MapState, double>();
        private double _TopResultUnfairness = double.MaxValue;
        private Map _Map = null;
        private bool _Stop = false;
        private bool _IsRunning = false;
        private System.Threading.Semaphore _StoppedSignal = null;
        private BalanceCheckerSettings _balanceCheckerSettings = null;
        private List<int> _FixedStarts = new List<int>();
        public MapState TopResult = null;
        public int NumberOfPositionsConsidered { get { return _FairnessDict.Count; } }
        public double TopResultUnfairness { get { return _TopResultUnfairness; } }
        public event EventHandler Progress;
        private DateTime _LastProgressTime;
        public TimeSpan ProgressFrequency = new TimeSpan(0, 0, 10);
        private int _MinimumDistanceBetweenCapitals;

        public void FindFairStartingLocations(Map map, BalanceCheckerSettings settings, List<int> fixedStarts, int minimumCapitalDistance)
        {
            _MinimumDistanceBetweenCapitals = minimumCapitalDistance;
            _FixedStarts = fixedStarts;
            _Map = (Map)map.Clone();
            _balanceCheckerSettings = settings;
            _States = new PrioQueue<MapState, double>();
            _States.Enqueue(MapState.FromMap(map), 0);
            TopResult = null;
            _FairnessDict = new Dictionary<string, double>();
            Start();
        }

        private void Start()
        {
            _Stop = false;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(Solve));
            t.Start();
        }

        public void Stop()
        {
            _Stop = true;
            _LastProgressTime = DateTime.Now;
            _StoppedSignal = new System.Threading.Semaphore(1, 1);
            _StoppedSignal.WaitOne();
        }

        public void Continue()
        {
            Start();
        }

        public bool IsRunning()
        {
            return _IsRunning;
        }

        private void Solve()
        {
            _IsRunning = true;
            while (!_Stop && !_States.IsEmpty())
            {
                MapState ms = _States.Dequeue();
                string key = ms.ToString();
                if (_FairnessDict.ContainsKey(key) && !double.IsNaN(_FairnessDict[key])) continue; // State already visited
                BalanceReport br = BalanceChecker.CheckBalance(_Map, _balanceCheckerSettings, ms);
                _FairnessDict[key] = br.Unfairness;
                if (br.Unfairness < _TopResultUnfairness)
                {
                    _TopResultUnfairness = br.Unfairness;
                    TopResult = ms;
                }
                foreach (MapState n in ms.GetNeighbouringStates(_Map, _FixedStarts, _MinimumDistanceBetweenCapitals))
                {
                    string k = n.ToString();
                    if (!_FairnessDict.ContainsKey(k))
                    {
                        _States.Enqueue(n, br.Unfairness); //100000.0 / br.Unfairness);
                        _FairnessDict[k] = double.NaN;
                    }
                }
                if (DateTime.Now - _LastProgressTime > ProgressFrequency)
                {
                    if(Progress != null) Progress(this, EventArgs.Empty);
                    _LastProgressTime = DateTime.Now;
                }
            }
            if(_StoppedSignal != null)
                _StoppedSignal.Release();
            _StoppedSignal = null;
            _IsRunning = false;
        }
    }
}
