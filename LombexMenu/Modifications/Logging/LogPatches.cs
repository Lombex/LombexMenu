using System.Reflection;
using Utils;
using VRC.Core;
using System;
using ExitGames.Client.Photon;
using LombexMenu.Configuration;
using VRC;
using System.Collections.Generic;
using MelonLoader;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using VRC.SDKBase;
using Photon.Pun;
using Newtonsoft.Json.Linq;
using Modifications.Visuals;
using ReMod.Core.VRChat;
using System.Text;


namespace Modifications.Logging
{
    public static class LogPatches
    {
        public static void SetLogPatch(HarmonyLib.Harmony Instance)
        {
            Instance.Patch(typeof(NetworkManager).GetMethod(nameof(NetworkManager.Method_Public_Void_Player_1), BindingFlags.Public | BindingFlags.Instance), typeof(LogPatches).GetPatch(nameof(OnPlayerJoin)));
            Instance.Patch(typeof(NetworkManager).GetMethod(nameof(NetworkManager.Method_Public_Void_Player_0), BindingFlags.Public | BindingFlags.Instance), typeof(LogPatches).GetPatch(nameof(OnPlayerLeave)));
            Instance.Patch(typeof(PipelineManager).GetMethod(nameof(PipelineManager.Start), BindingFlags.Public | BindingFlags.Instance), typeof(LogPatches).GetPatch(nameof(OnAvatarChange)));
            Instance.Patch(typeof(RoomManager).GetMethod(nameof(RoomManager.Method_Public_Static_Void_Int32_PortalInternal_1), BindingFlags.Public | BindingFlags.Static), typeof(LogPatches).GetPatch(nameof(OnPortalDrop)));
            Instance.Patch(typeof(PortalInternal).GetMethod(nameof(PortalInternal.Method_Private_Void_3), BindingFlags.Public | BindingFlags.Instance), finalizer: typeof(LogPatches).GetPatch(nameof(ExceptionHandler)));
            Instance.Patch(typeof(Photon.Realtime.LoadBalancingClient).GetMethod(nameof(Photon.Realtime.LoadBalancingClient.OnEvent)), typeof(LogPatches).GetPatch(nameof(OnModerationEvent)), finalizer: typeof(LogPatches).GetPatch(nameof(ExceptionHandler)));
        }
        private static Exception ExceptionHandler(Exception __exception)
        {
            return null;
        }
        private static void OnPlayerJoin(ref Player __0)
        {
            if (__0 == null || __0.field_Private_APIUser_0.IsLocalUser()) return;            
            if (Config.Instance.CapsuleESP) ESP.CapsuleESP();
            if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{__0.field_Private_APIUser_0.displayName} joined!", Config.Instance.HUDMessage);
            if (Config.Instance.OnPlayerJoinLog) if (!__0.field_Private_APIUser_0.hasModerationPowers || !__0.field_Private_APIUser_0.hasSuperPowers)
            {
                if (__0.field_Private_APIUser_0.isFriend) SetConsoleColor.WriteEmbeddedColorLine($"[ [darkyellow]+[/darkyellow] ] {__0.field_Private_APIUser_0.displayName} [[darkgray]{__0.field_Private_APIUser_0.id}[/darkgray]]", SetConsoleColor.ConsoleLogType.Info);
                else SetConsoleColor.WriteEmbeddedColorLine($"[ [green]+[/green] ] {__0.field_Private_APIUser_0.displayName} [[darkgray]{__0.field_Private_APIUser_0.id}[/darkgray]]", SetConsoleColor.ConsoleLogType.Info);
            }
            if (__0.field_Private_APIUser_0.hasModerationPowers || __0.field_Private_APIUser_0.hasSuperPowers) SetConsoleColor.WriteEmbeddedColorLine($"[ [Magenta]+[/Magenta] ] A Moderator Has Joined Your Instance [red]{__0.field_Private_APIUser_0.displayName}[/red] [[darkgray]{__0.field_Private_APIUser_0.id}[/darkgray]]", SetConsoleColor.ConsoleLogType.Warning);
        }
        private static void OnPlayerLeave(ref Player __0)
        {
            if (__0 == null || __0.field_Private_APIUser_0 == null) return;
            if (__0.field_Private_APIUser_0.IsLocalUser()) return;
            if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{__0.field_Private_APIUser_0.displayName} left!", Config.Instance.HUDMessage);
            if (Config.Instance.OnPlayerLeaveLog) SetConsoleColor.WriteEmbeddedColorLine($"[ [red]-[/red] ] {__0.field_Private_APIUser_0.displayName} [[darkgray]{__0.field_Private_APIUser_0.id}[/darkgray]]", SetConsoleColor.ConsoleLogType.Info);
        }

