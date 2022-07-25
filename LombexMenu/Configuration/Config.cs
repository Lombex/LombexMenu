using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Utils;

namespace LombexMenu.Configuration
{
    public static class StaticConfig
    {
        public static bool IKFreeze = false;
        public static bool LockInstance = false;

        public static bool GlobalEventBlock = false;

        public static Vector3 Gravity;
        public static bool Flight = false;

        public static bool PortableMirror = false;
        public static bool PortableBall = false;

        public static bool EarrapeMic;
        public static bool UseUSpeakExploit;

        public static bool Log_1_USpeak = false;
        public static bool Log_2_Executive_message = false;
        public static bool Log_3_Past_events = false;
        public static bool Log_4_Sync_events = false;
        public static bool Log_5_Sync_finished = false;
        public static bool Log_6_Process_event = false;
        public static bool Log_7_Vrc_serialization = false;
        public static bool Log_8_Frequency_request = false;
        public static bool Log_9_Vrc_reliable_serialization = false;
        public static bool Log_33_Executive_action = false;
        public static bool Log_60_Avatar_Phys_Events = false;
        public static bool Log_202_Instantiate = false;
        public static bool Log_209_Ownership_request = false;
        public static bool Log_210_Ownership_transfer = false;
        public static bool Log_223_Auth = false;
        public static bool Log_226_Stats = false;
        public static bool Log_230_Authenticate = false;
        public static bool Log_252_Set_properties = false;
        public static bool Log_253_Properties_changed = false;
        public static bool Log_254_Leave = false;
        public static bool Log_255_Join = false;
    }
    public class Config
    {
        private static Config _instance;
        public static Config Instance
        {
            get => _instance ?? (_instance = new Config());
            set { _instance = value; }
        }

        public bool OnPlayerJoinLog = true;
        public bool OnPlayerLeaveLog = true;
        public bool OnPlayerAvatarChangeLog = true;
        public bool OnPortalDropLog = true;
        public bool OnModerationEventLog = true;
        public bool OnRecievedEventsLog = true;
        public bool OnInGameNotifications = true;

        public float FieldOfView = 60f;

        public bool OnPortalEnter = false;
        public bool OnPortalFreeze = false;
        public bool AutoPortalDelete = false;
        public bool PortalConfirmation = false;
        public int PortalTime = 0;

        public float FlightSpeed = 10f;
        public float RotationSpeed = 50f;
        public bool InfiniteJump = false;

        public string PortableLightColor = "#FFFFFF";
        public float PortableLightIntensity = 10f;

        public bool AntiWarn = false;
        public bool AntiForceMicOff = false;
        public bool AntiUdon = false;

        public string ESPColor = "#FFFFFF";
        public string HUDMessage = "#FFFFFF";
        public bool PropESP = false;
        public bool CapsuleESP = false;

        public int ApplicationFrames = 144;

        public static void SaveSettings()
        {
            try { File.WriteAllText("./Mods/LombexMenu/Configuration.json", JsonConvert.SerializeObject(_instance, Formatting.Indented)); } catch (Exception e)
            {
                SetConsoleColor.WriteEmbeddedColorLine("Failed loading config file\n" + e.Message, SetConsoleColor.ConsoleLogType.Error);
            }
        }
    }
}
