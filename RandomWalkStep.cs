using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MDOL;

using System.Windows.Forms.DataVisualization.Charting;
namespace Optitracker
{
    public partial class RandomWalkStep : Form
    {
        public abstract class RandomWalkPacket : MDOL.IO.DataSaver.DataPacket
        {
            public class Startup : RandomWalkPacket
            {
                protected override string PacketStr => "RandomWalk;Startup";
                public Startup(string saveFile)
                {
                    AddData("saveFile",saveFile);
                }
            }

            
            public class FootStrike : RandomWalkPacket
            {
                protected override string PacketStr => "RandomWalk;FootStrike";
                public FootStrike(int ID,FormVisual.VisualMarker visualMarker)
                {
                    AddData("ID", ID);
                    AddData("Position", visualMarker);
                }
            }
            public class Frame : RandomWalkPacket
            {
                protected override string PacketStr => "RandomWalk;Frame";
                public Frame(FormVisual.VisualMarker[] LeftRight, FormVisual.VisualMarker user)
                {
                    AddData("LeftRight", LeftRight);
                    AddData("User", user);
                }
            }
            public class TargetCreated : RandomWalkPacket
            {
                protected override string PacketStr => "RandomWalk;TargetCreated";
                public TargetCreated(int TargetID, Vector<Dimensions.II> Start, double Radius, Vector<Dimensions.II> CircleStart, double CircleRadius)
                {
                    AddData("TargetID", TargetID);
                    AddData("StartX", Start.X);
                    AddData("StartY", Start.Y);
                    AddData("Radius", Radius);
                    AddData("CircleStartX", CircleStart.X);
                    AddData("CircleStartY", CircleStart.Y);
                    AddData("CircleRadius", CircleRadius);
                }
            }
            public class TargetShoot : RandomWalkPacket
            {
                protected override string PacketStr => "RandomWalk;TargetShoot";
                public TargetShoot(int TargetID,NoisyMarker user)
                {
                    AddData("TargetID", TargetID);
                    AddData("X", user.X);
                    AddData("Y", user.Y);
                    AddData("Z", user.Z);
                    AddData("TrueX", user.TrueMarker.X);
                    AddData("TrueY", user.TrueMarker.Y);
                }
            }
        }
        public class NoisyMarker : FormVisual.VisualMarker
        {
            public Vector<Dimensions.II> TrueMarker;
            public NoisyMarker(double X, double Y, double Z, Color color) : base(X, Y, Z, color)
            {
            }
        }
        public class Options
        {
            public event EventHandler OptionChanged = null;

            public int Baseradius_cm = 15;
            public int StopHeight_cm = 1;
            public double StopVel_m7s = 1;
            public bool IgnoreRadius = false;
            public int ArchSize_cm = 0;
            public int ArchSteps_degrees = 0;

            public double ShowingResult_s = 2;
            public double WaitingToStart_s = 1;
            public double TimeToHit_s = 0;

            public bool LeftLeg = false;
            public bool ShowStep = true;
            public bool ShowFeedback = true;
            public bool ShowHit = true;
            public bool ShowCircle = true;
            public int CircleDistanceMean_cm = 50;
            public int CircleDistanceStd_cm = 8;
            public int CircleAngleMean_degrees = 45;
            public int CircleAngleStd_degrees = 12;
            public int CircleRadius_cm = 25;
            public int TargetRadius_cm = 8;
            public int NumberOfSteps = 20;

            public int RandomMean_degrees = 0;
            public int RandomStd_degrees = 2;
            public int RandomWalkMean_degrees = 0;
            public int RandomWalkStd_degrees = 2;
            public int RandomWalkMax_degrees = 10;

            public double RandomWalkGain = 1;

            public bool EnableAdvancedElements = false; 

            public double TargetOn_sec = 2.0;
            public double TargetOff_sec = 4.0;
            public double GoSignalAfterTargetOn_sec = 1.0; 
            public bool TriggerGoSignal = false;
            public bool TriggerTargetOn = false;
            public bool TriggerTargetOff = false;
            public bool TriggerUserDefinedTimeBeforeTargetOn = false;
            public double TriggerUserDefinedTimeBeforeTargetOn_sec = 0;
            public bool TriggerUserDefinedTimeAfterTargetOn = false;
            public double TriggerUserDefinedTimeAfterTargetOn_sec = 0;

