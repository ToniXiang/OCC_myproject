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
        public static string projectDirectory;
        private static List<Train> trains = new List<Train>();
        private static List<Station> stations = new List<Station>();
        private static SqlConnection sqlconnection = new SqlConnection("Data Source=DESKTOP-Q78F81O,1433;Initial Catalog=OCC_DB;Integrated Security=True");
        private static string[] stationNames = { "台北", "新北", "桃園", "新竹", "苗栗", "台中", "彰化" };
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
        private static List<Label> labels1 = new List<Label>();
        private static List<string> tmp = new List<string>() { "高", "中", "低" };
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private static double delta_t = 1.0;
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.FullName;
            this.StartPosition = FormStartPosition.CenterScreen;
            Bitmap b = new Bitmap(projectDirectory + "\\programming.png");
            this.Icon = Icon.FromHandle(b.GetHicon());
            timer1.Start();
            TrainInfo.info = stationNames;
            int[] limitSpeeds = { 135, 135, 130, 125, 140, 150 };
            int[] lengths = { 2, 4, 4, 2, 2, 1 };//13, 20, 15, 20, 17, 30 
            int[] platform = { 2, 1, 1, 1, 1, 1, 2 };
            int[] prio = { 0, 1, 1, 1, 2, 2, 0 };
            PictureBox[] pic1 = { pictureBox8, pictureBox9, pictureBox10, pictureBox11, pictureBox12, pictureBox13 };
            PictureBox[] pic2 = { pictureBox1, pictureBox2, pictureBox4, pictureBox5, pictureBox6, pictureBox7 };
            labels1.AddRange(new Label[] { label2, label3, label12, label13, label14, label15, label16 });
            for (int i = 0; i < 7; i++)
            {
                stations.Add(new Station(stationNames[i], platform[i], prio[i], labels1[i]));
            }
            List<Track> tracks = new List<Track>();
            tracks.Add(null);
            for (int i = 0; i < 6; i++)
            {
                tracks.Add(new Track(limitSpeeds[i], lengths[i], stations[i], stations[i + 1], pic1[i], pic2[i]));
            }
            tracks.Add(null);
            for (int i = 0; i < 7; i++)
            {
                stations[i].set_connect(tracks[i], tracks[i + 1]);
            }
            trains.Add(new Train("3104", 1, stations[0], stations[6]));
            trains.Add(new Train("3211", 2, stations[6], stations[0]));
            trains.Add(new Train("3288", 2, stations[6], stations[5]));
            trains.Add(new Train("3518", 2, stations[3], stations[0]));
            trains.Add(new Train("3704", 2, stations[5], stations[3]));
            comboBox1.Items.AddRange(trains.Select(x => x.name).ToArray());
            comboBox2.Items.AddRange(stations.Select(x => x.name).ToArray());
            comboBox4.Items.AddRange(stations.Select(x => x.name).ToArray());
            tmp.ForEach(x => comboBox3.Items.Add(x));
            tmp.ForEach(x => comboBox7.Items.Add(x));
            List<string> tmp1 = new List<string>() { "車次", "時間", "開往", "狀態" };
            tmp1.ForEach(x => dgv.Columns.Add(x, x));
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.Columns.Cast<DataGridViewColumn>().ToList().ForEach(x => x.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells);
            dgv.DefaultCellStyle.Font = new Font("微軟正黑體", 12);
            label17.Text = "暫停模擬";
            label17.ForeColor = Color.Red;
            button3.Enabled = button4.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = false;
            label11.Text = label7.Text = "";
            string[] tmp2 = { "列車事故", "突發健康事件", "恐怖襲擊", "能源故障", "乘客滯留" };
            comboBox5.Items.AddRange(tmp2);
            checks = new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 };
            comboBox6.Items.AddRange(trains.Select(x => x.name).ToArray());
            foreach (Control control in this.Controls)
            {
                if (control is TextBox || control is Label || control is GroupBox)
                {
                    control.MouseDown += Form1_MouseDown;
                    control.MouseUp += Form1_MouseUp;
                    control.MouseMove += Form1_MouseMove;
                }
            }
            labels1.ForEach(x => { x.Click += Label_Click; x.MouseMove += Label_MouseMove; x.MouseLeave += Label_MouseLeave; });
            MessageBox.Show("簡易的行控中心(Simple operational control center)\r\n\r\n" +
                "1. 車站狀態\r\n請選擇車站名稱，可以調整優先序\r\n時刻表包含將進站的列車名、預估抵達時間、起始站、終點站\r\n\r\n" +
                "2. 列車狀態及操作\r\n請選擇列車名稱，可以調整優先序、更改終點站，但先臨停\r\n列車出站加速度為 1.0 m/s²，維持且不超過 110km/h\r\n列車進站加速度為 - 0.8 m/s²\r\n" +
                "調整時速請按「增速」和「減速」分別和進出站加速度相同，增減速不超過 ±10km/h\r\n" +
                "「煞車」煞車加速度為 - 2.5 m/s² 用於急煞\r\n「啟動」不等候直接出站或中途停止要開始行駛\r\n" +
                "「發車」等候發車\r\n「停靠站」在此站不發車，需要先停靠在車站\r\n車站優先序如果低於列車則不會臨停\r\n\r\n" +
                "[1] 同方向列車不會在車站同時發車，考慮安全問題\r\n"+
                "[2] 如果要進站的月台數不足則不會發車，需等待已在進站月台的列車離站\r\n[3] 一個月台有各一個北上和南下\r\n\r\n" +
                "3. 路線地圖\r\n點選車站名可直接更改列車站狀態的車站名\r\n黑色箭頭：無列車在此行駛\r\n橘色箭頭：有列車要進站或離站\r\n藍色箭頭：有列車站在此行駛\r\n紅色箭頭：有安全問題\r\n\r\n" +
                "4. 事件處理\r\n選擇事件及列車，勾選聯絡單位，詳細內容可為空\r\n模擬事件回報功能\r\n\r\n" +
                $"© 2024 陳國翔\r\n\r\nhttps://github.com/ChenGuoXiang940/NTCUST_OCC_myproject\r\n\r\n功能介绍 - 版本 {version.ToString()}", "功能簡介", MessageBoxButtons.OK);
        }
        #endregion
        #region 離開
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DialogResult result= MessageBox.Show("確定要離開嗎?", "離開", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        #endregion
        #region 時間_多執行緒
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = $"日期 : {DateTime.Now.ToString("yyyy年MM月dd號")}\r\n時間 : {DateTime.Now.ToString("T")}\r\n星期 : {DateTime.Now.ToString("dddd")}";
        }
        #endregion
        #region 簡介
        private async void textbox1_Click(object sender, EventArgs e)
        {
            // 版本信息為 主版本.次版本.生成號.修訂號
            MessageBox.Show("簡易的行控中心(Simple operational control center)\r\n\r\n1. 使用者可以自行操作列車" +
                "\r\n2. 時刻掌握當前時間\r\n3. 使用模擬計算來預估列車的當前狀態\r\n5. 路線地圖可視化展示\r\n6. 緊急事件處理\r\n" +
                "7. 用戶回饋連結資料庫\r\n8. 更多功能日後開發(維護和檢修計劃、能源管理連接資料庫)\r\n\r\n" +
                $"© 2024 陳國翔\r\n\r\nhttps://github.com/ChenGuoXiang940/NTCUST_OCC_myproject\r\n\r\n功能介绍 - 版本 {version.ToString()}", "簡介", MessageBoxButtons.OK);
            await FocusMethod();
        }
        #endregion
        #region 開啟/暫停
        private async void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "開始")
            {
                button1.Text = "暫停";
                timer2.Start();
                button7.Enabled = true;
                label17.Text = "正在模擬";
                label17.ForeColor = Color.LightGreen;
            }
            else
            {
                button1.Text = "開始";
                timer2.Stop();
                label17.Text = "暫停模擬";
                label17.ForeColor = Color.Red;
                button3.Enabled = button4.Enabled = button5.Enabled = button6.Enabled = button7.Enabled = false;
                try
                {
                    if (fg)
                    {
                        fg = false;
                        if (sqlconnection.State == ConnectionState.Closed)
                        {
                            await sqlconnection.OpenAsync();
                        }
                        using (Form2 f2 = new Form2())
                        {
                            f2.Focus();
                            f2.DataInputCompleted += Form2_DataInputCompleted;
                            DialogResult result = f2.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                //Nothing...
                            }
                            f2.Dispose();
                        }
                        sqlconnection.Close();
                    }
                }
                catch (TaskCanceledException)
                {
                    //Nothing...
                }
            }
        }
        private void Form2_DataInputCompleted(object sender, EventArgs e)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO Table_1 (電子郵件,回饋類型,具體內容,是否願意參與進一步討論或測試新功能,日期) VALUES (@email, @type, @content,@isok,@day)", sqlconnection))
            {
                cmd.Parameters.AddWithValue("@email", Form2.email);
                cmd.Parameters.AddWithValue("@type", Form2.type);
                cmd.Parameters.AddWithValue("@content", Form2.content);
                cmd.Parameters.AddWithValue("@isok", Form2.isOK);
                cmd.Parameters.AddWithValue("@day", DateTime.Now.ToString("d"));
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
                            train.stats = "已到達終點站";
                            if (comboBox1.SelectedIndex == comboBox1.FindStringExact(train.name))
                            {
                                button3.Enabled = button4.Enabled = button5.Enabled = button6.Enabled = false;
                            }
                        }
                        else
                        {
                            train.stats = "等待出站";
                            Track next_track = train.next_bool ? ((Station)train.cur_st).track1 : ((Station)train.cur_st).track2;
                            Station next_station = train.next_bool ? next_track.station1 : next_track.station2;
                            int rd = 0; // 紀錄列車開往的站有多少列車數量
                            bool check = false; // 紀錄同方向列車是否在車站同時發車
                            foreach(Train other_train in trains)
                            {
                                if (train == other_train) continue;
                                if (train.next_bool == other_train.next_bool)
                                {
                                    if (next_track == other_train.cur_st || next_station == other_train.cur_st)
                                    {
                                        rd++;
                                    }
                                    else if (train.cur_st == other_train.cur_st && trains.IndexOf(train) > trains.IndexOf(other_train))
                                    {
                                        check = true;
                                        break;
                                    }
                                }
                            }
                            if (rd > next_station.platform || check)
                            {
                                break;
                            }
                            if (comboBox1.SelectedIndex == comboBox1.FindStringExact(train.name))
                            {
                                button3.Enabled = button4.Enabled = false;
                                button5.Enabled = button6.Enabled = true;
                                button5.Text = "啟動";
                            }
                            if (train.wait >= 0 && ++train.wait >= 10 && (train.next_bool ? ((Station)train.cur_st).track1 : ((Station)train.cur_st).track2) != null)
                            {
                                train.cur_st = train.next_bool ? ((Station)train.cur_st).track1 : ((Station)train.cur_st).track2;
                                train.wait = -1;
                                train.change_image("penO");
                                train.stats = "正在行駛中";
                            }
                        }
                    }
                    else
                    {
                        bool fg = false;
                        if (train.wait == -1 && train.speed < 110)
                        {
                            double start_u = train.speed / 3.6;// 速度 m/s
                            double start_a = 1.0; // 假設列車啟動加速度為 1.0 m/s²
                            start_u = start_u + start_a * delta_t;
                            train.speed = start_u * 3.6 > 110.0 ? 110.0 : start_u * 3.6;
                            if (train.speed == 110) train.wait = 0;
                            fg = true;                       
                        }
                        else
                        {
                            train.change_image("penB");
                        }
                        train.move();
                        double end_u = train.speed / 3.6;// 速度 m/s
                        double end_a = 0.8; // 假設列車煞車加速度為 - 0.8 m/s²
                        double s = end_u * end_u / (2 * end_a);// 煞車距離
                        if (s >= (((Track)train.cur_st).length - train.length) * 1E3)
                        {
                            if ((train.next_bool ? ((Track)train.cur_st).station1.priority : ((Track)train.cur_st).station2.priority) > train.priority)
                            {
                                if (train.length >= ((Track)train.cur_st).length)
                                {
                                    train.change_image("pen");
                                    train.length = 0;
                                    train.cur_st = train.next_bool ? (((Track)train.cur_st).station1).track1 : (((Track)train.cur_st).station2).track2;
                                    train.change_image("penB");
                                }
                            }
                            else if (train.length >= ((Track)train.cur_st).length && train.speed <= 4)
                            {
                                fg = true;
                                train.change_image("pen");
                                train.length = 0;
                                train.speed = 0;
                                train.cur_st = train.next_bool ? ((Track)train.cur_st).station1 : ((Track)train.cur_st).station2;
                            }
                            else
                            {
                                fg = true;                               
                                train.change_image("penO");
                                end_u = end_u + (-end_a) * delta_t;
                                train.speed = end_u * 3.6 < 4.0 ? 4.0 : end_u * 3.6;
                                train.stats = "正在進站中";
                            }
                        }
                        else if (train.speed > ((Track)train.cur_st).limitspeed)
                        {
                            train.change_image("penR");
                            throw new SpeedException($"速度過快，請減速!");
                        }
                        if(comboBox1.SelectedIndex == comboBox1.FindStringExact(train.name))
                        {
                            if (fg)
                            {
                                button3.Enabled = button4.Enabled = button6.Enabled = false;
                                button5.Enabled = true;
                                button5.Text = "煞車";
                            }
                            else
                            {
                                button3.Enabled = button4.Enabled = true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    SystemSounds.Beep.Play();
                }
            }
            if(comboBox1.SelectedIndex != -1) train_set();
            if (comboBox2.SelectedIndex != -1) dgv_set();
        }
        #endregion
        #region 下拉選單的變更
        private async void train_change(object sender, EventArgs e)
        {
            label7.Text = "";
            button5.Text = trains[comboBox1.SelectedIndex].speed != 0 ? "煞車" : "啟動";
            button6.Text = trains[comboBox1.SelectedIndex].cur_st.isStation() ? "停靠站" : "發車";
            comboBox4.SelectedIndex = comboBox4.FindStringExact(trains[comboBox1.SelectedIndex].destination.name);
            comboBox3.SelectedIndex = trains[comboBox1.SelectedIndex].priority;
            if (trains[comboBox1.SelectedIndex].wait == -2)
            {
                button6.Text = "發車";
            }
            else if (trains[comboBox1.SelectedIndex].wait == -1)
            {
                button6.Enabled = false;
            }
            else
            {
                button6.Text = "停靠站";
            }
            train_set();
            await FocusMethod();
        }
        public void train_set()
        {
            textBox2.Text = $"{Math.Round(trains[comboBox1.SelectedIndex].speed, 2)} km/hr";
        }
        private void dgv_set()
        {
            dgv.Rows.Clear();
            foreach (Train train in trains)
            {
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
                            if(((Station)cur).priority <= train.priority)
                            {
                                dgv.Rows.Add(train.name, train.getTime(total_length, wait), train.destination.name, train.stats);
                            }
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
        private async void station_change(object sender, EventArgs e)
        {
            label11.Text = "";
            textBox3.Text = $"{stations[comboBox2.SelectedIndex].platform}";
            comboBox7.SelectedIndex = stations[comboBox2.SelectedIndex].priority;
            labels1.ForEach(x => x.ForeColor = Color.AliceBlue);
            labels1[comboBox2.SelectedIndex].ForeColor = Color.Aqua;
            dgv_set();
            await FocusMethod();
        }
        private async void station_prioritization_change(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != -1 && comboBox7.SelectedIndex != -1)
            {
                if (stations[comboBox2.SelectedIndex].priority != comboBox7.SelectedIndex)
                {
                    label11.Text = $"車站優先權已更改 {tmp[stations[comboBox2.SelectedIndex].priority]}➡️{tmp[comboBox7.SelectedIndex]}\r\n時間 : {DateTime.Now.ToString("T")}";
                    stations[comboBox2.SelectedIndex].priority = comboBox7.SelectedIndex;
                    dgv_set();
                }
            }
            else
            {
                comboBox7.SelectedIndex = -1;
            }
            await FocusMethod();
        }
        private async void prioritization_change(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1 && comboBox3.SelectedIndex != -1)
            {
                if(trains[comboBox1.SelectedIndex].priority != comboBox3.SelectedIndex)
                {
                    label7.Text = $"列車優先權已更改\r\n{tmp[trains[comboBox1.SelectedIndex].priority]}➡️{tmp[comboBox3.SelectedIndex]}\r\n時間 : {DateTime.Now.ToString("T")}";
                    trains[comboBox1.SelectedIndex].priority = comboBox3.SelectedIndex;
                    if (comboBox2.SelectedIndex != -1)
                    {
                        dgv_set();
                    }
                }
            }
            else
            {
                comboBox3.SelectedIndex = -1;
            }
            await FocusMethod();
        }
        private async void destination_change(object sender, EventArgs e)
        {
            await FocusMethod();
            if (comboBox1.SelectedIndex == -1)
            {
                comboBox4.SelectedIndex = -1;
                return;
            }
            if (trains[comboBox1.SelectedIndex].destination.name != comboBox4.Text)
            {
                if (!trains[comboBox1.SelectedIndex].cur_st.isStation())
                {
                    MessageBox.Show("列車正在行駛中，請先進站臨停","提示");
                    comboBox4.Text = trains[comboBox1.SelectedIndex].destination.name;
                    return;
                }
                if (trains[comboBox1.SelectedIndex].cur_st.isStation() && ((Station)trains[comboBox1.SelectedIndex].cur_st).name == comboBox4.Text)
                {
                    trains[comboBox1.SelectedIndex].stats = $"已到達終點站";
                }
                label7.Text = $"列車目的地已更改\r\n{trains[comboBox1.SelectedIndex].destination.name}➡️{comboBox4.Text}\r\n時間 : {DateTime.Now.ToString("T")}";
                trains[comboBox1.SelectedIndex].destination = stations[stations.FindIndex(x => x.name == comboBox4.Text)];
                trains[comboBox1.SelectedIndex].next_bool = TrainInfo.get_next((Station)trains[comboBox1.SelectedIndex].cur_st, trains[comboBox1.SelectedIndex].destination);
                if (comboBox2.SelectedIndex != -1) dgv_set();
            }
        }
        private async void incident_change(object sender, EventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                checks[i].Checked = is_check[comboBox5.SelectedIndex, i];
            }
            await FocusMethod();
        }
        private async void place_change(object sender, EventArgs e)
        {
            await FocusMethod();
        }
        #endregion
        #region 列車的操作功能
        private async void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                if (trains[comboBox1.SelectedIndex].cur_st.isStation()) return;
                Train train = trains[comboBox1.SelectedIndex];
                train.wait = 0;
                button3.Enabled = false;
                await faster(sender, e, train);
                train.wait = -1;
                label7.Text = $"列車已加速\r\n時間 : {DateTime.Now.ToString("T")}";
                button3.Enabled = true;
            }
        }
        private async Task faster(object sender, EventArgs e,Train train)
        {

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            try
            {
                double rd = 0;
                double a = 1.0; // 假設列車加速時加速度為 1.0 m/s²
                do
                {
                    if (token.IsCancellationRequested)
                    {
                        cancellationTokenSource = null;
                        return;
                    }
                    double u = train.speed / 3.6;
                    u = u + a * delta_t;
                    rd = rd + a * delta_t;
                    train.speed = u * 3.6;
                    train_change(sender, e);
                    await Task.Delay(1000);
                } while (rd * 3.6 < 10.0);
            }
            catch (TaskCanceledException)
            {
                //Nothing...
            }
            finally
            {
               cancellationTokenSource = null;
            }
        }
        private async void button4_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex != -1)
            {
                if (trains[comboBox1.SelectedIndex].cur_st.isStation()) return;
                Train train = trains[comboBox1.SelectedIndex];
                train.wait = 0;
                button4.Enabled = false;
                await slower(sender, e, train);
                train_set();
                label7.Text = $"列車已減速\r\n時間 : {DateTime.Now.ToString("T")}";
                button4.Enabled = true;
            }
        }
        private async Task slower(object sender,EventArgs e,Train train)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            try
            {
                double rd = 0;
                double a = -0.8; // 假設列車減速時加速度為 - 0.8 m/s²
                do
                {
                    if (token.IsCancellationRequested)
                    {
                        cancellationTokenSource = null;
                        return;
                    }
                    double u = train.speed / 3.6;
                    u = u + a * delta_t;
                    rd = rd + (-a) * delta_t;
                    train.speed = u * 3.6;
                    train_change(sender, e);
                    await Task.Delay(1000);
                } while (rd * 3.6 < 10.0);
            }
            catch (TaskCanceledException)
            {
                //Nothing...
            }
            finally
            {
                cancellationTokenSource = null;
            }
        }
        private async void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                button3.Enabled = button4.Enabled = false;
                if (button5.Text == "煞車")
                {
                    button5.Text = "啟動";
                    trains[comboBox1.SelectedIndex].wait = -3;
                    await braking(sender,e);
                    label7.Text = $"列車已停止\r\n時間 : {DateTime.Now.ToString("T")}";
                }
                else
                {
                    button5.Text = "煞車";
                    await active(sender, e);
                    label7.Text = $"列車已啟動\r\n時間 : {DateTime.Now.ToString("T")}";
                }
                button3.Enabled = button4.Enabled = true;
            }

        }
        private async Task braking(object sender, EventArgs e)
        {
            Train train = trains[comboBox1.SelectedIndex];
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            try
            {
                double rd = train.speed;
                double a = -2.5; // 假設列車急煞時加速度為 - 2.5 m/s²
                do
                {
                    if (token.IsCancellationRequested)
                    {
                        cancellationTokenSource = null;
                        return;
                    }
                    double u = train.speed / 3.6;
                    u = u + a * delta_t;
                    train.speed = u < 0 ? 0 : u * 3.6;
                    train_change(sender, e);
                    train.change_image("penB");
                    train_set();
                    await Task.Delay(1000);
                } while (train.speed > 0);
            }
            catch (TaskCanceledException)
            {
                //Nothing...
            }
            finally
            {
                cancellationTokenSource = null;
            }
        }
        private async Task active(object sender,EventArgs e)
        {
            Train train = trains[comboBox1.SelectedIndex];
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
            if ((train.wait >= 0 || train.wait == -2) && (train.next_bool ? ((Station)train.cur_st).track1 : ((Station)train.cur_st).track2) != null)
            {
                train.cur_st = train.next_bool ? ((Station)train.cur_st).track1 : ((Station)train.cur_st).track2;
                train.change_image("penO");
                train.stats = "正在行駛中";
            }
            train.wait = 0;
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            try
            {
                double rd = train.speed;
                double a = 1.0; // 假設列車啟動時加速度為 1.0 m/s²
                do
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                        {
                            cancellationTokenSource = null;
                            return;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        //Nothing...
                    }
                    double u = train.speed / 3.6;
                    u = u + a * delta_t;
                    trains[comboBox1.SelectedIndex].speed = u * 3.6 > 110.0 ? 110.0 : u * 3.6;
                    train_change(sender, e);
                    train.change_image("penB");
                    train_set();
                    await Task.Delay(1000);
                } while (train.speed != 110);
                train.wait = -1;
            }
            catch (TaskCanceledException)
            {
                //Nothing...
            }
            finally
            {
                cancellationTokenSource = null;
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (trains[comboBox1.SelectedIndex].wait != -2)
            {
                trains[comboBox1.SelectedIndex].wait = -2;
                button6.Text = "發車";
                label7.Text= $"列車已臨停\r\n時間 : {DateTime.Now.ToString("T")}";
            }
            else
            {
                trains[comboBox1.SelectedIndex].wait = 0;
                button6.Text = "停靠站";
                button5.Text = "煞車";
                label7.Text = $"列車已發車\r\n時間 : {DateTime.Now.ToString("T")}";
            }
        }
        #endregion
        #region 事件處理
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
        #region 拖曳視窗
        private static bool isDragging;
        private static Point offset;
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                offset = e.Location;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = this.PointToScreen(e.Location);
                this.Location = new Point(newLocation.X - offset.X, newLocation.Y - offset.Y);
            }
        }
        #endregion
        #region 路線地圖事件
        private void Label_Click(object sender,EventArgs args)
        {
            Label clickedLabel = sender as Label;
            if (clickedLabel != null)
            {
                comboBox2.SelectedIndex = labels1.IndexOf(clickedLabel);
            }
        }
        private void Label_MouseMove(object sender,EventArgs args)
        {
            Label label = sender as Label;
            if (label != null)
            {
                label.Font= new Font("微软雅黑", 12, FontStyle.Bold);
            }
        }
        private void Label_MouseLeave(object sender,EventArgs args)
        {
            Label leftLabel = sender as Label;
            if (leftLabel != null)
            {
                leftLabel.Font = new Font("微软雅黑", 12);
            }
        }
        #endregion
        #region ComboBox的焦點變更
        private async Task FocusMethod()
        {
            await Task.Delay(200);
            label1.Focus();
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
        public int platform { get; set; }
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
            if (cur_st == null || cur_st.isStation()) return;
            if (!next_bool) ((Track)cur_st).picture1.Image = new Bitmap(Form1.projectDirectory + "\\umtodm" + path + ".png");
            else ((Track)cur_st).picture2.Image = new Bitmap(Form1.projectDirectory + "\\dmtoum" + path + ".png");
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
    #endregion
}