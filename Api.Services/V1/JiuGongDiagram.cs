using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.IO;
using System.Net;

namespace Api.Services.V1
{
    public class JiuGongDiagram
    {
        private const int _width = 132; //合成九宫图的宽度
        private const int _height = 132; //合成九宫图的高度

        /// <summary>
        /// 获取坐标
        /// </summary>
        /// <param name="number">数量</param>
        /// <returns></returns>
        private string[] GetXy(int number)
        {
            string[] s = new string[number];
            int _x = 0;
            int _y = 0;
            switch (number)
            {
                case 1:
                    _x = _y = 6;
                    s[0] = "6,6";
                    break;
                case 2:
                    _x = _y = 4;
                    s[0] = "4," + (132 / 2 - 60 / 2);
                    s[1] = 60 + 2 * _x + "," + (132 / 2 - 60 / 2);
                    break;
                case 3:
                    _x = _y = 4;
                    s[0] = (132 / 2 - 60 / 2) + "," + _y;
                    s[1] = _x + "," + (60 + 2 * _y);
                    s[2] = (60 + 2 * _y) + "," + (60 + 2 * _y);
                    break;
                case 4:
                    _x = _y = 4;
                    s[0] = _x + "," + _y;
                    s[1] = (_x * 2 + 60) + "," + _y;
                    s[2] = _x + "," + (60 + 2 * _y);
                    s[3] = (60 + 2 * _y) + "," + (60 + 2 * _y);
                    break;
                case 5:
                    _x = _y = 3;
                    s[0] = (132 - 40 * 2 - _x) / 2 + "," + (132 - 40 * 2 - _y) / 2;
                    s[1] = ((132 - 40 * 2 - _x) / 2 + 40 + _x) + "," + (132 - 40 * 2 - _y) / 2;
                    s[2] = _x + "," + ((132 - 40 * 2 - _x) / 2 + 40 + _y);
                    s[3] = (_x * 2 + 40) + "," + ((132 - 40 * 2 - _x) / 2 + 40 + _y);
                    s[4] = (_x * 3 + 40 * 2) + "," + ((132 - 40 * 2 - _x) / 2 + 40 + _y);
                    break;
                case 6:
                    _x = _y = 3;
                    s[0] = _x + "," + ((132 - 40 * 2 - _x) / 2);
                    s[1] = (_x * 2 + 40) + "," + ((132 - 40 * 2 - _x) / 2);
                    s[2] = (_x * 3 + 40 * 2) + "," + ((132 - 40 * 2 - _x) / 2);
                    s[3] = _x + "," + ((132 - 40 * 2 - _x) / 2 + 40 + _y);
                    s[4] = (_x * 2 + 40) + "," + ((132 - 40 * 2 - _x) / 2 + 40 + _y);
                    s[5] = (_x * 3 + 40 * 2) + "," + ((132 - 40 * 2 - _x) / 2 + 40 + _y);
                    break;
                case 7:
                    _x = _y = 3;
                    s[0] = (132 - 40) / 2 + "," + _y;
                    s[1] = _x + "," + (_y * 2 + 40);
                    s[2] = (_x * 2 + 40) + "," + (_y * 2 + 40);
                    s[3] = (_x * 3 + 40 * 2) + "," + (_y * 2 + 40);
                    s[4] = _x + "," + (_y * 3 + 40 * 2);
                    s[5] = (_x * 2 + 40) + "," + (_y * 3 + 40 * 2);
                    s[6] = (_x * 3 + 40 * 2) + "," + (_y * 3 + 40 * 2);
                    break;
                case 8:
                    _x = _y = 3;
                    s[0] = (132 - 80 - _x) / 2 + "," + _y;
                    s[1] = ((132 - 80 - _x) / 2 + _x + 40) + "," + _y;
                    s[2] = _x + "," + (_y * 2 + 40);
                    s[3] = (_x * 2 + 40) + "," + (_y * 2 + 40);
                    s[4] = (_x * 3 + 40 * 2) + "," + (_y * 2 + 40);
                    s[5] = _x + "," + (_y * 3 + 40 * 2);
                    s[6] = (_x * 2 + 40) + "," + (_y * 3 + 40 * 2);
                    s[7] = (_x * 3 + 40 * 2) + "," + (_y * 3 + 40 * 2);
                    break;
                case 9:
                    _x = _y = 3;
                    s[0] = _x + "," + _y;
                    s[1] = _x * 2 + 40 + "," + _y;
                    s[2] = _x * 3 + 40 * 2 + "," + _y;
                    s[3] = _x + "," + (_y * 2 + 40);
                    s[4] = (_x * 2 + 40) + "," + (_y * 2 + 40);
                    s[5] = (_x * 3 + 40 * 2) + "," + (_y * 2 + 40);
                    s[6] = _x + "," + (_y * 3 + 40 * 2);
                    s[7] = (_x * 2 + 40) + "," + (_y * 3 + 40 * 2);
                    s[8] = (_x * 3 + 40 * 2) + "," + (_y * 3 + 40 * 2);
                    break;
            }
            return s;
        }

