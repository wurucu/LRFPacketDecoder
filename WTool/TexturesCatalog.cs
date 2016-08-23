using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WRC
{
    public class TexturesCatalog
    {
        public List<Texture> Textures;

        public TexturesCatalog()
        {
            this.Textures = new List<Texture>();
        }

        public static string ByteArrayToString(byte[] arrInput)
        {
            StringBuilder builder = new StringBuilder(arrInput.Length * 2);
            int num = arrInput.Length - 1;
            for (int i = 0; i <= num; i++)
            {
                builder.Append(arrInput[i].ToString("X2"));
            }
            return builder.ToString().ToLower();
        }

        public Texture GetTexture(string TexturePath)
        {
            if (!File.Exists(TexturePath))
            {
                return null;
            }
            string str = CryptHash(TexturePath);
            foreach (Texture texture in this.Textures)
            {
                if (str == texture.Checksum)
                {
                    return texture;
                }
            }
            Texture item = new Texture(TexturePath, str);
            this.Textures.Add(item);
            return item;
        }

        public bool HasTexture(string TexturePath)
        {
            if (File.Exists(TexturePath))
            {
                string str = CryptHash(TexturePath);
                foreach (Texture texture in this.Textures)
                {
                    if (str == texture.Checksum)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static BitmapImage LoadImageFromFile(string imagePath)
        {
            if ((imagePath == null) | !File.Exists(imagePath))
            {
                return null;
            }
            FileStream stream = File.OpenRead(imagePath);
            BitmapImage image2 = new BitmapImage
            {
                CacheOption = BitmapCacheOption.None
            };
            image2.BeginInit();
            image2.StreamSource = stream;
            image2.EndInit();
            return image2;
        }

        public static object PrepareDirectory(string path)
        {
            path = path.Replace("/", @"\");
            string parentPath = System.AppDomain.CurrentDomain.BaseDirectory + "temptextures";
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            return path;
        }

        public static string CryptHash(string filepath)
        {
            string str;
            using (FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider())
                {
                    str = ByteArrayToString(provider.ComputeHash(stream));
                }
            }
            return str;
        }

        public class Texture
        {
            public System.Windows.Media.Imaging.BitmapImage BitmapImage;
            public string Checksum;

            public Texture(string TexturePath, string string_0)
            { 
                this.BitmapImage = null;
                this.Checksum = string_0;
                string path = System.AppDomain.CurrentDomain.BaseDirectory + @"temptextures\" + this.Checksum + ".png";
                if (!File.Exists(path))
                {
                    TexturesCatalog.PrepareDirectory(path);
                    Process process = new Process();
                    ProcessStartInfo startInfo = process.StartInfo;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + @"\nconvert.exe";
                    string[] textArray1 = new string[] { "-out png -o \"", path, "\" \"", TexturePath, "\"" };
                    startInfo.Arguments = string.Concat(textArray1);
                    startInfo = null;
                    process.Start();
                    process.WaitForExit();
                }
                this.BitmapImage = TexturesCatalog.LoadImageFromFile(path);
            }
        }
    }
}