            public RandomWalkTarget CreateTarget()
            {
                double V = rndGaussian(CircleAngleMean_degrees, CircleAngleStd_degrees) / 180 * Math.PI;
                double D = rndGaussian(CircleDistanceMean_cm, CircleDistanceStd_cm) / 100.0;
                float X = (LeftLeg ? -1 : 1) * (float)(D * Math.Cos(V));
                float Y = (float)(D * Math.Sin(V));

                double r = rnd.Next(CircleRadius_cm - TargetRadius_cm) / 100.0;
                double v = rnd.Next(360) / 180.0 * Math.PI;
                double x = (float)(r * Math.Cos(v));
                double y = (float)(r * Math.Sin(v));
                return new RandomWalkTarget(TargetID++,
                    new Vector<Dimensions.II>(X + x, Y + y), TargetRadius_cm / 100.0,
                    new Vector<Dimensions.II>(X, Y), CircleRadius_cm / 100.0,
                    ShowCircle,
                    ArchSize_cm / 100.0, ArchSteps_degrees);
            }

            public void Start()
            {
                TargetID = 0;
                vPriming = rnd.Next(0, 360) / 180.0 * Math.PI;
                lastWalk = PointF.Empty;
            }
            public bool isDone()
            {
                bool isDone = TargetID >= NumberOfSteps;
                if (isDone)
                    FormVisual.FORMVISUAL.SetTime(0);
                else
                    FormVisual.FORMVISUAL.SetTime(TargetID * 100 / NumberOfSteps);
                return isDone;
            }

