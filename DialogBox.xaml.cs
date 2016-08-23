using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace LRFPacketDecoder
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : MetroWindow
    {
        public DialogBox(string Title, string Caption, string Content , bool MultiLine = false)
        {
            InitializeComponent();
            this.Title = Title;
            label.Text = Caption;
            textBox.Text = Content;
            textBox.Focus();
            textBox.SelectAll();

            if (MultiLine)
            {
                textBox.TextWrapping = TextWrapping.Wrap;
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBox.AcceptsReturn = true;
                textBox.AcceptsTab = true; 
                textBox.VerticalContentAlignment = VerticalAlignment.Top;
                textBox.TextAlignment = TextAlignment.Left;
                this.SizeToContent = SizeToContent.Manual;
                this.Height = 400;
            }
            else
            {
                
                this.Height = 165;
                textBox.TextWrapping = TextWrapping.NoWrap;
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                textBox.AcceptsReturn = false;
                textBox.VerticalContentAlignment = VerticalAlignment.Center;
                textBox.TextAlignment = TextAlignment.Center;
                textBox.AcceptsTab = false;
                textBox.KeyDown += TextBox_KeyDown;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void okey_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
