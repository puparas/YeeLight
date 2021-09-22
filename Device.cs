using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YeeLight
{

    class Device
    {
        TcpClient Tclient;
        MakeUi uiGenerator;
        public Dictionary<string, string> deviceParams;
        string[] locationDevice = new string[2];
        public string TextTestCommand { get; set; }
        private static Dictionary<object, object> commandTmpl = new Dictionary<object, object> { { "id", 1 }, { "method", "" }, { "params", "" } };
        public Device(Dictionary<string, string> deviceParams)
        {
            uiGenerator = new MakeUi(this);
            Tclient = new TcpClient();
            this.deviceParams = deviceParams;
            ParseDeviceLocation();
            Tclient.Connect(locationDevice[0], Int32.Parse(locationDevice[1]));
            StartListener();
            uiGenerator.MakeUiByParams(deviceParams);
        }

        public static Dictionary<string, string> GetParamsFormString(string deviceString)
        {
            string pattern = "(.*):(.*)\\r\\n";
            Regex rgx = new Regex(pattern);
            Dictionary<string, string> props = new Dictionary<string, string>();
            foreach (Match match in rgx.Matches(deviceString))
            {
                string[] str = match.Value.Split(new[] { ':' }, 2);
                string propertyName = str[0].Trim().Trim('"');
                string propertyValue = str[1].Trim().Trim('"');
                props.Add(propertyName, propertyValue);
            }
            return props;
        }

        private async void StartListener()
        {
            byte[] buffer = new byte[500];
            NetworkStream stream = Tclient.GetStream();
            int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, byteCount);
            Debug.WriteLine(request);
            ParseRequest(request);
            StartListener();
        }

        private void ParseRequest(string request)
        {
            try
            {
                var requestOnj = JToken.Parse(request);
                //var requestOnj = JsonConvert.DeserializeObject<JObject>(request);
                if (requestOnj["params"] != null)
                {
                    Dictionary<string, dynamic> paramsDvc = requestOnj["params"].ToObject<Dictionary<string, dynamic>>();
                    UpdateDeviceParams(paramsDvc);
                    uiGenerator.UpdateUiByParams(paramsDvc);
                }
                else if (requestOnj["error"] != null)
                {
                    MessageBox.Show("...и команда твоя тоже", "Ты инвалид...");
                }
            }
            catch (JsonReaderException jex)
            {
                //MessageBox.Show(jex.ToString(), "Твой json инвалид...");
            }
            catch (Exception jex)
            {
                //MessageBox.Show(jex.ToString(), "Твой json инвалид...");
            }



        }
        public void toggle(object sender, EventArgs e)
        {
            if (sender != null)
            {
                PictureBox objTextBox = (PictureBox)sender;
                objTextBox.Enabled = false;
            }

            string method = MethodBase.GetCurrentMethod().Name;
            commandTmpl["method"] = method;
            PreSend(commandTmpl);

        }
        public void set_name(object sender, EventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            if (!Regex.IsMatch(objTextBox.Text, @"\p{IsCyrillic}") && !objTextBox.ReadOnly)
            {
                objTextBox.ReadOnly = false;
                string method = MethodBase.GetCurrentMethod().Name;
                commandTmpl["method"] = method;
                commandTmpl["params"] = new string[] { objTextBox.Text };
                PreSend(commandTmpl);
            }
            else
            {
                string str = objTextBox.Text;
                Dictionary<string, dynamic> dataName = new Dictionary<string, object> { { "name", str.TrimEnd(str[str.Length - 1]) } };
                uiGenerator.UpdateUiByParams(dataName);

            }
        }
        public void set_hsv(object sender, EventArgs e)
        {
            ColorDialog CDialog = uiGenerator.GetCD();
            if (CDialog.ShowDialog() == DialogResult.OK)
            {
                short hue = (short)CDialog.Color.GetHue();
                short sat = (short)(CDialog.Color.GetSaturation() * 100);
                string method = MethodBase.GetCurrentMethod().Name;
                commandTmpl["method"] = method;
                commandTmpl["params"] = new dynamic[4] { hue, sat, "smooth", 1000 };
                PreSend(commandTmpl);
            }

        }

        public void set_bright(object sender, EventArgs e)
        {
            TrackBar objTextBox = (TrackBar)sender;
            string method = MethodBase.GetCurrentMethod().Name;
            commandTmpl["method"] = method;
            commandTmpl["params"] = new dynamic[3] { objTextBox.Value, "smooth", 1000 };
            PreSend(commandTmpl);
        }

        public void set_ct_abx(object sender, EventArgs e)
        {
            TrackBar objTextBox = (TrackBar)sender;
            string method = MethodBase.GetCurrentMethod().Name;
            commandTmpl["method"] = method;
            commandTmpl["params"] = new dynamic[3] { objTextBox.Value, "smooth", 1000 };
            PreSend(commandTmpl);
        }

        public void save_command(object sender, EventArgs e)
        {
            TextBox objTextBoxCommand = (TextBox)sender;
            string data = objTextBoxCommand.Text.Replace("\r\n", "");
            data += Environment.NewLine;
            TextTestCommand = data;
        }

        public void send_command(object sender, EventArgs e)
        {
            try
            {
                var obj = JToken.Parse(TextTestCommand);
                SendCommands(TextTestCommand);
            }
            catch (JsonReaderException jex)
            {
                MessageBox.Show(jex.ToString(), "send_command говорит: Твой json инвалид...");
            }
            catch (Exception jex)
            {
                MessageBox.Show(jex.ToString(), "send_command говорит: Твой json инвалид...");
            }

        }
        public void swichMode(string mode)
        {
            switch (mode)
            {
                case "2":
                    commandTmpl["method"] = "set_ct_abx";
                    commandTmpl["params"] = new dynamic[3] { Int32.Parse(deviceParams["ct"]), "smooth", 1000 };
                    PreSend(commandTmpl);
                    break;
                case "3":
                    commandTmpl["method"] = "set_hsv";
                    commandTmpl["params"] = new dynamic[4] { Int32.Parse(deviceParams["hue"]), Int32.Parse(deviceParams["sat"]), "smooth", 1000 };
                    PreSend(commandTmpl);
                    break;
                default:
                    break;
            }
        }

        private void ParseDeviceLocation()
        {
            locationDevice = deviceParams["Location"].Replace("yeelight://", "").Split(':');
        }


        private void PreSend(Dictionary<object, object> data)
        {
            string dataToSend = JsonConvert.SerializeObject(data);
            dataToSend += Environment.NewLine;
            SendCommands(dataToSend);
        }

        private void UpdateDeviceParams(Dictionary<string, dynamic> newDvcParams)
        {
            foreach (KeyValuePair<string, object> param in newDvcParams)
            {
                deviceParams[param.Key] = param.Value.ToString();
            }
        }
        private void SendCommands(string data)
        {
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                NetworkStream stream = Tclient.GetStream();
                stream.Write(dataBytes, 0, dataBytes.Length);
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show(e.ToString(), "SendCommands говорит: Ты инвалид...");
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.ToString(), "SendCommands говорит: Ты инвалид...");
            }

        }
    }

}
