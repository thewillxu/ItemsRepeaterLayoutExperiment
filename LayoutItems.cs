using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Foundation;
using static ItemRepeaterShiftedLayoutExample.LayoutItems;

namespace ItemRepeaterShiftedLayoutExample
{
    public partial class LayoutItems
    {
        public class RectWithIndex
        {
            public int Index;
            public Rect Rect;
        }

        private List<Rect> Items = new List<Rect>();
        private int FirstIndex = 0;

        private int LastIndex { get => FirstIndex + Count - 1; }

        public int Count { get => Items.Count; }

        public void Clear()
        {
            Items.Clear();
            FirstIndex = 0;
        }

        public RectWithIndex FirstItem { get => new RectWithIndex { Rect = Items[0], Index = FirstIndex }; }

        public RectWithIndex LastItem { get => new RectWithIndex { Rect = Items[Items.Count - 1], Index = FirstIndex + Count - 1 }; }

        public RectWithIndex TryGetItem(int index)
        {
            if (Count > 0 && index >= FirstIndex && index <= LastIndex)
            {
                return new RectWithIndex { Rect = Items[index - FirstIndex], Index = index };
            }
            else
            {
                return null;
            }
        }

        public void SetItem(int index, Rect item)
        {
            // For safety, ensure that the index is either within the current range of values or it is increasing the range by 1.

            if (Count == 0)
            {
                Items.Insert(0, item);
                FirstIndex = index;
            }
            else if (index == FirstIndex - 1)
            {
                Items.Insert(0, item);
                FirstIndex = index;
            }
            else if (index >= FirstIndex && index < FirstIndex + Count)
            {
                Items[index - FirstIndex] = item;
            }
            else if (index == FirstIndex + Count)
            {
                Items.Add(item);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }
    }

    public partial class LayoutItems : IEnumerable<RectWithIndex>
    {
        public IEnumerator<RectWithIndex> GetEnumerator()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                yield return new RectWithIndex { Rect = Items[i], Index = FirstIndex + i };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
