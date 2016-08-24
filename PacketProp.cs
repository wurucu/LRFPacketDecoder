using LRFPacketDecoder;
using LRFPacketDecoder.ParserClass;
using Newtonsoft.Json;
using PcapDecrypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;



//For listbox converter
namespace ValueConveters
{
    public class ValueSelector : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null || parameter.ToString() == "1")
            {
                var data = (PacketProp)(value as TextBlock).DataContext;
                return data.CustomValue != null ? data.CustomValue : data.ValueS;
            }
            if (value is Image)
            {
                var imgC = value as Image;

                var data = (PacketProp)(value as Image).DataContext;

                #region Player
                if ((data.PType == EPacketPropType.INT || data.PType == EPacketPropType.UINT) && parameter.ToString() == "2")
                { // Player
                    if (data.PType == EPacketPropType.INT && data.Value != null)
                        if (Statik.Players.Where(x => x.netid == (int)data.Value).Count() > 0)
                        {
                            var img = Statik.getIcon(Statik.Players.Where(x => x.netid == (int)data.Value).FirstOrDefault());
                            if (img != null)
                                imgC.Source = img;
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }


                    if (data.PType == EPacketPropType.UINT && data.Value != null)
                        if (Statik.Players.Where(x => x.netid == (uint)data.Value).Count() > 0)
                        {
                            var img = Statik.getIcon(Statik.Players.Where(x => x.netid == (uint)data.Value).FirstOrDefault());
                            if (img != null)
                                imgC.Source = img;
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                }
                #endregion

                #region Spell
                if ((data.PType == EPacketPropType.INT || data.PType == EPacketPropType.UINT) && parameter.ToString() == "3")
                { // Spell
                    if (data.PType == EPacketPropType.INT && data.Value != null)
                        if (Statik.Spells.Where(x => x.HashID == (int)data.Value).Count() > 0)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    if (data.PType == EPacketPropType.UINT && data.Value != null)
                        if (Statik.Spells.Where(x => x.HashID == (uint)data.Value).Count() > 0)
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;
                }
                #endregion

                #region Item
                if ((data.PType == EPacketPropType.INT || data.PType == EPacketPropType.UINT || data.PType == EPacketPropType.Short) && parameter.ToString() == "4")
                { // Item
                    if (data.PType == EPacketPropType.INT && data.Value != null)
                        if (Statik.Items.Where(x => x.ItemID == (int)data.Value).Count() > 0)
                        {
                            var img = Statik.getIcon(Statik.Items.Where(x => x.ItemID == (int)data.Value).FirstOrDefault());
                            if (img != null)
                                imgC.Source = img;
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    if (data.PType == EPacketPropType.UINT && data.Value != null)
                        if (Statik.Items.Where(x => x.ItemID == (uint)data.Value).Count() > 0)
                        {
                            var img = Statik.getIcon(Statik.Items.Where(x => x.ItemID == (uint)data.Value).FirstOrDefault());
                            if (img != null)
                                imgC.Source = img;
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    if (data.PType == EPacketPropType.Short && data.Value != null)
                        if (Statik.Items.Where(x => x.ItemID == (short)data.Value).Count() > 0)
                        {
                            var img = Statik.getIcon(Statik.Items.Where(x => x.ItemID == (short)data.Value).FirstOrDefault());
                            if (img != null)
                                imgC.Source = img;
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                }
                #endregion

                return Visibility.Collapsed;
            }
            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ValueColorSelector : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var data = (PacketProp)(value as TextBlock).DataContext;
            return data.CustomValue != null ? "#FFADDA4D" : "White";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace LRFPacketDecoder
{

    [Serializable]
    public enum EPacketPropType
    {
        Byte,
        Chars,
        Bytes,
        Short,
        UShort,
        INT,
        UINT,
        Long,
        ULong,
        SKIP,
        Float,
        Float_Inverse
    }

    [Serializable]
    public enum EPacketPropDynamicType
    {
        INIBINFile
    }

    [Serializable]
    public class PacketPropDynamicValue
    {
        public EPacketPropDynamicType DType { get; set; }
        public string FilePath { get; set; }
        public string Key { get; set; }
    }

    [Serializable]
    public class PacketPro
    {
        public string PacketDescription { get; set; }
        public PacketCmdS2C Head { get; set; }
        public List<PacketProp> Props { get; set; }

        [JsonIgnore]
        public int LastPosition { get; set; }

        public class HexStartEnd
        {
            private int mStartIndex;

            public int StartIndex
            {
                get { return mStartIndex == 1 ? 0 : mStartIndex; }
                set { mStartIndex = value; }
            }

            public int EndIndex { get; set; }
        }

        WPacket packet;

        public delegate void ReadPropError(PacketProp prop, string Error);
        public event ReadPropError onReadPropError;
        public delegate void ReadPropComplete(int LastPosition, List<HexStartEnd> Poss);
        public event ReadPropComplete onReadPropComplete;

        public void Load(WPacket packet)
        {
            this.packet = packet;
            this.Head = (PacketCmdS2C)packet.Bytes[0];
            //if (packet.Bytes.Length > 1)
            //{
            //    byte[] nwbts = new byte[packet.Bytes.Length - 1];
            //    Buffer.BlockCopy(packet.Bytes, 1, nwbts, 0, nwbts.Length);
            //    packet.Bytes = nwbts;
            //}

            string f1 = AppDomain.CurrentDomain.BaseDirectory + "/Packets/" + Convert.ToInt32(this.Head) + "_" + this.packet.Length + ".json";
            string f2 = AppDomain.CurrentDomain.BaseDirectory + "/Packets/" + Convert.ToInt32(this.Head) + ".json";
            string f3 = AppDomain.CurrentDomain.BaseDirectory + "/Packets/" + this.Head.ToString() + ".json";


            if (File.Exists(f1))
            {
                string rd = File.ReadAllText(f1);
                var thisClass = JsonConvert.DeserializeObject<PacketPro>(rd);
                setClass(thisClass);
                LoadPropsValues();
                return;
            }
            if (File.Exists(f2))
            {
                string rd = File.ReadAllText(f2);
                var thisClass = JsonConvert.DeserializeObject<PacketPro>(rd);
                setClass(thisClass);
                LoadPropsValues();
                return;
            }
            if (File.Exists(f3))
            {
                string rd = File.ReadAllText(f3);
                var thisClass = JsonConvert.DeserializeObject<PacketPro>(rd);
                setClass(thisClass);
                LoadPropsValues();
                return;
            }
            LoadPropsValues();
        }

        public void Save(bool WithLen)
        {
            string f1 = AppDomain.CurrentDomain.BaseDirectory + "/Packets/" + Convert.ToInt32(this.Head) + "_" + this.packet.Length + ".json";
            string f2 = AppDomain.CurrentDomain.BaseDirectory + "/Packets/" + Convert.ToInt32(this.Head) + ".json";


            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(WithLen ? f1 : f2, json);
        }

        public PacketPro()
        {

        }

        public PacketPro(WPacket packet)
        {
            this.Load(packet);
        }

        public void LoadPropsValues()
        {
            if (Props == null)
                return;

            List<HexStartEnd> lst = new List<HexStartEnd>();

            HexStartEnd ps = new HexStartEnd();
            object val = null;
            BinaryReader rdr = new BinaryReader(new MemoryStream(packet.Bytes));
            bool error = false;
            foreach (var prop in Props)
            {
                try
                {
                    switch (prop.PType)
                    {
                        case EPacketPropType.Byte:
                            val = rdr.ReadByte();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.Bytes:
                            val = rdr.ReadBytes(prop.Length);
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.Chars:
                            val = rdr.ReadChars(prop.Length);
                            prop.Value = val;
                            prop.ValueS = ValuetoText(new string((char[])val));
                            break;
                        case EPacketPropType.Short:
                            val = rdr.ReadInt16();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.UShort:
                            val = rdr.ReadUInt16();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.INT:
                            val = rdr.ReadInt32();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.UINT:
                            val = rdr.ReadUInt32();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.Long:
                            val = rdr.ReadInt64();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.ULong:
                            val = rdr.ReadUInt64();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.SKIP:
                            val = rdr.ReadBytes(prop.Length);
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.Float:
                            val = rdr.ReadSingle();
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        case EPacketPropType.Float_Inverse:
                            byte[] myData = new byte[4];
                            rdr.Read(myData, 0, 4);
                            Array.Reverse(myData);
                            Single myvalue = BitConverter.ToSingle(myData, 0);
                            val = myvalue;
                            prop.Value = val;
                            prop.ValueS = ValuetoText(val);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception exx)
                {
                    error = true;
                    if (onReadPropError != null)
                        onReadPropError(prop, exx.Message);
                }
            }

            this.LastPosition = Convert.ToInt32(rdr.BaseStream.Position);
            if (!error && onReadPropComplete != null)
                onReadPropComplete(Convert.ToInt32(rdr.BaseStream.Position), lst);
            rdr.Close();
        }

        /// <summary>
        /// For send server
        /// </summary>
        /// <returns></returns>
        public byte[] getBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter wrt = new BinaryWriter(ms);
            BinaryReader rdr = new BinaryReader(new MemoryStream(this.packet.Bytes));
            if (this.Props != null)
                foreach (var pp in this.Props)
                {
                    object val = pp.CustomValue != null ? pp.CustomValue : pp.Value;
                    switch (pp.PType)
                    {
                        case EPacketPropType.Byte:
                            wrt.Write((byte)val);
                            rdr.ReadByte();
                            break;
                        case EPacketPropType.Chars:
                            wrt.Write((char[])val);
                            rdr.ReadChars(((char[])val).Length);
                            break;
                        case EPacketPropType.Bytes:
                            wrt.Write((byte[])val);
                            rdr.ReadBytes(pp.Length);
                            break;
                        case EPacketPropType.Short:
                            wrt.Write((short)val);
                            rdr.ReadInt16();
                            break;
                        case EPacketPropType.UShort:
                            wrt.Write((ushort)val);
                            rdr.ReadUInt16();
                            break;
                        case EPacketPropType.INT:
                            wrt.Write((int)val);
                            rdr.ReadInt32();
                            break;
                        case EPacketPropType.UINT:
                            wrt.Write((uint)val);
                            rdr.ReadUInt32();
                            break;
                        case EPacketPropType.Long:
                            wrt.Write((long)val);
                            rdr.ReadInt64();
                            break;
                        case EPacketPropType.ULong:
                            wrt.Write((ulong)val);
                            rdr.ReadUInt64();
                            break;
                        case EPacketPropType.SKIP:
                            wrt.Write((byte[])val);
                            rdr.ReadBytes(pp.Length);
                            break;
                        case EPacketPropType.Float:
                            wrt.Write((float)val);
                            rdr.ReadSingle();
                            break;
                        case EPacketPropType.Float_Inverse:
                            wrt.Write((float)val);
                            rdr.ReadSingle();
                            break;
                        default:
                            break;
                    }

                }

            byte[] tmp = new byte[this.packet.Bytes.Length - rdr.BaseStream.Position];
            Array.Copy(this.packet.Bytes, rdr.BaseStream.Position, tmp, 0, tmp.Length);
            wrt.Write(tmp);

            wrt.Close();
            return ms.ToArray();
        }

        public byte[] getBytesEdits()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter wrt = new BinaryWriter(ms);
            foreach (var pp in this.Props)
            {
                object val = pp.CustomValue != null ? pp.CustomValue : pp.Value;
                switch (pp.PType)
                {
                    case EPacketPropType.Byte:
                        wrt.Write((byte)val);
                        break;
                    case EPacketPropType.Chars:
                        wrt.Write((char[])val);
                        break;
                    case EPacketPropType.Bytes:
                        wrt.Write((byte[])val);
                        break;
                    case EPacketPropType.Short:
                        wrt.Write((short)val);
                        break;
                    case EPacketPropType.UShort:
                        wrt.Write((ushort)val);
                        break;
                    case EPacketPropType.INT:
                        wrt.Write((int)val);
                        break;
                    case EPacketPropType.UINT:
                        wrt.Write((uint)val);
                        break;
                    case EPacketPropType.Long:
                        wrt.Write((long)val);
                        break;
                    case EPacketPropType.ULong:
                        wrt.Write((ulong)val);
                        break;
                    case EPacketPropType.SKIP:
                        wrt.Write((byte[])val);
                        break;
                    case EPacketPropType.Float:
                        wrt.Write((float)val);
                        break;
                    case EPacketPropType.Float_Inverse:
                        wrt.Write((float)val);
                        break;
                    default:
                        break;
                }

            }


            wrt.Close();
            return ms.ToArray();
        }

        public string getCSharpCode()
        {
            StringBuilder ret = new StringBuilder();

            BinaryReader rdr = new BinaryReader(new MemoryStream(this.packet.Bytes));
            foreach (var pp in this.Props)
            {
                object val = pp.CustomValue != null ? pp.CustomValue : pp.Value;
                switch (pp.PType)
                {
                    case EPacketPropType.Byte:
                        rdr.ReadByte();
                        ret.AppendLine("buffer.Write((byte)" + (byte)val + "); //" + pp.Name);
                        break;
                    case EPacketPropType.Chars:
                        rdr.ReadChars(((char[])val).Length);
                        break;
                    case EPacketPropType.Bytes:
                        ret.Append("buffer.Write(new byte[] {");
                        int indx = 0;
                        foreach (var byt in (byte[])val)
                        {
                            ret.Append((indx == 0 ? "" : " , ") + Convert.ToInt32(byt).ToString());
                            indx++;
                        }
                        ret.AppendLine("}; // " + pp.Name);
                        rdr.ReadBytes(pp.Length);
                        break;
                    case EPacketPropType.Short:
                        rdr.ReadInt16();
                        ret.AppendLine("buffer.Write((short)" + (short)val + "); //" + pp.Name);
                        break;
                    case EPacketPropType.UShort:
                        rdr.ReadUInt16();
                        ret.AppendLine("buffer.Write((ushort)" + (ushort)val + "); //" + pp.Name);
                        break;
                    case EPacketPropType.INT:
                        rdr.ReadInt32();
                        ret.AppendLine("buffer.Write((int)" + (int)val + "); //" + pp.Name);
                        break;
                    case EPacketPropType.UINT:
                        rdr.ReadUInt32();
                        ret.AppendLine("buffer.Write((uint)" + (uint)val + "); //" + pp.Name);
                        break;
                    case EPacketPropType.Long:
                        rdr.ReadInt64();
                        ret.AppendLine("buffer.Write((long)" + (long)val + "); //" + pp.Name);
                        break;
                    case EPacketPropType.ULong:
                        rdr.ReadUInt64();
                        ret.AppendLine("buffer.Write((ulong)" + (ulong)val + "); //" + pp.Name);
                        break;
                    case EPacketPropType.SKIP:
                        rdr.ReadBytes(pp.Length);
                        break;
                    case EPacketPropType.Float:
                        rdr.ReadSingle();
                        ret.AppendLine("buffer.Write((float)" + ((float)val).ToString().Replace(",", ".") + "); //" + pp.Name);
                        break;
                    case EPacketPropType.Float_Inverse:
                        rdr.ReadSingle();
                        ret.AppendLine("buffer.Write((float)" + (float)val + "); //" + pp.Name);
                        break;
                    default:
                        break;
                }

            }

            byte[] tmp = new byte[this.packet.Bytes.Length - rdr.BaseStream.Position];
            if (tmp.Length > 0)
            {
                ret.Append("buffer.Write(new byte[] {");
                int indx2 = 0;
                foreach (var byt in tmp)
                {
                    ret.Append((indx2 == 0 ? "" : " , ") + Convert.ToInt32(byt).ToString());
                    indx2++;
                }
                ret.AppendLine("}; // Kalan bytes [" + tmp.Length + "]");
            }


            return ret.ToString();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("0x{0:x2} ", b);
            return hex.ToString();
        }

        private string ValuetoText(object val)
        {
            if (val is byte)
                return val.ToString();

            if (val is string)
                return val.ToString().Trim().Replace("\0", "");

            if (val is byte[])
            {
                return ByteArrayToString((byte[])val);
            }

            if (val is int || val is uint)
            {
                if (val is int)
                {
                    var player = Statik.Players.Where(x => x.netid == Convert.ToInt32(val)).FirstOrDefault();
                    if (player != null)
                        return val.ToString() + " - (" + player.champion + " / " + player.name + " / " + (player.team == 1 ? "Blue" : "Red") + " / " + player.profilid + ")";
                }
                if (val is uint)
                {
                    var player = Statik.Players.Where(x => x.netid == Convert.ToUInt32(val)).FirstOrDefault();
                    if (player != null)
                        return val.ToString() + " - (" + player.champion + " / " + player.name + " / " + (player.team == 1 ? "Blue" : "Red") + " / " + player.profilid + ")";
                }
                return val.ToString();
            }

            if (val is long || val is UInt64 || val is ulong || val is short || val is ushort || val is float || val is decimal)
                return val.ToString();

            return "Unknown Type";
        }

        public void setClass(PacketPro clss)
        {
            this.PacketDescription = clss.PacketDescription;
            this.Head = clss.Head;
            this.Props = clss.Props;
        }
    }

    [Serializable]
    public class PacketProp
    {
        public EPacketPropType PType { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }

        // Listview için 
        private List<EPacketPropType> mTypeList;
        [JsonIgnore]
        public List<EPacketPropType> TypeList
        {
            get { return Enum.GetValues(typeof(EPacketPropType)).Cast<EPacketPropType>().ToList(); }
            set { mTypeList = value; }
        }

        //------------ 

        [JsonIgnore]
        public string ValueS { get; set; }
        [JsonIgnore]
        public object Value { get; set; }
        /// <summary>
        /// For SendServer
        /// </summary>
        [JsonIgnore]
        public object CustomValue { get; set; }

        public string Description { get; set; }
        public PacketPropDynamicValue DynamicValue { get; set; }
        public int Length { get; set; }
    }
}
