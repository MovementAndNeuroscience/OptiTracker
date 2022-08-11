using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Optitracker.GUI
{
    public partial class Tobii : UserControl,tabInterface
    {
        public class TobiiPacket : MDOL.IO.DataSaver.DataPacket
        {
            protected override string PacketStr => "Tobii";
            public TobiiPacket()
            {
                AddData("HeadUnit", Device.HeadUnit);
                AddData("RecordingUnit", Device.RecordingUnit);
                AddData("UUID", Device.Recorder_UUID);
                AddData("Folder", Device.Recorder_Folder);
            }
        }
        static class Device
        {
            public static List<string> Candidates = new List<string>() { "TG03B-080201012561"};
            public static int currentIP = -1;

            public static int Battery = -1;
            public static int SecondsLeft = -1;
            public static string HeadUnit = "";
            public static string RecordingUnit = "";
            public static int Calibrated = -1;
            public static DateTime TriggerSent = DateTime.MinValue;

            public static string Time = "";
            public static double Recorder_Duration = -1;
            public static string Recorder_UUID = "";
            public static string Recorder_Folder = "";
            public static bool Connected = false;

            private static async Task<string> Get(string obj)
            {
                if (!Connected)
                    return null;
                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                Console.WriteLine("get: " + obj);
                string body = await httpClient.GetStringAsync("http://" + Candidates[currentIP] + "/rest/" + obj);
                Console.WriteLine("get: " + obj + "- Done");
                return body;
            }
            private static async Task<string> Post(string obj, string content)
            {
                if (!Connected)
                    return null;
                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                Console.WriteLine("post: " + obj);
                try
                {
                    System.Net.Http.HttpResponseMessage response = await httpClient.PostAsync("http://" + Candidates[currentIP] + "/rest/" + obj, new System.Net.Http.StringContent(content));
                    string body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("post: " + obj + "- Done with message =" + body);
                    return body;
                }
                catch (Exception ex)
                {
                    if (obj == "recorder!stop")
                        MessageBox.Show("Something went wrong during storing to SD-Card - DO NOT START A NEW RECORDING!\r\nContact Mikkel and/or make a backup of the folder \"recorder\" on the SD-card");
                    return ex.Message;
                }
            }

            public static Dictionary<string,RecordingInfo> Recordings = new Dictionary<string, RecordingInfo>();
            public class RecordingInfo
            {
                public double Duration;
                public string UUID;
                public string Folder;
                public static async Task<RecordingInfo> GetRecordingInfo(string recording)
                {
                    try
                    {
                        string duration = await Get("recordings/" + recording + ".duration");
                        string folder = await Get("recordings/" + recording + ".folder");
                        return new RecordingInfo()
                        {
                            Duration = MDOL.Extension.ToDouble(duration),
                            Folder = folder,
                            UUID = recording
                        };
                    }
                    catch(Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                    return null;
                }
            }

            static System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            static bool pinging = false;
            public static async void Update()
            {
                if (Candidates.Count == 0 || pinging || Stopping)
                    return;

                if (currentIP == -1)
                    currentIP = 0;
                pinging = true;
                System.Net.NetworkInformation.PingReply reply = null;
                try
                {
                    reply = await ping.SendPingAsync(Candidates[currentIP]);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
                pinging = false;
                if (reply == null || reply.Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    Connected = false;
                    if (currentIP + 1 == Candidates.Count)
                        currentIP = 0;
                    else
                        currentIP++;
                    return;
                }
                Connected = true;

                try
                {
                    HeadUnit = await Get("system.recording-unit-serial");
                    RecordingUnit = await Get("system.head-unit-serial");
                    Time = await Get("system.time");
                    if (Time != null)
                        Time = Time.Replace("\"", "");
                    else
                        Time = "";

                    Battery = (int)(MDOL.Extension.ToDouble(await Get("system/battery.level")) * 100);
                    SecondsLeft = int.Parse(await Get("recorder.remaining-time"));
                    Recorder_Duration = MDOL.Extension.ToDouble(await Get("recorder.duration"));
                    if (Recorder_Duration > -1)
                    {
                        Recorder_UUID = (await Get("recorder.uuid")).Replace("\"", "");
                        Recorder_Folder = (await Get("recorder.folder")).Replace("\"", "");
                        Trigger();
                    }

                    System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                    string[] recordings = (await httpClient.GetStringAsync("http://" + Candidates[currentIP] + "/recordings/")).Split(new string[] { "[", "\"", "]", "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(string recording in recordings)
                    {
                        if(!Recordings.ContainsKey(recording))
                        {
                            RecordingInfo recordingInfo = await RecordingInfo.GetRecordingInfo(recording);
                            if (recordingInfo != null)
                                Recordings.Add(recording, recordingInfo);
                        }
                    }
                }
                catch (Exception ex) { Debug.Log(ex.Message); }
            }

            static DateTime lastSetTime = new DateTime();
            public static async void SetTime(DateTime dateTime)
            {
                try
                {
                    if ((DateTime.Now - lastSetTime).TotalSeconds > 5)
                    {
                        lastSetTime = DateTime.Now;
                        await Post("system!use-ntp", "[false]");
                        await Post("system!set-time", "[\"" + dateTime.ToString("yyyy-MM-ddTHH:mm:ss+00:00",System.Globalization.CultureInfo.InvariantCulture) + "\"]");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
            public static async void StartRecording()
            {
                try
                {
                    string started = await Post("recorder!start", "[]");
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
            static bool Stopping = false;
            public static async void StopRecording()
            {
                Stopping = true;
                try
                {
                    System.Threading.Thread.Sleep(500);
                    string stopped = await Post("recorder!stop", "[]");
                }
                catch(Exception ex)
                {
                    Debug.Log(ex.Message);
                }
                Stopping = false;
            }
            public static async void Download(string UUID)
            {
                if (UUID != "")
                {
                    System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                    byte[] g3 = await httpClient.GetByteArrayAsync("http://" + Candidates[currentIP] + "/recordings/" + Recorder_UUID);

                    if (!System.IO.Directory.Exists("TobiiRecordings/" + Recorder_Folder))
                    {
                        byte[] eventdata = await httpClient.GetByteArrayAsync("http://" + Candidates[currentIP] + "/recordings/" + Recorder_UUID + "/eventdata.gz");
                        byte[] gazedata = await httpClient.GetByteArrayAsync("http://" + Candidates[currentIP] + "/recordings/" + Recorder_UUID + "/gazedata.gz");
                        byte[] imudata = await httpClient.GetByteArrayAsync("http://" + Candidates[currentIP] + "/recordings/" + Recorder_UUID + "/imudata.gz");
                        byte[] scenevideo = await httpClient.GetByteArrayAsync("http://" + Candidates[currentIP] + "/recordings/" + Recorder_UUID + "/scenevideo.mp4");
                        System.IO.Directory.CreateDirectory("TobiiRecordings/" + Recorder_Folder);
                        System.IO.File.WriteAllBytes("TobiiRecordings/" + Recorder_Folder + "/recording.g3", g3);
                        System.IO.File.WriteAllBytes("TobiiRecordings/" + Recorder_Folder + "/eventdata.gz", eventdata);
                        System.IO.File.WriteAllBytes("TobiiRecordings/" + Recorder_Folder + "/gazedata.gz", gazedata);
                        System.IO.File.WriteAllBytes("TobiiRecordings/" + Recorder_Folder + "/imudata.gz", imudata);
                        System.IO.File.WriteAllBytes("TobiiRecordings/" + Recorder_Folder + "/scenevideo.mp4", scenevideo);
                    }
                }
            }
            public static async void Calibrate()
            {
                Calibrated = await Post("calibrate!run", "[]") == "true" ? 1 : 0;
                MessageBox.Show("Calibration " + (Calibrated==1 ? "Succeded!" : "Failed!"));
            }
            public static void WebClient()
            {
                System.Diagnostics.Process.Start("http://" + Candidates[currentIP]);
            }
            public static async void Trigger()
            {
                DateTime now = DateTime.Now;
                string result = await Post("recorder!send-event", "[\"Trigger\",{\"Ticks\":" + now.Ticks + "}]");
                if (result == "true")
                    TriggerSent = now;
            }
        }

        private void cmdCalibrate_Click(object sender, EventArgs e)
        {
            Device.Calibrate();
        }

        private void cmdREST_Click(object sender, EventArgs e)
        {
            Device.WebClient();
        }

        class ExtendedLabel
        {
            static int Y = 12;
            string Title;
            Action<Label> onUpdate = null;
            Label label;
            public ExtendedLabel(Control parent, string Title, Action<Label> onUpdate)
            {
                this.Title = Title;
                this.onUpdate = onUpdate;
                createControls(parent, new Label());
            }

            Button button;
            public bool Enabled
            {
                set
                {
                    if (button != null)
                        button.Enabled = value;
                }
            }
            public ExtendedLabel(Control parent, string Title, Action onClick, Action<Label> onUpdate)
            {
                this.Title = Title;
                this.onUpdate = onUpdate;

                button = new Button();
                button.Enabled = false;
                button.Click += (s, e) =>
                {
                    onClick();
                };
                createControls(parent, button);
            }
            void createControls(Control parent, Control Title)
            {
                int h = 20;
                Title.Text = this.Title;
                Title.Size = new Size(80, h);
                Title.Location = new Point(12, Y);
                label = new Label()
                {
                    Text = "",
                    Size = new Size(200, h),
                    Location = new Point(12 + 80 + 12, Y),
                };
                Y += h + 12;
                parent.Controls.Add(Title);
                parent.Controls.Add(label);
            }
            public void Update()
            {
                onUpdate(label);
            }
        }
        ExtendedLabel[] labels;

        bool Updating = true;
        public Tobii()
        {
            InitializeComponent();

            labels = new ExtendedLabel[] {
                new ExtendedLabel(this,"Update",()=>
                {
                    Updating = !Updating;
                    for(int i = 1;i < labels.Length;i++)
                        labels[i].Enabled = !Updating;
                },
                (label)=>
                {
                    label.Text = "";
                }){
                    Enabled = true
                },
                new ExtendedLabel(this,"HeadUnit",(label)=>
                {
                    label.Text = Device.HeadUnit;
                }),
                new ExtendedLabel(this,"RecordingUnit",(label)=>
                {
                    label.Text = Device.RecordingUnit;
                }),
                new ExtendedLabel(this,"WebClient",()=>
                {
                    Device.WebClient();
                },
                (label)=>
                {
                    if(!Device.Connected)
                    {
                        label.Text = "Connection lost!";
                        label.ForeColor = Color.Red;
                    }
                    else
                    {
                        label.Text = Device.Candidates[Device.currentIP];
                        label.ForeColor = Color.Black;
                    }
                }),
                new ExtendedLabel(this,"Battery",(label)=>
                {
                    label.Text = Device.Battery + " % (" + (Device.SecondsLeft/60) + " min left)";
                    if(Device.Battery>80)
                        label.ForeColor = Color.Green;
                    else if(Device.Battery>30)
                    { 
                        label.ForeColor = Color.Orange;
                    }
                    else
                        label.ForeColor = Color.Red;
                }),
                new ExtendedLabel(this,"Tobii Time",(label)=>
                {
                    label.Text = Device.Time;
                    DateTime dt;
                    if(DateTime.TryParse(Device.Time.Replace("Z",""), out dt))
                    {
                        if(Math.Abs((DateTime.Now-dt).TotalSeconds)>5)
                        {
                            Device.SetTime(DateTime.Now);
                            label.ForeColor = Color.Red;
                        }
                        else
                            label.ForeColor = Color.Green;
                    }
                }),
                new ExtendedLabel(this,"Calibrate",()=>
                {
                    Device.Calibrate();
                },
                (label)=>
                {
                    if(Device.Calibrated == -1)
                        label.Text = "Not Calibrated";
                    else if(Device.Calibrated==0)
                        label.Text = "Calibration Failed!";
                    else
                        label.Text = "Calibration Succeeded";
                    if(Device.Calibrated == 1)
                        label.ForeColor = Color.Green;
                    else
                        label.ForeColor = Color.Red;
                }),
                new ExtendedLabel(this,"Start/Stop Recording",()=>
                {
                    if(Device.Recorder_Duration == -1)
                        Device.StartRecording();
                    else
                        Device.StopRecording();
                },
                (label)=>
                {
                    if(Device.Recorder_Duration == -1)
                    {
                        label.Text = "Not Recording";
                        label.ForeColor = Color.Red;
                         }
                    else
                    {
                        label.Text = Device.Recorder_Folder + ": " + MDOL.Extension.ToStringD(Device.Recorder_Duration,2) + " s recorded";
                        label.ForeColor = Color.Green;
                    }
                }),
                new ExtendedLabel(this,"Sync signal",
                (label)=>
                {
                    if(Device.TriggerSent == DateTime.MinValue)
                    {
                        label.Text = "No Sync signal sent";
                        label.ForeColor = Color.Red;
                    }
                    else
                    {
                        label.Text = "Sync signal sent at " + Device.TriggerSent.ToString("yyyyMMdd_hhmmss");
                        label.ForeColor = Color.Green;
                    }
                })
            };

            Timer tmrUpdater = new Timer()
            {
                Interval = 1000
            };
            tmrUpdater.Tick += (s, e) =>
            {
                if (Updating)
                    updateGUI();
            };
            tmrUpdater.Start();
        }
        DateTime lastBrowse = DateTime.MinValue;

        void Connect()
        {
            if (!Device.Connected && (DateTime.Now - lastBrowse).TotalSeconds > 10)
            {
                try
                {
                    lastBrowse = DateTime.Now;
                    Mono.Zeroconf.ServiceBrowser browser = new Mono.Zeroconf.ServiceBrowser();
                    browser.ServiceAdded += (s1, e1) =>
                    {
                        e1.Service.Resolved += (s2, e2) =>
                        {
                            string ip = e2.Service.HostEntry.AddressList[0].ToString();
                            Console.WriteLine("ServiceAdd: " + ip);
                            if (!Device.Candidates.Contains(ip))
                                Device.Candidates.Add(ip);
                        };
                        try
                        {
                            e1.Service.Resolve();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    };
                    browser.Browse("_tobii-g3api._tcp", "local");
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }

        public void Destroy()
        {

        }
        DateTime lastUpdate = DateTime.Now;
        public void UpdateGUI()
        {

        }
        public void updateGUI()
        {
            if ((DateTime.Now - lastUpdate).TotalSeconds > 1)
            {
                Device.Update();
                lastUpdate = DateTime.Now;
            }
            foreach (ExtendedLabel label in labels)
                label.Update();
        }
        public string[] IsReady()
        {
            List<string> msg = new List<string>();
            if (Device.Recorder_Duration == -1)
                msg.Add("Recording not started");
            if (Device.Calibrated != 1)
                msg.Add("Device not calibrated");
            if (Device.Battery < 30)
                msg.Add("Battery is low");
            return msg.ToArray();
        }
    }
}