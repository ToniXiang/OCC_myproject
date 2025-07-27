using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficNode
{
    public interface TrafficNode
    {

    }
    public class Track : TrafficNode
    {
        public int limitspeed { get; set; }
        public double length { get; set; }
        public Track(int ls, int len)
        {
            limitspeed = ls;
            length = len;
        }
    }
    public class Station : TrafficNode
    {
        public string name { get; set; }
        public int priority { get; set; }
        public int platform { get; set; }
        public Station(string n, int pf, int p)
        {
            name = n;
            platform = pf;
            priority = p;
        }
        public override string ToString() => name;
    }
    public class Train
    {
        public string name { get; set; }
        public double length { get; set; }
        public int priority { get; set; }
        public int start { get; set; }
        public int cur { get; set; }
        public int destination { get; set; }
        public string destinationNAME {  get; set; }
        public int next { get; set; }
        public int wait { get; set; }
        public double speed { get; set; }
        public string stats { get; set; }
        public Train(string n, int p, string st, string end)
        {
            name = n;
            priority = p;
            start = cur = Traffic.GetIndex(st);
            destination = Traffic.GetIndex(end);
            destinationNAME = end;
            next = start < destination ? 1 : -1;
            wait = 0;
            speed = 0;
            stats = $"停靠 {((Station)Traffic.traffics[start]).name} 中";
        }
        public void move() => length += (speed == 0 ? 0 : speed / 3600);
    }
    public class Traffic
    {
        public static List<TrafficNode> traffics = new List<TrafficNode>();
        public static List<Train> trains = new List<Train>();
        public static string projectDirectory;
        public static string[] info;
        public static bool IsInitialized = false;
        public static int GetIndex(string name)
        {
            for (int i = 0; i < traffics.Count; i += 2)
            {
                if (((Station)traffics[i]).name == name) return i;
            }
            return -1;
        }
        public static string GetTime(double total_length, int count)
        {
            // 正常時速 110km/hr 行駛，停靠每個月台約10秒
            int sec = (int)(total_length / 110 * 3600) + count;
            if (sec <= 0) return "即將進站中";
            return $"約{sec / 60}分{sec % 60}秒進站";
        }
    }
}
