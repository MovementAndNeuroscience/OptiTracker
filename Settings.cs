using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MDOL;
namespace Optitracker
{
    public static class Settings
    {
        public static bool Debug = false;
        public static string TreadmillFile = "";
        public static string RandomWalkFile = "";
        public static IO.XML MainCamera = null;
        public static double TreadmillSimulatorSpeed = 1;
        public static double SimulatorFrequency = 1;
        public static double SimulatorNoise = 0;
        public static double SimulatorRotator = 0;
        public static GUI.MoCap.MoCapSystem.BodyModel BodyModel = new GUI.MoCap.MoCapSystem.BodyModel();
        public static Treadmill.PhaseEstimator.Phase TreadmillCalibrationLeft = new Treadmill.PhaseEstimator.Phase();
        public static Treadmill.PhaseEstimator.Phase TreadmillCalibrationRight = new Treadmill.PhaseEstimator.Phase();
        public static string RetisenseL = "";
        public static string RetisenseR = "";
        public static bool RetisenseStartup = false;
        public static void Load()
        {
            if (System.IO.File.Exists("Settings.xml"))
            {
                IO.XML xml = IO.XML.Read("Settings.xml");
                Debug = xml.getBool("Debug", false);
                TreadmillFile = xml.getString("TreadmillFile", "");
                TreadmillCalibrationLeft = Treadmill.PhaseEstimator.Phase.FromJson(xml.getString("TreadmillCalibrationLeft", ""));
                TreadmillCalibrationRight = Treadmill.PhaseEstimator.Phase.FromJson(xml.getString("TreadmillCalibrationRight", ""));
                RandomWalkFile = xml.getString("RandomWalkFile", "");
                MainCamera = xml.getElement("MainCamera");
                TreadmillSimulatorSpeed = xml.getDouble("TreadmillSimulatorSpeed", 1);
                SimulatorFrequency = xml.getDouble("SimulatorFrequency", 1);
                SimulatorNoise = xml.getDouble("SimulatorNoise", 0);
                SimulatorRotator = xml.getDouble("SimulatorRotator", 0);
                BodyModel = GUI.MoCap.MoCapSystem.BodyModel.FromJson(xml.getString("BodyModel", ""));
                if (BodyModel.BodyMode == GUI.MoCap.MoCapSystem.BodyModel.Modes.Absolute)
                    BodyModel.BodyMode = GUI.MoCap.MoCapSystem.BodyModel.Modes.Relative;
                RetisenseL = xml.getString("RetisenseL", "");
                RetisenseR = xml.getString("RetisenseR", "");
                RetisenseStartup = xml.getBool("RetisenseStartup", false);
            }
        }
        public static void Save()
        {
            IO.XML settings = new IO.XML("Settings",
               new IO.XML("Debug", Debug.ToString()),
               new IO.XML("TreadmillFile", TreadmillFile),
               new IO.XML("TreadmillCalibrationLeft", TreadmillCalibrationLeft.ToJson()),
               new IO.XML("TreadmillCalibrationRight", TreadmillCalibrationRight.ToJson()),
               new IO.XML("RandomWalkFile", RandomWalkFile),
               MainCamera,
               new IO.XML("TreadmillSimulatorSpeed", TreadmillSimulatorSpeed.ToStringD()),
               new IO.XML("SimulatorFrequency", SimulatorFrequency.ToStringD()),
               new IO.XML("SimulatorNoise", SimulatorNoise.ToStringD()),
               new IO.XML("SimulatorRotator", SimulatorRotator.ToStringD()),
               new IO.XML("BodyModel",BodyModel.ToJson()),
               new IO.XML("RetisenseL",RetisenseL),
               new IO.XML("RetisenseR", RetisenseR),
               new IO.XML("RetisenseStartup", RetisenseStartup.ToString())
               );
            System.IO.File.WriteAllText("Settings.xml", settings.ToString());
        }
    }
}
