using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Networking.NetworkOperators;
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
        private LayoutItems LayoutItems = new LayoutItems();
        private double AverageHeight = 100;

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            Debug.WriteLine($"MeasureOverride RealizationRect = {context.RealizationRect} LayoutOrigin = {context.LayoutOrigin}");

            Rect realizationRect = context.RealizationRect;
            realizationRect.Y -= context.LayoutOrigin.Y;

            Anchor anchor = GetAnchor(realizationRect);

            GenerateLayout(context, context.RealizationRect, availableSize, anchor);

            Rect extent = GetExtent(context, availableSize);

            MeasureLayout(context, extent);

            context.LayoutOrigin = new Point(extent.Left, extent.Top);

            RecalculateAverageHeight();

            Debug.WriteLine($"MeasureOverride extent = {extent}");

            return new Size(extent.Width, extent.Bottom);
        }
        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            ArrangeLayout(context);
            return finalSize;
        }

        private Anchor GetAnchor(Rect realizationRect)
        {
            RectWithIndex item = GetVisibleItem(realizationRect);
            if (item != null)
            {
                return new Anchor(item.Index, item.Rect.Top);
            }

            int estimatedIndex = Math.Max(0, (int)Math.Round(realizationRect.Top / AverageHeight));
            double estimatedTop = estimatedIndex * AverageHeight;
            var anchor = new Anchor(estimatedIndex, estimatedTop);

            Debug.WriteLine($"GetAnchor calculated new anchor {anchor.Index}, {anchor.Top}");
            return anchor;
        }

        private RectWithIndex GetVisibleItem(Rect realizationRect)
        {
            foreach (var item in LayoutItems)
            {
                if (DoesIntersect(item.Rect, realizationRect))
                {
                    return item;
                }
            }

            return null;
        }

        private void MeasureLayout(VirtualizingLayoutContext context, Rect extent)
        {
            foreach (var item in LayoutItems)
            {
                var element = context.GetOrCreateElementAt(item.Index);
                element.Measure(new Size(item.Rect.Width, item.Rect.Height));
            }
        }

        private void ArrangeLayout(VirtualizingLayoutContext context)
        {
            Point origin = context.LayoutOrigin;

            foreach (var item in LayoutItems)
            {
                var element = context.GetOrCreateElementAt(item.Index);

                Rect elementRect = item.Rect;
                elementRect.X -= origin.X;
                elementRect.Y -= origin.Y;
                element.Arrange(elementRect);
            }
        }

        private void RecalculateAverageHeight()
        {
            double top = LayoutItems.FirstItem.Rect.Top;
            double bottom = LayoutItems.LastItem.Rect.Bottom;
            int count = LayoutItems.Count;

            if (count > 0)
            {
                var viewHeight = bottom - top;
                var averageViewHeight = viewHeight / count;
                var newAverageHeight = (AverageHeight * 10 + averageViewHeight) / 11;
                newAverageHeight = Math.Round(newAverageHeight, 2);
                if (Math.Abs(newAverageHeight - AverageHeight) > 10)
                {
                    AverageHeight = newAverageHeight;
                    Debug.WriteLine($"AverageHeight = {AverageHeight}");
                }
            }
        }

        private Rect GetExtent(VirtualizingLayoutContext context, Size availableSize)
        {
            var firstItem = LayoutItems.FirstItem;
            var firstIndex = firstItem.Index;
            var firstTop = firstItem.Rect.Top;
            var estimatedTop = firstIndex * AverageHeight;
            var originOffset = firstTop - estimatedTop;

            var lastLayoutItem = LayoutItems.LastItem;
            var remainingItems = context.ItemCount - lastLayoutItem.Index;
            var bottom = lastLayoutItem.Rect.Bottom;
            var estimatedBottom = remainingItems >= 0 ? bottom + remainingItems * AverageHeight : bottom;

            return new Rect(0, originOffset, availableSize.Width, estimatedBottom);
        }

        private bool DoesIntersect(Rect rectA, Rect rectB)
        {
            return rectA.Bottom >= rectB.Top && rectA.Top <= rectB.Bottom;
        }

        private void GenerateLayout(VirtualizingLayoutContext context, Rect realizationRect, Size availableSize, Anchor anchor)
        {
            LayoutItems.Clear();
            GenerateLayoutUp(context, realizationRect, availableSize, anchor.Top, anchor.Index - 1);
            GenerateLayoutDown(context, realizationRect, availableSize, anchor.Top, anchor.Index);
        }

        private void GenerateLayoutUp(VirtualizingLayoutContext context, Rect realizationRect, Size availableSize, double bottom, int index)
        {
            while (bottom >= realizationRect.Top && index >= 0)
            {
                Item item = context.GetItemAt(index) as Item;
                var width = availableSize.Width;
                var height = item.Height;

                LayoutItems.SetItem(index, new Rect(0, bottom - height, width, height));

                bottom -= height;
                index--;
            }
        }

        private void GenerateLayoutDown(VirtualizingLayoutContext context, Rect realizationRect, Size availableSize, double top, int index)
        {
            while (top <= realizationRect.Bottom && index < context.ItemCount)
            {
                Item item = context.GetItemAt(index) as Item;
                var width = availableSize.Width;
                var height = item.Height;

                LayoutItems.SetItem(index, new Rect(0, top, width, height));

                top += height;
                index++;
            }
        }
    }
}
