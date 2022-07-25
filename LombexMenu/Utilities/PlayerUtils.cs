using System;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;
using VRC.DataModel;
using VRC.DataModel.Core;
using System.Reflection;
using System.Collections.Generic;

namespace Utils
{   
    public static class PlayerUtils
    {      
        public static Player GetPlayer(this APIUser user)
        {
            foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0) if (player.prop_APIUser_0.id == user.id) return player;
            return null;
        }
        public static bool GetVRMode(this Player player)
        {
            return player.field_Private_VRCPlayerApi_0.IsUserInVR() || player.prop_APIUser_0.last_platform != "standalonewindows";
        }
        public static Vector3 GetLocalViewPoint()
        {
            return GameObject.Find("Camera (eye)").transform.position;
        }
        public static VRCPlayer GetVRCPlayer()
        {
            return VRCPlayer.field_Internal_Static_VRCPlayer_0;
        }
        public static VRCPlayer GetVRCPlayer(this Player player)
        {
            return player.prop_VRCPlayer_0;
        }
        public static Player GetLocalPlayer()
        {
            return Player.prop_Player_0;
        }
        public static APIUser GetAPIUser(this Player player)
        {
            return player.prop_APIUser_0;
        }
        public static Player[] GetAllPlayers()
        {
            return PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.ToArray();
        }
        public static Color GetSocialRankColor(APIUser user)
        {
            return VRCPlayer.Method_Public_Static_Color_APIUser_0(user);
        }
        public static Player GetPlayerByInstigatorID(int instigatorID)
        {
            var matches = PlayerUtils.GetAllPlayers().Where(x => x.prop_VRCPlayerApi_0.playerId == instigatorID);
            return matches.Any() ? matches.First() : null;
        }
        public static APIUser CurrentUser()
        {
            return APIUser.CurrentUser;
        }
        public static int GetCreatorID(this PortalInternal portalInternal)
        {
            return portalInternal.prop_Int32_0;
        }
        public static string GetWorldID(this PortalInternal portalInternal)
        {
            return portalInternal.field_Private_String_2;
        }      
        public static int GetPlayerFPS(this Player player)
        {
            return player.prop_PlayerNet_0.prop_Byte_0 <= 0 ? (int)(1000f / player.prop_PlayerNet_0.prop_Byte_0) : 0;
        }
        public static int GetPlayerPing(this Player player)
        {
            return player.prop_VRCPlayer_0.prop_Int16_0;
        }
        public static VRCPlayerApi GetVRCPlayerApi(this Player player)
        {
            return player.field_Private_VRCPlayerApi_0;
        }
        public static bool IsLocal(this Player player)
        {
            return player.field_Private_VRCPlayerApi_0 != null && player.GetVRCPlayerApi().isLocal;
        }
        public static bool IsLocalUser(this APIUser apiUser)
        {
            if (APIUser.CurrentUser.id == null || apiUser.id == null) return false;
            return apiUser.id == APIUser.CurrentUser.id;
        }
        public static bool IsFriend(this APIUser apiUser)
        {
            return APIUser.CurrentUser.friendIDs.Contains(apiUser.id);
        }
        
        private static MethodInfo _apiUserToIUser;
        public static IUser ToIUser(this APIUser value)
        {
            return ((Il2CppSystem.Object) _apiUserToIUser.Invoke(DataModelManager.field_Private_Static_DataModelManager_0.field_Private_DataModelCache_0, new object[3] { value.id, value, false })).Cast<IUser>();
        }
        public static APIUser ToAPIUser(this IUser value)
        {
            return value.Cast<DataModel<APIUser>>().field_Protected_TYPE_0;
        }
        public static bool IsInVR
        {
            get
            {
                bool Result;
                try { Result = Player.prop_Player_0.prop_VRCPlayerApi_0.IsUserInVR(); }
                catch { Result = Environment.GetCommandLineArgs().All((string args) => !args.Equals("--no-vr", StringComparison.OrdinalIgnoreCase)); }
                return Result;
            }
        }
    }
}
