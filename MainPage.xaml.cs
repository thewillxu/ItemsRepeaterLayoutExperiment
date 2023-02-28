using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Composition;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ItemRepeaterShiftedLayoutExample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        internal List<Item> Items;

        public MainPage()
        {
            Random random = new Random();
            Items = new List<Item>();
            for (int i = 0; i < 1000; i++)
            {
                byte randomColorR = (byte)random.Next(0, 255);
                byte randomColorG = (byte)random.Next(0, 255);
                byte randomColorB = (byte)random.Next(0, 255);
                Color randomColor = Color.FromArgb(255, randomColorR, randomColorG, randomColorB);
                Items.Add(new Item() { Height = random.Next(50, 200), Text = i.ToString(), Color = randomColor });
            }

            this.InitializeComponent();
        }

        public void ScrollToTop()
        {
            Animated_ScrollViewer.ChangeView(null, 0, null, true);
        }

        public void ScrollToBottom()
        {
            Animated_ScrollViewer.ChangeView(null, Animated_ScrollViewer.ExtentHeight, null, true);
        }    
    }
}
