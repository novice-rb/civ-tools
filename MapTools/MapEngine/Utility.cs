using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    public class Utility
    {
        private class ShuffleItem<T> : IComparable<ShuffleItem<T>>
        {
            public T item;
            double randValue;

            public ShuffleItem(T item, Random r)
            {
                this.item = item;
                this.randValue = r.NextDouble();
            }

            public int CompareTo(ShuffleItem<T> other)
            {
                return this.randValue.CompareTo(other.randValue);
            }
        }

        public static List<T> ShuffleList<T>(List<T> list)
        {
            Random r = new Random();
            List<ShuffleItem<T>> items = new List<ShuffleItem<T>>();
            foreach (var item in list) items.Add(new ShuffleItem<T>(item, r));
            items.Sort();
            List<T> result = new List<T>();
            foreach (var item in items) result.Add(item.item);
            return result;
        }

        public static void SwapTiles(Tile t1, Tile t2)
        {
            Tile tmp = (Tile)t1.Clone();
            _CopyTileInfo(t1, t2);
            _CopyTileInfo(t2, tmp);
        }

        private static void _CopyTileInfo(Tile to, Tile from)
        {
            to.BonusType = from.BonusType;
            to.FeatureType = from.FeatureType;
            to.FeatureVariety = from.FeatureVariety;
            to.FreshWaterType = from.FreshWaterType;
            to.PlotType = from.PlotType;
            to.Terrain = from.Terrain;
        }

    }
}
