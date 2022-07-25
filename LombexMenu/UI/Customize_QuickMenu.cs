using Utils;
using System;
using VRC.Core;
using Modifications.Visuals;
using Modifications.Logging;
using System.Linq;
using UnityEngine;
using MelonLoader;
using ReMod.Core.UI;
using UnityEngine.UI;
using Newtonsoft.Json;
using LombexMenu.Configuration;
using System.Collections.Generic;
using LombexMenu.Networking.Exploits;
using System.Windows.Forms;
using Modifications.Movement;
using System.IO;
using ReMod.Core.VRChat;
using System.Collections;
using Modifications.Misc;
using ReMod.Core.UI.QuickMenu;
using LombexMenu.GetResources;

namespace UI
{
    public class Customize_QuickMenu
    {
        public static GameObject GetQuickMenu { get; set; }
        private static IEnumerator BuildMenu()
        {
            while (ReferenceEquals(GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)"), null)) yield return new WaitForEndOfFrame();
            CustomResources.CreateBundle();
            GetQuickMenu = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)");

            #region RemoveAds
            GameObject VRCPlusAdds = GetQuickMenu?.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.Equals("Carousel_Banners", StringComparison.Ordinal)).gameObject;
            GameObject VRCPlusBanner = GetQuickMenu?.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.Equals("VRC+_Banners", StringComparison.Ordinal)).gameObject;
            GameObject.Destroy(VRCPlusAdds); GameObject.Destroy(VRCPlusBanner);
            #endregion RemoveAds 

