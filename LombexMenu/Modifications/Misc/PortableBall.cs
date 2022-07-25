using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using System.Collections;
using Utils;
using VRCSDK2;

namespace Modifications.Misc
{
    public static class PortableBall
    {
        public static GameObject GetPortableBall;
        private static void SetupPortableBall()
        {
            PortableBall.GetPortableBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            PortableBall.GetPortableBall.name = "LombexMenuPortableBall";

            Rigidbody rigidbody = PortableBall.GetPortableBall.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            VRC_Pickup PickUpComponent = PortableBall.GetPortableBall.AddComponent<VRC_Pickup>();
            PickUpComponent.name = "LombexMenuPortableBall";
            PickUpComponent.UseText = "PortableBall";
            PickUpComponent.proximity = 25f;
            PickUpComponent.pickupable = true;
            PickUpComponent.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
            PickUpComponent.DropEventName = string.Empty;
            PickUpComponent.PickupEventName = string.Empty;
            PickUpComponent.UseDownEventName = string.Empty;
            PickUpComponent.UseUpEventName = string.Empty;

            PortableBall.GetPortableBall.AddComponent<VRC_EventHandler>();
            PortableBall.GetPortableBall.AddComponent<ObjectInstantiator>();

            PortableBall.GetPortableBall.layer = 13;
            PortableBall.GetPortableBall.transform.localScale = new Vector3(-0.1f, -0.1f, -0.1f);
        }
        private static IEnumerator CreatePortableBall()
        {
            while (ReferenceEquals(PlayerUtils.GetVRCPlayer(), null)) yield return new WaitForEndOfFrame();
            PortableBall.SetupPortableBall();
            PortableBall.GetPortableBall.SetActive(false);
            yield break;
        }
        public static void Start() => MelonCoroutines.Start(PortableBall.CreatePortableBall());
    }
}
