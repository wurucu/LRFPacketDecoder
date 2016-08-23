using LRFPacketDecoder.Editor;
using LRFPacketDecoder.ParserClass;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using PcapDecrypt.Json;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using System.Text;
using System.Windows.Media;
using LRFPacketDecoder.WTool;

namespace LRFPacketDecoder
{

    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Replay rply;
         

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            var packet = ((WPacket)row.DataContext);

            PacketEditor editor = new PacketEditor(packet);
            TabItem tbitem = new TabItem() { Header = packet.Head.ToString() };
            tbitem.Content = editor;
            tabControl.Items.Add(tbitem);
            tabControl.SelectedItem = tbitem;
        }

        private void dataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
        }
        private void dataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void BarItemClick(object sender, RoutedEventArgs e)
        {
             
        }
        private void tabControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                var element = VisualTreeHelper.HitTest(tabControl, Mouse.GetPosition(tabControl)).VisualHit;

                do
                {
                    element = VisualTreeHelper.GetParent(element);
                } while (element != null && !(element is TabItem));

                this.tabControl.Items.Remove(element);
            }
        }

        public void readPlayers(Replay rply)
        {
            Statik.Players.Clear();
            foreach (var pckt in rply.WPackets)
            {
                if (pckt.Head == PcapDecrypt.PacketCmdS2C.PKT_S2C_HeroSpawn && pckt.Length > 200)
                {
                    Player pl = new Player();
                    BinaryReader rdr = new BinaryReader(new MemoryStream(pckt.Bytes));
                    rdr.ReadByte();
                    rdr.ReadInt32();
                    pl.netid = rdr.ReadInt32();
                    pl.profilid = rdr.ReadInt32(); 
                    rdr.ReadBytes(2);
                    pl.team = Convert.ToInt32(rdr.ReadByte());
                    rdr.ReadBytes(2);
                    rdr.ReadInt32(); // skinno
                    pl.name = Encoding.UTF8.GetString(rdr.ReadBytes(128)).Replace("\0", "").Trim();
                    pl.champion = Encoding.UTF8.GetString(rdr.ReadBytes(40)).Replace("\0", "").Trim();
                    Statik.Players.Add(pl);
                }
            }
        }

        DBDataContext db = new DBDataContext();
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.AllowDrop = true;

            //ReplayParser - arg : filename - out : file.json
            //OpenFile(@"C:\Users\Gokhan\Documents\visual studio 2015\Projects\ReplayDownloader\ReplayDownloader\bin\Debug\download\Ahri\WillKillU2 - Ahri.lrf");
            OpenFile(@"C:\Users\Gokhan\Documents\Visual Studio 2015\Projects\ReplayDownloader\ReplayDownloader\bin\Debug\download\Aatrox\Kreps - Aatrox.lrf");
            Statik.Spells = db.Spells.ToList();
            Statik.Champions = db.Champions.ToList();
            Statik.Items = db.Items.ToList();
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            { 
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                //ReplayFile rf = ReplayFile.Open(files[0]);

            }
        }

        public void ParseFile(string file)
        {
            Process p = new Process();
            p.StartInfo.Arguments = "\"" + file + "\"";
            p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "/ReplayParser.exe";
            p.Start();
            p.WaitForExit();
        }

        public void OpenJSON(string file)
        {
            loading.Show("Bekleyin..", "Yükleniyor..");
            Thread th = new Thread(() =>
            {
                var json = File.ReadAllText(file);
                this.rply = JsonConvert.DeserializeObject<Replay>(json);
                //replay.packets = replay.packets.OrderBy(x => x.Time).ToList();

                rply.Decodes();
                string fileName = new FileInfo(file).Directory.FullName + "/" + Path.GetFileNameWithoutExtension(file) + ".wdc";
                rply.SaveWDC(fileName);
                readPlayers(rply);
                Dispatcher.Invoke(() =>
                { 
                    packetList.ItemsSource = rply.WPackets;
                    loading.Hide();
                });
            });
            th.Start(); 
        }

        public void OpenWDC(string file)
        {
            loading.Show("Bekleyin..", "Yükleniyor..");
            Thread th = new Thread(() =>
            {
                var json = File.ReadAllText(file);
                rply = JsonConvert.DeserializeObject<Replay>(json);
                readPlayers(rply);
                Dispatcher.Invoke(() =>
                { 
                    packetList.ItemsSource = rply.WPackets;
                    loading.Hide();
                });
            });
            th.Start();
        }

        public void OpenFile(string fileName)
        {
            if (fileName.ToLower().EndsWith(".lrf"))
            {
                loading.Show("Bekleyin..", "Yükleniyor..");
                string checkWDC = new FileInfo(fileName).Directory.FullName + "/" + Path.GetFileNameWithoutExtension(fileName) + ".wdc";
                if (File.Exists(checkWDC))
                {
                    OpenFile(checkWDC);
                    return;
                } 
                ParseFile(fileName);
                string newFile = new FileInfo(fileName).Directory.FullName + "/" + Path.GetFileNameWithoutExtension(fileName) + ".json";
                OpenJSON(newFile);
            }
            else if (fileName.ToLower().EndsWith(".json"))
            {
                string checkWDC = new FileInfo(fileName).Directory.FullName + "/" + Path.GetFileNameWithoutExtension(fileName) + ".wdc";
                if (File.Exists(checkWDC))
                {
                    OpenFile(checkWDC);
                    return;
                }
                OpenJSON(fileName);
            }
            else if (fileName.ToLower().EndsWith(".wdc"))
            {
                OpenWDC(fileName);
            }
            else
            {
                //Fake packet
                byte[] bts = File.ReadAllBytes(fileName);
                WPacket fake = new WPacket();
                fake.Head = 0;
                fake.Bytes = bts;

                PacketEditor editor = new PacketEditor(fake);
                TabItem tbitem = new TabItem() { Header = fileName };
                tbitem.Content = editor;
                tabControl.Items.Add(tbitem);
                tabControl.SelectedItem = tbitem;

            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog(); 
            dlg.DefaultExt = ".lrf,.JSON,.wdc";
            dlg.Filter = "LRF|*.lrf;*.wdc;*.json|all|*.*";
            dlg.FileName = @"C:\Users\Gokhan\Documents\visual studio 2015\Projects\ReplayDownloader\ReplayDownloader\bin\Debug\download\";
            if (dlg.ShowDialog() == true)
            { 
                OpenFile(dlg.FileName); 
            }
        }

        private void buttonara_Click(object sender, RoutedEventArgs e)
        {
            if (this.rply == null || this.rply.WPackets == null)
                return;
            DialogSearch src = new DialogSearch(this.rply);
            src.Owner = this;
            if (src.ShowDialog() == true)
            {
                var data = this.rply.WPackets.Where(x => src.Selected.Where(s => s.HeadInt == (int)x.Head).Count() > 0).ToList();
                if (src.chTime.IsChecked == true)
                {
                    if (src.startTime.Value.HasValue)
                        data = data.Where(x => x.Time >= src.startTime.Value.Value.TotalMilliseconds).ToList();
                    if (src.endTime.Value.HasValue)
                        data = data.Where(x => x.Time <= src.endTime.Value.Value.TotalMilliseconds).ToList();
                }
                if (src.firstPlayers.SelectedItem != null)
                { // İlk NetID
                    var netID = (Player)src.firstPlayers.SelectedItem;
                    data = data.Where(x => x.Length >= 5 && BitConverter.ToUInt32(x.Bytes, 1) == (uint)netID.netid).ToList();
                }
                if (src.containPlayers.SelectedItem != null)
                { // Contain NetID
                    var netID = (Player)src.containPlayers.SelectedItem;
                    var bNetID = BitConverter.GetBytes((uint)netID.netid);
                    data = data.Where(x => x.Bytes.isMatch(bNetID)).ToList();
                }

                packetList.ItemsSource = data.OrderBy(x => x.Time).ToList();
            }
        }
         

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowPlayers wp = new WindowPlayers();
            wp.Owner = this;
            wp.Show();
        }
    }
}