            Customize_QuickMenu.OnUiManagerInit();
            Customize_QuickMenu.SelectedUserMenu();
        }
        private static void OnUiManagerInit()
        {
            ReTabButton.Create("LombexMenu", "Open LombexMenu by clicking this button", "LombexMenu", CustomResources.QuickMenuSprite);
            ReCategoryPage MainMenu = ReCategoryPage.Create("LombexMenu", true);

            ReMenuPage MovementMenu = ReMenuPage.Create("Movement", false);
            ReMenuPage MiscMenu = ReMenuPage.Create("Misc", false);
            ReMenuPage ProtectionMenu = ReMenuPage.Create("Protection", false);
            ReMenuPage VisualsMenu = ReMenuPage.Create("Visuals", false);
            ReMenuPage NetworkingMenu = ReMenuPage.Create("Networking", false);
            ReMenuPage LoggingMenu = ReMenuPage.Create("Logging", false);
            ReMenuPage SystemMenu = ReMenuPage.Create("System", false);
            ReMenuPage UtilsMenu = ReMenuPage.Create("Utils", false);           

            ReUiButton Movement = new ReUiButton("Movement", new Vector2(0f, 300f), new Vector2(2.5f, 1.25f), () => MovementMenu.Open(), MainMenu.GameObject.transform);
            MovementMenu.AddToggle("Flight", "Toggle your flight mode [<color=yellow>CTRL+F</color>]", toggle => 
            {
                StaticConfig.Flight = toggle;
                Rotation.ToggleRotation(StaticConfig.Flight);
                if (StaticConfig.Flight)
                {
                    StaticConfig.Gravity = Physics.gravity;
                    Physics.gravity = Vector3.zero;
                    PlayerUtils.GetVRCPlayer().GetComponent<CharacterController>().enabled = false;
                } else
                {
                    Physics.gravity = StaticConfig.Gravity;
                    PlayerUtils.GetVRCPlayer().GetComponent<CharacterController>().enabled = true;
                }
            }, StaticConfig.Flight);
            MovementMenu.AddButton("Flight\nSpeed", "Set your custom flight speed", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Flight Speed", Config.Instance.FlightSpeed.ToString(), InputField.InputType.Standard, true, "Submit", (s, k, t) =>
                {
                    if (string.IsNullOrEmpty(s)) return;
                    if (!float.TryParse(s, out var flySpeed)) return;
                    Config.Instance.FlightSpeed = flySpeed;
                }, null);
            });
            MovementMenu.AddButton("Rotation\nSpeed", "Set your custom rotation speed", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Rotation Speed", Config.Instance.RotationSpeed.ToString(), InputField.InputType.Standard, true, "Submit", (s, k, t) => 
                {
                    if (string.IsNullOrEmpty(s)) return;
                    if (!float.TryParse(s, out var rotationspeed)) return;
                    Config.Instance.RotationSpeed = rotationspeed;
                }, null);
            });
            MovementMenu.AddButton("Enable Jump", "Enables jump in non jump worlds", () =>
            {
                VRC.SDKBase.VRCPlayerApi APIPlayer = PlayerUtils.GetVRCPlayer().field_Private_VRCPlayerApi_0;
                APIPlayer.SetJumpImpulse(3f);
            });
            MovementMenu.AddButton("Jump Strength", "Allows u to set a custom jump strength", () => 
            {
                VRC.SDKBase.VRCPlayerApi APIPlayer = PlayerUtils.GetVRCPlayer().field_Private_VRCPlayerApi_0;                
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Jump Strength", "", InputField.InputType.Standard, true, "Submit", (s, k, t) =>
                {
                    if (string.IsNullOrEmpty(s)) return;
                    if (!float.TryParse(s, out var JumpStrength)) return;
                    APIPlayer.SetJumpImpulse(JumpStrength);
                }, null);
            });
            MovementMenu.AddToggle("Infinite Jump", "Allows u to jump infinite", toggle => Config.Instance.InfiniteJump = toggle, Config.Instance.InfiniteJump);
            MovementMenu.AddButton("T Pose", "Tpose your humanoid avatar", () =>
            {
                Animator GetAnimator = PlayerUtils.GetVRCPlayer().transform.Find("ForwardDirection/Avatar").GetComponent<Animator>();
                GetAnimator.enabled = !GetAnimator.enabled;
            });

            ReUiButton Misc = new ReUiButton("Misc", new Vector2(0f, 200f), new Vector2(2.5f, 1.25f), () => MiscMenu.Open(), MainMenu.GameObject.transform);
            MiscMenu.AddToggle("Mirror", "Toggle your portable mirror in [<color=yellow>CTRL + 1</color>]", toggle =>
            {
                StaticConfig.PortableMirror = toggle;
                if (StaticConfig.PortableMirror)
                {
                    PortableMirror.GetMirror.SetActive(true);
                    Vector3 Position = PlayerUtils.GetVRCPlayer().transform.position + PlayerUtils.GetVRCPlayer().transform.forward;
                    Position.y += 1f;
                    PortableMirror.GetMirror.transform.position = Position;
                    PortableMirror.GetMirror.transform.rotation = PlayerUtils.GetVRCPlayer().transform.rotation;
                }
                else PortableMirror.GetMirror.SetActive(false);
            }, StaticConfig.PortableMirror);
            MiscMenu.AddToggle("PickableBall", "Toggle your pickable ball in [<color=yellow>CTRL + 2</color>]", toggle =>
            {
                StaticConfig.PortableBall = toggle;
                if (StaticConfig.PortableBall)
                {
                    PortableBall.GetPortableBall.SetActive(true);
                    Vector3 pos = PlayerUtils.GetLocalViewPoint() + PlayerUtils.GetVRCPlayer().transform.forward;
                    PortableBall.GetPortableBall.transform.position = pos;
                }
                else PortableBall.GetPortableBall.SetActive(false);
            }, StaticConfig.PortableBall);
            ReMenuPage PortableLight = MiscMenu.AddMenuPage("Portable\nLight", "Get to the portable light page");
            PortableLight.AddToggle("Portable Light", "Toggle your portable light source", toggle => PortableLightSource.SetupPortableLightSource(toggle, Config.Instance.PortableLightColor, Config.Instance.PortableLightIntensity));
            PortableLight.AddButton("Color", "Set Color for the light", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Color", Config.Instance.PortableLightColor.ToString(), InputField.InputType.Standard, false, "Submit", (s, k, t) => Config.Instance.PortableLightColor = s, null);
            });
            PortableLight.AddButton("Intensity", "Set Intensity for the light", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Intensity", Config.Instance.PortableLightIntensity.ToString(), InputField.InputType.Standard, true, "Submit", (s, k, t) =>
                {
                    if (string.IsNullOrEmpty(s)) return;
                    if (!float.TryParse(s, out var Intensity)) return;
                    Config.Instance.PortableLightIntensity = Intensity;
                }, null);
            });

            MiscMenu.AddButton("Portal Time", "Set your custom portaltime", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set PortalTime", Config.Instance.PortalTime.ToString(), InputField.InputType.Standard, true, "Submit", (s, k, t) =>
                {
                    if (string.IsNullOrEmpty(s)) return;
                    if (!int.TryParse(s, out var PortalTime)) return;
                    Config.Instance.PortalTime = PortalTime - 30;
                }, null);
            });
            MiscMenu.AddToggle("Freeze Portals", "Freeze all portals", toggle => Config.Instance.OnPortalFreeze = toggle, Config.Instance.OnPortalFreeze);
            MiscMenu.AddButton("Delete all Portals", "Deletes all portals droped by players", () => Utilities.DeleteAllPortals());
            MiscMenu.AddToggle("Earrape Mic", "Toggle a very loud mic [<color=yellow>CTRL + M</color>]", toggle => 
            {
                if (toggle) USpeaker.field_Internal_Static_Single_1 = float.MaxValue;
                else USpeaker.field_Internal_Static_Single_1 = 1;
            });
            
            ReUiButton Protection = new ReUiButton("Protection", new Vector2(0f, 100f), new Vector2(2.5f, 1.25f), () => ProtectionMenu.Open(), MainMenu.GameObject.transform);
            ProtectionMenu.AddToggle("Anti Portal", "Disabled the collision of portals", toggle => Config.Instance.OnPortalEnter = toggle, Config.Instance.OnPortalEnter);
            ProtectionMenu.AddToggle("Portal Confirmation", "Askes u if u want to enter a specific portal", toggle => Config.Instance.PortalConfirmation = toggle, Config.Instance.PortalConfirmation);
            ProtectionMenu.AddToggle("Auto Portal Delete", "Deletes a portal automaticly", toggle => Config.Instance.AutoPortalDelete = toggle, Config.Instance.AutoPortalDelete);
            ProtectionMenu.AddToggle("Anti Udon", "Removes all udon sync RPC's", toggle => Config.Instance.AntiUdon = toggle, Config.Instance.AntiUdon);
            ProtectionMenu.AddButton("Clear EventBlocked Users", "Clears all the users that are currently event blocked", () => LogPatches.BlacklistedUsers.Clear());
            ProtectionMenu.AddToggle("Global EventBlock", "Blocks every potential harmfull event that a user can send to you", toggle => StaticConfig.GlobalEventBlock = toggle);

            ReUiButton Visuals = new ReUiButton("Visuals", new Vector2(0f, 0f), new Vector2(2.5f, 1.25f), () => VisualsMenu.Open(), MainMenu.GameObject.transform);
            VisualsMenu.AddButton("Camera\nClipping", "Set your clipping to smallest value", () => { Camera.main.nearClipPlane = 0.01f; });
            VisualsMenu.AddToggle("PropESP", "ESP for all props and triggers", toggle => 
            {
                Config.Instance.PropESP = toggle;
                ESP.PropESP();
            }, Config.Instance.PropESP);
            VisualsMenu.AddToggle("CapsuleESP", "ESP for all players", toggle =>
            {
                Config.Instance.CapsuleESP = toggle;
                ESP.CapsuleESP();
            }, Config.Instance.CapsuleESP);
            VisualsMenu.AddButton("ESP Color", "Set your ESP color", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set ESPColor", Config.Instance.ESPColor.ToString(), InputField.InputType.Standard, false, "Submit", (s, k, t) => Config.Instance.ESPColor = s, null);
            });

            ReUiButton Networking = new ReUiButton("Networking", new Vector2(0f, -100f), new Vector2(2.5f, 1.25f), () => NetworkingMenu.Open(), MainMenu.GameObject.transform);
            NetworkingMenu.AddToggle("IKFreeze", "Freeze yourself in place for other people [<color=yellow>CTRL+0</color>]", toggle => 
            {
                StaticConfig.IKFreeze = toggle;
                Utilities.CloneLocalAvatar();
                SetConsoleColor.WriteEmbeddedColorLine(StaticConfig.IKFreeze ? "IKFreeze Has Been Activated!" : "IKFreeze Has Been Deactivated!", SetConsoleColor.ConsoleLogType.Message);
            });          
            NetworkingMenu.AddToggle("AntiWarn", "Stops users to be able to warn u", toggle => Config.Instance.AntiWarn = toggle, Config.Instance.AntiWarn);
            NetworkingMenu.AddToggle("Anti ForceMic Off", "Stops from users making ur mic go off when they are the instance creator", toggle => Config.Instance.AntiForceMicOff = toggle, Config.Instance.AntiForceMicOff);
            ReMenuPage Exploits = NetworkingMenu.AddMenuPage("Exploits", "Open the exploits menu", null);
            Exploits.AddToggle("Lock Instance", "Locking the instacne when u are instance master", toggle => StaticConfig.LockInstance = toggle);
            Exploits.AddToggle("USpeak\nExploit", "Earrape All people around you with a loud noise [<color=red>Risky</color>]", toggle => 
            {
                StaticConfig.UseUSpeakExploit = toggle;
                if (toggle) USpeaker.field_Internal_Static_Single_1 = float.MaxValue;
                else USpeaker.field_Internal_Static_Single_1 = 1;
                USpeakExploits.StartUSpeakExploits();
            });
            Exploits.AddButton("Quest Crash", "Crash all quest users in the lobby", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("QuestCrash Failed!", "Currently there are no avatars available to use to perform this action, if u have any (public) quest crasher avatar please report it into the discord!", 15f);
                /*
                System.Random rand = new System.Random();
                List<string> AvatarIDs = new List<string>() { "", "" };
                int RandomID = rand.Next(AvatarIDs.Count);
                Utilities.ForceCloneAvatar(AvatarIDs[RandomID]);
                */
            }); 

            ReUiButton Logging = new ReUiButton("Logging", new Vector2(0f, -200f), new Vector2(2.5f, 1.25f), () => LoggingMenu.Open(), MainMenu.GameObject.transform);
            LoggingMenu.AddToggle("Join Notifications", "Get a player join notification", toggle => Config.Instance.OnPlayerJoinLog = toggle, Config.Instance.OnPlayerJoinLog);
            LoggingMenu.AddToggle("Leave Notifications", "Get a player leave notification", toggle => Config.Instance.OnPlayerLeaveLog = toggle, Config.Instance.OnPlayerLeaveLog);
            LoggingMenu.AddToggle("Portal Drop Notifications", "Get a droped portal notification", toggle => Config.Instance.OnPortalDropLog = toggle, Config.Instance.OnPortalDropLog);
            LoggingMenu.AddToggle("Moderation Notifications", "Get moderation notification", toggle => Config.Instance.OnModerationEventLog = toggle, Config.Instance.OnModerationEventLog);
            LoggingMenu.AddToggle("Avatar Notification", "Get avatar change notification", toggle => Config.Instance.OnPlayerAvatarChangeLog = toggle, Config.Instance.OnPlayerAvatarChangeLog);
            LoggingMenu.AddToggle("In game notifications", "Get in game notification shown to you", toggle => Config.Instance.OnInGameNotifications = toggle, Config.Instance.OnInGameNotifications);
            LoggingMenu.AddButton("Pickup Info", "See who owns pickups", () => 
            {
                Dictionary<(string, string), int> AmountOwnedList = new Dictionary<(string, string), int>();
                foreach (VRC.SDKBase.VRC_Pickup PickUps in Resources.FindObjectsOfTypeAll<VRC.SDKBase.VRC_Pickup>())
                {
                    string[] IgnoredPickups = { "LombexMenuPortableBall", "LombexMenuMirror", "OscDebugConsole", "PhotoCamera", "ViewFinder", "AvatarDebugConsole" };
                    if (IgnoredPickups.Contains(PickUps.gameObject?.name)) continue;
                    if (PickUps.gameObject.GetComponent<Photon.Pun.PhotonView>() != null)
                    {
                        int PhotonID = PickUps.GetComponent<Photon.Pun.PhotonView>().field_Private_Int32_0;
                        try
                        {
                            if (PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(PhotonID) != null)
                            {
                                APIUser User = PhotonExtentions.LoadBalancingClient.GetPhotonPlayer(PhotonID).field_Public_Player_0.field_Private_APIUser_0;
                                if (!AmountOwnedList.ContainsKey((User.displayName, User.id))) AmountOwnedList.Add((User.displayName, User.id), 1);
                                else AmountOwnedList[(User.displayName, User.id)]++;
                            }
                        }
                        catch { }
                    }
                }
                string OwnedPickUpsList = JsonConvert.SerializeObject(AmountOwnedList, Formatting.Indented);
                SetConsoleColor.WriteEmbeddedColorLine(OwnedPickUpsList, SetConsoleColor.ConsoleLogType.Info);
                AmountOwnedList.Clear();
            });

            ReMenuPage Events = LoggingMenu.AddMenuPage("Event Logger", "Log Events Page");
            Events.AddToggle("1 USpeak", "Log this specific event", toggle => StaticConfig.Log_1_USpeak = toggle);
            Events.AddToggle("2 Executive message", "Log this specific event", toggle => StaticConfig.Log_2_Executive_message = toggle);
            Events.AddToggle("3 Past events", "Log this specific event", toggle => StaticConfig.Log_3_Past_events = toggle);
            Events.AddToggle("4 Sync events", "Log this specific event", toggle => StaticConfig.Log_4_Sync_events = toggle);
            Events.AddToggle("5 Sync finished", "Log this specific event", toggle => StaticConfig.Log_5_Sync_finished = toggle);
            Events.AddToggle("6 Process event", "Log this specific event", toggle => StaticConfig.Log_6_Process_event = toggle);
            Events.AddToggle("7 VRC serialization", "Log this specific event", toggle => StaticConfig.Log_7_Vrc_serialization = toggle);
            Events.AddToggle("8 Frequency request", "Log this specific event", toggle => StaticConfig.Log_8_Frequency_request = toggle);
            Events.AddToggle("9 VRC reliable serialization", "Log this specific event", toggle => StaticConfig.Log_9_Vrc_reliable_serialization = toggle);
            Events.AddToggle("33 Executive action", "Log this specific event", toggle => StaticConfig.Log_33_Executive_action = toggle);
            Events.AddToggle("60 Avatar Phys Event", "Log this specific event", toggle => StaticConfig.Log_60_Avatar_Phys_Events = toggle);
            Events.AddToggle("202 Instantiate", "Log this specific event", toggle => StaticConfig.Log_202_Instantiate = toggle);
            Events.AddToggle("209 Ownership request", "Log this specific event", toggle => StaticConfig.Log_209_Ownership_request = toggle);
            Events.AddToggle("210 Ownership transfer", "Log this specific event", toggle => StaticConfig.Log_210_Ownership_transfer = toggle);
            Events.AddToggle("223 Auth", "Log this specific event", toggle => StaticConfig.Log_223_Auth = toggle);
            Events.AddToggle("226 Stats", "Log this specific event", toggle => StaticConfig.Log_226_Stats = toggle);
            Events.AddToggle("230 Authenticate", "Log this specific event", toggle => StaticConfig.Log_230_Authenticate = toggle);
            Events.AddToggle("252 Set properties", "Log this specific event", toggle => StaticConfig.Log_252_Set_properties = toggle);
            Events.AddToggle("253 Properties changed", "Log this specific event", toggle => StaticConfig.Log_253_Properties_changed = toggle);
            Events.AddToggle("254 Leave", "Log this specific event", toggle => StaticConfig.Log_254_Leave = toggle);
            Events.AddToggle("255 Join", "Log this specific event", toggle => StaticConfig.Log_255_Join = toggle);

            ReUiButton Utils = new ReUiButton("Utils", new Vector2(0f, -300f), new Vector2(2.5f, 1.25f), () => UtilsMenu.Open(), MainMenu.GameObject.transform);
            ReMenuPage WorldSettings = UtilsMenu.AddMenuPage("World\nSettings", "Got to the world settings");
            WorldSettings.AddButton("Respawn\nPickups", "Respawn all pickups to respawn point", () => 
            {
               if (Resources.FindObjectsOfTypeAll<VRC.SDK3.Components.VRCObjectSync>() != null)
               {
                    foreach (var PickUp in Resources.FindObjectsOfTypeAll<VRC.SDK3.Components.VRCObjectSync>())
                    {
                        VRC.SDKBase.Networking.SetOwner(VRC.SDKBase.Networking.LocalPlayer, PickUp.gameObject);
                        PickUp.Respawn();
                    }
               }
               if (Resources.FindObjectsOfTypeAll<VRCSDK2.VRC_ObjectSync>() != null)
               {
                    foreach (var PickUp in Resources.FindObjectsOfTypeAll<VRCSDK2.VRC_ObjectSync>())
                    {
                        VRC.SDKBase.Networking.SetOwner(VRC.SDKBase.Networking.LocalPlayer, PickUp.gameObject);
                        PickUp.Respawn();
                    }
               }
            });
            WorldSettings.AddToggle("Post Processing", "Toggle Post Processing", toggle => 
            {
                foreach (UnityEngine.Rendering.PostProcessing.PostProcessVolume PostProcessing in Resources.FindObjectsOfTypeAll<UnityEngine.Rendering.PostProcessing.PostProcessVolume>()) PostProcessing.gameObject.SetActive(toggle);
            }, true);
            WorldSettings.AddToggle("Toggle Pickups", "Toggle all pickups", toggle => 
            {
                foreach (VRC.SDKBase.VRC_Pickup Pickups in Resources.FindObjectsOfTypeAll<VRC.SDKBase.VRC_Pickup>())
                {
                    string[] IgnoredPickups = { "LombexMenuPortableBall", "LombexMenuMirror", "OscDebugConsole", "PhotoCamera", "ViewFinder", "AvatarDebugConsole" };
                    if (IgnoredPickups.Contains(Pickups.gameObject?.name)) continue;
                    if (Pickups.gameObject != null) Pickups.gameObject.SetActive(toggle);
                }
            }, true);
            ReMenuPage APITools = UtilsMenu.AddMenuPage("ApiTools", "Click here for api tools");
            APITools.AddButton("Force Change Avatar", "Change into a avatar", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set AvatarID", "", InputField.InputType.Standard, false, "Submit", (s, k, t) =>
                {
                    if (Utilities.AvatarRegex.IsMatch(s)) 
                    {
                        API.Fetch<ApiAvatar>(s, new Action<ApiContainer>(container =>
                        {
                            ApiAvatar apiAvatar = container.Model.Cast<ApiAvatar>();
                            if (apiAvatar.releaseStatus.IndexOf("public", StringComparison.OrdinalIgnoreCase) >= 0 || apiAvatar.authorId.Equals(APIUser.CurrentUser.id)) Utilities.ForceCloneAvatar(s);
                            else SetConsoleColor.WriteEmbeddedColorLine("This avatar is private", SetConsoleColor.ConsoleLogType.Warning);
                        })); 
                    } else SetConsoleColor.WriteEmbeddedColorLine("The avatar id isnt valid", SetConsoleColor.ConsoleLogType.Warning);
                }, null);
            });
            APITools.AddButton("Force Join World", "ForceJoin a world by id", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set WorldID", "", InputField.InputType.Standard, false, "Submit", (s, k, t) =>
                {
                    VRC.SDKBase.Networking.GoToRoom(s);
                }, null);
            });
            APITools.AddButton("Drop Custom Portal", "Drop a portal with your custom world id", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set WorldID", "", InputField.InputType.Standard, false, "Submit", (s, k, t) =>
                {
                    if (Utilities.WorldRegex.IsMatch(s)) Utilities.CreatePortal(s, 1, PlayerUtils.GetVRCPlayer().transform);
                    else SetConsoleColor.WriteEmbeddedColorLine("The avatar id isnt valid", SetConsoleColor.ConsoleLogType.Warning);
                }, null);
            });
            APITools.AddButton("Copy\nInstance ID", "", () => Clipboard.SetText(RoomManager.field_Internal_Static_ApiWorldInstance_0.id));

            ReUiButton System = new ReUiButton("System", new Vector2(0f, -400f), new Vector2(2.5f, 1.25f), () => SystemMenu.Open(), MainMenu.GameObject.transform);
            SystemMenu.AddButton("Restart", "Restart your game [<color=yellow>CTRL+KeypadMultiply</color>]", () => Utilities.RestartGame());
            SystemMenu.AddButton("Hud Message Color", "Change the color of the hud message", () => 
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set HUD Color", Config.Instance.HUDMessage, InputField.InputType.Standard, false, "Submit", (s, k, t) => Config.Instance.HUDMessage = s, null);
            });
            SystemMenu.AddButton("Clear Console", "Clears out the console", () => 
            {
                Console.Clear();
                SetConsoleColor.WriteEmbeddedColorLine("Console cleared!", SetConsoleColor.ConsoleLogType.Info);
            });
        }
        private static void SelectedUserMenu()
        {
            ReMenuPage MainMenu = ReMenuPage.Create("SelectedUser", true);
            Transform MainPage = GameObject.Find($"UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup").transform;
            ReUiButton SelectedUserPage = new ReUiButton("LombexClient", new Vector2(0f, 0f), new Vector2(1f, 1f), () => MainMenu.Open(), MainPage);

            ReMenuPage Misc = MainMenu.AddMenuPage("SelectedUser\nMisc", "Open misc options for this user");
            Misc.AddButton("Teleport", "Teleport to this user", () => 
            {
                VRCPlayer _Player = PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_VRCPlayer_0;
                PlayerUtils.GetVRCPlayer().transform.position = _Player.transform.position; 
            });
            Misc.AddButton("ForceClone", "Clone this avatar", () => 
            {
                VRCPlayer _Player = PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_VRCPlayer_0;
                if (_Player.field_Private_ApiAvatar_0.releaseStatus == "public") Utilities.ForceCloneAvatar(_Player.field_Private_ApiAvatar_0.id);
                else SetConsoleColor.WriteEmbeddedColorLine("The avatar u tried to clone is private", SetConsoleColor.ConsoleLogType.Warning);
            });

            ReMenuPage Exploits = MainMenu.AddMenuPage("SelectedUser\nExploits", "Open exploits options for this user");
            Exploits.AddToggle("Repeat\nCapture", "Repeats a photo capture sound at players location", toggle => {
                VRCPlayer _Player = PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_VRCPlayer_0;
                RPCNetworking.CameraCaptureToggle = toggle;
                RPCNetworking.RPCPhotoCapture(_Player.prop_Player_0, 0.35f); 
            });
            Exploits.AddToggle("Attach\nObjects", "Attach all pickups to the player", toggle => {
                VRCPlayer _Player = PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_VRCPlayer_0;
                PickupNetworking.AttachPickups = toggle;
                PickupNetworking.AttachObjects(_Player.prop_Player_0);
            });

            ReMenuPage Protection = MainMenu.AddMenuPage("SelectedUser\nProtection", "Open protections menu for this user");
            Protection.AddButton("EventBlock", "Block every event that comes from this user", () => 
            {
                VRCPlayer _Player = PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_VRCPlayer_0;
                LogPatches.BlacklistedUsers.Add(_Player.prop_Player_0.field_Private_APIUser_0.id);
            });
            Protection.AddButton("Unblock Events", "Unblocks events that where blocked from this user", () => 
            {
                VRCPlayer _Player = PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_VRCPlayer_0;
                if (!LogPatches.BlacklistedUsers.Contains(_Player.prop_Player_0.field_Private_APIUser_0.id)) SetConsoleColor.WriteEmbeddedColorLine($"{_Player.prop_Player_0.field_Private_APIUser_0.displayName} Has not been event blocked", SetConsoleColor.ConsoleLogType.Error);
                else LogPatches.BlacklistedUsers.Remove(_Player.prop_Player_0.field_Private_APIUser_0.id);
            });

            ReMenuPage Logs = MainMenu.AddMenuPage("SelectedUser\nLogs", "Open logs option for this user");
            Logs.AddButton("Logging Info", "Log information of this user", () =>
            {
                SetConsoleColor.WriteEmbeddedColorLine(
                            $"\nPlayerName: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.field_Private_APIUser_0.displayName}[/darkyellow]\n" +
                            $"UserID: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.field_Private_APIUser_0.id}[/darkyellow]\n" +
                            $"PhotonID: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_PlayerNet_0.prop_Int32_0}[/darkyellow]\n" +
                            $"FPS: [darkyellow]{1000f / (int)(PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?._playerNet.prop_Byte_0)}[/darkyellow]\n" +
                            $"Ping: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.GetPlayerPing()}[/darkyellow]\n" +
                            $"AvatarName: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.name}[/darkyellow]\n" +
                            $"AvatarID: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.id}[/darkyellow]\n" +
                            $"AvatarAuthor: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.authorName}[/darkyellow]\n" +
                            $"AvatarAuthorID: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.authorId}[/darkyellow]\n" +
                            $"AvatarAsset: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.assetUrl}[/darkyellow]\n" +
                            $"AvatarImageUrl: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.imageUrl}[/darkyellow]\n" +
                            $"Avatar Publish State: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.releaseStatus}[/darkyellow]\n" +
                            $"AvatarVersion: [darkyellow]{PlayerUtils.GetPlayer(PlayerUtils.ToAPIUser(QuickMenuEx.SelectedUserLocal.field_Private_IUser_0))?.prop_ApiAvatar_0.version}[/darkyellow]", SetConsoleColor.ConsoleLogType.Message);
            });
        }
        public static void Start() => MelonCoroutines.Start(Customize_QuickMenu.BuildMenu());
    }

}