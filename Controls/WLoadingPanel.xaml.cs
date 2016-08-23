using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LRFPacketDecoder.Controls
{
    /// <summary>
    /// Interaction logic for WLoadingPanel.xaml
    /// </summary>
    public partial class WLoadingPanel : UserControl
    {
        public WLoadingPanel()
        {
            InitializeComponent();
        }

        public void Show(string Title, string Caption)
        {
            this.txtContent.Text = Caption;
            this.txtTitle.Text = Title;
            this.Visibility = Visibility.Visible;
        }
        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }
        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
