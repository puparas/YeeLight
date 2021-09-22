using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace YeeLight
{
    class MakeUi
    {
        private readonly List<string> usableParams = new List<string> {
            "name",
            "hue",
            "power",
            "bright",
            "rgb",
            "bright",
            "color_mode",
            "ct"
        };
        private GroupBox grBox;
        private Device dvc;
        private Dictionary<string, Control> controls = new Dictionary<string, Control>();
        private ToolStripMenuItem toolStripMenuItem;
        public MakeUi(Device device)
        {
            toolStripMenuItem = new ToolStripMenuItem();
            this.dvc = device;
        }

        public void MakeUiByParams(Dictionary<string, string> deviceParams)
        {
            grBox = CreateGroupBox();
            TextBoxForTests();
            TextBoxForTestsSend();
            AddStripMenuItem();
            foreach (KeyValuePair<string, string> param in deviceParams)
            {
                if (usableParams.IndexOf(param.Key) >= 0)
                {
                    if (Type.GetType("YeeLight.MakeUi").GetMethod(param.Key) != null)
                    {
                        string[] args = new string[] { param.Key, param.Value };
                        InvokeMethod(param.Key, args);
                    }

                }
            }
            Application.OpenForms[0].Controls.Add(grBox);
        }
        public void UpdateUiByParams(Dictionary<string, dynamic> deviceParams)
        {
            if (dvc == null)
                return;
            foreach (KeyValuePair<string, object> param in deviceParams)
            {
                if (usableParams.IndexOf(param.Key) >= 0)
                {

                    if (Type.GetType("YeeLight.MakeUi").GetMethod(param.Key) != null)
                    {
                        string[] args = new string[] { param.Key, param.Value.ToString() };
                        InvokeMethod(param.Key, args);
                    }

                }
            }
        }

        public void power(string k, string v)
        {
            if (controls.ContainsKey(k))
            {
                controls[k].BackgroundImage = (v == "off") ? Properties.Resources.off : Properties.Resources.on;
                controls[k].Enabled = true;
                AddStripMenuItem(v);
                return;
            }
            PictureBox OnOff = new PictureBox();
            OnOff.BackgroundImage = (v == "off") ? Properties.Resources.off : Properties.Resources.on;
            OnOff.BackgroundImageLayout = ImageLayout.Center;
            OnOff.Location = new Point(36, 20);
            OnOff.Name = "OnOff";
            OnOff.Size = new Size(28, 28);
            OnOff.TabIndex = 0;
            OnOff.TabStop = false;
            OnOff.Click += new EventHandler(dvc.toggle);
            controls.Add(k, OnOff);
            grBox.Controls.Add(OnOff);
        }
        public void name(string k, string v)
        {
            if (controls.ContainsKey(k))
            {
                TextBox txtBox = (TextBox)controls[k];
                txtBox.ReadOnly = false;
                txtBox.Text = v;
                txtBox.SelectionStart = v.Length;
                return;
            }
            TextBox LightName = new TextBox();
            LightName.PlaceholderText = "Name";
            LightName.BackColor = Color.WhiteSmoke;
            LightName.BorderStyle = BorderStyle.FixedSingle;
            LightName.Font = new Font("Microsoft Sans Serif", 9.75F, ((FontStyle)((FontStyle.Bold | FontStyle.Underline))), GraphicsUnit.Point);
            LightName.Location = new Point(100, 25);
            LightName.MaxLength = 15;
            LightName.Name = "LightName";
            LightName.Size = new Size(200, 20);
            LightName.TabIndex = 1;
            LightName.TextAlign = HorizontalAlignment.Center;
            LightName.Text = v;
            LightName.KeyUp += new KeyEventHandler(dvc.set_name);
            controls.Add(k, LightName);
            grBox.Controls.Add(LightName);
        }
        public void hue(string k, string v)
        {
            if (controls.ContainsKey(k))
            {
                PictureBox ColorPck = (PictureBox)controls[k];
                ColorPck.Visible = (dvc.deviceParams["color_mode"] == "3" || dvc.deviceParams["color_mode"] == "1");
                return;
            }
            PictureBox ColorPick = new PictureBox();
            ColorPick.BackgroundImage = Properties.Resources.color;
            ColorPick.Visible = (dvc.deviceParams["color_mode"] == "3" || dvc.deviceParams["color_mode"] == "1");
            ColorPick.BackColor = Color.Maroon;
            ColorPick.BackgroundImageLayout = ImageLayout.Center;
            ColorPick.Location = new Point(336, 20);
            ColorPick.Name = "Color";
            ColorPick.Size = new Size(28, 28);
            ColorPick.TabIndex = 2;
            ColorPick.TabStop = false;
            ColorPick.Click += new EventHandler(dvc.set_hsv);
            controls.Add(k, ColorPick);
            grBox.Controls.Add(ColorPick);
            rgb(null, dvc.deviceParams["rgb"]);
        }
        public void ct(string k, string v)
        {
            if (controls.ContainsKey(k))
            {
                TrackBar TempBar = (TrackBar)controls[k];
                PictureBox TemPict = (PictureBox)controls["TemPic"];
                TempBar.Visible = TemPict.Visible = (dvc.deviceParams["color_mode"] == "2");
                TempBar.Value = Int32.Parse(v);
                return;
            }
            TrackBar TemBar = new TrackBar();
            TemBar.Visible = (dvc.deviceParams["color_mode"] == "2");
            TemBar.SendToBack();
            TemBar.Location = new Point(343, 10);
            TemBar.Name = "trackBar1";
            TemBar.Orientation = Orientation.Vertical;
            TemBar.RightToLeft = RightToLeft.No;
            TemBar.Size = new Size(45, 90);
            TemBar.TabIndex = 1;
            TemBar.Minimum = 1700;
            TemBar.Maximum = 6500;
            TemBar.Value = Int32.Parse(v);
            TemBar.MouseCaptureChanged += new EventHandler(dvc.set_ct_abx);
            TemBar.TickStyle = TickStyle.TopLeft;

            PictureBox TemPic = new PictureBox();
            TemPic.Visible = (dvc.deviceParams["color_mode"] == "2");
            TemPic.BackgroundImage = Properties.Resources.img2;
            TemPic.BackgroundImageLayout = ImageLayout.Stretch;
            TemPic.Enabled = false;
            TemPic.BringToFront();
            TemPic.Location = new Point(344, 17);
            TemPic.Name = "pictureBox1";
            TemPic.Size = new Size(26, 76);
            TemPic.TabIndex = 2;
            TemPic.TabStop = false;

            controls.Add(k, TemBar);
            grBox.Controls.Add(TemBar);

            controls.Add("TemPic", TemPic);
            grBox.Controls.Add(TemPic);
            grBox.Controls.SetChildIndex((Control)TemPic, 0);
        }
        public void bright(string k, string v)
        {
            if (controls.ContainsKey(k))
            {
                TrackBar TkBr = (TrackBar)controls[k];
                TkBr.Value = Int32.Parse(v);
                return;
            }
            TextBox TextBoxBrightVal = new TextBox();
            TextBoxBrightVal.BackColor = Color.WhiteSmoke;
            TextBoxBrightVal.BorderStyle = BorderStyle.None;
            TextBoxBrightVal.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point);
            TextBoxBrightVal.Location = new Point(175, 50);
            TextBoxBrightVal.Name = "textBox1";
            TextBoxBrightVal.Size = new Size(50, 13);
            TextBoxBrightVal.TabIndex = 1;
            TextBoxBrightVal.Text = v;
            TextBoxBrightVal.TextAlign = HorizontalAlignment.Center;

            TrackBar BrightBar = new TrackBar();
            BrightBar.AutoSize = false;
            BrightBar.BackColor = Color.WhiteSmoke;
            BrightBar.Location = new Point(100, 63);
            BrightBar.Maximum = 100;
            BrightBar.Minimum = 1;
            BrightBar.Name = "trackBar1";
            BrightBar.Size = new Size(200, 28);
            BrightBar.SmallChange = 10;
            BrightBar.TabIndex = 0;
            BrightBar.TickStyle = TickStyle.TopLeft;
            BrightBar.Value = Int32.Parse(v);
            BrightBar.MouseCaptureChanged += new EventHandler(dvc.set_bright);
            BrightBar.ValueChanged += new EventHandler((object sender, EventArgs e) =>
            {
                TextBoxBrightVal.Text = BrightBar.Value.ToString();
            });


            controls.Add(k, BrightBar);
            grBox.Controls.Add(BrightBar);
            grBox.Controls.Add(TextBoxBrightVal);
        }
        /*Обновляет цвет индикатора цвета после его изменения через hue*/
        public void rgb(string k, string v)
        {
            if (controls.ContainsKey("hue"))
            {
                byte[] byteColor = BitConverter.GetBytes(Int32.Parse(v));
                var rgbColor = Color.FromArgb(byteColor[2], byteColor[1], byteColor[0]);
                controls["hue"].BackColor = rgbColor;
            }
        }
        public void color_mode(string k, string v)
        {
            SwitchModsButtons(v);
            if (controls.ContainsKey("hue") || controls.ContainsKey("ct"))
            {
                hue("hue", null);
                ct("ct", null);
            }


        }
        public ColorDialog GetCD()
        {
            ColorDialog CDialog = new ColorDialog();
            CDialog.AllowFullOpen = true;
            CDialog.ShowHelp = false;
            CDialog.AnyColor = true;
            CDialog.FullOpen = true;
            CDialog.SolidColorOnly = false;
            return CDialog;
        }
        private void SwitchModsButtons(string v)
        {
            if (controls.ContainsKey("ModButtonOne") && controls.ContainsKey("ModButtonToo"))
            {
                controls["ModButtonOne"].BackColor = (v == "3" || v == "1") ? Color.Green : Color.WhiteSmoke;
                controls["ModButtonToo"].BackColor = (v == "2") ? Color.Green : Color.WhiteSmoke;
                return;
            }
            Button ModButtonOne = new Button();
            ModButtonOne.Location = new Point(200, 1);
            ModButtonOne.Size = new Size(50, 20);
            ModButtonOne.BackColor = (v == "3" || v == "1") ? Color.Green : Color.WhiteSmoke;
            ModButtonOne.Font = new Font("Microsoft Sans Serif", 7.75F, FontStyle.Bold, GraphicsUnit.Point);
            ModButtonOne.Text = "Clr";
            ModButtonOne.Click += new EventHandler((o, e) =>
            {
                dvc.swichMode("3");
            });

            Button ModButtonToo = new Button();
            ModButtonToo.Location = new Point(150, 1);
            ModButtonToo.Size = new Size(50, 20);
            ModButtonToo.BackColor = (v == "2") ? Color.Green : Color.WhiteSmoke;
            ModButtonToo.Font = new Font("Microsoft Sans Serif", 7.75F, FontStyle.Bold, GraphicsUnit.Point);
            ModButtonToo.Text = "Tmpr";
            ModButtonToo.Click += new EventHandler((o, e) =>
            {
                dvc.swichMode("2");
            });


            controls.Add("ModButtonOne", ModButtonOne);
            controls.Add("ModButtonToo", ModButtonToo);
            grBox.Controls.Add(ModButtonOne);
            grBox.Controls.Add(ModButtonToo);

        }
        private void TextBoxForTests()
        {
            TextBox TextCommand = new TextBox();
            TextCommand.PlaceholderText = "Command";
            TextCommand.BackColor = Color.WhiteSmoke;
            TextCommand.BorderStyle = BorderStyle.FixedSingle;
            TextCommand.Font = new Font("Microsoft Sans Serif", 9.75F, (FontStyle.Bold), GraphicsUnit.Point);
            TextCommand.Location = new Point(10, 120);
            TextCommand.MaxLength = 15;
            TextCommand.Multiline = true;
            TextCommand.Name = "SendCommand";
            TextCommand.Size = new Size(380, 40);
            TextCommand.MaxLength = 150;
            TextCommand.TabIndex = 1;
            TextCommand.Text = "{\"id\":1,\"method\":\"set_power\",\"params\":[\"on\", \"smooth\", 500]}";
            TextCommand.TextAlign = HorizontalAlignment.Center;
            TextCommand.KeyUp += new KeyEventHandler(dvc.save_command);
            controls.Add("textBoxForTests", TextCommand);
            grBox.Controls.Add(TextCommand);
        }

        private void TextBoxForTestsSend()
        {
            Button SendCommand = new Button();
            SendCommand.Location = new Point(175, 170);
            SendCommand.Size = new Size(50, 25);
            SendCommand.Font = new Font("Microsoft Sans Serif", 8.75F, FontStyle.Bold, GraphicsUnit.Point);
            SendCommand.Text = "Send";
            SendCommand.Click += new EventHandler(dvc.send_command);
            controls.Add("textBoxForTestsSend", SendCommand);
            grBox.Controls.Add(SendCommand);
        }
        private GroupBox CreateGroupBox()
        {

            GroupBox groupBox = new GroupBox();
            var count = Application.OpenForms[0].Controls.Find("groupBox", false).Length;
            groupBox.SuspendLayout();
            groupBox.BackgroundImageLayout = ImageLayout.None;
            groupBox.Location = new Point(5, count * 200 + 25);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(400, 200);
            groupBox.TabIndex = 1;
            groupBox.TabStop = false;
            return groupBox;
        }
        private void AddStripMenuItem(string v = null)
        {
            if (v != null)
            {
                toolStripMenuItem.Checked = (v == "on");
                return;
            }
            toolStripMenuItem.Name = dvc.deviceParams["name"];
            toolStripMenuItem.Size = new Size(180, 22);
            toolStripMenuItem.Checked = (dvc.deviceParams["power"] == "on");
            toolStripMenuItem.Text = dvc.deviceParams["name"];
            toolStripMenuItem.ToolTipText = dvc.deviceParams["name"];
            toolStripMenuItem.Click += new EventHandler((s, e) => { dvc.toggle(null, null); });
            Form1.Notifier.ContextMenuStrip.Items.Add(toolStripMenuItem);
        }
        private void InvokeMethod(string methodName, string[] args)
        {
            Type.GetType("YeeLight.MakeUi").GetMethod(methodName).Invoke(this, args);
        }
    }
}
