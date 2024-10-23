using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace 簡易的行控中心
{
    public partial class Form2 : Form
    {
        public event EventHandler DataInputCompleted;
        public static string email;
        public static string type;
        public static string content;
        public static bool isOK;
        private static CheckBox[] checks;
        public Form2()
        {
            InitializeComponent();
            checks = new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4 };
            foreach (var control in this.Controls.OfType<Control>().Where(c => c is TextBox || c is Label || c is RichTextBox))
            {
                control.MouseDown += Form2_MouseDown;
                control.MouseUp += Form2_MouseUp;
                control.MouseMove += Form2_MouseMove;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string[] texts = new string[] { "問題報告", "功能建議", "使用體驗", textBox3.Text };
                type = string.Join("、", checks.Select((check, index) => new { check, text = texts[index] }).Where(x => x.check.Checked).Select(x => x.text));
                string[] errorMessages = new string[] {"電子郵件為空","其他類型名字超過 6 或已選但為空", "內容為空，字長需在 50 以內", "請選擇任何一個回饋類型" };
                string[] stringBox = new string[] { textBox2.Text, textBox3.Text, richTextBox1.Text, type };
                Func<string, bool>[] conditions = new Func<string, bool>[]
                {
                    tb => string.IsNullOrWhiteSpace(tb),
                    tb => checkBox4.Checked && (string.IsNullOrWhiteSpace(tb) || tb.Length > 6),
                    tb => string.IsNullOrWhiteSpace(tb) || tb.Length > 50,
                    tb => tb == ""
                };
                for (int i = 0; i < stringBox.Length; i++)
                {
                    if (conditions[i](stringBox[i]))
                    {
                        throw new FormatException(errorMessages[i]);
                    }
                }
                email = textBox2.Text;
                content =  richTextBox1.Text;
                isOK = checkBox5.Checked;
                this.Close();
                DataInputCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "格式錯誤", MessageBoxButtons.OK);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.Visible = checkBox4.Checked;
            textBox3.Text = checkBox4.Checked ? textBox3.Text : "";
            checkBox4.Text = checkBox4.Checked ? "其他（請說明）：" : "其他";
        }
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            label5.Visible = checkBox5.Checked;
        }
        class FormatException : Exception
        {
            public FormatException(string message) : base(message)
            {

            }
        }
        #region 拖曳視窗
        private static bool isDragging;
        private static Point offset;
        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                offset = e.Location;
            }
        }
        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = this.PointToScreen(e.Location);
                this.Location = new Point(newLocation.X - offset.X, newLocation.Y - offset.Y);
            }
        }
        #endregion
    }
}
