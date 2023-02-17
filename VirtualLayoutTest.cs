using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.Foundation;

namespace ItemRepeaterShiftedLayoutExample
{

    public class VirtualLayoutTest : VirtualizingLayout
    {
        private struct Anchor
        {
            public Anchor(int index, double top)
            {
                Index = index;
                Top = top;
            }

            public readonly int Index;
            public readonly double Top;
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            Debug.WriteLine($"MeasureOverride {context.RealizationRect}");

            Anchor anchor = GetAnchor(context);
            var (lastIndex, lastBottom) = GenerateLayout(context, availableSize, anchor);

            var remainingItems = context.ItemCount - lastIndex;
            var estimatedBottom = remainingItems >= 0 ? lastBottom + remainingItems * AverageHeight : lastBottom;

            return new Size(availableSize.Width, estimatedBottom);
        }

        private Anchor GetAnchor(VirtualizingLayoutContext context)
        {
            int estimatedIndex = Math.Max(0, (int)Math.Round(context.RealizationRect.Top / AverageHeight));
            double estimatedTop = estimatedIndex * AverageHeight;
            return new Anchor(estimatedIndex, estimatedTop);
        }

        private (int, double) GenerateLayout(VirtualizingLayoutContext context, Size availableSize, Anchor anchor)
        {
            var currentTop = anchor.Top;
            var index = anchor.Index;

            while(currentTop <= context.RealizationRect.Bottom && index < context.ItemCount)
            {
                var element = context.GetOrCreateElementAt(index);
                Item item = context.GetItemAt(index) as Item;
                var width = availableSize.Width;
                var height = item.Height;
                element.Measure(new Size(width, height));
                element.Arrange(new Rect(0, currentTop, width, height));

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

        private double AverageHeight = 100;
    }
}
