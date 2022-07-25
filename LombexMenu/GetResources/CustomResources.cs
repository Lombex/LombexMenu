using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;
using MelonLoader;

namespace LombexMenu.GetResources
{
    public static class CustomResources
    {
        private static string StreamPath = "LombexMenu.GetResources.Bundles.quickmenutab.assetbundle";
        private static AssetBundle MenuSprite;   
        private static IEnumerator Bundle()
        {
            while (ReferenceEquals(VRCUiManager.prop_VRCUiManager_0, null)) yield return null;
            while (ReferenceEquals(NetworkManager.field_Internal_Static_NetworkManager_0, null)) yield return null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(StreamPath))
            {
                using (MemoryStream memorystream = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(memorystream);
                    CustomResources.MenuSprite = AssetBundle.LoadFromMemory_Internal(memorystream.ToArray(), 0);
                    CustomResources.MenuSprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }
                CustomResources.QuickMenuSprite = MenuSprite.LoadAsset_Internal("Assets/QuickMenu/LogoImage.png", Il2CppType.Of<Sprite>()).Cast<Sprite>();
                CustomResources.QuickMenuSprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }
            yield break;
        }

        public static Sprite QuickMenuSprite;
        public static void CreateBundle() => MelonCoroutines.Start(CustomResources.Bundle());
    }
}
