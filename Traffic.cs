using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace 簡易的行控中心
{
    internal class Traffic
    {
        // 檔案位置
        public static string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
    }
    public interface TrafficNode
    {
        bool isStation();
    }
    public class Track : TrafficNode
    {
        // 交通節點的限速(km/hr)
        public int limitspeed { get; set; }
        // 交通節點的長度(km)
        public double length { get; set; }
        public Station station1 { get; set; }
        public Station station2 { get; set; }
        public PictureBox picture1 { get; set; }
        public PictureBox picture2 { get; set; }
        public Track(int ls, int len, Station st1, Station st2, PictureBox p1, PictureBox p2)
        {
            limitspeed = ls;
            length = len;
            station1 = st1;
            station2 = st2;
            picture1 = p1;
            picture2 = p2;
        }
        public bool isStation() => false;
    }
    public class Station : TrafficNode
    {
        // 車站名稱
        public string name { get; set; }
        // 列車優先權大於車站 => 不停站
        public int priority { get; set; }
        // 是否有車站在月台
        public int platform { get; set; }
        // 南下的軌道
        public Track track1 { get; set; }
        // 北上的軌道
        public Track track2 { get; set; }
        public Label label { get; set; }
        public void set_connect(Track t1, Track t2)
        {
            track1 = t1;
            track2 = t2;
        }
        public Station(string n, int pf, int p, Label l)
        {
            name = n;
            platform = pf;
            priority = p;
            label = l;
        }
        public bool isStation() => true;
    }
    public class Train
    {
        // 列車的名稱
        public string name { get; set; }
        // 在此鐵路已經行駛的距離
        public double length { get; set; }
        // 列車的優先權(1：表示最高權)
        public int priority { get; set; }
        // 列車的起始車站
        public Station start { get; set; }
        // 列車開出的車站或是正在停靠的車站名
        public TrafficNode cur_st { get; set; }
        // 列車開往的目的地
        public Station destination { get; set; }
        // 判斷北上還是南下列車
        public bool next_bool { get; set; }
        // 列車在車站的正在等待時間(約10秒)
        public int wait { get; set; }
        // 列車的時速(km/hr)
        public double speed { get; set; }
        public string stats { get; set; }
        public Train(string n, int p, Station st, Station end)
        {
            name = n;
            priority = p;
            start = st;
            cur_st = st;
            destination = end;
            next_bool = TrainInfo.get_next(st, end);
            wait = 0;
            speed = 0;
            stats = $"停靠 {((Station)cur_st).name} 中";
        }
        public string getTime(double total_length, int count)
        {
            // 正常時速 110km/hr 行駛，停靠每個月台約10秒
            int sec = (int)((total_length - length) / 110 * 3600) + count;
            if (sec <= 0) return "即將進站中";
            return $"約{sec / 60}分{sec % 60}秒進站";
        }
        public void move() => length += (speed == 0 ? 0 : speed / 3600);
        public void change_image(string color)
        {
            if (cur_st == null || cur_st.isStation()) return;
            if (!next_bool) ((Track)cur_st).picture1.Image = new Bitmap(Traffic.projectDirectory + $"\\scr\\utod{color}.jpg");
            else ((Track)cur_st).picture2.Image = new Bitmap(Traffic.projectDirectory + $"\\scr\\dtou{color}.jpg");
        }
    }
    public class TrainInfo
    {
        public static string[] info;
        public static bool get_next(Station cur_st, Station end)
        {
            return Array.IndexOf(info, cur_st.name) > Array.IndexOf(info, end.name);
        }
    }
    public class SpeedException : Exception
    {
        public SpeedException(string message) : base(message)
        {

        }
    }
    class FormatException : Exception
    {
        public FormatException(string message) : base(message)
        {

        }
    }
}
