using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace IBaseFramework.Helper
{
    /// <summary>
    /// 图片模板处理帮助类
    /// </summary>
    public class TemplateImageHelper
    {
        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="imageList"></param>
        /// <returns></returns>
        public static Bitmap AddImages(Bitmap templateSource, List<ImageTemplateItem> imageList)
        {
            if (templateSource == null || imageList == null || imageList.Count < 1)
                return templateSource;

            var templateImage = new TemplateImage(templateSource);
            return templateImage.Generate(imageList.ToArray());
        }

        /// <summary>
        /// 添加二维码
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="qrCodeList"></param>
        /// <returns></returns>
        public static Bitmap AddQrCode(Bitmap templateSource, List<QrCodeTemplateItem> qrCodeList)
        {
            if (templateSource == null || qrCodeList == null || qrCodeList.Count < 1)
                return templateSource;

            var templateImage = new TemplateImage(templateSource);
            return templateImage.Generate(qrCodeList.ToArray());
        }

        /// <summary>
        /// 添加文本
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="textList"></param>
        /// <returns></returns>
        public static Bitmap AddText(Bitmap templateSource, List<StringTemplateItem> textList)
        {
            if (templateSource == null || textList == null || textList.Count < 1)
                return templateSource;

            var templateImage = new TemplateImage(templateSource);
            return templateImage.Generate(textList.ToArray());
        }
    }


    /// <summary>
    /// 使用一个bitmap作为模板,后在模板上叠加图片或文字并导出为新的图片,该操作不会影响作为模板的图片
    /// </summary>
    public class TemplateImage : IDisposable
    {
        private Bitmap _templateSource;
        private Stream _sourceStream;

        public TemplateImage(Bitmap templateSource)
        {
            _templateSource = templateSource;
        }

        /// <summary>
        /// 模板图片的构造函数
        /// </summary>
        /// <param name="templatePath">模板图片文件绝对路径</param>
        /// <param name="isWatchFileModify">是否自动监控文件,当文件有变动时,自动重新加载模板文件
        /// </param>
        public TemplateImage(string templatePath, bool isWatchFileModify = false)
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(nameof(templatePath));
            }

            using var file = File.OpenRead(templatePath);

            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);

            var data = memoryStream.ToArray();

            var s1 = new MemoryStream(data);
            _sourceStream = s1;
            _templateSource = (Bitmap)Image.FromStream(s1);

            if (isWatchFileModify)
            {
                var dir = Path.GetDirectoryName(templatePath);

                if (Directory.Exists(dir))
                {
                    var wather = new FileSystemWatcher(templatePath) { EnableRaisingEvents = true };
                    wather.Changed += Watcher_changed;
                }
            }
        }

        private void Watcher_changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                using var file = File.OpenRead(e.FullPath);

                using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);

                var data = memoryStream.ToArray();

                var oldValue = _sourceStream;
                var templateSource = _templateSource;
                var s1 = new MemoryStream(data);
                var newTemplateSource = (Bitmap)Image.FromStream(s1);

                _sourceStream = s1;
                _templateSource = newTemplateSource;

                oldValue.Close();
                oldValue.Dispose();
                templateSource.Dispose();
            }
        }

        public SmoothingMode SmoothingMode { set; get; } = SmoothingMode.HighQuality;

        public TextRenderingHint TextRenderingHint { set; get; } = TextRenderingHint.AntiAlias;

        public CompositingQuality CompositingQuality { set; get; } = CompositingQuality.HighQuality;

        /// <summary>
        /// 根据传入的数据,套入模板图片,生成新的图片
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Bitmap Generate(TemplateItemBase[] settings)
        {
            var newImg = (Bitmap)_templateSource.Clone();

            Graphics.FromImage(_templateSource);

            using var g = Graphics.FromImage(newImg);
            g.SmoothingMode = SmoothingMode;
            g.TextRenderingHint = TextRenderingHint;
            g.CompositingQuality = CompositingQuality;

            foreach (var item in settings)
            {
                item.Draw(g, newImg.Size);
            }

            return newImg;
        }

        public void Dispose()
        {
            _templateSource.Dispose();

            _sourceStream?.Close();
            _sourceStream?.Dispose();
        }
    }

    public enum HorizontalPosition
    {
        Custom = -1,

        Center = 1,
    }

    public enum VerticalPosition
    {
        Custom = -1,

        Middle = 1
    }

    public abstract class TemplateItemBase
    {
        /// <summary>
        /// 水平方向对其方式,默认为Custom,使用Location定位
        /// </summary>
        public HorizontalPosition Horizontal { set; get; } = HorizontalPosition.Custom;

        /// <summary>
        /// 垂直方向对其方式,默认为Custom,使用Location定位
        /// </summary>
        public VerticalPosition Vertical { set; get; } = VerticalPosition.Custom;

        /// <summary>
        /// 输出项定位
        /// </summary>
        public Point Location { set; get; }

        public abstract void Draw(Graphics graphics, Size newBitmapSize);

    }

    /// <summary>
    /// 传入一个图片
    /// </summary>
    public class ImageTemplateItem : TemplateItemBase
    {
        /// <summary>
        /// 图片信息
        /// </summary>
        public Bitmap Image { set; get; }

        /// <summary>
        /// 图片输出到模板图的时候的大小
        /// </summary>
        public Size Size { set; get; }

        public override void Draw(Graphics graphics, Size newBitmapSize)
        {
            var location = this.Location;

            if (this.Horizontal == HorizontalPosition.Center || this.Vertical == VerticalPosition.Middle)
            {
                location = new Point(this.Location.X, this.Location.Y);

                if (this.Horizontal == HorizontalPosition.Center)
                {
                    var newx = newBitmapSize.Width / 2 - this.Size.Width / 2;

                    location.X = newx;
                }

                if (this.Vertical == VerticalPosition.Middle)
                {
                    var newy = newBitmapSize.Height / 2 - this.Size.Height / 2;
                    location.Y = newy;
                }
            }

            graphics.DrawImage(Image, new Rectangle(location, this.Size), new Rectangle(0, 0, this.Image.Width, this.Image.Height), GraphicsUnit.Pixel);

        }
    }

    /// <summary>
    /// 二维码项
    /// </summary>
    public class QrCodeTemplateItem : TemplateItemBase
    {
        /// <summary>
        /// 二维码内实际存储的字符数据
        /// </summary>
        public string QrCode { set; get; }

        /// <summary>
        /// 二维码中心的icon图标
        /// </summary>
        public Bitmap Icon { set; get; }

        /// <summary>
        /// 二维码尺寸
        /// </summary>
        public Size Size { set; get; }

        /// <summary>
        /// 容错级别,默认为M
        /// </summary>
        public QRCodeGenerator.ECCLevel EccLevel { set; get; } = QRCodeGenerator.ECCLevel.M;

        public override void Draw(Graphics graphics, Size newBitmapSize)
        {
            var location = this.Location;

            if (this.Horizontal == HorizontalPosition.Center || this.Vertical == VerticalPosition.Middle)
            {
                location = new Point(this.Location.X, this.Location.Y);

                if (this.Horizontal == HorizontalPosition.Center)
                {
                    var newx = newBitmapSize.Width / 2 - this.Size.Width / 2;

                    location.X = newx;
                }

                if (this.Vertical == VerticalPosition.Middle)
                {
                    var newy = newBitmapSize.Height / 2 - this.Size.Height / 2;
                    location.Y = newy;
                }
            }

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(QrCode, EccLevel);
            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, Icon);
            graphics.DrawImage(qrCodeImage, new Rectangle(location, this.Size), new Rectangle(0, 0, qrCodeImage.Width, qrCodeImage.Height), GraphicsUnit.Pixel);
        }
    }

    /// <summary>
    /// 普通字符串项
    /// </summary>
    public class StringTemplateItem : TemplateItemBase
    {
        /// <summary>
        /// 文本字符串值
        /// </summary>
        public string Value { set; get; }

        /// <summary>
        /// 字体信息
        /// </summary>
        public Font Font { set; get; }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color Color { set; get; } = Color.Black;

        /// <summary>
        /// 文本输出的最大宽度,如果为0,则自动,,如果非0,则只用最大宽度,并自动根据最大宽度修改计算字符串所需高度
        /// </summary>
        public int MaxWidth { set; get; } = 0;

        /// <summary>
        /// 字符串输出参数
        /// </summary>
        /// <example>
        /// 如纵向输出:
        /// new StringFormat(StringFormatFlags.DirectionVertical)
        /// 
        /// </example>
        public StringFormat StringFormat { set; get; }

        public override void Draw(Graphics graphics, Size newBitmapSize)
        {
            var location = this.Location;
            SizeF size = default(Size);
            if (this.Horizontal == HorizontalPosition.Center || this.Vertical == VerticalPosition.Middle)
            {
                location = new Point(this.Location.X, this.Location.Y);

                if (this.MaxWidth > 0)
                {
                    size = graphics.MeasureString(this.Value, this.Font, this.MaxWidth);
                }
                else
                {
                    size = graphics.MeasureString(this.Value, this.Font);
                }

                if (this.Horizontal == HorizontalPosition.Center)
                {
                    var newx = newBitmapSize.Width / 2 - (int)(size.Width / 2);
                    location.X = newx;
                }

                if (this.Vertical == VerticalPosition.Middle)
                {
                    var newy = newBitmapSize.Height / 2 - (int)(size.Height / 2);
                    location.Y = newy;
                }
            }
            else if (MaxWidth > 0)
            {
                size = graphics.MeasureString(this.Value, this.Font, this.MaxWidth);
            }

            if (MaxWidth > 0)
            {
                graphics.DrawString(this.Value, this.Font, new SolidBrush(this.Color), new RectangleF(location, size), StringFormat);
            }
            else
            {
                graphics.DrawString(this.Value, this.Font, new SolidBrush(this.Color), location, StringFormat);
            }


        }
    }
}