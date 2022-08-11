using System;
using System.Drawing;
using System.Windows.Forms;

namespace Optitracker.GUI
{
    public partial class Trigger : UserControl,tabInterface
    {
        public class TriggerPacket : MDOL.IO.DataSaver.DataPacket
        {
            protected override string PacketStr => "Trigger";
        }

        static System.IO.Ports.SerialPort serialPort = null;
        public Trigger()
        {
            InitializeComponent();

            int[] ports = MDOL.IO.Ports.GetPorts(MDOL.IO.Ports.TTL);
            if (ports.Length == 1)
                SetPort(ports[0]);
        }
        public void onLoad()
        {
        }

        public void Destroy()
        {
            if (serialPort != null)
            {
                serialPort.Close();
                serialPort = null;
            }
        }
        public string[] IsReady()
        {
            if (serialPort == null || !serialPort.IsOpen)
                return new string[] { "Triggerport not connected" };
            return new string[0];
        }

        public void UpdateGUI()
        {

        }

        void SetPort(int port)
        {
            if (serialPort != null)
                serialPort.Close();
            serialPort = new System.IO.Ports.SerialPort("COM" + port);
            serialPort.Open();
            lblPort.ForeColor = Color.Green;
            lblPort.Text = "COM" + port;
            cmdSendTrigger.Enabled = true;
        }
        public static void SendTrigger()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                MDOL.IO.DataSaver.AddPacket(new TriggerPacket());
                serialPort.RtsEnable = true;
                serialPort.RtsEnable = false;
            }
        }

        private void cmdSetPort_Click(object sender, EventArgs e)
        {
            int[] ports = MDOL.IO.Ports.GetPorts(MDOL.IO.Ports.TTL);
            if (ports.Length == 0)
                MessageBox.Show("No USB Serial Ports was found");
            else if (ports.Length == 1)
                SetPort(ports[0]);
            else
            {
                Form form = new Form()
                {
                    Width = 120,
                    Height = ports.Length * (30 + 3) + 20 + 40,
                    Text = "Choose Triggerport (see DeviceManager->Ports)",
                    StartPosition = FormStartPosition.CenterParent
                };
                for (int i = 0; i < ports.Length; i++)
                {
                    Button button = new Button()
                    {
                        Bounds = new Rectangle(10, (30 + 3) * i + 10, 100, 30),
                        Text = "COM" + ports[i],
                    };
                    button.Click += (button_s, button_e) =>
                    {
                        SetPort(ports[i]);
                        form.Close();
                    };
                    form.Controls.Add(button);
                }
                form.ShowDialog();
            }
        }

        private void cmdSendTrigger_Click(object sender, EventArgs e)
        {
            SendTrigger();
        }
    }
}