        private static readonly Dictionary<string, string> AvatarDupeCheck = new Dictionary<string, string>();
        private static void OnAvatarChange(ref PipelineManager __instance)
        {
            if (!Config.Instance.OnPlayerAvatarChangeLog) return;
            if (__instance.GetComponentInParent<Player>() == null) return;
            Player player = __instance.GetComponentInParent<Player>();
            if (player.prop_ApiAvatar_0 == null || player.field_Private_APIUser_0.IsSelf) return;
            if (!AvatarDupeCheck.ContainsKey(player.prop_APIUser_0.id)) AvatarDupeCheck.Add(player.prop_APIUser_0.id, string.Empty);
            if (AvatarDupeCheck[player.prop_APIUser_0.id] != player.prop_ApiAvatar_0.id)
            {
                AvatarDupeCheck[player.prop_APIUser_0.id] = player.prop_ApiAvatar_0.id;
                SetConsoleColor.WriteEmbeddedColorLine($"{player.field_Private_APIUser_0.displayName} [darkyellow]>[/darkyellow] {player.prop_ApiAvatar_0.name} [[darkgray]{player.prop_ApiAvatar_0.id}[/darkgray]]", SetConsoleColor.ConsoleLogType.Info);
            }
        }
        private static IEnumerator WaitOnPortalDrop(int __0, PortalInternal __1, Player __2)
        {
            while (ReferenceEquals(__1.field_Private_ApiWorld_0, null)) yield return new WaitForEndOfFrame();
            if (Config.Instance.AutoPortalDelete) Utils.Utilities.DeleteAllPortals();
            if (__2.field_Private_APIUser_0.IsLocalUser()) __1.field_Private_Single_1 = -Config.Instance.PortalTime;
            ApiWorld GetApiWorld = __1.field_Private_ApiWorld_0;
            SetConsoleColor.WriteEmbeddedColorLine($"{__2.GetAPIUser().displayName} dropped a portal to '[darkyellow]{GetApiWorld.name}[/darkyellow]' [X:[darkyellow]{(int)__1.transform.position.x}[/darkyellow] Y:[darkyellow]{(int)__1.transform.position.y}[/darkyellow] Z:[darkyellow]{(int)__1.transform.position.z}[/darkyellow]]\n[[darkgray]{GetApiWorld.id}:{__1.field_Private_String_4}[/darkgray]]", SetConsoleColor.ConsoleLogType.Info);
        }
        private static void OnPortalDrop(int __0, PortalInternal __1)
        {
            if (!Config.Instance.OnPortalDropLog) return;
            Player player = PlayerUtils.GetPlayerByInstigatorID(__1.GetCreatorID());
            if (player != null) MelonCoroutines.Start(LogPatches.WaitOnPortalDrop(__0, __1, player));
        }

        private static Dictionary<int, bool> ModerationBlockState = new Dictionary<int, bool>();
        private static Dictionary<int, bool> ModerationMuteState = new Dictionary<int, bool>();
        private static IEnumerator ModerationStates(int ActorID, bool BlockState, bool MuteState)
        {
            while (true)
            {
                Photon.Realtime.Player _GetPlayer = PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(ActorID);
                if (_GetPlayer.field_Public_Player_0 != null && _GetPlayer.field_Public_Player_0.field_Private_APIUser_0 != null)
                {
                    if (BlockState && !ModerationBlockState[ActorID])
                    {
                        if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} Has Blocked U!", Config.Instance.HUDMessage);
                        SetConsoleColor.WriteEmbeddedColorLine($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} [{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.id}] Has [red]Blocked[/red] u!", SetConsoleColor.ConsoleLogType.Warning);
                    }              
                    if (!BlockState && ModerationBlockState[ActorID])
                    {
                        if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} Has Unblocked U!", Config.Instance.HUDMessage);
                        SetConsoleColor.WriteEmbeddedColorLine($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} [{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.id}] Has [green]Unblocked[/green] u!", SetConsoleColor.ConsoleLogType.Warning);
                    }
                    if (MuteState && !ModerationMuteState[ActorID])
                    {
                        if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} Has Muted U!", Config.Instance.HUDMessage);
                        SetConsoleColor.WriteEmbeddedColorLine($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} [{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.id}] Has [red]Muted[/red] u!", SetConsoleColor.ConsoleLogType.Warning);
                    }             
                    if (!MuteState && ModerationMuteState[ActorID])
                    {
                        if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} Has Unmuted U!", Config.Instance.HUDMessage);
                        SetConsoleColor.WriteEmbeddedColorLine($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} [{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.id}] Has [green]Unmuted[/green] u!", SetConsoleColor.ConsoleLogType.Warning);
                    }
                    