            protected int TargetID = 0;
            protected double vPriming;
            protected PointF lastWalk;
            public static T FromJson<T>(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            public string ToJson()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
            bool Ignore = false;
            void Change(System.Reflection.FieldInfo fieldInfo, object value)
            {
                if (Ignore)
                    return;
                fieldInfo.SetValue(this, value);
                OptionChanged?.Invoke(this, null);
            }
            public void SetPanel(Panel panel)
            {
                Ignore = true;
                System.Reflection.FieldInfo[] fields = this.GetType().GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    Control control = panel.Controls[i * 2 + 1];
                    if (control.GetType() == typeof(TextBox))
                        ((TextBox)control).Text = (string)fields[i].GetValue(this);
                    else if (control.GetType() == typeof(NumericUpDown))
                    {
                        if (fields[i].GetValue(this).GetType() == typeof(int))
                            ((NumericUpDown)control).Value = (int)fields[i].GetValue(this);
                        else if (fields[i].GetValue(this).GetType() == typeof(double))
                            ((NumericUpDown)control).Value = (decimal)(double)fields[i].GetValue(this);
                    }
                    else if (control.GetType() == typeof(CheckBox))
                        ((CheckBox)control).Checked = (bool)fields[i].GetValue(this);
                    else if (control.GetType() == typeof(ComboBox))
                        ((ComboBox)control).SelectedItem = fields[i].GetValue(this);
                }
                Ignore = false;
            }
            public void PopulatePanel(Panel panel)
            {
                panel.Controls.Clear();
                int h = 20;
                int hSpace = 12;
                System.Reflection.FieldInfo[] fields = GetType().GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    string strName = fields[i].Name.Replace('7', '/');
                    if (strName.Contains('_'))
                        strName = strName.Replace("_", " (") + ')';
                    panel.Controls.Add(new Label()
                    {
                        Size = new Size(panel.Width / 3, h),
                        Location = new Point(12, (h + hSpace) * i + 12),
                        Text = strName
                    });
                    object value = fields[i].GetValue(this);
                    Control control = null;
                    if (value.GetType() == typeof(string))
                    {
                        TextBox txt = new TextBox();
                        txt.TextChanged += (nud_s, nud_e) =>
                        {
                            Change((System.Reflection.FieldInfo)txt.Tag, txt.Text);
                        };
                        control = txt;
                    }
                    else if (value.GetType() == typeof(int))
                    {
                        NumericUpDown nud = new NumericUpDown()
                        {
                            Minimum = -200,
                            Maximum = 200,
                            Value = (int)value,
                        };
                        nud.MouseWheel += (nud_s, nud_e) =>
                        {
                            ((HandledMouseEventArgs)nud_e).Handled = true;
                        };
                        nud.ValueChanged += (nud_s, nud_e) =>
                        {
                            Change((System.Reflection.FieldInfo)nud.Tag, (int)nud.Value);
                        };
                        control = nud;
                    }
                    else if (value.GetType() == typeof(double))
                    {
                        NumericUpDown nud = new NumericUpDown()
                        {
                            Minimum = -200,
                            Maximum = 200,
                            Value = (decimal)(double)value,
                            Increment = (decimal)0.01,
                            DecimalPlaces = 2
                        };
                        nud.MouseWheel += (nud_s, nud_e) =>
                        {
                            ((HandledMouseEventArgs)nud_e).Handled = true;
                        };
                        nud.ValueChanged += (nud_s, nud_e) =>
                        {
                            Change((System.Reflection.FieldInfo)nud.Tag, (double)nud.Value);
                        };
                        control = nud;
                    }
                    else if (value.GetType() == typeof(bool))
                    {
                        CheckBox chk = new CheckBox()
                        {
                            Checked = (bool)value,
                        };
                        chk.CheckedChanged += (chk_s, chk_e) =>
                        {
                            Change((System.Reflection.FieldInfo)chk.Tag, chk.Checked);
                        };
                        control = chk;
                    }
                    else if (value.GetType() == typeof(Color))
                    {
                        ComboBox cmb = new ComboBox();
                        cmb.Items.Add(Color.Red);
                        cmb.Items.Add(Color.Green);
                        cmb.Items.Add(Color.Blue);
                        cmb.Items.Add(Color.Magenta);
                        cmb.Items.Add(Color.Cyan);
                        cmb.Items.Add(Color.Yellow);
                        cmb.Items.Add(Color.Transparent);
                        cmb.Items.Add(Color.Black);
                        cmb.SelectedItem = (Color)value;
                        cmb.SelectedIndexChanged += (chk_s, chk_e) =>
                        {
                            Change((System.Reflection.FieldInfo)cmb.Tag, cmb.SelectedItem);
                        };
                        cmb.MouseWheel += (chk_s, chk_e) =>
                        {
                            ((HandledMouseEventArgs)chk_e).Handled = true;
                        };
                        control = cmb;
                    }
                    else if (value.GetType() == typeof(Modes))
                    {
                        ComboBox cmb = new ComboBox();
                        foreach (Modes mode in Enum.GetValues(typeof(Modes)))
                            cmb.Items.Add(mode);
                        cmb.SelectedItem = (Modes)value;
                        cmb.SelectedIndexChanged += (chk_s, chk_e) =>
                        {
                            Change((System.Reflection.FieldInfo)cmb.Tag, cmb.SelectedItem);
                        };
                        control = cmb;
                    }
                    else
                        throw new NotImplementedException();
                    if (control != null)
                    {
                        control.Tag = fields[i];
                        control.Size = new Size(panel.Width / 3, h);
                        control.Location = new Point((int)(12 + panel.Width / 3 * 1.5), (h + hSpace) * i + 12);
                        panel.Controls.Add(control);
                    }
                }
            }
        }
        NoisyMarker user = new NoisyMarker(0,0,0,Color.Blue);
        enum Modes { None,WaitingToStart,WaitingForShot,ShowingResult,StepBack,
        EmptyStart, TargetOn, TargetOff, SendTrigger, GoSignal};
        Modes Mode = Modes.None;
        DateTime TargetShown;
        public RandomWalkStep()
        {
            InitializeComponent();

            new MDOL.FormResizer(this, pnlOptions);

            GUI.MoCap.moCapSystem.FrameReceived += MoCapSystem_FrameReceived;

            options.PopulatePanel(pnlOptions);
            options.OptionChanged += (s, e) =>
              {
                  System.IO.File.WriteAllText(Settings.RandomWalkFile, saveFile());
                  serieBase.MarkerSize = (int)(options.Baseradius_cm / 100.0 * 2 * FormVisual.FORMVISUAL.meter2unit);
              };
            serieBase = new Series()
            {
                ChartType = SeriesChartType.Point,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = (int)(options.Baseradius_cm / 100.0 * 2 * FormVisual.FORMVISUAL.meter2unit),
                BorderWidth = 2,
                Color = Color.Transparent,
                BorderColor = Color.Blue
            };
            serieBase.Points.AddXY(double.Epsilon, double.Epsilon);
            FormVisual.FORMVISUAL.AddSeries(serieBase);
            FormVisual.FORMVISUAL.ZoomChanged += (s, e) =>
              {
                  serieBase.MarkerSize = (int)(options.Baseradius_cm / 100.0 * 2 * FormVisual.FORMVISUAL.meter2unit);
              };
            Timer tmr = new Timer() { Interval = 30 };
            FormVisual.VisualMarker prev = null;
            long prevUserTime = 0;
            DateTime prevTime = new DateTime();
            bool goSignalSend = false;
            bool enableTargetOff = false;
            bool enableGoSignal = false;
            bool enableTriggerAfterOnset = false;
            System.Media.SoundPlayer SPbeep = new System.Media.SoundPlayer(Properties.Resources.beep);
            tmr.Tick += (s, e) =>
              {
                  if (!options.EnableAdvancedElements)
                  {
                      switch (Mode)
                      {
                          case Modes.WaitingToStart:
                              if (sw.ElapsedMilliseconds >= (options.WaitingToStart_s * 1000))
                              {
                                  TargetOn();

                                  Mode = Modes.WaitingForShot;
                                  GUI.Trigger.SendTrigger();
                                  Timer tmrBip = new Timer() { Interval = 1000 };
                                  tmrBip.Tick += (tmr_s, tmr_e) =>
                                    {
                                        SPbeep.Play();
                                        tmrBip.Stop();
                                    };
                                  tmrBip.Start();
                              }
                              break;
                          case Modes.WaitingForShot:
                              FormVisual.FORMVISUAL.DrawUser(user);
                              if (prevUserTime != UserTime)
                              {
                                  prevUserTime = UserTime;
                                  if (user.Length > (options.Baseradius_cm / 100.0) && user.Z <= options.StopHeight_cm / 100.0 && prev != null)
                                  {
                                      double velocity = Math.Sqrt((user.X - prev.X).Sqr() + (user.Y - prev.Y).Sqr()) / (DateTime.Now - prevTime).TotalSeconds;
                                      if (velocity <= options.StopVel_m7s)
                                      {
                                          target.shoot(user, options.ShowHit, options.ShowFeedback, options.IgnoreRadius);
                                          if (options.TimeToHit_s > 0 && (DateTime.Now - TargetShown).TotalSeconds > options.TimeToHit_s)
                                              FormVisual.FORMVISUAL.Alert();
                                          Mode = Modes.ShowingResult;
                                          sw.Restart();
                                      }
                                  }
                                  prev = new FormVisual.VisualMarker(user.X, user.Y, user.Z, Color.Blue);
                                  prevTime = DateTime.Now;
                              }
                              break;
                          case Modes.ShowingResult:
                              if (options.ShowFeedback)
                                  user.Color = Color.Blue;
                              if (sw.ElapsedMilliseconds >= (options.ShowingResult_s * 1000))
                              {
                                  CleanUp();
                                  Mode = Modes.StepBack;
                              }
                              break;
                          case Modes.StepBack:
                              if (prevUserTime != UserTime)
                              {
                                  if (!options.ShowStep)
                                  {
                                      if (user.Length > (options.Baseradius_cm / 100.0) && user.Length < (options.Baseradius_cm / 100.0))
                                          user.Color = Color.Blue;
                                      else
                                          user.Color = Color.Transparent;
                                  }
                                  prevUserTime = UserTime;
                                  if (user.Length <= (options.Baseradius_cm / 100.0) && user.Z <= options.StopHeight_cm / 100.0 && prev != null)
                                  {
                                      double velocity = Math.Sqrt((user.X - prev.X).Sqr() + (user.Y - prev.Y).Sqr()) / (DateTime.Now - prevTime).TotalSeconds;
                                      if (velocity <= options.StopVel_m7s)
                                      {
                                          Mode = Modes.None;
                                          if (options.isDone())
                                              cmdStart_Click(null, null);
                                          else
                                              Start();
                                      }
                                  }
                                  prev = new FormVisual.VisualMarker(user.X, user.Y, user.Z, Color.Blue);
                                  prevTime = DateTime.Now;
                              }
                              break;
                      }
                  }
                  else if(options.EnableAdvancedElements)
                  {
                      switch (Mode)
                      {
                          case Modes.EmptyStart:
                              if(options.TriggerUserDefinedTimeBeforeTargetOn)
                              {
                                  if(sw.ElapsedMilliseconds >= ((options.TargetOn_sec - options.TriggerUserDefinedTimeBeforeTargetOn_sec) * 1000))
                                  {
                                      GUI.Trigger.SendTrigger();
                                      Mode = Modes.TargetOn;
                                      Console.Out.WriteLine(sw.ElapsedMilliseconds + " ms Trigger before Taraget on"); 
                                  }
                              }
                              else if(!options.TriggerUserDefinedTimeBeforeTargetOn)
                              {
                                  Mode = Modes.TargetOn;
                              }
                               break; 

                          case Modes.TargetOn:
                              if (sw.ElapsedMilliseconds >= (options.TargetOn_sec * 1000))
                              {
                                  TargetOn();

                                  if (options.TriggerTargetOn)
                                  {
                                      GUI.Trigger.SendTrigger();
                                      Console.Out.WriteLine(sw.ElapsedMilliseconds + " ms Trigger Taraget on");
                                  }
                                  enableTargetOff = true;
                                  enableGoSignal = true;
                                  if(options.TriggerUserDefinedTimeAfterTargetOn)
                                      enableTriggerAfterOnset = true; 
                                  Mode = Modes.TargetOff;
                              }
                              break;

                          case Modes.TargetOff:
                                  if (sw.ElapsedMilliseconds >= ((options.TriggerUserDefinedTimeAfterTargetOn_sec + options.TargetOn_sec) * 1000) && options.TriggerUserDefinedTimeAfterTargetOn && enableTriggerAfterOnset)
                                  {
                                      GUI.Trigger.SendTrigger();
                                  Console.Out.WriteLine(sw.ElapsedMilliseconds + " ms Trigger after Taraget on");
                                  enableTriggerAfterOnset = false; 
                                  }
                              
                              if (sw.ElapsedMilliseconds >= ((options.TargetOff_sec + options.TargetOn_sec) * 1000) && enableTargetOff)
                                  {
                                      TargetOff();

                                        if (options.TriggerTargetOff)
                                        {
                                          GUI.Trigger.SendTrigger();
                                          Console.Out.WriteLine(sw.ElapsedMilliseconds + " ms Trigger Target Off");
                                        }
                                  enableTargetOff = false;
                                  }
                              
                              if (enableGoSignal)
                                  {
                                      Timer tmrBip = new Timer() { Interval = Convert.ToInt32(options.GoSignalAfterTargetOn_sec * 1000) };
                                  tmrBip.Tick += (tmr_s, tmr_e) =>
                                      {
                                          SPbeep.Play();
                                          Console.Out.WriteLine(sw.ElapsedMilliseconds + " ms Go Signal is playing ");
                                        if (options.TriggerGoSignal)
                                           {
                                               GUI.Trigger.SendTrigger();
                                               Console.Out.WriteLine(sw.ElapsedMilliseconds + " ms Triggeron go signal ");
                                           }
                                          tmrBip.Stop();
                                      };  
                                  
                                  tmrBip.Start();
                                      goSignalSend = true;
                                      enableGoSignal = false;
                                  }

                              if (goSignalSend && !enableTargetOff && !enableGoSignal && !enableTriggerAfterOnset)
                              {
                                  Mode = Modes.WaitingForShot;
                                  goSignalSend = false;
                              }
                              break;
                          case Modes.WaitingForShot:
                              FormVisual.FORMVISUAL.DrawUser(user);
                              if (prevUserTime != UserTime)
                              {
                                  prevUserTime = UserTime;
                                  if (user.Length > (options.Baseradius_cm / 100.0) && user.Z <= options.StopHeight_cm / 100.0 && prev != null)
                                  {
                                      double velocity = Math.Sqrt((user.X - prev.X).Sqr() + (user.Y - prev.Y).Sqr()) / (DateTime.Now - prevTime).TotalSeconds;
                                      if (velocity <= options.StopVel_m7s)
                                      {
                                          target.shoot(user, options.ShowHit, options.ShowFeedback, options.IgnoreRadius);
                                          if (options.TimeToHit_s > 0 && (DateTime.Now - TargetShown).TotalSeconds > options.TimeToHit_s)
                                              FormVisual.FORMVISUAL.Alert();
                                          Mode = Modes.ShowingResult;
                                          sw.Restart();
                                      }
                                  }
                                  prev = new FormVisual.VisualMarker(user.X, user.Y, user.Z, Color.Blue);
                                  prevTime = DateTime.Now;
                              }
                              break;

                          case Modes.ShowingResult:
                              if (options.ShowFeedback)
                                  user.Color = Color.Blue;
                              if (sw.ElapsedMilliseconds >= (options.ShowingResult_s * 1000))
                              {
                                  CleanUp();
                                  Mode = Modes.StepBack;
                              }
                              break;
                          case Modes.StepBack:

                              if (prevUserTime != UserTime)
                              {
                                  if (!options.ShowStep)
                                  {
                                      if (user.Length > (options.Baseradius_cm / 100.0) && user.Length < (options.Baseradius_cm / 100.0))
                                          user.Color = Color.Blue;
                                      else
                                          user.Color = Color.Transparent;
                                  }
                                  prevUserTime = UserTime;
                                  if (user.Length <= (options.Baseradius_cm / 100.0) && user.Z <= options.StopHeight_cm / 100.0 && prev != null)
                                  {
                                      double velocity = Math.Sqrt((user.X - prev.X).Sqr() + (user.Y - prev.Y).Sqr()) / (DateTime.Now - prevTime).TotalSeconds;
                                      if (velocity <= options.StopVel_m7s)
                                      {
                                          Mode = Modes.None;
                                          if (options.isDone())
                                              cmdStart_Click(null, null);
                                          else
                                              Start();
                                      }
                                  }
                                  prev = new FormVisual.VisualMarker(user.X, user.Y, user.Z, Color.Blue);
                                  prevTime = DateTime.Now;
                              }
                              break;
                      }

                  }


                  FormVisual.FORMVISUAL.DrawUser(user);
              };
            tmr.Start();
        }

