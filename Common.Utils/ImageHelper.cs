using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2020-01-07 10:33:37
	*描  述  ：
	***********************************************************************/
    public static class ImageHelper
    {
        /// <summary>
        /// 按照指定的高和宽生成相应的规格的图片，采用此方法生成的缩略图片不会失真
        /// </summary>
        /// <param name="width">指定宽度</param>
        /// <param name="height">指定高度</param>
        /// <param name="path">原图片路径</param>
        /// <param name="thumbPath">缩略图保存路径</param>
        /// <returns>返回新生成的图</returns>
        public static void GetReducedImage(int width, int height, string path, string thumbPath)
        {
            using (Image imageFrom = Image.FromFile(path))
            {
                // 源图宽度及高度 
                int imageFromWidth = imageFrom.Width;
                int imageFromHeight = imageFrom.Height;

                // 生成的缩略图实际宽度及高度.如果指定的高和宽比原图大，则返回原图；否则按照指定高宽生成图片
                if (width >= imageFromWidth && height >= imageFromHeight)
                {
                    imageFrom.Save(thumbPath, ImageFormat.Png);
                }
                else
                {
                    // 生成的缩略图在上述"画布"上的位置
                    int X = 0;
                    int Y = 0;

                    // 创建画布
                    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    bmp.SetResolution(imageFrom.HorizontalResolution, imageFrom.VerticalResolution);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        // 用白色清空 
                        g.Clear(Color.White);

                        // 指定高质量的双三次插值法。执行预筛选以确保高质量的收缩。此模式可产生质量最高的转换图像。 
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        // 指定高质量、低速度呈现。 
                        g.SmoothingMode = SmoothingMode.HighQuality;

                        // 在指定位置并且按指定大小绘制指定的 Image 的指定部分。 
                        g.DrawImage(imageFrom, new Rectangle(X, Y, width, height),
                            new Rectangle(0, 0, imageFromWidth, imageFromHeight), GraphicsUnit.Pixel);

                        //将图片以指定的格式保存到到指定的位置
                        bmp.Save(thumbPath, ImageFormat.Png);
                    }
                }
            }
        }


        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="sFile">原图片地址</param>
        /// <param name="dFile">压缩后保存图片地址</param>
        /// <param name="flag">压缩质量（数字越小压缩率越高）1-100</param>
        /// <param name="size">压缩后图片的最大大小</param>
        /// <returns></returns>
        public static void CompressImage(string sFile, string dFile, int flag = 90, int size = 300)
        {
            Image iSource = Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int dHeight = iSource.Height / 2;
            int dWidth = iSource.Width / 2;
            int sW = 0, sH = 0;
            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();

            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;

            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo jpegICIinfo = null;
            for (int x = 0; x < arrayICI.Length; x++)
            {
                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                {
                    jpegICIinfo = arrayICI[x];
                    break;
                }
            }
            if (jpegICIinfo != null)
            {
                ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径
                FileInfo fi = new FileInfo(dFile);
                if (fi.Length > 1024 * size)
                {
                    flag = flag - 10;
                    CompressImage(sFile, dFile, flag, size);
                }
            }
            else
            {
                ob.Save(dFile, tFormat);
            }

            iSource.Dispose();
            ob.Dispose();
        }
    }
}
