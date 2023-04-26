using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using static ItemRepeaterShiftedLayoutExample.LayoutItems;

namespace ItemRepeaterShiftedLayoutExample
{
    internal class StaticLayout
    {
        private double _width = 0;
        private Task<LayoutItems> _layout = null;
        private readonly LayoutItems _layoutItems = new LayoutItems();

        public void Init(List<Item> items, double width)
        {
            if (_width != width)
            {
                _width = width;
                _layoutItems.Clear();
                _layout = Task<Rect>.Run(() => CalcLayout(items, _width));
            }
        }

        public Rect? GetExtent()
        {
            if (_layout == null || !_layout.IsCompleted)
            {
                Debug.WriteLine($"StaticLayout::GetExtent - static layout NOT ready yet");
                return null;
            }

            if (_layoutItems.Count == 0)
            {
                return new Rect(0, 0, _width, 0);
            }
            else
            {
                Rect extent = new Rect(0, 0, _width, _layoutItems.LastItem.Rect.Bottom);
                Debug.WriteLine($"StaticLayout::GetExtent - static layout ready. Extent = {extent}");
                return extent;
            }

        }

        public Anchor GetAnchor(Rect realizationRect)
        {
            if (_layout == null || !_layout.IsCompleted)
            {
                Debug.WriteLine($"StaticLayout::GetAnchor - static layout NOT ready yet");
                return null;
            }

            if (GetVisibleItem(realizationRect) is RectWithIndex item)
            {
                Anchor anchor = new Anchor(item.Index, item.Rect.Top);
                Debug.WriteLine($"StaticLayout::GetAnchor - static layout ready. Anchor = {anchor}");
                return anchor;
            }
            else
            {
                Debug.WriteLine($"StaticLayout: NO anchor!");
                return new Anchor(0, 0);
            }
        }

        public LayoutItems GetLayout(Rect realizationRect, int anchorIndex)
        {
            if (_layout == null || !_layout.IsCompleted)
            {
                Debug.WriteLine($"StaticLayout::GetLayout - static layout NOT ready yet");
                return null;
            }

            LayoutItems layoutItems = new LayoutItems();
            
            for (int index = anchorIndex; index < _layoutItems.Count; ++index)
            {
                Rect itemRect = _layoutItems.TryGetItem(index).Rect;

                if (DoesIntersect(itemRect, realizationRect))
                {
                    layoutItems.SetItem(index, itemRect);
                }
                else
                { 
                    break; 
                }
            }
            Debug.WriteLine($"StaticLayout::GetLayout - static layout ready. Return LayoutItems.Count={layoutItems.Count}");
            return layoutItems;
        }

        
        private LayoutItems CalcLayout(List<Item> items, double width)
        {
            int index = 0;
            double top = 0;

            // Simulate delay
            Task.Delay(10 * 1000);

            foreach (var item in items)
            {
                var height = item.Height;

                _layoutItems.SetItem(index, new Rect(0, top, width, height));

                top += height;
                index++;
            }

            return _layoutItems;
        }

        private RectWithIndex GetVisibleItem(Rect realizationRect)
        {
            foreach (var item in _layoutItems)
            {
                if (DoesIntersect(item.Rect, realizationRect))
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
    }
}
