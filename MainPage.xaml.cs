using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

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
            for(int i = 0;i < 1000; i++)
            {
                Items.Add(new Item() { Height = random.Next(50, 200), Text = i.ToString() });
            }

            this.InitializeComponent();
        }
    }
}
