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

    }
}