        private void TargetOn()
        {
            target = options.CreateTarget();
            if (options.ShowFeedback)
                if (options.ShowStep)
                    user.Color = Color.Blue;
                else
                    user.Color = Color.Transparent;
            TargetShown = DateTime.Now;
        }

        private void TargetOff()
        {
            target.MakeTransparent(); 
        }

        long UserTime = 0;
        private void MoCapSystem_FrameReceived(object sender, GUI.MoCap.MoCapSystem.Frame e)
        {
            FormVisual.VisualMarker[] LeftRight = ((GUI.MoCap.MoCapSystem)sender).StepPrepare(e);
            FormVisual.VisualMarker current = LeftRight[options.LeftLeg ? 0 : 1];
            UserTime = e.Timestamp;
            user.TrueMarker = new Vector<Dimensions.II>(current.X, current.Y);
            user.X = current.X;
            user.Y = current.Y;
            user.Z = current.Z;
            MDOL.IO.DataSaver.AddPacket(new RandomWalkPacket.Frame(LeftRight,user));
        }

        readonly System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        Series serieBase = null;
        static Random rnd = new Random(DateTime.Now.Millisecond);
        RandomWalkTarget target = null;
        bool Started = false;
        private void cmdStart_Click(object sender, EventArgs e)
        {
            Started = !Started;
            cmdStart.BackColor = Started ? Color.Red : Color.Lime;
            cmdStart.Text = Started ? "Stop" : "Start";
            if (Started)
            {
                if (!options.ShowFeedback)
                {
                    user.Color = Color.Transparent;
                    FormVisual.FORMVISUAL.HideScore();
                }
                else
                {
                    user.Color = Color.Blue;
                    FormVisual.FORMVISUAL.ShowScore();
                }

                options.Start();

                string filename = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                filename += txtPostfix.Text == "" ? "" : "_" + txtPostfix.Text;
                if (!System.IO.Directory.Exists("Recordings"))
                    System.IO.Directory.CreateDirectory("Recordings");
                MDOL.IO.DataSaver.NewFile("Recordings/" + filename + ".bin", true);
                MDOL.IO.DataSaver.AddPacket(new FormVisual.VisualPacket.Startup(FormVisual.FORMVISUAL));
                MDOL.IO.DataSaver.AddPacket(new RandomWalkPacket.Startup(saveFile()));
                if (GUI.MoCap.moCapSystem != null)
                    MDOL.IO.DataSaver.AddPacket(new GUI.MoCap.MoCapPacket.Info(GUI.MoCap.moCapSystem));
                MDOL.IO.DataSaver.AddPacket(new GUI.Tobii.TobiiPacket());

                FormVisual.FORMVISUAL.ClearScore();

                Start();
            }
            else
            {
                MDOL.IO.DataSaver.Close();
                CleanUp();
            }
        }
        void Start()
        {
            if (options.ShowFeedback)
                if (options.ShowStep)
                    user.Color = Color.Blue;
                else
                    user.Color = Color.Transparent;
            sw.Restart();
            if (options.EnableAdvancedElements)
            {
                Mode = Modes.EmptyStart;
            }
            else if(!options.EnableAdvancedElements)
            {
                Mode = Modes.WaitingToStart;
            }
        }
        void CleanUp()
        {
            Mode = Modes.None;
            sw.Reset();
            if (target != null)
                target.Destroy();
        }

