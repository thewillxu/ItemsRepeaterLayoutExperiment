using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using static ItemRepeaterShiftedLayoutExample.LayoutItems;

namespace ItemRepeaterShiftedLayoutExample
{

    public class Anchor
    {
        public Anchor(int index, double top)
        {
            Index = index;
            Top = top;
        }

        public readonly int Index;
        public readonly double Top;
    }

    public class VirtualLayoutTest : VirtualizingLayout
    {
        private Anchor LastAnchor;
        private LayoutItems LayoutItems = new LayoutItems();
        private double AverageHeight = 100;

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            Debug.WriteLine($"MeasureOverride {context.RealizationRect}");

            Anchor anchor = GetAnchor(context);
            var (lastIndex, lastBottom) = GenerateLayout(context, availableSize, anchor);

            MeasureAndArrangeLayout(context);

            var remainingItems = context.ItemCount - lastIndex;
            var estimatedBottom = remainingItems >= 0 ? lastBottom + remainingItems * AverageHeight : lastBottom;

            return new Size(availableSize.Width, estimatedBottom);
        }

        private void MeasureAndArrangeLayout(VirtualizingLayoutContext context)
        {
            foreach (var item in LayoutItems)
            {
                var element = context.GetOrCreateElementAt(item.Index);
                element.Measure(new Size(item.Rect.Width, item.Rect.Height));
                element.Arrange(item.Rect);
            }
        }

        private Anchor GetAnchor(VirtualizingLayoutContext context)
        {
            RectWithIndex item = GetVisibleItem(context);
            if (item != null)
            {
                return new Anchor(item.Index, item.Rect.Top);
            }

            int estimatedIndex = Math.Max(0, (int)Math.Round(context.RealizationRect.Top / AverageHeight));
            double estimatedTop = estimatedIndex * AverageHeight;
            LastAnchor = new Anchor(estimatedIndex, estimatedTop);
            return LastAnchor;
        }

        private RectWithIndex GetVisibleItem(VirtualizingLayoutContext context)
        {
            foreach (var item in LayoutItems)
            {
                if (DoesIntersect(item.Rect, context.RealizationRect))
                {
                    return item;
                }
            }

            return null;
        }

        private bool DoesIntersect(Rect rectA, Rect rectB)
        {
            return rectA.Bottom >= rectB.Top && rectA.Top <= rectB.Bottom;
        }

        private (int, double) GenerateLayout(VirtualizingLayoutContext context, Size availableSize, Anchor anchor)
        {
            var currentTop = anchor.Top;
            var index = anchor.Index;

            LayoutItems.Clear();

            while (currentTop <= context.RealizationRect.Bottom && index < context.ItemCount)
            {
                Item item = context.GetItemAt(index) as Item;
                var width = availableSize.Width;
                var height = item.Height;

                LayoutItems.SetItem(index, new Rect(0, currentTop, width, height));

                currentTop += height;
                index++;
            }

            var viewCount = index - anchor.Index;
            if (viewCount > 0)
            {
                var viewHeight = currentTop - context.RealizationRect.Top;
                var averageViewHeight = viewHeight / viewCount;
                var newAverageHeight = (AverageHeight * 10 + averageViewHeight) / 11;
                if (newAverageHeight != AverageHeight)
                {
                    AverageHeight = newAverageHeight;
                    Debug.WriteLine($"AverageHeight = {AverageHeight}");
                }
            }

            return (index, currentTop);
        }

    }
}
