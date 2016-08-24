using BlowFishCS;
using LRFPacketDecoder.ParserClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static BlowFishCS.BlowFishCS;

namespace PcapDecrypt.Json
{
    public unsafe class Packet
    {
        private static Dictionary<int, Dictionary<int, byte[]>> fragmentBuffer = new Dictionary<int, Dictionary<int, byte[]>>();

        public float Time { get; set; }
        public int Length { get; set; }
        public byte[] Bytes { get; set; }

        public byte[] DBytes { get; set; }

        public List<WPacket> Decode(string Key)
        {
            initBlowfish(Key);
            if (Length < 45)
            {
                return null;
            }
            return F_OnPacketArrival();
        }

        private static BlowFish* _blowfish;
        private static string tmpKey = "";

        private static List<byte> fakeUDPHeader(byte[] temp, int count)
        {
            var totalLen = BitConverter.GetBytes((ushort)(count + 28));
            var payloadLen = BitConverter.GetBytes((ushort)count + 8);
            var ret = new List<byte>()
            {
                0x45, 0x00, totalLen[1], totalLen[0], 0x03, 0x3f, 0x00, 0x00, 0x80, 0x11, 0x00, 0x00, 0x7f, 0x00, 0x00, 0x01, 0x7f, 0x00, 0x00, 0x01,
                0x13, 0xf7, 0xf6, 0x26, payloadLen[1], payloadLen[0], 0x31, 0xa7
            };
            ret.AddRange(temp);

            return ret;
        }

        private List<WPacket> F_OnPacketArrival()
        {
            var reader = new BinaryReader(new MemoryStream(fakeUDPHeader(this.Bytes, this.Length).ToArray()));
            reader.ReadBytes(20); // IPv4 data

            var sourcePort = reader.ReadUInt16(true);
            var destPort = reader.ReadUInt16(true);
            var len = reader.ReadUInt16(true);

            if (len - 8 < 2)
                return null;

            reader.ReadBytes(2); // Rest of UDP data
            reader.ReadBytes(8); // Enet protocol header

            var opCode = (EnetOpCodes)(reader.ReadByte() & 0x0F); // Enet opcode
            reader.ReadBytes(3); //Rest of enet command header
            switch (opCode)
            {
                case EnetOpCodes.NONE:
                case EnetOpCodes.ACKNOWLEDGE:
                case EnetOpCodes.CONNECT:
                case EnetOpCodes.VERIFY_CONNECT:
                case EnetOpCodes.DISCONNECT:
                case EnetOpCodes.PING:
                case EnetOpCodes.THROTTLE_CONFIGURE:
                    return null;
                case EnetOpCodes.SEND_RELIABLE:
                    return handleReliable(reader, this.Time, sourcePort > destPort);
                case EnetOpCodes.SEND_FRAGMENT:
                    var pck = handleFragment(reader, this.Time, sourcePort > destPort);
                    if (pck != null)
                        return new List<WPacket>() { pck };
                    else
                        return null;
            }

            return null;
            // ENET Türü bilinmiyor !
            logLine("Unknown enet command " + opCode + " " + BitConverter.ToString(new byte[] { (byte)opCode }));
            //printPacket(e.Packet.Data, e.Packet.Timeval.Miliseconds, sourcePort > destPort);
        }

        private static void initBlowfish(string key)
        {
            if (tmpKey != key)
            {
                var decodeKey = Convert.FromBase64String(key);
                if (decodeKey.Length <= 0)
                    return;

                fixed (byte* s = decodeKey)
                    _blowfish = BlowFishCreate(s, new IntPtr(16));
                tmpKey = key;
            }
        }


