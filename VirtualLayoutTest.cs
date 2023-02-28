using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Reflection;
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

            Anchor anchor = GetAnchor(context);
            GenerateLayout(context, availableSize, anchor);

            RecalculateAverageHeight();

            MeasureAndArrangeLayout(context);

            var lastLayoutItem = LayoutItems.LastItem;
            var remainingItems = context.ItemCount - lastLayoutItem.Index;
            var bottom = lastLayoutItem.Rect.Bottom;
            var estimatedBottom = remainingItems >= 0 ? bottom + remainingItems * AverageHeight : bottom;

            return new Size(availableSize.Width, estimatedBottom);
        }

        private void MeasureAndArrangeLayout(VirtualizingLayoutContext context)
        {
            foreach (var item in LayoutItems)
            {
                var element = context.GetOrCreateElementAt(item.Index);
                element.Measure(new Size(item.Rect.Width, item.Rect.Height));
                element.Arrange(item.Rect);
                // Debug.WriteLine($"MeasureAndArrangeLayout {item.Index} = {item.Rect}");
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
            var anchor = new Anchor(estimatedIndex, estimatedTop);

            Debug.WriteLine($"GetAnchor calculated new anchor {anchor.Index}, {anchor.Top}");
            return anchor;
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

        private void GenerateLayout(VirtualizingLayoutContext context, Size availableSize, Anchor anchor)
        {
            LayoutItems.Clear();
            GenerateLayoutUp(context, availableSize, anchor.Top, anchor.Index - 1);
            GenerateLayoutDown(context, availableSize, anchor.Top, anchor.Index);
        }

        private void GenerateLayoutUp(VirtualizingLayoutContext context, Size availableSize, double bottom, int index)
        {
            while (bottom >= context.RealizationRect.Top && index >= 0)
            {
                Item item = context.GetItemAt(index) as Item;
                var width = availableSize.Width;
                var height = item.Height;

                LayoutItems.SetItem(index, new Rect(0, bottom - height, width, height));

                bottom -= height;
                index--;
            }
        }

        private void GenerateLayoutDown(VirtualizingLayoutContext context, Size availableSize, double top, int index)
        {
            while (top <= context.RealizationRect.Bottom && index < context.ItemCount)
            {
                Item item = context.GetItemAt(index) as Item;
                var width = availableSize.Width;
                var height = item.Height;

                LayoutItems.SetItem(index, new Rect(0, top, width, height));

                top += height;
                index++;
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
                if (newAverageHeight != AverageHeight)
                {
                    AverageHeight = newAverageHeight;
                    Debug.WriteLine($"AverageHeight = {AverageHeight}");
                }
            }
        }
    }
}
