using PcapDecrypt.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WRC;
using static System.Net.Mime.MediaTypeNames;

namespace LRFPacketDecoder
{
    public static class Statik
    {
        static string ClientPath = @"I:\LOLPvp\Client\LOL2GB\DATA\";

        public static uint LastServerNetID = 0;
        public static List<Champion> Champions = new List<Champion>();
        public static List<Spell> Spells = new List<Spell>();
        public static List<Player> Players = new List<Player>();
        public static List<Item> Items = new List<Item>();
        public static TexturesCatalog DDS = new TexturesCatalog();

        public static ImageSource getIcon(object dat)
        {
            if (dat is Player)
            { // Get player icon
                Player player = dat as Player;
                string fileName = ClientPath + "Characters\\" + player.champion + "\\HUD\\" + player.champion + "_Square.dds";
                if (File.Exists(fileName))
                { 
                    var dds = DDS.GetTexture(fileName);
                    return dds.BitmapImage;
                }
                fileName = ClientPath + "Characters\\" + player.champion + "\\info\\" + player.champion + "_Square.dds";
                if (File.Exists(fileName))
                {
                    var dds = DDS.GetTexture(fileName);
                    return dds.BitmapImage;
                }
                fileName = ClientPath + "Characters\\" + player.champion + "\\info\\" + player.champion + "_Square_0.dds";
                if (File.Exists(fileName))
                {
                    var dds = DDS.GetTexture(fileName);
                    return dds.BitmapImage;
                }
            }

            if (dat is Item)
            { // Get Item icon
                Item item = dat as Item;
                string fileName = ClientPath + "\\Items\\Icons2D\\" + item.Icon;
                if (File.Exists(fileName))
                {
                    var dds = DDS.GetTexture(fileName);
                    return dds.BitmapImage;
                }
            }

            return null;
        }
    }
}
