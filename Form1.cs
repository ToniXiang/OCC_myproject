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
using static 簡易的行控中心.Form2;
using 簡易的行控中心.TrafficComponents;
namespace 簡易的行控中心
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public Form1()
        {
            InitializeComponent();
            Traffic.projectDirectory= Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            PictureBox[] pic1 = new PictureBox[] { pictureBox8, pictureBox9, pictureBox10, pictureBox11, pictureBox12, pictureBox13 };
            PictureBox[] pic2 = new PictureBox[] { pictureBox1, pictureBox2, pictureBox4, pictureBox5, pictureBox6, pictureBox7 };
            checks = new List<CheckBox> { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 };
            List<Label> labels = new List<Label> { label2, label3, label12, label13, label14, label15, label16 };
            DrawReadImg.DoImg(Traffic.projectDirectory, ref pic1, ref pic2);
            Traffic.info = new string[] { "台北", "新北", "桃園", "新竹", "苗栗", "台中", "彰化" };
            int[] limitSpeeds = { 135, 135, 130, 125, 140, 150 };
            int[] lengths = { 2, 4, 4, 2, 2, 1 };
            int[] platform = { 4, 2, 2, 2, 2, 2, 4 };
            int[] prio = { 2, 2, 1, 0, 1, 1, 2 };
            for(int i = 0; i < 6; i++)
            {
                Traffic.traffics.Add(new Station(Traffic.info[i], platform[i], prio[i], labels[i]));
                Traffic.traffics.Add(new Track(limitSpeeds[i], lengths[i], pic1[i], pic2[i]));
            }
            Traffic.traffics.Add(new Station(Traffic.info.Last(), platform.Last(), prio.Last(), labels.Last()));
            Traffic.trains.Add(new Train("3104", 0, "台北", "彰化"));
            Traffic.trains.Add(new Train("3211", 0, "新北", "桃園"));
            Traffic.trains.Add(new Train("3288", 0, "台中", "彰化"));
            Traffic.trains.Add(new Train("3518", 0, "彰化", "新竹"));
            Traffic.trains.Add(new Train("3704", 0, "桃園", "台北"));
            Traffic.trains.ForEach(x => { comboBox1.Items.Add(x.name); comboBox6.Items.Add(x.name); });
            dgv.Columns.Cast<DataGridViewColumn>().ToList().ForEach(x => x.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells);
            this.Icon = Icon.FromHandle((new Bitmap(Path.Combine(Traffic.projectDirectory, "programming.png"))).GetHicon());
            InitializeDrag();
            labels.ForEach(x => { x.Click += Label_Click; x.MouseMove += Label_MouseMove; x.MouseLeave += Label_MouseLeave; });
            timer1.Start();
        }
        #region 自訂 Combobox
        private void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            string text = (sender as ComboBox).Items[e.Index].ToString();
            e.DrawBackground();
            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                SizeF textSize = e.Graphics.MeasureString(text, e.Font);
                float textX = (e.Bounds.Width - textSize.Width) / 2;
                float textY = (e.Bounds.Height - textSize.Height) / 2;
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X + textX, e.Bounds.Y + textY);
            }
            e.DrawFocusRectangle();
        }
        #endregion
        #region 可拖曳 Form1 視窗
        private static bool isDragging;
        private static Point offset;
        private void InitializeDrag()
        {
            Action<Control> attachMouseEvents = control =>
            {
                control.MouseDown += Form1_MouseDown;
                control.MouseUp += Form1_MouseUp;
                control.MouseMove += Form1_MouseMove;
            };
            foreach (var control in this.Controls.OfType<Control>().Where(c => c is TextBox || c is Label || c is GroupBox))
            {
                attachMouseEvents(control);
                if (control is GroupBox groupBox)
                {
                    groupBox.Controls.OfType<ComboBox>().ToList().ForEach(x => { x.DrawMode = DrawMode.OwnerDrawFixed; x.DrawItem += DrawItem; });
                    groupBox.Controls.OfType<Label>().ToList().ForEach(x => attachMouseEvents(x));
                }
            }
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            isDragging = true;
            offset = e.Location;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            isDragging = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;
            Point newLocation = this.PointToScreen(e.Location);
            this.Location = new Point(newLocation.X - offset.X, newLocation.Y - offset.Y);
        }
        #endregion
        #region 用戶回饋傳給資料庫 & 離開
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DialogResult result = new DialogResult();
            using (Form2 f2 = new Form2())
            {
                f2.Focus();
                f2.DataInputCompleted += Form2_DataInputCompleted;
                result = f2.ShowDialog();
                f2.Dispose();
            }
            result = MessageBox.Show("確定要離開嗎?", "離開", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes) Application.Exit();
        }
        private async void Form2_DataInputCompleted(object sender, DICEventArgs e)
        {
            try
            {
                using(SqlConnection sql = new SqlConnection("Data Source=DESKTOP-Q78F81O,1433;Initial Catalog=OCC_DB;Integrated Security=True"))
                {
                    if (sql.State == ConnectionState.Closed) await sql.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Table_1 (電子郵件,回饋類型,具體內容,是否願意參與進一步討論或測試新功能,日期) VALUES (@email, @type, @content,@isok,@day)", sql))
                    {
                        //欄位名 資料類型
                        cmd.Parameters.AddWithValue("@email", e.email);//電子郵件 nvarchar(30)
                        cmd.Parameters.AddWithValue("@type", e.type);//回饋類型 nvarchar(20)
                        cmd.Parameters.AddWithValue("@content", e.content);//具體內容 nchar(50)
                        cmd.Parameters.AddWithValue("@isok", e.isOK);//是否願意參與進一步討論或測試新功能 bit
                        cmd.Parameters.AddWithValue("@day", DateTime.Now.ToString("d"));//日期 nchar(10)
                        cmd.ExecuteNonQuery();
                    }
                    sql.Close();
                }               
            }
            catch (TaskCanceledException)
            {
                // 請使用自己的資料庫連接資訊，確保應用程式能夠正常連接至資料庫並運行
                // 請確認資料庫的連接字串是否正確，包括伺服器名稱、連接埠、資料庫名稱，以及是否使用正確的驗證方式。
                //Nothing...
            }
        }
        /*
         * 補充註解：
         *  1.顯示回饋表單（Form2），用於讓使用者輸入資料。表單顯示於最上層。
         *  2.當使用者輸入完成後，透過 DataInputCompleted 事件回傳資料。
         *  3.在使用者輸入完成並關閉表單後，與資料庫的連線（異步打開與關閉）並將有完整填寫的回饋資料存入資料庫並關
         *  4.系統彈出確認對話框，詢問使用者是否要離開應用程式。
         *  
         *  using 語句可以確保 (...) 在使用完畢後正確地釋放資源
         */
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
                button3.Enabled = button4.Enabled = button7.Enabled = false;
            }
        }
        #endregion
        #region 行駛列車_多執行緒
        private void timer2_Tick(object sender, EventArgs e)
        {
            foreach (var train in Traffic.trains)
            {
                if ((train.cur + 2) % 2 == 0)
                {
                    if (train.cur == train.destination)
                    {
                        train.stats = "已到達終點站";
                        if (comboBox1.SelectedIndex == train.cur / 2)
                        {
                            button3.Enabled = button4.Enabled = false;
                        }
                    }
                    else
                    {
                        train.stats = "等待出站";
                        bool check = false;
                        int rd = 0;
                        foreach(Train other_train in Traffic.trains)
                        {
                            if (train == other_train) continue;
                            //紀錄同方向的 軌道和下一站是否有幾個列車
                            if (train.next == other_train.next)
                            {
                                if(train.next + train.cur == other_train.cur)
                                {
                                    check = true;
                                    break;
                                }
                                else if (train.next * 2 + train.cur == other_train.cur)
                                {
                                    rd++;
                                }
                            }
                        }
                        if (train.wait == -2 || check || rd >= ((Station)Traffic.traffics[train.cur + train.next * 2]).platform)
                        {
                            continue;
                        }
                        if (comboBox1.SelectedIndex == train.cur / 2)
                        {
                            button3.Enabled = false;
                            button4.Enabled = true;
                        }
                        if (train.cur + train.next >= 0 && train.cur + train.next < Traffic.traffics.Count && ++train.wait >= 10)
                        {
                            train.cur += train.next;
                            train.wait = 0;
                            ((Track)Traffic.traffics[train.cur]).change_image("O", train.next);
                            train.stats = "正在行駛中";
                            if(comboBox1.SelectedIndex == train.cur / 2)
                            {
                                button3.Enabled = true;
                                button4.Enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    Track track = (Track)Traffic.traffics[train.cur];
                    train.move();
                    double end_u = train.speed / 3.6;// 速度 m/s
                    double end_a = 2.0; // 假設列車煞車加速度為 - 2.0 m/s²
                    double s = end_u * end_u / (2 * end_a);// 煞車距離
                    if (s >= (track.length - train.length) * 1E3)
                    {
                        if (((Station)Traffic.traffics[train.cur + train.next]).priority < train.priority)
                        {
                            if (train.length >= track.length)
                            {
                                track.change_image("", train.next);
                                train.length = 0;
                                train.wait = 0;
                                ((Track)Traffic.traffics[train.cur]).change_image("B", train.next);
                                train.cur += train.next * 2;
                            }
                        }
                        else if (train.length > track.length && train.speed <= 4)
                        {
                            track.change_image("", train.next);
                            train.length = 0;
                            train.speed = 0;
                            train.cur += train.next;
                        }
                        else
                        {
                            track.change_image("O", train.next);
                            end_u = end_u + (-end_a) * 1.0;
                            train.speed = end_u * 3.6 < 4.0 ? 4.0 : end_u * 3.6;
                            train.stats = "正在進站中";
                        }
                    }
                    else if (train.speed < 110 && train.wait != -2)
                    {
                        double start_u = train.speed / 3.6;// 速度 m/s
                        double start_a = 2.5; // 假設列車啟動加速度為 2.5 m/s²
                        start_u = start_u + start_a;
                        train.speed = start_u * 3.6 > 110.0 ? 110.0 : start_u * 3.6;
                        train.wait += 1;
                        if (train.wait == 10) track.change_image("B", train.next);
                        button3.Text = "煞車";
                    }
                    else if (train.speed > track.limitspeed)
                    {
                        track.change_image("R", train.next);
                        train.stats = $"速度過快，請減速!";
                    }                    
                }
            }
            train_set();
            dgv_set();
        }
        #endregion
        #region 下拉選單的變更
        private async void train_change(object sender, EventArgs e)
        {
            label7.Text = "";
            textBox2.Text = $"{Traffic.trains[comboBox1.SelectedIndex].speed.ToString()}km/h";
            button3.Text = Traffic.trains[comboBox1.SelectedIndex].speed != 0 ? "煞車" : "啟動";
            comboBox4.SelectedIndex = Traffic.trains[comboBox1.SelectedIndex].destination / 2;
            comboBox3.SelectedIndex = Traffic.trains[comboBox1.SelectedIndex].priority;
            if (button1.Text == "開始") return;
            if ((Traffic.trains[comboBox1.SelectedIndex].cur + 2) % 2 == 0 && Traffic.trains[comboBox1.SelectedIndex].wait == -2)
            {
                button4.Text = "發車";
                button3.Enabled = false;
                button4.Enabled = true;
            }
            else
            {
                button4.Text = "停靠站";
                button3.Enabled = true;
                button4.Enabled = false;
            }
            train_set();
            await FocusMethod();
        }
        public void train_set()
        {
            if (comboBox1.SelectedIndex == -1) return;
            textBox2.Text = $"{Math.Round(Traffic.trains[comboBox1.SelectedIndex].speed, 2)} km/hr";
        }
        private void dgv_set()
        {
            if(comboBox2.SelectedIndex == -1)return;
            dgv.Rows.Clear();
            foreach (Train train in Traffic.trains)
            {
                int cur_index = train.cur;
                double total_length = -train.length;
                int wait = 0;
                if ((cur_index + 2) % 2 == 0 && cur_index == Traffic.GetIndex(comboBox2.Text))
                {
                    dgv.Rows.Add(train.name, "等待發車", ((Station)Traffic.traffics[train.destination]).name, train.stats);
                    continue;
                }
                while (cur_index >= 0 && cur_index < Traffic.traffics.Count)
                {
                    if ((cur_index + 2) % 2 == 0)
                    {
                        if (((Station)Traffic.traffics[cur_index]).name == comboBox2.Text && ((Station)Traffic.traffics[cur_index]).priority >= train.priority) 
                        {
                            dgv.Rows.Add(train.name, Traffic.GetTime(total_length, wait), ((Station)Traffic.traffics[train.destination]).name, train.stats);
                            break;
                        }
                        if (cur_index == train.destination)
                        {
                            break;
                        }
                        else if (train.priority <= ((Station)Traffic.traffics[cur_index]).priority) wait += 10;
                    }
                    else
                    {
                        total_length += ((Track)Traffic.traffics[cur_index]).length;
                    }
                    cur_index += train.next;
                }
            }
        }
        private async void station_change(object sender, EventArgs e)
        {
            label11.Text = "";
            Station cur = ((Station)Traffic.traffics[comboBox2.SelectedIndex * 2]);
            textBox3.Text = $"{cur.platform}";
            comboBox7.SelectedIndex = cur.priority;
            for(int i = 0; i < Traffic.traffics.Count; i += 2)
            {
                ((Station)Traffic.traffics[i]).label.ForeColor = Color.AliceBlue;
            }
            cur.label.ForeColor = Color.Aqua;
            dgv_set();
            await FocusMethod();
        }
        private async void station_prioritization_change(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != -1 && comboBox7.SelectedIndex != -1)
            {
                Station cur = ((Station)Traffic.traffics[comboBox2.SelectedIndex * 2]);
                if (cur.priority != comboBox7.SelectedIndex)
                {
                    label11.Text = $"車站優先權已更改 {comboBox7.Items[cur.priority]}➡️{comboBox7.Items[comboBox7.SelectedIndex]}\r\n時間 : {DateTime.Now.ToString("T")}";
                    cur.priority = comboBox7.SelectedIndex;
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
                Train train = Traffic.trains[comboBox1.SelectedIndex];
                if (train.priority != comboBox3.SelectedIndex)
                {
                    label7.Text = $"列車優先權已更改\r\n{comboBox3.Items[train.priority]}➡️{comboBox3.Items[comboBox3.SelectedIndex]}\r\n時間 : {DateTime.Now.ToString("T")}";
                    train.priority = comboBox3.SelectedIndex;
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
            Train train = Traffic.trains[comboBox1.SelectedIndex];
            int change_destination = Traffic.GetIndex(comboBox4.Text);
            if (train.destination != change_destination)
            {
                if ((train.cur + 2) % 2 != 0)
                {
                    MessageBox.Show("列車正在行駛中，請先進站臨停","提示");
                    comboBox4.SelectedIndex = train.destination / 2;
                    return;
                }
                if (train.cur == change_destination)
                {
                    train.stats = $"已到達終點站";
                }
                label7.Text = $"列車目的地已更改\r\n{((Station)Traffic.traffics[train.destination]).name}➡️{comboBox4.Text}\r\n時間 : {DateTime.Now.ToString("T")}";
                train.destination = change_destination;
                train.next = train.cur > change_destination ? 1 : -1;
                if (comboBox2.SelectedIndex != -1) dgv_set();
            }
        }
        private static List<CheckBox> checks;
        private static bool[,] is_check =
        {
            { true , true, true, true, true, true },
            { false, false, false, true, false, true },
            { false, true, false, true, false, true },
            { false, false, false, false, true, false },
            { false, false, false, false, false, true }
        };
        private async void incident_change(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex == -1) return;
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
                Train train = Traffic.trains[comboBox1.SelectedIndex];
                if (button3.Text == "煞車")
                {
                    button3.Text = "啟動";
                    train.wait = -2;
                    await braking(sender, e, train);
                    label7.Text = $"列車已停止\r\n時間 : {DateTime.Now.ToString("T")}";
                }
                else
                {
                    button3.Text = "煞車";
                    train.wait = 0;
                    await active(sender, e, train);
                    label7.Text = $"列車已啟動\r\n時間 : {DateTime.Now.ToString("T")}";
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Train train = Traffic.trains[comboBox1.SelectedIndex];
            if (train.wait != -2)
            {
                train.wait = -2;
                button3.Enabled = false;
                button4.Text = "發車";
                label7.Text = $"列車已臨停\r\n時間 : {DateTime.Now.ToString("T")}";
            }
            else
            {
                train.wait = 0;
                button4.Enabled = true;
                button4.Text = "停靠站";
                label7.Text = $"列車已發車\r\n時間 : {DateTime.Now.ToString("T")}";
            }
        }
        private async Task braking(object sender, EventArgs e,Train train)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            try
            {
                double rd = train.speed;
                double a = -3.0; // 假設列車急煞時加速度為 - 3.0 m/s²
                do
                {
                    if (token.IsCancellationRequested)
                    {
                        cancellationTokenSource = null;
                        return;
                    }
                    double u = train.speed / 3.6;
                    u = u + a * 1.0;
                    train.speed = u < 0 ? 0 : u * 3.6;
                    train_change(sender, e);
                    ((Track)Traffic.traffics[train.cur]).change_image("B", train.next);
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
        private async Task active(object sender, EventArgs e, Train train)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
            if (train.cur + train.next >= 0 && train.cur + train.next < Traffic.traffics.Count && (train.cur + 2) % 2 == 0)
            {
                train.cur += train.next;
                ((Track)Traffic.traffics[train.cur]).change_image("O", train.next);
                train.stats = "正在行駛中";
            }
            train.wait = 0;
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            try
            {
                double rd = train.speed;
                double a = 2.5; // 假設列車啟動時加速度為 2.5 m/s²
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
                    u = u + a * 1.0;
                    Traffic.trains[comboBox1.SelectedIndex].speed = u * 3.6 > 110.0 ? 110.0 : u * 3.6;
                    train_change(sender, e);
                    ((Track)Traffic.traffics[train.cur]).change_image("B", train.next);
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
        #endregion
        #region 送出事件處理
        private void button7_Click(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex != -1 && comboBox6.SelectedIndex != -1)
            {
                MessageBox.Show($"事件：{comboBox5.Text}\r\n\r\n列車：{comboBox6.Text}\r\n\r\n已聯絡：" +
                    $"{string.Join("、", checks.Where(x => x.Checked).Select(x => x.Text))}\r\n\r\n" +
                    $"詳細內容：\r\n{(richTextBox1.Text == "" ? "None" : richTextBox1.Text)}", "緊急處理", MessageBoxButtons.OK);
                comboBox5.SelectedIndex = comboBox6.SelectedIndex = -1;
                checks.ForEach(check => check.Checked = false);
                richTextBox1.Text = "";
            }
        }
        #endregion
        #region 自訂 Label 站名事件 (路線地圖)
        private void Label_Click(object sender,EventArgs args)
        {
            Label clickedLabel = sender as Label;
            if (clickedLabel != null)
            {
                comboBox2.SelectedIndex = Traffic.GetIndex(clickedLabel.Text) / 2;
            }
        }
        private void Label_MouseMove(object sender,EventArgs args)
        {
            Label label = sender as Label;
            if (label != null) label.Font = new Font("微软雅黑", 12, FontStyle.Bold);
        }
        private void Label_MouseLeave(object sender,EventArgs args)
        {
            Label leftLabel = sender as Label;
            if (leftLabel != null) leftLabel.Font = new Font("微软雅黑", 12);
        }
        #endregion
        #region ComboBox 的焦點變更
        private async Task FocusMethod()
        {
            await Task.Delay(200);
            label1.Focus();
        }
        #endregion
    }
}