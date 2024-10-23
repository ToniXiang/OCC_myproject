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
using System.Reflection;
namespace 簡易的行控中心
{
    public partial class Form1 : Form
    {
        #region 宣告
        private static List<Train> trains = new List<Train>();
        private static List<Station> stations = new List<Station>();
        private static SqlConnection sqlconnection = new SqlConnection("Data Source=DESKTOP-Q78F81O,1433;Initial Catalog=OCC_DB;Integrated Security=True");
        private static CheckBox[] checks;
        private static bool[,] is_check =
        {
            { true , true, true, true, true, true },
            { false, false, false, true, false, true },
            { false, true, false, true, false, true },
            { false, false, false, false, true, false },
            { false, false, false, false, false, true }
        };
        private static List<Label> labels1 = new List<Label>();
        private static List<string> tmp = new List<string>() { "高", "中", "低" };
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static double delta_t = 1.0;
        public static PictureBox[] pic1, pic2;
        public Form1()
        {
            InitializeComponent();
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            pic1 = new PictureBox[] { pictureBox8, pictureBox9, pictureBox10, pictureBox11, pictureBox12, pictureBox13 };
            pic2 = new PictureBox[] { pictureBox1, pictureBox2, pictureBox4, pictureBox5, pictureBox6, pictureBox7 };
            labels1.AddRange(new Label[] { label2, label3, label12, label13, label14, label15, label16 });
            DrawImg.DoImg(projectDirectory);
            this.Icon = Icon.FromHandle((new Bitmap(Path.Combine(projectDirectory, "programming.png"))).GetHicon());
            foreach (var control in this.Controls.OfType<Control>().Where(c => c is TextBox || c is Label || c is GroupBox))
            {
                control.MouseDown += Form1_MouseDown;
                control.MouseUp += Form1_MouseUp;
                control.MouseMove += Form1_MouseMove;
                if (control is GroupBox groupBox)
                {
                    foreach (var comboBox in groupBox.Controls.OfType<ComboBox>())
                    {
                        comboBox.DrawMode = DrawMode.OwnerDrawFixed;
                        comboBox.DrawItem += DrawItem;
                    }
                    foreach(var label in groupBox.Controls.OfType<Label>())
                    {
                        label.MouseDown += Form1_MouseDown;
                        label.MouseUp += Form1_MouseUp;
                        label.MouseMove += Form1_MouseMove;
                    }
                }
            }
            labels1.ForEach(x => { x.Click += Label_Click; x.MouseMove += Label_MouseMove; x.MouseLeave += Label_MouseLeave; });
            TrainInfo.info = new string[] { "台北", "新北", "桃園", "新竹", "苗栗", "台中", "彰化" };
            int[] limitSpeeds = { 135, 135, 130, 125, 140, 150 };
            int[] lengths = { 2, 4, 4, 2, 2, 1 };
            int[] platform = { 2, 1, 1, 1, 1, 1, 2 };
            int[] prio = { 0, 1, 1, 1, 2, 2, 0 };
            for (int i = 0; i < 7; i++)
            {
                stations.Add(new Station(TrainInfo.info[i], platform[i], prio[i], labels1[i]));
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
            dgv.Columns.Cast<DataGridViewColumn>().ToList().ForEach(x => x.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells);
            string[] tmp2 = { "列車事故", "突發健康事件", "恐怖襲擊", "能源故障", "乘客滯留" };
            comboBox5.Items.AddRange(tmp2);
            checks = new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 };
            comboBox6.Items.AddRange(trains.Select(x => x.name).ToArray());
            timer1.Start();
        }
        #endregion
        #region 自訂 Combobox
        private void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            ComboBox comboBox = sender as ComboBox;
            string text = comboBox.Items[e.Index].ToString();
            e.DrawBackground();
            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                // 計算文字的寬度和高度
                SizeF textSize = e.Graphics.MeasureString(text, e.Font);
                // 計算文字的起始位置，使其置中
                float textX = (e.Bounds.Width - textSize.Width) / 2;
                float textY = (e.Bounds.Height - textSize.Height) / 2;
                // 繪製文字
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X + textX, e.Bounds.Y + textY);
            }
            e.DrawFocusRectangle();
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
        #region 離開
        private async void pictureBox3_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = new DialogResult();
                if (sqlconnection.State == ConnectionState.Closed)
                {
                    await sqlconnection.OpenAsync();
                }
                using (Form2 f2 = new Form2())
                {
                    f2.Focus();
                    f2.DataInputCompleted += Form2_DataInputCompleted;
                    result = f2.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        //Nothing...
                    }
                    f2.Dispose();
                }
                sqlconnection.Close();
                result = MessageBox.Show("確定要離開嗎?", "離開", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            catch (TaskCanceledException)
            {
                //Nothing...
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
                $"© 2024 陳國翔\r\n\r\nhttps://chenguoxiang940.github.io/project.html\r\n\r\n功能介绍 - 版本 {Assembly.GetExecutingAssembly().GetName().Version.ToString()}", "簡介", MessageBoxButtons.OK);
            await FocusMethod();
        }
        #endregion
        #region 開啟/暫停
        private void button1_Click(object sender, EventArgs e)
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
                                train.change_image("O");
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
                            train.change_image("B");
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
                                    train.change_image("");
                                    train.length = 0;
                                    train.cur_st = train.next_bool ? (((Track)train.cur_st).station1).track1 : (((Track)train.cur_st).station2).track2;
                                    train.change_image("B");
                                }
                            }
                            else if (train.length >= ((Track)train.cur_st).length && train.speed <= 4)
                            {
                                fg = true;
                                train.change_image("");
                                train.length = 0;
                                train.speed = 0;
                                train.cur_st = train.next_bool ? ((Track)train.cur_st).station1 : ((Track)train.cur_st).station2;
                            }
                            else
                            {
                                fg = true;                               
                                train.change_image("O");
                                end_u = end_u + (-end_a) * delta_t;
                                train.speed = end_u * 3.6 < 4.0 ? 4.0 : end_u * 3.6;
                                train.stats = "正在進站中";
                            }
                        }
                        else if (train.speed > ((Track)train.cur_st).limitspeed)
                        {
                            train.change_image("R");
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
                    train.change_image("B");
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
                train.change_image("O");
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
                    train.change_image("B");
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
                MessageBox.Show($"事件：{comboBox5.Text}\r\n\r\n列車：{comboBox6.Text}\r\n\r\n已聯絡：" +
                    $"{string.Join("、", checks.Where(x => x.Checked).Select(x => x.Text))}\r\n\r\n" +
                    $"詳細內容：\r\n{(richTextBox1.Text == "" ? "None" : richTextBox1.Text)}", "緊急處理", MessageBoxButtons.OK);
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
}