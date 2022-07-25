using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Object = Il2CppSystem.Object;

namespace Utils
{
    public static class PhotonExtentions
    {
        private static readonly Dictionary<int, Player> ActorIDToPhotonPlayer = new Dictionary<int, Player>();
        public static void RefreshPhotonPlayerList()
        {
            ActorIDToPhotonPlayer.Clear();
            foreach (var pl in GetAllPhotonPlayers(LoadBalancingClient)) ActorIDToPhotonPlayer.Add(pl.GetPhotonID(), pl);
        }
        public static LoadBalancingClient LoadBalancingClient => PhotonNetwork.field_Public_Static_LoadBalancingClient_0;
        public static string GetUsername(this Player player)
        {
            if (player.GetRawHashtable().ContainsKey("user")) if (player.GetHashtable()["user"] is Dictionary<string, object> dict) return (string)dict["username"];
            return "No username";
        }
        public static string GetUserID(this Player player)
        {
            if (player.GetRawHashtable().ContainsKey("user")) if (player.GetHashtable()["user"] is Dictionary<string, object> dict) return (string)dict["id"];
            return "No ID";
        }
        public static string GetDisplayName(this Player player)
        {
            if (player.GetRawHashtable().ContainsKey("user")) if (player.GetHashtable()["user"] is Dictionary<string, object> dict) return (string)dict["displayName"];
            return "No DisplayName";
        }
        public static int GetPhotonID(this Player player) => player.field_Private_Int32_0;
        public static VRC.Player GetPlayer(this Player player) => player.field_Public_Player_0;
        public static System.Collections.Hashtable GetHashtable(this Player player) => Serialization.FromIL2CPPToManaged<System.Collections.Hashtable>(player.GetRawHashtable());
        public static Il2CppSystem.Collections.Hashtable GetRawHashtable(this Player player) => player.prop_Hashtable_0;
        public static List<Player> GetAllPhotonPlayers(this LoadBalancingClient Instance)
        {
            List<Player> result = new List<Player>();
            foreach (var x in Instance.prop_Player_0.prop_Room_0.field_Private_Dictionary_2_Int32_Player_0) result.Add(x.Value);
            return result;
        }
        public static Player GetPhotonPlayer(this LoadBalancingClient Instance, int photonID)
        {
            if (ActorIDToPhotonPlayer.ContainsKey(photonID)) return ActorIDToPhotonPlayer[photonID];  
            else
            {
                PhotonExtentions.RefreshPhotonPlayerList();
                return ActorIDToPhotonPlayer[photonID];
            }
        }       
        public static VRC.Player GetCurrentPhotonPlayer(this LoadBalancingClient Instance)
        {
            return Instance.prop_Player_0.field_Public_Player_0;
        }
    }

    [Serializable]
    public static class Serialization
    {
        public static byte[] ToByteArray(Object obj)
        {
            if (obj == null) return null;
            var bf = new Il2CppSystem.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var ms = new Il2CppSystem.IO.MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
        public static byte[] ToByteArray(object obj)
        {
            if (obj == null) return null;
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null) return default;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
        public static T IL2CPPFromByteArray<T>(byte[] data)
        {
            if (data == null) return default(T);
            var bf = new Il2CppSystem.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var ms = new Il2CppSystem.IO.MemoryStream(data);
            object obj = bf.Deserialize(ms);
            return (T)obj;
        }
        public static T FromIL2CPPToManaged<T>(Object obj)
        {
            return FromByteArray<T>(ToByteArray(obj));
        }
        public static T FromManagedToIL2CPP<T>(object obj)
        {
            return IL2CPPFromByteArray<T>(ToByteArray(obj));
        }
    }

    public static class PhotonEvents
    {
        public static void OpRaiseEvent(byte code, Object customObject, RaiseEventOptions RaiseEventOptions, SendOptions sendOptions) => PhotonNetwork.Method_Private_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0(code, customObject, RaiseEventOptions, sendOptions);
        public static void OpRaiseEvent(byte code, object customObject, RaiseEventOptions RaiseEventOptions, SendOptions sendOptions)
        {
            Object customObject2 = Serialization.FromManagedToIL2CPP<Object>(customObject);
            PhotonEvents.OpRaiseEvent(code, customObject2, RaiseEventOptions, sendOptions);
        }
    }
}
