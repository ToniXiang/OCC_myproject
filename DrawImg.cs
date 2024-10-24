using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace 簡易的行控中心
{
    internal class DrawReadImg
    {
        public static void DoImg(string filepath, ref PictureBox[] pic1, ref PictureBox[] pic2)
        {
            List<Color> colors = new List<Color>() { Color.Black, Color.Blue, Color.Orange, Color.Red };
            List<string> str = new List<string>() { "", "B", "O", "R" };
            List<string> head = new List<string>() { "utod", "dtou" };
            string scrFolderPath = Path.Combine(filepath, "scr");
            Pen pen = new Pen(colors[0], 2);
            pen.CustomEndCap = new AdjustableArrowCap(5, 5);
            if (!Directory.Exists(scrFolderPath)) Directory.CreateDirectory(scrFolderPath);
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < str.Count; j++)
                {
                    if (File.Exists(Path.Combine(scrFolderPath, head[i] + str[j] + ".jpg"))) continue;
                    using(Bitmap bitmap = new Bitmap(50, 50))
                    {
                        pen.Color = colors[j];
                        DrawArrow(bitmap, pen, head[i]);
                        bitmap.Save(Path.Combine(scrFolderPath, head[i] + str[j] + ".jpg"));
                    }
                }
            }
            for (int i = 0; i < 6; ++i)
            {
                pic1[i].Image = new Bitmap(Path.Combine(scrFolderPath, "utod.jpg"));
                pic2[i].Image = new Bitmap(Path.Combine(scrFolderPath, "dtou.jpg"));
            }
            // 儲存在資料夾 scr 內，每張照片為透明背景 50 x 50 Jpg
            // 已經有相同的"檔案名"則不覆蓋
        }
        private static Bitmap DrawArrow(Bitmap bitmap, Pen pen, string direction)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                if (direction == "utod")
                {
                    g.DrawLine(pen, 25, 0, 25, 50);
                }
                else if (direction == "dtou")
                {
                    g.DrawLine(pen, 25, 50, 25, 0);
                }
            }
            return bitmap;
        }
    }
}