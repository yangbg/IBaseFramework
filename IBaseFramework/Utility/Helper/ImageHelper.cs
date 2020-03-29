using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace IBaseFramework.Utility.Helper
{
    /// <summary>
    /// 图片辅助方法
    /// </summary>
    public class ImageHelper
    {
        private const int Threshold = 125;

        #region 压缩图片

        /// <summary>
        /// 压缩图片，返回map
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static Bitmap ResizeImg(Bitmap source, int width, int height)
        {
            if (source == null)
                return null;
            int ow = source.Width, oh = source.Height; //原始大小
            if (height == -2)
            {
                var rale = (double)width / Math.Max(ow, oh); //压缩比例
                width = (int)Math.Ceiling(ow * rale);
                height = (int)Math.Ceiling(oh * rale);
            }
            else
            {
                width = (width != -1 ? width : ow);
                height = (height != -1 ? height : oh);
            }
            var bm = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            using (var g = Graphics.FromImage(bm))
            {
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //                g.Clear(Color.White);

                g.DrawImage(source, new Rectangle(0, 0, width, height), new Rectangle(0, 0, ow, oh),
                    GraphicsUnit.Pixel);
            }
            return bm;
        }

        #endregion

        #region 剪切图片

        /// <summary>
        /// 剪切图片
        /// </summary>
        /// <param name="source">原图片</param>
        /// <param name="x">起始x坐标</param>
        /// <param name="y">起始y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static Bitmap MakeImage(Bitmap source, int x, int y, int width, int height)
        {
            if (source == null)
                return null;
            width = Math.Min(width, source.Width - x);
            height = Math.Min(height, source.Height - y);

            var newBmp = new Bitmap(width, height); // 生成新的画布
            using (var g = Graphics.FromImage(newBmp))
            {
                // 将画布读取到图像中
                var origRect = new Rectangle(x, y, width, height); // 原始图像矩形框
                var destRect = new Rectangle(0, 0, width, height); // 新画布图像矩形框
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 将原始图像矩形框中的内容生成到新画布中去
                g.DrawImage(source, destRect, origRect, GraphicsUnit.Pixel);
                return newBmp;
            }
        }

        /// <summary>
        /// 剪切图片
        /// </summary>
        /// <param name="stream">原图片流</param>
        /// <param name="x">起始x坐标</param>
        /// <param name="y">起始y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>剪切后的图片 Base64 流</returns>
        public static string MakeImage(Stream stream, int x, int y, int width, int height)
        {
            if (!(x >= 0 && y > 0 && width > 0 && height > 0))
                return string.Empty;
            try
            {
                var source = new Bitmap(stream);
                var bmp = MakeImage(source, x, y, width, height);
                var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                ms.Seek(0, SeekOrigin.Begin);
                var bytes = new byte[ms.Length];
                ms.Read(bytes, 0, bytes.Length);
                source.Dispose();
                bmp.Dispose();
                ms.Close();
                ms.Dispose();
                return @"data:image/png;base64," + Convert.ToBase64String(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
        
        #region 图片旋转

        /// <summary>
        /// 任意旋转角度
        /// </summary>
        /// <param name="source">原图片</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="color">背景颜色</param>
        /// <returns></returns>
        public static Bitmap RotateImage(Bitmap source, float angle, Color color)
        {
            var pixelFormat = source.PixelFormat;
            var pixelFormatOld = pixelFormat;
            if (source.Palette.Entries.Count() > 0)
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }
            var tmpBitmap = new Bitmap(source.Width, source.Height, pixelFormat);
            tmpBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var g = Graphics.FromImage(tmpBitmap))
            {

                g.FillRectangle(new SolidBrush(color), 0, 0, source.Width, source.Height);
                g.RotateTransform(angle);
                g.DrawImage(source, 0, 0);
            }
            switch (pixelFormatOld)
            {
                case PixelFormat.Format8bppIndexed:
                    tmpBitmap = CopyTo8Bpp(tmpBitmap);
                    break;
                case PixelFormat.Format1bppIndexed:
                    tmpBitmap = CopyTo1Bpp(tmpBitmap);
                    break;
            }
            return tmpBitmap;
        }

        /// <summary>
        /// 任意旋转角度
        /// </summary>
        /// <param name="source">原图片</param>
        /// <param name="angle">旋转角度</param>
        /// <returns></returns>
        public static Bitmap RotateImage(Bitmap source, float angle)
        {
            return RotateImage(source, angle, Color.White);
        }

        #endregion

        #region 黑阶色判断

        /// <summary>
        /// 是否是黑阶色
        /// </summary>
        /// <param name="bmap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsBlack(Bitmap bmap, int x, int y)
        {
            var c = bmap.GetPixel(x, y);
            return IsBlack(c);
        }

        /// <summary>
        /// 是否是黑阶色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsBlack(Color color)
        {
            return (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114) < 140;
        }

        #endregion

        #region 图像灰度化

        /// <summary>
        /// 图像灰度化
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ToGray(Bitmap bmp)
        {
            for (var i = 0; i < bmp.Width; i++)
            {
                for (var j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    var color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    var newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 图像灰度反转
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap GrayReverse(Bitmap bmp)
        {
            for (var i = 0; i < bmp.Width; i++)
            {
                for (var j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    var color = bmp.GetPixel(i, j);
                    var newColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        #endregion
        
        #region 图像包含判断

        /// <summary>
        /// 判断图形里是否存在另外一个图形 并返回所在位置
        /// </summary>
        /// <param name="pSourceBitmap">原始图形</param>
        /// <param name="pPartBitmap">小图形</param>
        /// <param name="pFloat">溶差</param>
        /// <returns>坐标</returns>
        public static Point ImageContains(Bitmap pSourceBitmap, Bitmap pPartBitmap, int pFloat)
        {
            var sourceWidth = pSourceBitmap.Width;
            var sourceHeight = pSourceBitmap.Height;

            var partWidth = pPartBitmap.Width;
            var partHeight = pPartBitmap.Height;

            var sourceBitmap = new Bitmap(sourceWidth, sourceHeight);
            var graphics = Graphics.FromImage(sourceBitmap);
            graphics.DrawImage(pSourceBitmap, new Rectangle(0, 0, sourceWidth, sourceHeight));
            graphics.Dispose();
            var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceWidth, sourceHeight),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var sourceByte = new byte[sourceData.Stride * sourceHeight];
            //复制出p_SourceBitmap的相素信息 
            Marshal.Copy(sourceData.Scan0, sourceByte, 0, sourceByte.Length);

            for (var i = 2; i < sourceHeight; i++)
            {
                //如果 剩余的高 比需要比较的高 还要小 就直接返回
                if (sourceHeight - i < partHeight) return new Point(-1, -1);
                //临时存放坐标 需要保证找到的是在一个X点上
                var pointX = -1;
                //是否都比配的上
                var sacnOver = true;
                //循环目标进行比较
                for (var z = 0; z < partHeight - 1; z++)
                {
                    var trueX = GetImageContains(sourceByte, i * sourceData.Stride, sourceWidth, partWidth, pFloat);
                    //如果没找到
                    if (trueX == -1)
                    {
                        //设置坐标为没找到
                        pointX = -1;
                        //设置不进行返回
                        sacnOver = false;
                        break;
                    }
                    if (z == 0) pointX = trueX;
                    //如果找到了 也的保证坐标和上一行的坐标一样 否则也返回
                    if (pointX != trueX)
                    {
                        //设置坐标为没找到
                        pointX = -1;
                        //设置不进行返回
                        sacnOver = false;
                        break;
                    }
                }

                if (sacnOver) return new Point(pointX, i);
            }
            return new Point(-1, -1);
        }

        #endregion

        #region 私有方法

        private static Bitmap CopyTo1Bpp(Bitmap b)
        {
            int w = b.Width, h = b.Height;
            var r = new Rectangle(0, 0, w, h);
            if (b.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                var temp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                temp.SetResolution(b.HorizontalResolution, b.VerticalResolution);
                var g = Graphics.FromImage(temp);
                g.DrawImage(b, r, 0, 0, w, h, GraphicsUnit.Pixel);
                g.Dispose();
                b = temp;
            }
            var bdat = b.LockBits(r, ImageLockMode.ReadOnly, b.PixelFormat);
            var b0 = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            b0.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            var b0Dat = b0.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var index = y * bdat.Stride + (x * 4);
                    if (
                        Color.FromArgb(Marshal.ReadByte(bdat.Scan0, index + 2), Marshal.ReadByte(bdat.Scan0, index + 1),
                            Marshal.ReadByte(bdat.Scan0, index)).GetBrightness() > 0.5f)
                    {
                        var index0 = y * b0Dat.Stride + (x >> 3);
                        var p = Marshal.ReadByte(b0Dat.Scan0, index0);
                        var mask = (byte)(0x80 >> (x & 0x7));
                        Marshal.WriteByte(b0Dat.Scan0, index0, (byte)(p | mask));
                    }
                }
            }
            b0.UnlockBits(b0Dat);
            b.UnlockBits(bdat);
            return b0;
        }

        private static Bitmap CopyTo8Bpp(Bitmap bmp)
        {
            if (bmp == null) return null;

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            var width = bmpData.Width;
            var height = bmpData.Height;
            var stride = bmpData.Stride;
            var offset = stride - width * 3;
            var ptr = bmpData.Scan0;
            var scanBytes = stride * height;

            int posScan = 0, posDst = 0;
            var rgbValues = new byte[scanBytes];
            Marshal.Copy(ptr, rgbValues, 0, scanBytes);
            var grayValues = new byte[width * height];

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var temp = rgbValues[posScan++] * 0.11 +
                                  rgbValues[posScan++] * 0.59 +
                                  rgbValues[posScan++] * 0.3;
                    grayValues[posDst++] = (byte)temp;
                }
                posScan += offset;
            }

            Marshal.Copy(rgbValues, 0, ptr, scanBytes);
            bmp.UnlockBits(bmpData);

            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            bitmap.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            var offset0 = bitmapData.Stride - bitmapData.Width;
            var scanBytes0 = bitmapData.Stride * bitmapData.Height;
            var rawValues = new byte[scanBytes0];

            var posSrc = 0;
            posScan = 0;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    rawValues[posScan++] = grayValues[posSrc++];
                }
                posScan += offset0;
            }

            Marshal.Copy(rawValues, 0, bitmapData.Scan0, scanBytes0);
            bitmap.UnlockBits(bitmapData);

            ColorPalette palette;
            using (var bmp0 = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp0.Palette;
            }
            for (var i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;

            return bitmap;
        }

        /// <summary>
        /// 比较数组相似度
        /// </summary>
        /// <param name="firstNum"></param>
        /// <param name="scondNum"></param>
        /// <returns></returns>
        private static float CompareHisogram(int[] firstNum, int[] scondNum)
        {
            if (firstNum.Length != scondNum.Length)
                return 0;
            float result = 0;
            var j = firstNum.Length;
            for (var i = 0; i < j; i++)
            {
                result += 1 - GetAbs(firstNum[i], scondNum[i]);
            }
            return result / j;
        }

        /// <summary>
        /// 计算相减后的绝对值
        /// </summary>
        /// <param name="firstNum"></param>
        /// <param name="secondNum"></param>
        /// <returns></returns>
        private static float GetAbs(int firstNum, int secondNum)
        {
            var abs = Math.Abs(firstNum - secondNum);
            var result = Math.Max(firstNum, secondNum);
            if (result == 0)
                result = 1;
            return abs / (float)result;
        }

        /// <summary>
        /// 判断图形里是否存在另外一个图形 所在行的索引
        /// </summary>
        /// <param name="pSource">原始图形数据</param>
        /// <param name="pSourceIndex">开始位置</param>
        /// <param name="pSourceWidth">原始图形宽</param>
        /// <param name="pPartWidth">小图宽</param>
        /// <param name="pFloat">溶差</param>
        /// <returns>所在行的索引 如果找不到返回-1</returns>
        private static int GetImageContains(byte[] pSource, int pSourceIndex, int pSourceWidth, int pPartWidth,
            int pFloat)
        {
            var sourceIndex = pSourceIndex;
            for (var i = 0; i < pSourceWidth; i++)
            {
                if (pSourceWidth - i < pPartWidth) return -1;
                var currentlyColor = Color.FromArgb(pSource[sourceIndex + 3], pSource[sourceIndex + 2],
                    pSource[sourceIndex + 1], pSource[sourceIndex]);
                sourceIndex += 4;
                var scanColor = ScanColor(currentlyColor, pFloat);

                if (scanColor)
                {
                    var sourceRva = sourceIndex;
                    var equals = true;
                    for (var z = 0; z != pPartWidth - 1; z++)
                    {
                        currentlyColor = Color.FromArgb(pSource[sourceRva + 3], pSource[sourceRva + 2],
                            pSource[sourceRva + 1], pSource[sourceRva]);
                        if (!ScanColor(currentlyColor, pFloat))
                        {
                            equals = false;
                            break;
                        }
                        sourceRva += 4;
                    }
                    if (equals) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 检查色彩(可以根据这个更改比较方式)
        /// </summary>
        /// <param name="pCurrentlyColor">当前色彩</param>
        /// <param name="pFloat">溶差</param>
        /// <returns></returns>
        private static bool ScanColor(Color pCurrentlyColor, int pFloat)
        {
            return (pCurrentlyColor.R + pCurrentlyColor.G + pCurrentlyColor.B) / 3 < pFloat;
        }

        #endregion
    }
}
