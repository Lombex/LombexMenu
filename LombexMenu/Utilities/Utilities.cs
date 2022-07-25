using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using VRCSDK2;
using LombexMenu.Configuration;
using RootMotion.FinalIK;
using Photon.Realtime;
using ExitGames.Client.Photon;
using VRC.UI;
using VRC.Core;
using System.Text.RegularExpressions;
using VRC.SDKBase;

namespace Utils
{
    public static class Utilities
    {
        public static void RestartGame(bool InstanceState = true)
        {
            List<string> Arguments = Environment.GetCommandLineArgs().ToList();
            Arguments.RemoveAll(Args => Args.IndexOf("vrchat://launch", StringComparison.OrdinalIgnoreCase) != -1);
            if (InstanceState) Arguments.Add($"vrchat://launch/?ref=vrchat.com&id={RoomManager.field_Internal_Static_ApiWorld_0.id}:{RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId}");
            Process GetProcess = new Process { StartInfo = { FileName = "VRChat.exe", WorkingDirectory = Directory.GetCurrentDirectory(), Arguments = String.Join(" ", Arguments) } };           
            GetProcess.Start();
            Config.SaveSettings();
            Process.GetCurrentProcess().CloseMainWindow();
            Process.GetCurrentProcess().Close();
        }
        public static HarmonyMethod GetPatch(this Type Type, string Name, bool PublicMethods = false)
        {
            return new HarmonyMethod(Type.GetMethod(Name, (PublicMethods ? BindingFlags.Public : BindingFlags.NonPublic) | BindingFlags.Static));
        }
        public static void DeleteAllPortals()
        {
            (from portal in Resources.FindObjectsOfTypeAll<PortalInternal>() where portal.gameObject.activeInHierarchy && !portal.gameObject.GetComponentInParent<VRC.SDKBase.VRC_PortalMarker>()
             select portal).ToList<PortalInternal>().ForEach(delegate (PortalInternal p) { UnityEngine.Object.Destroy(p.transform.root.gameObject); });
        }
        internal static float GetAxis(string axis, bool control = false, bool shift = false)
        {
            bool GetControl = !control;
            bool GetShift = !shift;
            bool ControlCheck = control && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            if (ControlCheck) GetControl = true;
            bool ShiftCheck = shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            if (ShiftCheck) GetShift = true;
            bool GetResult = GetControl && GetShift;
            float result;
            if (GetResult) result = Input.GetAxis(axis); else result = 0f;
            return result;
        }
        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            bool GetComponent = component == null;
            T result;
            if (GetComponent) result = gameObject.GetComponent<T>();
            else result = component;
            return result;
        }
        public static void ForceCloneAvatar(string AvatarID)
        {
            PageAvatar Avatar = GameObject.Find("Screens").transform.Find("Avatar").GetComponent<PageAvatar>();
            Avatar.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0 = new ApiAvatar
            {
                id = AvatarID
            };
            Avatar.ChangeToSelectedAvatar();
        }
        public static GameObject CreatePortal(string id, int playerCount, Transform transform, float distance = 2f)
        {
            Vector3 forward = transform.forward;
            Vector3 position = transform.position + forward * distance;
            GameObject portal = VRC.SDKBase.Networking.Instantiate(VRC.SDKBase.VRC_EventHandler.VrcBroadcastType.Always, "Portals/PortalInternalDynamic", position, Quaternion.FromToRotation(Vector3.forward, forward));
            VRC.SDKBase.Networking.RPC(RPC.Destination.AllBufferOne, portal, "ConfigurePortal", new[] { 
                (global::Il2CppSystem.String)id.Split(':')[0], (global::Il2CppSystem.String)id.Split(':')[1], 
                new global::Il2CppSystem.Int32 { m_value = playerCount }.BoxIl2CppObject()
            });
            return portal;
        }

        private static GameObject FrozenClone;
        public static void CloneLocalAvatar()
        {
            if (StaticConfig.IKFreeze)
            {
                Transform localTransform = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform;
                FrozenClone = UnityEngine.Object.Instantiate(VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Internal_GameObject_0);
                FrozenClone.transform.position = localTransform.position;
                FrozenClone.transform.rotation = localTransform.rotation;

                var cloneAnimator = FrozenClone.GetComponent<Animator>();
                if (cloneAnimator != null && cloneAnimator.isHuman)
                {
                    Animator localAnimator = VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Internal_GameObject_0.GetComponent<Animator>();

                    cloneAnimator.runtimeAnimatorController = null;
                    foreach (string name in Enum.GetNames(typeof(HumanBodyBones)))
                    {
                        if (name.Equals(nameof(HumanBodyBones.LastBone))) continue;
                        if (!Enum.TryParse(name, out HumanBodyBones currentBone)) continue;

                        Transform cloneBoneTransform = cloneAnimator.GetBoneTransform(currentBone);
                        if (!cloneBoneTransform) continue;
                        Transform localBoneTransform = localAnimator.GetBoneTransform(currentBone);
                        cloneBoneTransform.position = localBoneTransform.position;
                        cloneBoneTransform.rotation = localBoneTransform.rotation;
                    }
                    cloneAnimator.GetBoneTransform(HumanBodyBones.Head).localScale = Vector3.one;
                }

                UnityEngine.Object.Destroy(FrozenClone.GetComponent<VRIK>());
                UnityEngine.Object.Destroy(FrozenClone.GetComponent<FullBodyBipedIK>());
                UnityEngine.Object.Destroy(FrozenClone.GetComponent<LimbIK>());
                UnityEngine.Object.Destroy(FrozenClone.GetComponent<NetworkMetadata>());
            }
            else UnityEngine.Object.Destroy(FrozenClone);
        }
        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
        public static Color ToColor(this string HexColor)
        {
            Color SetColor;
            ColorUtility.TryParseHtmlString(HexColor, out SetColor);
            return SetColor;
        }
        public static Regex WorldRegex => new Regex("wrld_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}");
        public static Regex AvatarRegex => new Regex("avtr_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}");
        public static Regex UserRegex => new Regex("usr_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}");
    }
    internal interface ISanitizer
    {
        bool OnPhotonEvent(LoadBalancingClient loadBalancingClient, EventData eventData);
        bool VRCNetworkingClientOnPhotonEvent(EventData eventData);
    }
}
