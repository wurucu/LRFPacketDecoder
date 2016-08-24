using LRFPacketDecoder.ParserClass;
using Microsoft.Win32;
using PcapDecrypt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using WTool;

namespace LRFPacketDecoder.Editor
{
   
    /// <summary>
    /// Interaction logic for PacketEditor.xaml
    /// </summary>
    public partial class PacketEditor : UserControl
    {
        PacketPro pro;
        WPacket packet;

        Thread THcheckServer;
        public string saveFilePath = "";

        public PacketEditor(WPacket packet)
        {
            this.packet = packet;
            pro = new PacketPro(packet);
            pro.onReadPropError += Pro_onReadPropError;
            pro.onReadPropComplete += Pro_onReadPropComplete;


            InitializeComponent();
        }

        bool FLoaded = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!FLoaded)
            {
                if (pro.Props != null && pro.Props.Count > 0)
                    indxID = pro.Props.OrderByDescending(x => x.ID).FirstOrDefault().ID + 1;

                Description.Text = pro.PacketDescription;
                HeadName.Text = "Head  : " + pro.Head.ToString() + " , HeadInt : " + Convert.ToInt32(pro.Head) + " , Length : " + packet.Length;

                pro.LoadPropsValues();
                loadPropsList();
                UpdateHexEdit(0);
                FLoaded = true;
                CheckServer();
            }
        }

        Thread thUpdateHex = null;
        public void UpdateHexEdit(int LastPosition, List<PacketPro.HexStartEnd> poss = null)
        {
            try
            {
                if (thUpdateHex != null)
                    thUpdateHex.Abort();
                thUpdateHex = null;
            }
            catch (Exception)
            {  }
            
            hex1.Document.Blocks.Clear();
            hex2.Document.Blocks.Clear();

            thUpdateHex = new Thread(() =>
            {
                StringBuilder sb1 = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();

                foreach (var item in this.packet.Bytes)
                    sb1.Append(item.ToString("X2") + " ");
                foreach (var item in this.packet.Bytes)
                    sb2.Append(Encoding.UTF8.GetString(new byte[] { item }));

                Dispatcher.Invoke(() =>
                {
                    hex1.AppendText(sb1.ToString());
                    hex2.AppendText(sb2.ToString());
                    setColor(hex1, 0, (LastPosition * 2) + LastPosition - 1, (Color)ColorConverter.ConvertFromString("#FF3399FF"));
                    setColor(hex2, 0, LastPosition, (Color)ColorConverter.ConvertFromString("#FF3399FF"));
                }); 
            });
            thUpdateHex.Start();
        }

        public void setColor(RichTextBox txt, int startIndex, int endIndex, Color color)
        {
            try
            {
                TextRange tr = new TextRange(txt.Document.ContentStart, txt.Document.ContentEnd);
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                TextSelection textRange = txt.Selection;
                TextPointer start = txt.Document.ContentStart;
                TextPointer startPos = GetTextPointAt(start, startIndex);
                TextPointer endPos = GetTextPointAt(start, endIndex);
                textRange.Select(startPos, endPos);
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));
            }
            catch (Exception)
            {
            }
        }

        private static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            TextPointer ret = from;
            int i = 0;

            while ((i < pos) && (ret != null))
            {
                if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
                    i++;

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }

        private void Pro_onReadPropComplete(int LastPosition, List<PacketPro.HexStartEnd> poss)
        {
            message.Text = "";
            UpdateHexEdit(LastPosition, poss);
        }

        private void Pro_onReadPropError(PacketProp prop, string Error)
        {
            message.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            message.Text = "Name : " + prop.Name + ", Type : " + prop.PType.ToString() + Environment.NewLine + "Message:" + Error;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pro.PacketDescription = Description.Text;
            pro.Save(false);
        }

        public string getNewName()
        {
            int indx = 1;
            bool exist = pro.Props.Where(x => x.Name.Trim().ToLower() == "unk" + indx).Count() > 0;
            while (exist)
            {
                indx++;
                exist = pro.Props.Where(x => x.Name.Trim().ToLower() == "unk" + indx).Count() > 0;
                if (exist == false)
                    return "Unk" + indx;
            }
            return "Unk" + indx;
        }

        int indxID = 1;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (pro.Props == null)
                pro.Props = new List<PacketProp>();

            var ppro = new PacketProp();
            ppro.ID = indxID++;
            ppro.Description = "";
            ppro.Name = getNewName();
            ppro.PType = EPacketPropType.INT;
            pro.Props.Add(ppro);

            pro.LoadPropsValues();
            loadPropsList();
            PacketPropList.Items.Refresh();
        }

        private void loadPropsList()
        {
            if (pro.Props == null)
                return;
            PacketPropList.ItemsSource = pro.Props;
            //PacketPropList.Items.Clear();
            //if (pro.Props == null)
            //    return;
            //foreach (var item in pro.Props)
            //{
            //    PacketPropList.Items.Add(item);
            //}
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var pp = (PacketProp)((TextBlock)sender).DataContext;
                DialogBox db = new DialogBox(pp.Name, "Name", pp.Name);
                db.Owner = Window.GetWindow(this);
                if (db.ShowDialog() == true)
                    pp.Name = db.textBox.Text;
                PacketPropList.Items.Refresh();
            }
        }
        private void TextBlock2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var pp = (PacketProp)((TextBlock)sender).DataContext;
                DialogBox db = new DialogBox(pp.Name, "Açıklama", pp.Description);
                db.Owner = Window.GetWindow(this);
                if (db.ShowDialog() == true)
                    pp.Description = db.textBox.Text;
                PacketPropList.Items.Refresh();
            }
        }
        private void TextBlock3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var pp = (PacketProp)((TextBlock)sender).DataContext;
                DialogBox db = new DialogBox(pp.Name, "Length", pp.Length.ToString());
                db.Owner = Window.GetWindow(this);
                if (db.ShowDialog() == true)
                    pp.Length = Convert.ToInt32(db.textBox.Text);
                pro.LoadPropsValues();
                PacketPropList.Items.Refresh();
            }
        }

        private void CheckServer()
        {
            THcheckServer = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        uint ServerNetID = 0;
                        WClient wc = new WClient("127.0.0.1", 5999);
                        byte[] ret = wc.SendReceive(new byte[] { 0 }); // get netid proc
                        wc.Close();
                        BinaryReader rdr = new BinaryReader(new MemoryStream(ret));
                        rdr.ReadByte();
                        ServerNetID = rdr.ReadUInt32();
                        Statik.LastServerNetID = ServerNetID;
                        Dispatcher.Invoke(() =>
                        {
                            btnSendServer.IsEnabled = true;
                        });
                    }
                    catch (Exception)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            btnSendServer.IsEnabled = false;
                        });
                    }
                    Thread.Sleep(1000);
                }
            });
            THcheckServer.Start();
        }

        private void PacketPropList_Drop(object sender, DragEventArgs e)
        {
            ListBox parent = sender as ListBox;
            PacketProp data = e.Data.GetData(typeof(PacketProp)) as PacketProp;
            PacketProp objectToPlaceBefore = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as PacketProp;
            if (data != null && objectToPlaceBefore != null && data.ID != objectToPlaceBefore.ID)
            {
                int index = pro.Props.IndexOf(objectToPlaceBefore);
                pro.Props.Remove(data);
                pro.Props.Insert(index, data);
                PacketPropList.SelectedItem = null;
                pro.LoadPropsValues();
                loadPropsList();
                PacketPropList.Items.Refresh();
            }
        }

        private static object GetObjectDataFromPoint(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    if (element == source)
                        return null;
                }
                if (data != DependencyProperty.UnsetValue)
                    return data;
            }

            return null;
        }


        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = PacketPropList;
            PacketProp data = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as PacketProp;
            if (data != null)
            {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }

        private void PacketPropList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox && (sender as ComboBox).IsDropDownOpen && (sender as ComboBox).SelectedItem != null)
            {
                var type = ((EPacketPropType)(((ComboBox)sender).SelectedItem));
                PacketProp prop = (PacketProp)(((ComboBox)sender).DataContext);
                prop.PType = type;
                pro.LoadPropsValues();
                PacketPropList.Items.Refresh();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var pp = (PacketProp)((Button)sender).DataContext;
            pro.Props.Remove(pp);
            pro.LoadPropsValues();
            loadPropsList();
            PacketPropList.Items.Refresh();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            pro.PacketDescription = Description.Text;
            pro.Save(true);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            pro.Props.Clear();
            pro.LoadPropsValues();
            loadPropsList();
            PacketPropList.Items.Refresh();
        }

        private void btnSendServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bytes = this.pro.getBytes();
                byte[] tmp = new byte[bytes.Length + 1];
                tmp[0] = 1;
                Array.Copy(bytes, 0, tmp, 1, bytes.Length);
                WClient wc = new WClient("127.0.0.1", 5999);
                wc.Send(tmp);
                wc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var pp = (PacketProp)((TextBlock)sender).DataContext;
                DialogBox db = new DialogBox(pp.Name, "Custom Value { " + pp.PType.ToString() + " - " + pp.ValueS + " }", pp.CustomValue != null ? pp.CustomValue.ToString() : pp.Value.ToString());
                db.Owner = Window.GetWindow(this);
                if (db.ShowDialog() == true)
                {
                    if (db.textBox.Text.Trim() == "")
                        pp.CustomValue = null;
                    else
                    {
                        if (pp.PType == EPacketPropType.Byte)
                            pp.CustomValue = Convert.ToByte(db.textBox.Text);
                        if (pp.PType == EPacketPropType.Float)
                            pp.CustomValue = Convert.ToSingle(db.textBox.Text);
                        if (pp.PType == EPacketPropType.INT)
                            pp.CustomValue = Convert.ToInt32(db.textBox.Text);
                        if (pp.PType == EPacketPropType.UINT)
                            pp.CustomValue = Convert.ToUInt32(db.textBox.Text);
                        if (pp.PType == EPacketPropType.Short)
                            pp.CustomValue = Convert.ToInt16(db.textBox.Text);
                        if (pp.PType == EPacketPropType.UShort)
                            pp.CustomValue = Convert.ToUInt16(db.textBox.Text);
                        if (pp.PType == EPacketPropType.Long)
                            pp.CustomValue = Convert.ToInt64(db.textBox.Text);
                        if (pp.PType == EPacketPropType.ULong)
                            pp.CustomValue = Convert.ToUInt64(db.textBox.Text);
                    }
                }
                pro.LoadPropsValues();
                PacketPropList.Items.Refresh();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (THcheckServer != null)
            {
                try
                {
                    THcheckServer.Abort();
                    THcheckServer = null;
                }
                catch (Exception)
                {
                }
            }
        }

        private void menubtn_SetNetID_Click(object sender, RoutedEventArgs e)
        {
            if (PacketPropList.SelectedIndex == -1)
                return;
            var pp = (PacketProp)PacketPropList.SelectedItem;
            if (pp.PType != EPacketPropType.INT && pp.PType != EPacketPropType.UINT)
                return;
            if (pp.PType == EPacketPropType.INT)
                pp.CustomValue = (int)Statik.LastServerNetID;
            if (pp.PType == EPacketPropType.UINT)
                pp.CustomValue = (uint)Statik.LastServerNetID;
            pro.LoadPropsValues();
            PacketPropList.Items.Refresh();
        }

        private void PacketPropList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PacketPropList.UnselectAll();
        }

        private void imgChampion_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void imgSpell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var pp = (PacketProp)((Image)sender).DataContext;
                SpellWindow sp = new SpellWindow(pp);
                sp.Owner = Window.GetWindow(this);
                sp.ShowDialog();
            }
        }

        private void menubtn_Not_Click(object sender, RoutedEventArgs e)
        {
            if (PacketPropList.SelectedIndex == -1)
                return;
            var pp = (PacketProp)PacketPropList.SelectedItem;

            DialogBox db = new DialogBox("Not", pp.Name, pp.Description, true);
            db.Owner = Window.GetWindow(this);
            if (db.ShowDialog() == true)
                pp.Description = db.textBox.Text;
        }

        private void imgItem_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (saveFilePath == "")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                    saveFilePath = saveFileDialog.FileName;
            }

            if (saveFilePath != "")
            {
                File.WriteAllBytes(saveFilePath, this.pro.getBytes());
            }
        }

        private void btnSaveFileEditor_Click(object sender, RoutedEventArgs e)
        {
            if (saveFilePath == "")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                    saveFilePath = saveFileDialog.FileName;
            }

            if (saveFilePath != "")
            {
                File.WriteAllBytes(saveFilePath, this.pro.getBytesEdits());
            }
        }

        private void getAllCSharpCode_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(pro.getCSharpCode());
            MessageBox.Show("Hafızaya kopyalandı !");
        }
    }
}