        private WPacket handleFragment(BinaryReader reader, float time, bool C2S)
        {
            var fragmentGroup = reader.ReadUInt16(); // Fragment start number
            var len = reader.ReadUInt16(true);

            if (reader.BaseStream.Length - reader.BaseStream.Position < len + 16)
                return null;

            var totalFragments = reader.ReadInt32(true);
            var currentFragment = reader.ReadInt32(true);
            var totalLen = reader.ReadInt32(true);
            reader.ReadInt32(); // Offset
            var payload = reader.ReadBytes(len);

            if (!fragmentBuffer.ContainsKey(fragmentGroup))
                fragmentBuffer.Add(fragmentGroup, new Dictionary<int, byte[]>());

            var buff = fragmentBuffer[fragmentGroup];
            if (buff.ContainsKey(currentFragment))
                buff[currentFragment] = payload;
            else
                buff.Add(currentFragment, payload);

            if (buff.Count == totalFragments)
            {
                var packet = new List<byte>();
                var temp = buff.OrderBy(x => x.Key);
                foreach (var t in temp)
                    packet.AddRange(t.Value);

                if (totalLen != packet.Count)
                    return null;// logLine("Fragment's fishy. " + totalLen + "!=" + packet.Count);

                var decrypted = decrypt(packet.ToArray());

                printPacket(decrypted, time, C2S);

                WPacket pck = new WPacket();
                pck.Time = time;
                pck.Head = (PacketCmdS2C)decrypted[0];
                pck.Bytes = decrypted;
                pck.Length = pck.Bytes.Length;
                fragmentBuffer.Remove(fragmentGroup);
                return pck;
            }

            return null;
        }

        private List<WPacket> handleReliable2(BinaryReader reader, float time, bool C2S)
        {
            var len = reader.ReadUInt16(true);
            //if (reader.BaseStream.Length - reader.BaseStream.Position < len)
            //   return;

            List<WPacket> ret = new List<WPacket>();
            var packet = reader.ReadBytes(len);
            if (packet.Length < 1)
                return null;

            var decrypted = decrypt(packet);

            WPacket pck = new WPacket();
            pck.Time = this.Time;
            pck.Bytes = decrypted;
            pck.Length = pck.Bytes.Length;
            pck.Head = (PacketCmdS2C)decrypted[0];
            ret.Add(pck);

            printPacket(decrypted, time, C2S, false);

            if (decrypted[0] == 0xFF)
            {
                logLine(Environment.NewLine + "===Printing batch===");
                try
                {
                    ret.AddRange(decodeBatch(decrypted, time, C2S));
                }
                catch
                {
                    logLine("Batch parsing threw an exception.");
                }
                logLine("======================end batch==========================" + Environment.NewLine);
            }

            return ret;
        }