        string saveFile()
        {
            return new MDOL.IO.XML("RandomWalk", new MDOL.IO.XML("Options", options.ToJson())).ToString();
        }
        Options options = new Options();

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                InitialDirectory = Application.StartupPath,
                Filter = "Random Walk Settings (*.rndwlk)|*.rndwlk",
                Title = "Choose file for saving RandomWalk-settings"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Settings.RandomWalkFile = sfd.FileName;
                Text = Settings.RandomWalkFile;
                Settings.Save();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                InitialDirectory = Application.StartupPath,
                Filter = "Random Walk Settings (*.rndwlk)|*.rndwlk",
                Title = "Choose file containing RandomWalk-settings"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Settings.RandomWalkFile = ofd.FileName;
                Settings.Save();
            }
            LoadFile();
        }

        void LoadFile()
        {
            Text = Settings.RandomWalkFile;
            MDOL.IO.XML settings = MDOL.IO.XML.Read(Settings.RandomWalkFile);

            string json = settings.getString("Options", "");
            if (json != "")
            {
                options = Options.FromJson<Options>(json);
                serieBase.MarkerSize = (int)(options.Baseradius_cm / 100.0 * 2 * FormVisual.FORMVISUAL.meter2unit);
                options.PopulatePanel(pnlOptions);
                options.OptionChanged += (s, e) =>
                {
                    System.IO.File.WriteAllText(Settings.RandomWalkFile, saveFile());
                    serieBase.MarkerSize = (int)(options.Baseradius_cm / 100.0 * 2 * FormVisual.FORMVISUAL.meter2unit);
                };
                options.SetPanel(pnlOptions);
            }
        }

        private void RandomWalkStep_Load(object sender, EventArgs e)
        {
            Bounds = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height);
            if (Settings.RandomWalkFile == "" || !System.IO.File.Exists(Settings.RandomWalkFile))
                newToolStripMenuItem_Click(null, null);
            else
                LoadFile();
        }

        static double rndGaussian(double mean, double std)
        {
            double u1 = 1.0 - rnd.NextDouble();
            double u2 = 1.0 - rnd.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2);
            return mean + std * randStdNormal;
        }

        public class RandomWalkTarget
        {
            readonly System.Media.SoundPlayer SP1 = new System.Media.SoundPlayer(Properties.Resources.hit);
            readonly System.Media.SoundPlayer SP3 = new System.Media.SoundPlayer(Properties.Resources.hit3);
            public int TargetID;
            Series[] series;
            Vector<Dimensions.II> Start;
            Vector<Dimensions.II> CircleStart;
            double Radius;

            public double CircleDist(FormVisual.VisualMarker user)
            {
                return (user - CircleStart).Length;
            }
            public RandomWalkTarget(int TargetID, Vector<Dimensions.II> Start, double Radius, Vector<Dimensions.II> CircleStart, double CircleRadius, bool ShowCircle, double ArchSize, int ArchSteps)
            {
                this.CircleStart = CircleStart;
                this.TargetID = TargetID;
                MDOL.IO.DataSaver.AddPacket(new RandomWalkPacket.TargetCreated(TargetID, Start, Radius, CircleStart, CircleRadius));
                this.Start = Start;
                this.Radius = Radius;
                series = new Series[] {
                    new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        MarkerStyle = MarkerStyle.Circle,
                        MarkerSize = (int)(CircleRadius*2*FormVisual.FORMVISUAL.meter2unit),
                        Color = ShowCircle?Color.FromArgb(100,200,200,200):Color.Transparent,
                        BorderColor = Color.Transparent,
                        BorderWidth = 2
                    },
                    new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        BorderColor = Color.Red,
                        Color = Color.White,
                        MarkerStyle = MarkerStyle.Circle,
                        BorderWidth = 2,
                        MarkerSize = (int)(Radius*2*FormVisual.FORMVISUAL.meter2unit),
                    },new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        BorderColor = Color.Red,
                        Color = Color.White,
                        MarkerStyle = MarkerStyle.Circle,
                        BorderWidth = 2,
                        MarkerSize = (int)(Radius/2*2*FormVisual.FORMVISUAL.meter2unit),
                    },new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        Color = Color.Transparent,
                        MarkerSize = 10,
                        MarkerStyle = MarkerStyle.Cross,
                    },new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        BorderColor = ArchSize == 0 ? Color.Transparent : Color.Gray,
                        Color = Color.Transparent,
                        MarkerStyle = MarkerStyle.Circle,
                        BorderWidth = 2,
                        MarkerSize = (int)(ArchSize/2*2*FormVisual.FORMVISUAL.meter2unit)
                    }
                };
                series[0].Points.AddXY(CircleStart.X, CircleStart.Y);
                series[1].Points.AddXY(Start.X, Start.Y);
                series[2].Points.AddXY(Start.X, Start.Y);

                if (ArchSize != 0 && ArchSteps != 0)
                {
                    series[4].Points.AddXY(Start.X, Start.Y);
                    double vStart = Math.Atan2(Start.Y, Start.X);
                    double lStart = Start.Length;
                    int vLeft = (int)(vStart / Math.PI * 180)+ArchSteps;
                    while (vLeft < 180)
                    {
                        series[4].Points.AddXY(Math.Cos(vLeft / 180.0 * Math.PI) * lStart, Math.Sin(vLeft / 180.0 * Math.PI) * lStart);
                        vLeft += ArchSteps;
                    }
                    int vRight = (int)(vStart / Math.PI * 180) - ArchSteps;
                    while (vRight > 0)
                    {
                        series[4].Points.AddXY(Math.Cos(vRight / 180.0 * Math.PI) * lStart, Math.Sin(vRight / 180.0 * Math.PI) * lStart);
                        vRight -= ArchSteps;
                    }
                }

                FormVisual.FORMVISUAL.AddSeries(series);
            }

            public void shoot(NoisyMarker user,bool ShowHit,bool ShowFeedback,bool IgnoreRadius)
            {
                MDOL.IO.DataSaver.AddPacket(new RandomWalkPacket.TargetShoot(TargetID, user));
                if(IgnoreRadius)
                {
                    double u = user.Length;
                    double t = Start.Length;
                    user.X *= t / u;
                    user.Y *= t / u;
                }
                if (ShowHit)
                {
                    series[3].Points.AddXY(user.X, user.Y);
                    series[3].Color = Color.Black;
                }
                if (!ShowFeedback)
                    return;
                double dist = Math.Sqrt((user.X - Start.X).Sqr() + (user.Y - Start.Y).Sqr());
                if (dist <= Radius)
                {
                    if (dist > Radius/2)
                    {
                        FormVisual.FORMVISUAL.Score += 1;
                        series[1].Color = Color.Red;
                        SP1.Play();
                    }
                    else
                    {
                        FormVisual.FORMVISUAL.Score += 3;
                        series[2].Color = Color.Lime;
                        SP3.Play();
                    }
                }
                FormVisual.FORMVISUAL.MaxScore += 3;
                FormVisual.FORMVISUAL.UpdateScore(true);
            }
            public void Destroy()
            {
                FormVisual.FORMVISUAL.RemoveSeries(series);
            }
            public void MakeTransparent()
            {
                foreach( var serie in series)
                {
                    serie.Color = Color.Transparent;
                    serie.BorderColor = Color.Transparent;
                }
            }
        }
        private void RandomWalkStep_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormVisual.FORMVISUAL.RemoveSeries(serieBase);
            MDOL.IO.DataSaver.Close();
            CleanUp();
        }
    }
}