                    ModerationBlockState[ActorID] = BlockState;
                    ModerationMuteState[ActorID] = MuteState;
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        public enum GetModerationState
        {
            Block,
            Mute
        }   
        private static IEnumerator NewModerationState(GetModerationState Type, int ActorID)
        {
            while (true)
            {
                Photon.Realtime.Player _GetPlayer = PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(ActorID);
                if (_GetPlayer.field_Public_Player_0 != null && _GetPlayer.field_Public_Player_0.field_Private_APIUser_0 != null)
                {
                    switch (Type)
                    {
                        case GetModerationState.Block:
                            if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} Has Blocked U!", Config.Instance.HUDMessage);
                            SetConsoleColor.WriteEmbeddedColorLine($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} [{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.id}] Has [red]Blocked[/red] u!", SetConsoleColor.ConsoleLogType.Warning);
                            yield break;
                        case GetModerationState.Mute:
                            if (Config.Instance.OnInGameNotifications) VRCUiManager.prop_VRCUiManager_0.QueueHudMessage($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} Has Muted U!", Config.Instance.HUDMessage);
                            SetConsoleColor.WriteEmbeddedColorLine($"{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.displayName} [{_GetPlayer.field_Public_Player_0.field_Private_APIUser_0.id}] Has [red]Muted[/red] u!", SetConsoleColor.ConsoleLogType.Warning);
                            yield break;
                    }
                }
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }
        }

        public static List<string> BlacklistedUsers = new List<string>();
        private static bool OnModerationEvent(EventData __0)
        {
            object DataRPC = Serialization.FromIL2CPPToManaged<object>(__0.CustomData);
            string Data = JsonConvert.SerializeObject(DataRPC, Formatting.Indented);
            switch (__0.Code)
            {
                case 1:
                    if (StaticConfig.Log_1_USpeak) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]1[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 2:
                    if (StaticConfig.Log_2_Executive_message) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]2[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 3:
                    if (StaticConfig.Log_3_Past_events) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]3[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 4:
                    if (StaticConfig.Log_4_Sync_events) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]4[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 5:
                    if (StaticConfig.Log_5_Sync_finished) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]5[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 6:
                    if (StaticConfig.Log_6_Process_event) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]6[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 7:
                    if (StaticConfig.Log_7_Vrc_serialization) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]7[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 8:
                    if (StaticConfig.Log_8_Frequency_request) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]8[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 9:
                    if (StaticConfig.Log_9_Vrc_reliable_serialization) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]9[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 33:
                    if (!Config.Instance.OnModerationEventLog) return true;
                    if (StaticConfig.Log_33_Executive_action) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]33[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    JObject ParsedData = JObject.Parse(Data);
                    if (Config.Instance.AntiWarn && ParsedData["2"] != null && ParsedData["2"].ToString().Contains("You have been warned for your behavior. If you continue, you may be kicked out of the instance"))
                    {
                        SetConsoleColor.WriteEmbeddedColorLine("You have been [red]Warned[/red] by the instance creator", SetConsoleColor.ConsoleLogType.Info);
                        return false;
                    }
                    if (Config.Instance.AntiForceMicOff && ParsedData["0"] != null && ParsedData["0"].ToString().Contains("8"))
                    {
                        SetConsoleColor.WriteEmbeddedColorLine("The instance creator tried [red]ForceMic Off[/red] your mic", SetConsoleColor.ConsoleLogType.Info);
                        return false;
                    }
                    if (ParsedData["1"] != null && ParsedData["10"] != null && ParsedData["11"] != null)
                    {
                        int ActorID = ParsedData["1"].ToObject<int>();
                        bool BlockState = ParsedData["10"].ToObject<bool>();
                        bool MuteState = ParsedData["11"].ToObject<bool>();
                        if (!ModerationBlockState.ContainsKey(ActorID))
                        {
                            ModerationBlockState.Add(ActorID, BlockState);
                            if (BlockState) MelonCoroutines.Start(NewModerationState(GetModerationState.Block, ActorID));
                        }
                        if (!ModerationMuteState.ContainsKey(ActorID))
                        {
                            ModerationMuteState.Add(ActorID, MuteState);
                            if (MuteState) MelonCoroutines.Start(NewModerationState(GetModerationState.Mute, ActorID));
                        }
                        MelonCoroutines.Start(ModerationStates(ActorID, BlockState, MuteState));
                        return true;
                    }
                    return true;
                case 60:
                    if (StaticConfig.Log_60_Avatar_Phys_Events) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]60[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 202:
                    if (StaticConfig.Log_202_Instantiate) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]202[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 209:
                    if (StaticConfig.Log_209_Ownership_request) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]209[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 210:
                    if (StaticConfig.Log_210_Ownership_transfer) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]210[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.GlobalEventBlock) return false;
                    if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender) != null &&
                        BlacklistedUsers.Contains(PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(__0.Sender).field_Public_Player_0.field_Private_APIUser_0.id)) return false;
                    return true;
                case 223:
                    if (StaticConfig.Log_223_Auth) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]223[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 226:
                    if (StaticConfig.Log_226_Stats) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]226[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 230:
                    if (StaticConfig.Log_230_Authenticate) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]230[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 252:
                    if (StaticConfig.Log_252_Set_properties) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]252[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 253:
                    if (StaticConfig.Log_253_Properties_changed) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]253[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 254:
                    if (StaticConfig.Log_254_Leave) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]254[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                case 255:
                    if (StaticConfig.Log_255_Join) SetConsoleColor.WriteEmbeddedColorLine($"[Event: [darkyellow]255[/darkyellow]] {Data}", SetConsoleColor.ConsoleLogType.Message);
                    return true;
                default:
                    return true;
            }
        }
    }
}