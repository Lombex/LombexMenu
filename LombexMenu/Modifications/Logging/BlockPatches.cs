using ExitGames.Client.Photon;
using Photon.Pun;
using System.Reflection;
using Utils;
using LombexMenu.Configuration;
using System.Collections.Generic;
using System.Linq;
using VRC.SDKBase;
using UnityEngine;
using Photon.Realtime;
using Newtonsoft.Json;
using System;
using HarmonyLib;
using System.Collections;
using ReMod.Core.VRChat;
using UnhollowerRuntimeLib.XrefScans;
using MelonLoader;
using VRC;
using VRC.Networking;

namespace Modifications.Logging
{
    public static class BlockPatches
    {
        private static PropertyInfo _settleStartTime;
        public static void OnBlockPatch(HarmonyLib.Harmony Instance) 
        {
            Instance.Patch(typeof(PhotonNetwork).GetMethod(nameof(PhotonNetwork.Method_Public_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0), BindingFlags.Public | BindingFlags.Static), typeof(BlockPatches).GetPatch(nameof(OnSendedEvent)), finalizer: typeof(BlockPatches).GetPatch(nameof(ExceptionHandler)));
            Instance.Patch(typeof(PortalTrigger).GetMethod(nameof(PortalTrigger.OnTriggerEnter), BindingFlags.Public | BindingFlags.Instance), typeof(BlockPatches).GetPatch(nameof(OnPortalTrigger)));
            Instance.Patch(typeof(UdonSync).GetMethod(nameof(UdonSync.UdonSyncRunProgramAsRPC), BindingFlags.Public | BindingFlags.Instance), typeof(BlockPatches).GetPatch(nameof(OnUdonSyncRPC)));
            Instance.Patch(typeof(VRC_EventDispatcherRFC).GetMethod(nameof(VRC_EventDispatcherRFC.Method_Public_Boolean_Player_VrcEvent_VrcBroadcastType_0), BindingFlags.Public | BindingFlags.Instance), typeof(BlockPatches).GetPatch(nameof(OnRPCEvent)));

            MethodInfo PortalUpdateMethod = typeof(PortalInternal).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(float) && m.XRefScanForGlobal("SetTimerRPC") && m.XRefScanForGlobal("00"));
            Instance.Patch(PortalUpdateMethod, typeof(BlockPatches).GetPatch(nameof(OnPortalUpdate)));
            IEnumerable<MethodInfo> ForceDestroy = typeof(PortalInternal).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name.StartsWith("Method") && m.GetParameters().Length == 0 && m.XRefScanForMethod("Destroy"));
            foreach (MethodInfo info in ForceDestroy) Instance.Patch(info, typeof(BlockPatches).GetPatch(nameof(OnPortalUpdate)));

            foreach (Type type in typeof(VRCFlowManagerVRC).GetNestedTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.Name != "MoveNext") continue;
                    if (XrefScanner.XrefScan(method).Any(z => z.Type == XrefType.Global && z.ReadAsObject() != null && z.ReadAsObject().ToString() == "Executing Buffered Events"))
                    {
                        _settleStartTime = type.GetProperty("field_Private_Single_0");
                        Instance.Patch(method, typeof(BlockPatches).GetMethod(nameof(AntiLockInstance), BindingFlags.Static | BindingFlags.NonPublic).ToNewHarmonyMethod());
                    }
                }
            }
        }
        private static Exception ExceptionHandler(Exception __exception)
        {
            return null;
        }
        private static void AntiLockInstance(object __instance)
        {
            if (__instance == null) return;
            VRC_EventLog.EventReplicator eventReplicator = VRC_EventLog.field_Internal_Static_VRC_EventLog_0?.field_Internal_EventReplicator_0;
            if (eventReplicator != null && !eventReplicator.field_Private_Boolean_0 && Time.realtimeSinceStartup - (float)_settleStartTime.GetValue(__instance) >= 10.0)
            {
                eventReplicator.field_Private_Boolean_0 = true;
                SetConsoleColor.WriteEmbeddedColorLine("Instance was locked or broken, joining the lobby anyways", SetConsoleColor.ConsoleLogType.Action);
            }
        }      
        private static bool OnPortalUpdate(ref PortalInternal __instance)
        {
            if (VRC.SDKBase.Networking.LocalPlayer == null || __instance.GetCreatorID() != VRC.SDKBase.Networking.LocalPlayer.playerId) return true;
            return !Config.Instance.OnPortalFreeze;
        }
        private static bool _GetPortalConfirm { get; set; }
        private static bool OnPortalTrigger(Collider __0) 
        {
            if (Config.Instance.PortalConfirmation)
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowStandardPopupV2("Portal Confirmation", "Are you sure u want to enter this portal", "Cancel", () => 
                {
                    _GetPortalConfirm = true;
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.HideCurrentPopup();
                }, "Enter!", () => 
                {
                    _GetPortalConfirm = false;
                }, null);
                return _GetPortalConfirm;
            } else
            {
                return !Config.Instance.OnPortalEnter;
            }
        }
        private static bool OnRPCEvent(VRC.Player __0, VRC_EventHandler.VrcEvent __1, VRC_EventHandler.VrcBroadcastType __2)
        {
            
            return true;
        }
        private static bool OnUdonSyncRPC(string __0, VRC.Player __1)
        {
            if (Config.Instance.AntiUdon) return false;
            else return true;
        }
        private static bool OnSendedEvent(byte __0, ref Il2CppSystem.Object __1, ref SendOptions __3)
        {
            switch (__0)
            {
                case 5:
                    if (StaticConfig.LockInstance) return false;
                    return true;              
                case 7:
                    if (StaticConfig.IKFreeze) return false;
                    return true;
                case 9:
                    if (StaticConfig.IKFreeze) return false;
                    return true;
            }
            return true;
        }
    }
}