        private List<WPacket> decodeBatch2(byte[] decrypted, float time, bool C2S)
        {
            var reader = new BinaryReader(new MemoryStream(decrypted));
            var unk1 = reader.ReadByte();
            List<WPacket> ret = new List<WPacket>();


            try
            {
                var packetCount = reader.ReadByte();
                var size = reader.ReadByte();
                var opCode = reader.ReadByte();
                var netId = reader.ReadUInt32(true);
                var firstPacket = new List<byte>();

                firstPacket.Add(opCode);
                firstPacket.AddRange(BitConverter.GetBytes(netId).Reverse());

                if (size > 5)
                    firstPacket.AddRange(reader.ReadBytes(size - 5));

                logLine("Packet 1, Length " + size);
                printPacket(firstPacket.ToArray(), time, C2S, false);
                WPacket pck = new WPacket();
                pck.Time = this.Time;
                pck.Bytes = firstPacket.ToArray();
                pck.Length = pck.Bytes.Length;
                pck.Head = (PacketCmdS2C)pck.Bytes[0];
                ret.Add(pck);

                for (int i = 2; i < packetCount + 1; i++)
                {
                    var buffer = new List<byte>();
                    uint newId = 0;
                    byte command;

                    var flagsAndLength = 0;

                    try
                    {
                        if (reader.BaseStream.Position + 1 > reader.BaseStream.Length)
                            break;
                        reader.ReadByte(); // 6 first bits = size (if not 0xFC), 2 last bits = flags 
                    }
                    catch (Exception)
                    {

                    }

                    size = (byte)(flagsAndLength >> 2);

                    if ((flagsAndLength & 0x01) > 0)
                    { // additionnal byte, skip command
                        command = opCode;
                        try
                        {
                            if (reader.BaseStream.Position + 1 > reader.BaseStream.Length)
                                break;
                            reader.ReadByte();
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        if (reader.BaseStream.Position + 1 > reader.BaseStream.Length)
                            break;
                        command = reader.ReadByte();
                        if ((flagsAndLength & 0x02) > 0)
                        {
                            try
                            {
                                if (reader.BaseStream.Position + 1 > reader.BaseStream.Length)
                                    break;
                                reader.ReadByte();
                            }
                            catch (Exception)
                            {

                            }
                        }

                        else
                        {
                            try
                            {
                                if (reader.BaseStream.Position + 4 > reader.BaseStream.Length)
                                    break;
                                newId = reader.ReadUInt32(true);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }

                    try
                    {
                        if (reader.BaseStream.Position + 1 > reader.BaseStream.Length)
                            break;

                        if (size == 0x3F)
                            size = reader.ReadByte(); // size is too big to be on 6 bits, so instead it's stored later
                    }
                    catch (Exception)
                    {

                    }


                    //logLine("Packet " + i + ", Length " + (size + 5));
                    buffer.Add(command);
                    if (newId > 0)
                        buffer.AddRange(BitConverter.GetBytes(newId).Reverse());
                    else
                        buffer.AddRange(BitConverter.GetBytes(netId).Reverse());

                    try
                    {
                        if (reader.BaseStream.Position + size > reader.BaseStream.Length)
                            break;

                        buffer.AddRange(reader.ReadBytes(size));
                    }
                    catch (Exception)
                    { }

                    printPacket(buffer.ToArray(), time, C2S, false);
                    WPacket pck2 = new WPacket();
                    pck2.Time = this.Time;
                    pck2.Bytes = buffer.ToArray();
                    pck2.Length = pck2.Bytes.Length;
                    pck2.Head = (PacketCmdS2C)pck2.Bytes[0];
                    ret.Add(pck2);

                    opCode = command;
                }
            }
            catch (Exception)
            {

            }
            return ret;
        }

        private List<WPacket> handleReliable(BinaryReader reader, float time, bool C2S)
        {
            List<WPacket> ret = new List<WPacket>();
            var len = reader.ReadUInt16(true);
            //if (reader.BaseStream.Length - reader.BaseStream.Position < len)
            //   return;

            var packet = reader.ReadBytes(len);
            if (packet.Length < 1)
                return null;

            var decrypted = decrypt(packet);
            WPacket pc = new WPacket();
            pc.Time = this.Time;
            pc.Bytes = decrypted;
            pc.Head = (PacketCmdS2C)pc.Bytes[0];
            pc.Length = pc.Bytes.Length;
            ret.Add(pc);

            if (decrypted[0] == 0xFF)
            {
                logLine(Environment.NewLine + "===Printing batch===");
                try
                {
                    ret.AddRange(decodeBatch(decrypted, time, C2S));
                }
                catch
                {
                    logLine("Batch parsing threw an exception.");
                }
                logLine("======================end batch==========================" + Environment.NewLine);
            }

            return ret;
        }

        private List<WPacket> decodeBatch(byte[] decrypted, float time, bool C2S)
        {
            List<WPacket> ret = new List<WPacket>();

            var reader = new BinaryReader(new MemoryStream(decrypted));
            reader.ReadByte();

            var packetCount = reader.ReadByte();
            var size = reader.ReadByte();
            var opCode = reader.ReadByte();
            var netId = reader.ReadUInt32(true);
            var firstPacket = new List<byte>();

            firstPacket.Add(opCode);
            firstPacket.AddRange(BitConverter.GetBytes(netId).Reverse());

            if (size > 5)
                firstPacket.AddRange(reader.ReadBytes(size - 5));

            logLine("Packet 1, Length " + size);
            printPacket(firstPacket.ToArray(), time, C2S, false);
            WPacket pc = new WPacket();
            pc.Time = this.Time;
            pc.Bytes = firstPacket.ToArray();
            pc.Length = pc.Bytes.Length;
            pc.Head = (PacketCmdS2C)pc.Bytes[0];
            ret.Add(pc);

            for (int i = 2; i < packetCount + 1; i++)
            {
                var buffer = new List<byte>();
                uint newId = 0;
                bool netIdChanged = false;
                byte command;

                var flagsAndLength = reader.ReadByte(); // 6 first bits = size (if not 0xFC), 2 last bits = flags
                size = (byte)(flagsAndLength >> 2);

                if ((flagsAndLength & 0x01) > 0)
                { // additionnal byte, skip command
                    command = opCode;
                    if ((flagsAndLength & 0x02) > 0)
                        reader.ReadByte();
                    else
                    {
                        newId = reader.ReadUInt32(true);
                        netIdChanged = true;
                    }
                }
                else
                {
                    command = reader.ReadByte();
                    if ((flagsAndLength & 0x02) > 0)
                        reader.ReadByte();                    
                    else
                    {
                        newId = reader.ReadUInt32(true);
                        netIdChanged = true;
                    }
                }

                if (size == 0x3F)
                    size = reader.ReadByte(); // size is too big to be on 6 bits, so instead it's stored later

                logLine("Packet " + i + ", Length " + (size + 5));
                buffer.Add(command);
                if (netIdChanged)
                {
                    buffer.AddRange(BitConverter.GetBytes(newId).Reverse());
                    netId = newId; 
                } 
                else
                    buffer.AddRange(BitConverter.GetBytes(netId).Reverse());
                buffer.AddRange(reader.ReadBytes(size));
                printPacket(buffer.ToArray(), time, C2S, false);

                WPacket pc2 = new WPacket();
                pc2.Time = this.Time;
                pc2.Bytes = buffer.ToArray();
                pc2.Length = pc2.Bytes.Length;
                pc2.Head = (PacketCmdS2C)pc2.Bytes[0];
                ret.Add(pc2);

                opCode = command;
            }

            return ret;
        }

        private static byte[] decrypt(byte[] packet)
        {
            var temp = packet.ToArray();
            if (temp.Length >= 8)
                fixed (byte* ptr = temp)
                    Decrypt1(_blowfish, ptr, new IntPtr(temp.Length - (temp.Length % 8)));
            return temp;
        }


        private static void printPacket(byte[] packet, float time, bool C2S, bool addSeparator = true)
        {
            //var tSent = TimeSpan.FromMilliseconds(time);
            //var tt = tSent.ToString("mm\\:ss\\.ffff");
            //tt += C2S ? " C2S: " + (PacketCmdS2C)(packet[0]) : " S2C: " + (PacketCmdS2C)(packet[0]);
            //tt += " Length:" + packet.Length + Environment.NewLine;
            //int i = 0;
            //if (packet.Length > 15)
            //{
            //    for (i = 16; i <= packet.Length; i += 16)
            //    {
            //        for (var j = 16; j > 0; j--)
            //            tt += packet[i - j].ToString("X2") + " ";
            //        for (var j = 16; j > 0; j--)
            //        {
            //            if (packet[i - j] >= 32 && packet[i - j] <= 126)
            //                tt += Encoding.Default.GetString(new byte[] { packet[i - j] });
            //            else
            //                tt += ".";
            //        }
            //        tt += Environment.NewLine;
            //    }
            //}

            //var temp = i;
            //if (temp != packet.Length + 16)
            //{
            //    if (temp > 15)
            //        temp -= 16;
            //    var ssss = packet.Length - temp;
            //    while (temp < packet.Length)
            //    {
            //        tt += packet[temp].ToString("X2") + " ";
            //        temp++;
            //    }
            //    for (var j = temp % 16; j < 16; j++)
            //        tt += "   ";

            //    temp = i > 15 ? i - 16 : i;
            //    for (var j = 0; j < ssss; j++)
            //    {
            //        if (packet[temp + j] >= 32 && packet[temp + j] <= 126)
            //            tt += Encoding.Default.GetString(new byte[] { packet[temp + j] });
            //        else
            //            tt += ".";
            //    }
            //}
            //logLine(tt + Environment.NewLine);

            //if (addSeparator)
            //    logLine("----------------------------------------------------------------------------");

        }

        private static void logLine(string line)
        {
            //System.Diagnostics.Debug.WriteLine(line);
            //Console.WriteLine(line);
            //File.AppendAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "/decry.txt", new string[] { line + Environment.NewLine });
        }


    }
}
