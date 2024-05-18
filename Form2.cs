using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 簡易的行控中心
{
    public partial class Form2 : Form
    {
        public event EventHandler DataInputCompleted;
        public static string email;
        public static string type;
        public static string content;
        public static string[] texts;
        public static bool isOK;
        private static CheckBox[] checks;
        private static bool isDragging;
        private static Point offset;
        public Form2()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            checks = new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4 };
            texts = new string[] { "問題報告", "功能建議", "使用體驗", "" };
            foreach(Control control in this.Controls)
            {
                if (control is TextBox || control is Label || control is RichTextBox)
                {
                    control.MouseDown += Form2_MouseDown;
                    control.MouseUp += Form2_MouseUp;
                    control.MouseMove += Form2_MouseMove;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    throw new FormatException("電子郵件為空");
                }
                if (checkBox4.Checked && (string.IsNullOrWhiteSpace(textBox3.Text) || textBox3.Text.Length > 6))
                {
                    throw new FormatException("其他類型名字超過 6 或已選但為空");
                }
                else
                {
                    texts[3] = textBox3.Text;
                }
                if (string.IsNullOrWhiteSpace(richTextBox1.Text) || richTextBox1.Text.Length > 50)
                {
                    throw new FormatException("內容為空，字長需在 50 以內");
                }
                var typeBuilder = new StringBuilder();
                for (int i = 0; i < 4; i++)
                {
                    if (checks[i].Checked) typeBuilder.Append(texts[i] + "、");          
                }
                if (type == "")
                {
                    throw new FormatException("沒有選擇任何類型");
                }
                email = textBox2.Text;
                type = typeBuilder.ToString().TrimEnd('、');
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
            if (checkBox4.Checked)
            {
                textBox3.Visible = true;
            }
            else
            {
                textBox3.Text = "";
                textBox3.Visible = false;
            }
        }

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
    }
    class FormatException : Exception
    {
        public FormatException(string message) : base(message)
        {
        }
    }
}
