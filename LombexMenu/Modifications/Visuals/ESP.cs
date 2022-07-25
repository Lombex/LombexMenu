using LombexMenu.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Modifications.Visuals
{
    public static class ESP
    {
        public static void PropESP()
        {
            HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0.SetColor("_HighlightColor", Utilities.ToColor(Config.Instance.ESPColor));
            foreach (VRC.SDKBase.VRC_Pickup PickUps in Resources.FindObjectsOfTypeAll<VRC.SDKBase.VRC_Pickup>()) HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(PickUps.GetComponent<Renderer>(), Config.Instance.PropESP);
            foreach (VRC.SDKBase.VRC_Trigger trigger in Resources.FindObjectsOfTypeAll<VRC.SDKBase.VRC_Trigger>()) HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(trigger.GetComponent<Renderer>(), Config.Instance.PropESP);
        } 
        public static void CapsuleESP()
        {
            HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0.SetColor("_HighlightColor", Utilities.ToColor(Config.Instance.ESPColor));
            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
            {
                Transform obj = gameObject.transform.Find("SelectRegion");
                if (obj) HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(obj.GetComponent<Renderer>(), Config.Instance.CapsuleESP);
            }
        }
    }
}
