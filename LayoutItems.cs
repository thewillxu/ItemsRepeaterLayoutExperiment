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
        private int StartIndex = 0;

        public Rect this[int i] => Items[i - StartIndex];

        public int Count { get => Items.Count; }

        public void Clear()
        {
            Items.Clear();
            StartIndex = 0;
        }

        public void SetItem(int index, Rect item)
        {
            // For safety, ensure that the index is either within the current range of values or it is increasing the range by 1.

            if (Count == 0)
            {
                Items.Insert(0, item);
                StartIndex = index;
            }
            else if (index == StartIndex - 1)
            {
                Items.Insert(0, item);
                StartIndex = index;
            }
            else if (index >= StartIndex && index < StartIndex + Count)
            {
                Items[index - StartIndex] = item;
            }
            else if (index == StartIndex + Count)
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
                yield return new RectWithIndex { Rect = Items[i], Index = StartIndex + i };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
