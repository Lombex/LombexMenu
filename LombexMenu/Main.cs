using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Utils;
using Modifications.Logging;
using Controls;
using Modifications.Movement;
using LombexMenu.Configuration;
using Modifications.Misc;
using Modifications.Visuals;
using UnityEngine;
using System.Diagnostics;
using Protection;
using UI;
using LombexMenu.Networking.Exploits;
using System.IO;
using Newtonsoft.Json;

namespace LombexMenu
{
    public class Main
    {
        public static HarmonyLib.Harmony Harmony = new HarmonyLib.Harmony("LombexClient");
        public static void OnApplicationStart()
        {
            Console.Clear();
            if (File.Exists("./Mods/LombexMenu/Configuration.json"))
            {
                try
                {
                    Config.Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./Mods/LombexMenu/Configuration.json"));
                } catch (Exception e)
                {
                    File.Delete("./Mods/LombexMenu/Configuration.json");
                    SetConsoleColor.WriteEmbeddedColorLine("Failed to load Configuration file\n" + e.Message, SetConsoleColor.ConsoleLogType.Error);
                }
            }
            SetEventLogic.InitializeEventLogic();
            LogPatches.SetLogPatch(Harmony);
            BlockPatches.OnBlockPatch(Harmony);
            ThirdPerson.Start();
            Customize_QuickMenu.Start();
            SetConsoleColor.WriteEmbeddedColorLine("LombexMenu Has Been Loaded Successfully", SetConsoleColor.ConsoleLogType.Info);
        }
        public static void OnUpdate()
        {
            KeyBindings.OnKeyPress();
            FOVChanger.Update();
            if (StaticConfig.Flight) Flight.OnFlightMode();
        }
        public static void OnGUI()
        {

        }
        public static void OnFixedUpdate()
        {

        }
        public static void OnLevelWasLoaded(int level)
        {
            Rotation.AlineTrackingToPlayer = null;
            if (PortableMirror.GetMirror != null) GameObject.Destroy(PortableMirror.GetMirror);
            if (PortableBall.GetPortableBall != null) GameObject.Destroy(PortableBall.GetPortableBall);
            StaticConfig.Flight = false;
            StaticConfig.IKFreeze = false;
            StaticConfig.LockInstance = false;
            if (level == -1)
            {
                PortableMirror.Start();
                PortableBall.Start();
                PortableLightSource.SetupPortableLightSource(false, Config.Instance.PortableLightColor, Config.Instance.PortableLightIntensity);
            }
        }
        public static void OnApplicationQuit()
        {
            Config.SaveSettings();
            Process.GetCurrentProcess().Kill();
        }
    }
}