        /// <summary>
        /// 获取尺寸
        /// </summary>
        /// <param name="number">数量</param>
        /// <returns></returns>
        private float GetSize(int number)
        {
            int w = 0;
            if (number == 1)
            {
                w = 120;
            }
            if (number > 1 && number <= 4)
            {
                w = 60;
            }
            if (number >= 5)
            {
                w = 40;
            }
            return w;
        }

        /// <summary>
        /// 图片压缩
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <returns></returns>
        private Bitmap ImageCompression(Bitmap bitmap, int width, int height)
        {
            using (var bmp = new Bitmap(width, height))
            {
                //从Bitmap创建一个Graphics对象，用来绘制高质量的缩小图。  
                using (var gr = Graphics.FromImage(bmp))
                {
                    //设置 System.Drawing.Graphics对象的SmoothingMode属性为HighQuality  
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    //下面这个也设成高质量  
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                    //下面这个设成High  
                    gr.InterpolationMode = InterpolationMode.High;
                    //把原始图像绘制成上面所设置宽高的截取图  
                    var rectDestination = new Rectangle(0, 0, width, height);
                    //绘制
                    gr.DrawImage(bitmap, 0, 0, rectDestination, GraphicsUnit.Pixel);
                    //保存截取的图片  
                    return bitmap;
                }
            }
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        private Bitmap PathConvertBitmap(string path, bool isHttp)
        {
            if (!isHttp)
            {
                return new Bitmap(path);
            }
            using (WebClient client = new WebClient())
            {
                using (Stream stream = client.OpenRead(path))
                {
                    return (Bitmap)Image.FromStream(stream);
                }
            }
        }

        /// <summary>
        /// 合成九宫图
        /// </summary>
        /// <param name="imgPath">图片路径(绝对路径)</param>
        /// <param name="isHttp">网络图片</param>
        /// <returns></returns>
        public Bitmap Synthetic(string[] imgPath, bool isHttp = false)
        {
            //合成数量
            int number = imgPath.Length;
            //坐标
            string[] xys = this.GetXy(number);
            //尺寸
            var size = this.GetSize(number);
            Bitmap backgroudBitmap = new Bitmap(_width, _height);
            using (Graphics g = Graphics.FromImage(backgroudBitmap))
            {
                //设置 System.Drawing.Graphics对象的SmoothingMode属性为HighQuality  
                g.SmoothingMode = SmoothingMode.HighQuality;
                //下面这个也设成高质量  
                g.CompositingQuality = CompositingQuality.HighQuality;
                //下面这个设成High  
                g.InterpolationMode = InterpolationMode.High;
                //清除画布,背景设置为透明色
                g.Clear(Color.FromArgb(255, 255, 255));
                for (int i = 0; i < imgPath.Length; i++)
                {
                    var bitmap = this.PathConvertBitmap(imgPath[i], isHttp);
                    string[] xy = xys[i].Split(',');
                    int x = int.Parse(xy[0]);
                    int y = int.Parse(xy[1]);
                    bitmap = this.ImageCompression(bitmap, x, y);
                    g.DrawImage(bitmap, x, y, size, size);
                    bitmap.Dispose();
                }
            }
            return backgroudBitmap;
        }
    }
}
