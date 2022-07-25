using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRCSDK2;
using MelonLoader;
using System.Collections;
using Utils;

namespace Modifications.Misc
{
    public static class PortableMirror
    {
        public static GameObject GetMirror;        
        private static void SetupMirror()
        {
            PortableMirror.GetMirror = GameObject.CreatePrimitive(PrimitiveType.Quad);
            PortableMirror.GetMirror.name = "LombexMenuMirror";

            PortableMirror.GetMirror.AddOrGetComponent<MeshRenderer>().material.shader = Shader.Find("FX/MirrorReflection");
            VRC_MirrorReflection Reflection = PortableMirror.GetMirror.AddComponent<VRC_MirrorReflection>();
            Reflection.m_DisablePixelLights = true;
            Reflection.TurnOffMirrorOcclusion = false;

            Rigidbody GetRigidBody = PortableMirror.GetMirror.AddComponent<Rigidbody>();
            GetRigidBody.isKinematic = true;
            GetRigidBody.useGravity = false;

            VRC_Pickup PickUpComponent = PortableMirror.GetMirror.AddComponent<VRC_Pickup>();
            PickUpComponent.name = "LombexMenuMirror";
            PickUpComponent.UseText = "LombexMenuMirror";
            PickUpComponent.proximity = 0.3f;
            PickUpComponent.pickupable = true;
            PickUpComponent.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
            PickUpComponent.DropEventName = string.Empty;
            PickUpComponent.PickupEventName = string.Empty;
            PickUpComponent.UseDownEventName = string.Empty;
            PickUpComponent.UseUpEventName = string.Empty;

            PortableMirror.GetMirror.AddComponent<VRC_EventHandler>();
            PortableMirror.GetMirror.AddComponent<ObjectInstantiator>();

            PortableMirror.GetMirror.layer = 13;
            PortableMirror.GetMirror.transform.localScale = new Vector3(3f, 2f, 1f);
        }
        private static IEnumerator CreateMirror()
        {
            while (ReferenceEquals(PlayerUtils.GetVRCPlayer(), null)) yield return new WaitForEndOfFrame();
            PortableMirror.SetupMirror();
            PortableMirror.GetMirror.SetActive(false);
            yield break;
        }
        public static void Start() => MelonCoroutines.Start(PortableMirror.CreateMirror());
    }
}
