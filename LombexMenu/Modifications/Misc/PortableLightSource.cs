using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;
using MelonLoader;

namespace Modifications.Misc
{
    public static class PortableLightSource
    {
        private static GameObject SetLightSource;
        private static IEnumerator SetupLightSource(bool State, string Color, float Intensity)
        {
            while (ReferenceEquals(PlayerUtils.GetVRCPlayer(), null)) yield return new WaitForEndOfFrame();                       
            if (SetLightSource == null)
            {
                SetLightSource = new GameObject();
                SetLightSource.name = "LombexMenuLightSource";
                SetLightSource.transform.parent = PlayerUtils.GetVRCPlayer().transform;
                SetLightSource.transform.localPosition = PlayerUtils.GetLocalViewPoint();
                Light GetLight = SetLightSource.AddComponent<Light>();
                GetLight.color = Utilities.ToColor(Color);
                GetLight.intensity = Intensity / 2;
                GetLight.range = Intensity;
                SetLightSource.SetActive(State);
            } else
            {
                Light GetLight = SetLightSource.GetComponent<Light>();
                GetLight.color = Utilities.ToColor(Color);
                GetLight.intensity = Intensity / 2;
                GetLight.range = Intensity;
                SetLightSource.SetActive(State);
            }
        }
        public static void SetupPortableLightSource(bool State, string Color, float Intensity) => MelonCoroutines.Start(SetupLightSource(State, Color, Intensity));
    }
}
