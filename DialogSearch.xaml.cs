using MahApps.Metro.Controls;
using PcapDecrypt;
using PcapDecrypt.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LRFPacketDecoder
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogSearch : MetroWindow
    {
        private readonly CollectionViewSource viewSource = new CollectionViewSource();

        public class PacketItem 
        {
            public int Count { get; set; }
            public int HeadInt { get; set; }
            public PacketCmdS2C Head { get; set; }
            public string HeadS { get; set; } 
            public string Text
            {
                get
                {
                    return this.HeadS + " ( " + this.Count + " )";
                }
            }
        }

        public class PacketFilter : INotifyPropertyChanged
        {
            private string _searchText;

            public string SearchText
            {
                get { return _searchText; }
                set
                {
                    _searchText = value;

                    OnPropertyChanged("SearchText");
                    OnPropertyChanged("MyFilteredItems");
                }
            }

            public List<PacketItem> List { get; set; }

            public IEnumerable<PacketItem> MyFilteredItems
            {
                get
                {
                    if (string.IsNullOrEmpty(SearchText)) return this.List.OrderBy(x=>x.HeadS); 
                    return this.List.Where(x=> x.HeadS != null && x.HeadS.Trim().ToLower().IndexOf(this.SearchText.ToLower().Trim()) > -1).OrderBy(x=>x.HeadS);
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged(string name)
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public List<PacketItem> Selected = new List<PacketItem>();
        PacketFilter filter = new PacketFilter();

        public DialogSearch(Replay rply)
        {
            InitializeComponent();
            

            List<PacketItem> lst = new List<PacketItem>();

            foreach (var item in rply.WPackets)
            {
                var first = lst.Where(x => x.HeadInt == Convert.ToInt32(item.Head)).FirstOrDefault();
                if (first != null)
                {
                    first.Count++;
                }
                else
                {
                    lst.Add(new PacketItem() { Count = 1, Head = item.Head, HeadInt = Convert.ToInt32(item.Head), HeadS = item.SHead });
                }
            }
             
            
            filter.List = lst; 
            list.ItemsSource = filter.MyFilteredItems;

            containPlayers.ItemsSource = Statik.Players;
            firstPlayers.ItemsSource = Statik.Players;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void okey_Click(object sender, RoutedEventArgs e)
        {
            Selected.Clear();
            foreach (var item in list.Items)
            {
                ListBoxItem container = list.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                 
                if (container == null)
                    continue;

                var target = FindDescendant<CheckBox>(container);

                if (target != null && target.IsChecked == true)
                    Selected.Add((PacketItem)item);
            }
            this.DialogResult = true;
            this.Close();
             
        }

        public T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }

        private void allselect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list.Items)
            {
                ListBoxItem container = list.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container == null)
                    continue;

                var target = FindDescendant<CheckBox>(container);

                if (target != null)
                    target.IsChecked = true;
            }
        }

        private void txtsearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter.SearchText = txtsearch.Text;
            list.ItemsSource = filter.MyFilteredItems;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            firstPlayers.SelectedItem = null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            containPlayers.SelectedItem = null;
        }
    }
}
