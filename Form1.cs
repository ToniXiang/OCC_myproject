using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using System.Data.SqlClient;
using System.IO;

namespace 簡易的行控中心
{
    public partial class Form1 : Form
    {
        #region 宣告
        private static List<Train> trains = new List<Train>();
        private static List<Station> stations = new List<Station>();
        private static SqlConnection db = new SqlConnection("Data Source=DESKTOP-Q78F81O;Initial Catalog=OCC_DB;User ID=cgibe;Password=45rain78bow_K");
        private static string[] stationNames =
        {
            "Sky Station",
            "Sun Station",
            "Starlight Station",
            "Aurora Station",
            "Celestial Station",
            "Solaris Station",
            "Horizon Station"
        };
        private static bool fg = true;
        private static CheckBox[] checks;
        private static bool[,] is_check =
        {
            { true, true, true, true, true, true },
            { false, false, false, true, false, true },
            { false, true, false, true, false, true },
            { false, false, false, false, true, false },
            { false, false, false, false, false, true }
        };
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            Bitmap b = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "programming.png");
            this.Icon = Icon.FromHandle(b.GetHicon());
            timer1.Start();
            TrainInfo.info = stationNames;
            int[] limitSpeeds = { 110, 120, 120, 120, 120, 130 };
            int[] lengths = { 13, 20, 15, 20, 17, 30 };
            int[] platform = { 2, 1, 1, 1, 1, 1, 2 };
            int[] prio = { 1, 2, 2, 2, 3, 3, 1 };
            PictureBox[] pic1 = { pictureBox8, pictureBox9, pictureBox10, pictureBox11, pictureBox12, pictureBox13 };
            PictureBox[] pic2 = { pictureBox1, pictureBox2, pictureBox4, pictureBox5, pictureBox6, pictureBox7 };
            Label[] labels = { label2, label3, label12, label13, label4, label5, label6 };
            for (int i = 0; i < 7; i++)
            {
                stations.Add(new Station(stationNames[i], platform[i], prio[i], labels[i]));
            }
            List<Track> tracks = new List<Track>();
            tracks.Add(null);
            for (int i = 0; i < 6; i++)
            {
                tracks.Add(new Track(limitSpeeds[i], lengths[i], stations[i], stations[i + 1], pic1[i], pic2[i]));
            }
            tracks.Add(null);
            for(int i = 0; i < 7; i++)
            {
                stations[i].set_connect(tracks[i], tracks[i + 1]);
            }
            trains.Add(new Train("Apple 1104", 2, stations[0], stations[6], TrainInfo.get_next(stations[0], stations[1])));
            trains.Add(new Train("Banana 1111", 3, stations[6], stations[0], TrainInfo.get_next(stations[6], stations[1])));
            comboBox1.Items.AddRange(trains.Select(x => x.name).ToArray());
            comboBox2.Items.AddRange(stations.Select(x => x.name).ToArray());
            comboBox4.Items.AddRange(stations.Select(x => x.name).ToArray());
            string[] tmp = { "1", "2", "3" };
            tmp.ToList().ForEach(x => comboBox3.Items.Add(x));
            string[] tmp1 = { "車次", "時間", "開往", "狀態" };
            tmp1.ToList().ForEach(x => dgv.Columns.Add(x, x));
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.Columns.Cast<DataGridViewColumn>().ToList().ForEach(x => x.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells);
            dgv.DefaultCellStyle.Font = new Font("微軟正黑體", 9);
            label17.Text = "暫停模擬";
            label17.ForeColor = Color.Red;
            label11.Text = label7.Text = "";
            string[] tmp2 = { "列車事故", "突發健康事件", "恐怖襲擊", "能源故障", "乘客滯留" };
            comboBox5.Items.AddRange(tmp2);
            checks = new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 };
            comboBox6.Items.AddRange(trains.Select(x => x.name).ToArray());
        }
        #endregion
        #region 離開
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion
        #region 時間_多執行緒
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = $"日期 : {DateTime.Now.ToString("yyyy年MM月dd號")}\r\n時間 : {DateTime.Now.ToString("T")}\r\n星期 : {DateTime.Now.ToString("dddd")}";
        }
        #endregion
        #region 介紹
        private void textbox1_Click(object sender, EventArgs e)
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show("簡易的行控中心(Simple operational control center)\r\n1. 使用者可以自行操作列車" +
                "\r\n2. 時刻掌握當前時間\r\n3. 使用模擬計算來預估列車的當前狀態\r\n5. 路線地圖可視化展示\r\n6. 用戶回饋連結資料庫\r\n7. 更多功能日後開發\r\n\r\n" +
                "© 2024 陳國翔\r\n\r\n"+ "功能介绍 - 版本 " + version.ToString(), "簡介", MessageBoxButtons.OK);
            label1.Focus();
        }   
        private void Form1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("1. 車站狀態\r\n請選擇車站名稱\r\n時刻表包含將進站的列車名、預估抵達時間、起始站、終點站\r\n2. 列車狀態及操作\r\n請選擇列車名稱\r\n調整時速請按「增速」和「減速」±10km/h" +
                "\r\n「煞車」行駛的列車經過幾秒停止\r\n「啟動」列車時速維持到110km/h\r\n3. 路線地圖\r\n黑色：無列車在此行駛" +
                "\r\n橘色：有列車要進站或離站\r\n藍色：有列車站在此行駛\r\n紅色：有安全問題", "功能簡介", MessageBoxButtons.OK);
        }
        #endregion
        #region 開啟/暫停
        private void button1_Click(object sender, EventArgs e)
        {
            timer2.Start();
            label17.Text = "正在模擬";
            label17.ForeColor = Color.AliceBlue;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            timer2.Stop();
            label17.Text = "暫停模擬";
            label17.ForeColor = Color.Red;
            if (fg)
            {
                db.Open();
                using (Form2 f2 = new Form2())
                {
                    f2.Focus();
                    f2.DataInputCompleted += Form2_DataInputCompleted;//完成將執行的函式
                    DialogResult result = f2.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        //Nothing...
                    }
                }
                db.Close();
            }
            fg = false;
        }
        private void Form2_DataInputCompleted(object sender, EventArgs e)
        {
            using(SqlCommand cmd=new SqlCommand("INSERT INTO Table_1 (電子郵件,回饋類型,具體內容,是否願意參與進一步討論或測試新功能,日期) VALUES (@email, @type, @content,@isok,@day)", db))
            {
                cmd.Parameters.AddWithValue("@email", Form2.email);
                cmd.Parameters.AddWithValue("@type", Form2.type);
                cmd.Parameters.AddWithValue("@content", Form2.content);
                cmd.Parameters.AddWithValue("@isok", Form2.isOK);
                cmd.Parameters.AddWithValue("@day", DateTime.Now.ToString("d"));
                Form2.email = Form2.type = Form2.content = "";
                Form2.isOK = false;
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
        #region 行駛列車_多執行緒
        private void timer2_Tick(object sender, EventArgs e)
        {
            foreach (var train in trains)
            {
                try
                {
                    if (train.cur_st.isStation())
                    {
                        if (((Station)train.cur_st).name == train.destination.name)
                        {
                            train.speed = 0;
                            train.stats = "已到達終點站";
                        }
                        else
                        {
                            train.stats = "等待出站";
                            if (++train.wait >= 10 && (train.next_bool ? ((Station)train.cur_st).track1 : ((Station)train.cur_st).track2) != null)
                            {
                                train.cur_st = train.next_bool ? ((Station)train.cur_st).track1 : ((Station)train.cur_st).track2;
                                train.wait = 0;
                                train.speed += 10;
                                train.change_image("penO");
                                train.stats = "正在行駛中";
                            }
                        }
                    }
                    else
                    {
                        if (train.length <= 1 && train.speed < 110)
                        {
                            train.speed += 10;
                        }
                        else
                        {
                            train.change_image("penB");
                        }
                        train.move();
                        double u = train.speed * 1000 / 3600;
                        double s = u * u / (2 * 10);
                        if ((train.next_bool ? ((Track)train.cur_st).station1.priority : ((Track)train.cur_st).station2.priority) < train.priority)
                        {
                            train.change_image("pen");
                            train.length = 0;
                            train.cur_st = train.next_bool ? (((Track)train.cur_st).station1).track1 : (((Track)train.cur_st).station2).track2;
                            train.change_image("penB");
                        }
                        else if (s >= (((Track)train.cur_st).length - train.length) * 1E3 && train.speed > 0)
                        {
                            train.change_image("penO");
                            train.speed = train.speed - 10 < 0 ? 0 : train.speed - 10;
                            train.stats = "正在進站中";
                        }
                        else if (train.length >= ((Track)train.cur_st).length)
                        {
                            train.change_image("pen");
                            train.length = 0;
                            train.cur_st = train.next_bool ? ((Track)train.cur_st).station1 : ((Track)train.cur_st).station2;

                        }
                        else if (train.speed > 150)
                        {
                            train.change_image("penR");
                            throw new SpeedException($"{train.name}的速度過快，請減速!");
                        }
                        else if (train.speed == 0)
                        {
                            train.stats= "正在停靠中";
                        }
                        else
                        {
                            train.stats = "正在行駛中";
                        }
                        foreach (var other_train in trains)
                        {
                            if (train != other_train)
                            {
                                if (train.cur_st == other_train.cur_st && other_train.length - train.length < 0.5)
                                {
                                    train.change_image("penR");
                                    throw new NearException($"{train.name}和{other_train.name}的距離過近，請減速!");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SystemSounds.Beep.Play();
                    train.stats = $"{ex.Message}";
                    comboBox1.SelectedIndex = comboBox1.FindStringExact(train.name);
                    train_set(sender, e);
                }
            }
            if(comboBox1.SelectedIndex != -1) train_set(sender, e);
            if (comboBox2.SelectedIndex != -1) station_change(sender, e);
        }
        #endregion
        #region 下拉選單的變更
        private void train_change(object sender, EventArgs e)
        {
            train_set(sender, e);
            label7.Text = $"列車已選擇\r\n時間 : {DateTime.Now.ToString("T")}";
        }
        public void train_set(object sender, EventArgs e)
        {

            textBox2.Text = $"{trains[comboBox1.SelectedIndex].speed}km/hr";
            comboBox4.Text = trains[comboBox1.SelectedIndex].destination.name;
            comboBox3.Text = trains[comboBox1.SelectedIndex].priority.ToString();
            label7.Text = $"{trains[comboBox1.SelectedIndex].stats}\r\n時間:{ DateTime.Now.ToString("T")}";
        }
        private void station_change(object sender, EventArgs e)
        {
            label11.Text = "";
            dgv.Rows.Clear();
            foreach (Train train in trains)
            {
                if (train.stats != "" && train.cur_st == stations[comboBox2.SelectedIndex]) label11.Text = $"{train.name}{train.stats}";
                double total_length = 0;
                int wait = 0;
                if (train.cur_st.isStation() && ((Station)train.cur_st).name == comboBox2.Text)
                {
                    dgv.Rows.Add(train.name, "等待發車", train.destination.name, train.stats);
                    continue;
                }
                TrafficNode cur = train.cur_st;
                while (cur != null)
                {
                    if (cur.isStation())
                    {
                        if (((Station)cur).name == comboBox2.Text)
                        {
                            dgv.Rows.Add(train.name, train.getTime(total_length, wait), train.destination.name, train.stats);
                            break;
                        }
                        else if (((Station)cur).name == train.destination.name)
                        {
                            break;
                        }
                        if (train.priority <= ((Station)cur).priority) wait += 10;
                        cur = train.next_bool ? ((Station)cur).track1 : ((Station)cur).track2;
                    }
                    else
                    {
                        total_length += ((Track)cur).length;
                        cur = train.next_bool ? ((Track)cur).station1 : ((Track)cur).station2;
                    }
                }
            }
        }

        private void prioritization_change(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex != -1)
            {
                if(trains[comboBox1.SelectedIndex].priority != comboBox3.SelectedIndex + 1)
                {
                    label7.Text = $"列車優先權已更改\r\n{trains[comboBox1.SelectedIndex].priority}->{comboBox3.SelectedIndex + 1}\r\n時間 : {DateTime.Now.ToString("T")}";
                }
                trains[comboBox1.SelectedIndex].priority = comboBox3.SelectedIndex + 1;                
            }
        }
        private void destination_change(object sender, EventArgs e)
        {
            if (trains[comboBox1.SelectedIndex].destination.name != comboBox4.Text)
            {
                if (!trains[comboBox1.SelectedIndex].cur_st.isStation())
                {
                    MessageBox.Show("列車正在行駛中，請先進站臨停");
                    return;
                }
                label7.Text = $"列車目的地已更改\r\n{trains[comboBox1.SelectedIndex].destination.name}->{comboBox4.Text}\r\n時間 : {DateTime.Now.ToString("T")}";
                trains[comboBox1.SelectedIndex].destination = stations[stations.FindIndex(x => x.name == comboBox4.Text)];
                trains[comboBox1.SelectedIndex].next_bool = TrainInfo.get_next((Station)trains[comboBox1.SelectedIndex].cur_st, trains[comboBox1.SelectedIndex].destination);
                if (comboBox2.SelectedIndex != -1) station_change(sender, e);
            }
        }
        private void incident_change(object sender, EventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                checks[i].Checked = is_check[comboBox5.SelectedIndex, i];
            }
        }
        #endregion
        #region 列車的操作功能
        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                if (trains[comboBox1.SelectedIndex].cur_st.isStation()) return;
                trains[comboBox1.SelectedIndex].speed += 10;
                train_change(sender, e);
                label7.Text = $"列車已加速\r\n時間 : {DateTime.Now.ToString("T")}";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex != -1)
            {
                if (trains[comboBox1.SelectedIndex].cur_st.isStation()) return;
                if (trains[comboBox1.SelectedIndex].speed > 10) trains[comboBox1.SelectedIndex].speed -= 10;
                else trains[comboBox1.SelectedIndex].speed = 0;
                train_set(sender, e);
                label7.Text = $"列車已減速\r\n時間 : {DateTime.Now.ToString("T")}";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                if (trains[comboBox1.SelectedIndex].cur_st.isStation()) return;
                label7.Text = $"列車正在停止\r\n時間 : {DateTime.Now.ToString("T")}";
                while (trains[comboBox1.SelectedIndex].speed != 0)
                {
                    trains[comboBox1.SelectedIndex].speed -= 10;
                    train_set(sender, e);
                    Thread.Sleep(1000);
                    Application.DoEvents();
                }
                label7.Text = $"列車已經停止\r\n時間 : {DateTime.Now.ToString("T")}";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1 && trains[comboBox1.SelectedIndex].speed == 0)
            {
                label7.Text = $"列車重新啟動\r\n時間 : {DateTime.Now.ToString("T")}";
                if (trains[comboBox1.SelectedIndex].cur_st.isStation()) trains[comboBox1.SelectedIndex].cur_st = trains[comboBox1.SelectedIndex].next_bool ? ((Station)trains[comboBox1.SelectedIndex].cur_st).track1 : ((Station)trains[comboBox1.SelectedIndex].cur_st).track2;
                trains[comboBox1.SelectedIndex].wait = 0;
                while (trains[comboBox1.SelectedIndex].speed != 110)
                {
                    trains[comboBox1.SelectedIndex].speed += 10;
                    train_change(sender, e);
                    Thread.Sleep(1000);
                    Application.DoEvents();
                }
                label7.Text = $"列車已正常行駛\r\n時間 : {DateTime.Now.ToString("T")}";
            }
        }
        #endregion
        #region 緊急處理
        private void button7_Click(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex != -1 && comboBox6.SelectedIndex != -1)
            {
                var typeBuilder = new StringBuilder();
                for(int i = 0; i < 6; i++)
                {
                    if (checks[i].Checked) typeBuilder.Append(checks[i].Text + "、");
                }
                MessageBox.Show($"事件：{comboBox5.Text}\r\n\r\n列車：{comboBox6.Text}\r\n\r\n已聯絡：{typeBuilder.ToString().TrimEnd('、')}\r\n\r\n" +
                    $"詳細內容：\r\n{(richTextBox1.Text == "" ? "None" : richTextBox1.Text)}", "緊急處理", MessageBoxButtons.OK);
            }
        }
        #endregion
    }
    #region 類別
    public interface TrafficNode
    {
        bool isStation();
    }
    public class Track:TrafficNode
    {
        // 交通節點的限速(km/hr)
        public int limitspeed { get; set; }
        // 交通節點的長度(km)
        public double length { get; set; }
        public Station station1 { get; set; }
        public Station station2 { get; set; }
        public PictureBox picture1 { get; set; }
        public PictureBox picture2 { get; set; }
        public Track(int ls, int len, Station st1,Station st2, PictureBox p1,PictureBox p2)
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
        public bool[] platform { get; set; }
        // 南下的軌道
        public Track track1 { get; set; }
        // 北上的軌道
        public Track track2 { get; set; }
        public Label label { get; set; }
        public void set_connect(Track t1,Track t2)
        {
            track1 = t1;
            track2 = t2;
        }
        public Station(string n, int count, int p, Label l)
        {
            name = n;
            platform = new bool[count * 2];
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
        public Train(string n, int p, Station st, Station end, bool b)
        {
            name = n;
            priority = p;
            start = st;
            cur_st = st;
            destination = end;
            next_bool = b;
            wait = 0;
            speed = 0;
            stats = $"停靠{((Station)cur_st).name}中";
        }
        public string getTime(double total_length,int count)
        {
            // 正常時速 110km/hr 行駛，停靠每個月台約10秒
            int sec = (int)((total_length - length) / 110 * 3600) + count;
            if (sec <= 0) return "即將進站中";
            return $"約{sec / 60}分{sec % 60}秒進站";
        }
        public void move() => length += (speed == 0 ? 0 : speed / 3600);
        public void change_image(string path)
        {
            if (cur_st.isStation()) return;
            if (!next_bool) ((Track)cur_st).picture1.Image = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "umtodm" + path + ".png");
            else ((Track)cur_st).picture2.Image = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "dmtoum" + path + ".png");
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
    #endregion
    #region 錯誤類別
    // 列車速度過快
    public class SpeedException : Exception
    {
        public SpeedException(string message) : base(message)
        {
        }
    }
    // 列車距離過近
    public class NearException : Exception
    {
        public NearException(string message) : base(message)
        {
        }
    }
    // 月台已滿
    public class FullException : Exception
    {
        public FullException(string message) : base(message)
        {
        }
    }
    // 列車事故
    public class AccidentException : Exception
    {
        public AccidentException(string message) : base(message)
        {
        }
    }
    #endregion
}
