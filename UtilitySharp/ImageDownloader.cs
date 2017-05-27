using SharpDX;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using EloBuddy.SDK.Rendering;

namespace UtilitySharp
{
    class ImageDownloader
    {
        public static Sprite CreateSummonerSprite(string name)
        {
            Bitmap srcBitmap;
            try
            {
                var file = Program.ConfigFolderPath + "\\" + name;
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/" + name,
                        file);
                }
                srcBitmap = new Bitmap(file);
            }
            catch
            {
                var file = Program.ConfigFolderPath + "\\Default";
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/Default",
                        file);
                }
                srcBitmap = new Bitmap(file);
            }
            var img = new Bitmap(srcBitmap.Width + 2, srcBitmap.Width + 2);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (Bitmap sourceImage = srcBitmap)
            {
                using (Bitmap croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (Graphics g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            var finalSprite = new Sprite(TextureLoader.BitmapToTexture(img));
            return finalSprite;
        }

        public static Sprite CreateMinimapSprite(string name)
        {
            Bitmap srcBitmap;
            try
            {
                var file = Program.ConfigFolderPath + "\\" + name;
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/" + name,
                        file);
                }
                srcBitmap = new Bitmap(file);
            }
            catch
            {
                var file = Program.ConfigFolderPath + "\\Default";
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/Default",
                        file);
                }
                srcBitmap = new Bitmap(file);
            }

            var img = new Bitmap(srcBitmap.Width + 2, srcBitmap.Width + 2);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (Bitmap sourceImage = srcBitmap)
            {
                using (Bitmap croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (Graphics g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);

                            var p = new Pen(System.Drawing.Color.White, 1) { Alignment = PenAlignment.Center };
                            g.DrawEllipse(p, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            Sprite finalSprite = new Sprite(TextureLoader.BitmapToTexture(img)) { Scale = new Vector2(0.2f, 0.2f) };

            return finalSprite;
        }

        public static Sprite GetSprite(string name)
        {
            Sprite sprite;
            try
            {
                var file = Program.ConfigFolderPath + "\\" + name;
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/" + name,
                        file);
                }
                sprite = new Sprite(TextureLoader.BitmapToTexture(new Bitmap(file)));
            }
            catch
            {
                var file = Program.ConfigFolderPath + "\\Default";
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/Default",
                        file);
                }
                sprite = new Sprite(TextureLoader.BitmapToTexture(new Bitmap(file)));
            }

            return sprite;
        }

        public static Sprite CreateRadrarIcon(string name, System.Drawing.Color color, int opacity = 60)
        {
            Bitmap srcBitmap;
            try
            {
                var file = Program.ConfigFolderPath + "\\" + name;
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/" + name,
                        file);
                }
                srcBitmap = new Bitmap(file);
            }
            catch
            {
                var file = Program.ConfigFolderPath + "\\Default";
                if (!File.Exists(file))
                {
                    new WebClient().DownloadFile("https://github.com/Sexsimiko7/Elobuddy/raw/master/Resources/Default",
                        file);
                }
                srcBitmap = new Bitmap(file);
            }
            var img = new Bitmap(srcBitmap.Width + 20, srcBitmap.Width + 20);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (Bitmap sourceImage = srcBitmap)
            {
                using (Bitmap croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (Graphics g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);

                            var p = new Pen(color, 5) { Alignment = PenAlignment.Inset };
                            g.DrawEllipse(p, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            Sprite finalSprite =
                new Sprite(TextureLoader.BitmapToTexture(ChangeOpacity(img, opacity))) { Scale = new Vector2(1f, 1f) };
            //finalSprite.X = -25;
            //finalSprite.Color = System.Drawing.Color.LightGray;
            return finalSprite;
        }

        public static Bitmap ChangeOpacity(Bitmap img, int opacity)
        {
            float iconOpacity = opacity / 100.0f;
            var bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
            Graphics graphics = Graphics.FromImage(bmp);
            var colormatrix = new ColorMatrix { Matrix33 = iconOpacity };
            var imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(
                img, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel,
                imgAttribute);
            graphics.Dispose(); // Releasing all resource used by graphics
            img.Dispose();
            return bmp;
        }
    }
}
