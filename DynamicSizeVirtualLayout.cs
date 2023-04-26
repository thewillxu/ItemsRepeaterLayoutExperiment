﻿using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Networking.NetworkOperators;
using Windows.UI.Xaml;
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

    public class DynamicSizeVirtualLayout : VirtualizingLayout
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
           "Items",
           typeof(List<Item>),
           typeof(DynamicSizeVirtualLayout),
           new PropertyMetadata(null));

        private LayoutItems LayoutItems = new LayoutItems();
        private Anchor LastAnchor = null;
        private double AverageHeight = 100;
        private double AverageHeightAccumulator = 100;
        private StaticLayout StaticLayout = new StaticLayout();

        public List<Item> Items
        {
            get => (List<Item>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            Debug.WriteLine($"MeasureOverride: RealizationRect = {context.RealizationRect} LayoutOrigin = {context.LayoutOrigin}");

            StaticLayout.Init(Items, availableSize.Width);

            Anchor anchor = GetAnchor(context.RealizationRect, context.ItemCount);

            GenerateLayout(context, context.RealizationRect, availableSize, anchor);

            MeasureLayout(context);

            Rect extent = GetExtent(context, availableSize);

            context.LayoutOrigin = new Point(extent.Left, extent.Top);

            LastAnchor = anchor;

            Size result = new Size(extent.Width, extent.Height);

            return result;
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            ArrangeLayout(context);

            // This must happen after the MeasureOverride and ArrangeOverride to ensure that
            // both are using the same value of AverageHeight.
            RecalculateAverageHeight();

            return finalSize;
        }

        private Anchor GetAnchor(Rect realizationRect, int itemCount)
        {
            Anchor staticLayoutAnchor = StaticLayout.GetAnchor(realizationRect);
            if (staticLayoutAnchor != null)
            {
                return staticLayoutAnchor;
            }
            
            // Reuse the previous anchor if it is visible to help prevent an oscillation loop of the anchor
            // when recalculating the extent. Without this we may get into an infinite iteration where
            // 
            // * MeasureOverride called
            //   * Anchor is A
            //   * LayoutOrigin is set to Y
            // * MeasureOverride called
            //   * Anchor is calculated as A+1 (because the RealizationRect was shifted enough due to the change in LayoutOrigin)
            //   * LayoutOrigin is set to Y-Z
            // * MeasureOverride called
            //   * Anchor is calculated as A (because the RealizationRect shifted due to Y-Z causing A to be visible again)
            //   * LayoutOrigin is set to Y
            // Repeat ad infinitum
            if (LastAnchor != null &&
                LayoutItems.TryGetItem(LastAnchor.Index) is RectWithIndex anchorItem &&
                DoesIntersect(realizationRect, anchorItem.Rect))
            {
                Debug.WriteLine($"GetAnchor: reusing previous anchor {anchorItem.Index}, {anchorItem.Rect.Top}");
                return new Anchor(anchorItem.Index, anchorItem.Rect.Top);
            };

            // Reuse the first visible item from the previous layout.
            // This keeps the anchor consistent with the previous render so it doesn't jump around the screen
            if (GetVisibleItem(realizationRect) is RectWithIndex item)
            {
                Debug.WriteLine($"GetAnchor: reusing previous layout item {item.Index}, {item.Rect.Top}");
                return new Anchor(item.Index, item.Rect.Top);
            }

            // If we have no history then calculate a new anchor from the origin of 0.
            // This should also produce an Extent later that is at (or near) 0,0
            int estimatedIndex = Math.Min(itemCount - 1, Math.Max(0, (int)Math.Round(realizationRect.Top / AverageHeight)));
            double estimatedTop = estimatedIndex * AverageHeight;
            var anchor = new Anchor(estimatedIndex, estimatedTop);

            Debug.WriteLine($"GetAnchor: calculated new anchor {anchor.Index}, {anchor.Top}");
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

        private void MeasureLayout(VirtualizingLayoutContext context)
        {
            foreach (var item in LayoutItems)
            {
                var element = context.GetOrCreateElementAt(item.Index);
                element.Measure(new Size(item.Rect.Width, item.Rect.Height));
            }
            if (LayoutItems.Count > 0)
            {
                Debug.WriteLine($"MeasureLayout: {LayoutItems.FirstItem.Index} - {LayoutItems.LastItem.Index}");
            }
            else
            {
                Debug.WriteLine($"MeasureLayout: Nothing laid out");
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
            int count = LayoutItems.Count;
            if (count > 0)
            {
                double top = LayoutItems.FirstItem.Rect.Top;
                double bottom = LayoutItems.LastItem.Rect.Bottom;

                var viewHeight = bottom - top;
                var averageViewHeight = viewHeight / count;
                AverageHeightAccumulator = (AverageHeightAccumulator * 100 + averageViewHeight) / 101;

                // Reduce change frequency of AverageHeight to reduce oscillations due to recalculation.
                if (Math.Abs(AverageHeightAccumulator - AverageHeight) > 1)
                {
                    AverageHeight = AverageHeightAccumulator;
                    Debug.WriteLine($"RecalculateAverageHeight: *** {AverageHeight} ***");
                }
            }
        }

        private Rect GetExtent(VirtualizingLayoutContext context, Size availableSize)
        {
            Rect? staticExtent = StaticLayout.GetExtent();
            if (staticExtent.HasValue)
            {
                return staticExtent.Value;
            }
            
            if (LayoutItems.Count <= 0)
            {
                Debug.WriteLine($"GetExtent - No Items were laid out, resetting extent");
                return new Rect(0, 0, availableSize.Width, context.ItemCount * AverageHeight);
            }

            var firstItem = LayoutItems.FirstItem;
            var firstIndex = firstItem.Index;
            var firstTop = firstItem.Rect.Top;
            var estimatedTop = firstIndex * AverageHeight;
            var originOffset = firstTop - estimatedTop;

            var lastLayoutItem = LayoutItems.LastItem;
            var lastItemIndex = context.ItemCount - 1;
            var remainingItems = lastItemIndex - lastLayoutItem.Index;
            var bottom = lastLayoutItem.Rect.Bottom;
            var estimatedBottom = remainingItems >= 0 ? bottom + remainingItems * AverageHeight : bottom;
            var estimatedHeight = estimatedBottom - originOffset;

            var extent = new Rect(0, originOffset, availableSize.Width, estimatedHeight);

            Debug.WriteLine($"GetExtent: {extent}");

            return extent;
        }

        private bool DoesIntersect(Rect rectA, Rect rectB)
        {
            return rectA.Bottom >= rectB.Top && rectA.Top <= rectB.Bottom;
        }

        private void GenerateLayout(VirtualizingLayoutContext context, Rect realizationRect, Size availableSize, Anchor anchor)
        {
            LayoutItems staticLayout = StaticLayout.GetLayout(realizationRect, anchor.Index);
            if (staticLayout != null)
            {
                LayoutItems = staticLayout;
                return;
            }

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
