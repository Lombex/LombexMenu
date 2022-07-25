using System;
using System.Linq;
using UnityEngine;
using Utils;
using LombexMenu.Configuration;
using System.Reflection;

namespace Modifications.Movement
{
    public static class Flight
    {
        private static readonly string LeftHorizontal = "Horizontal";
        private static readonly string LeftTrigger = "Oculus_CrossPlatform_PrimaryIndexTrigger";
        private static readonly string LeftVertical = "Vertical";
        private static readonly string RightVertical = "Oculus_CrossPlatform_SecondaryThumbstickVertical";    
        public static void OnFlightMode()
        {
            if (PlayerUtils.GetVRCPlayer() != null && PlayerUtils.GetVRCPlayer().GetComponent<LocomotionInputController>() != null) 
            {
                float ShiftSpeed = Event.current.shift ? Config.Instance.FlightSpeed * 2 : Config.Instance.FlightSpeed;
                Transform GetPlayer = PlayerUtils.GetVRCPlayer().transform;
                if (PlayerUtils.IsInVR)
                {
                    ShiftSpeed = Mathf.Lerp(Config.Instance.FlightSpeed, Config.Instance.FlightSpeed * 2, Input.GetAxis(LeftTrigger));
                    if (Math.Abs(Input.GetAxis(LeftVertical)) > 0) GetPlayer.position += ShiftSpeed * Time.deltaTime * Input.GetAxis(LeftVertical) * GetPlayer.forward;
                    if (Math.Abs(Input.GetAxis(LeftHorizontal)) > 0) GetPlayer.position += ShiftSpeed * Time.deltaTime * Input.GetAxis(LeftHorizontal) * GetPlayer.right;
                    if (Math.Abs(Input.GetAxis(RightVertical)) > 0.1f) GetPlayer.position += ShiftSpeed * Time.deltaTime * Input.GetAxis(RightVertical) * Vector3.up;
                }
                else if (Input.anyKey)
                {
                    if (Input.GetKey(KeyCode.W)) GetPlayer.position += GetPlayer.forward * ShiftSpeed * Time.deltaTime;
                    if (Input.GetKey(KeyCode.A)) GetPlayer.position += GetPlayer.right * -1f * ShiftSpeed * Time.deltaTime;
                    if (Input.GetKey(KeyCode.S)) GetPlayer.position += GetPlayer.forward * -1f * ShiftSpeed * Time.deltaTime;
                    if (Input.GetKey(KeyCode.D)) GetPlayer.position += GetPlayer.right * ShiftSpeed * Time.deltaTime;
                    if (Input.GetKey(KeyCode.Q)) GetPlayer.position += GetPlayer.up * -1f * ShiftSpeed * Time.deltaTime;
                    if (Input.GetKey(KeyCode.E)) GetPlayer.position += GetPlayer.up * ShiftSpeed * Time.deltaTime;

                    if (Input.GetKey(KeyCode.I)) GetPlayer.RotateAround(PlayerUtils.GetVRCPlayer().transform.position, GetPlayer.right, Config.Instance.RotationSpeed * Config.Instance.FlightSpeed * Time.deltaTime);
                    if (Input.GetKey(KeyCode.K)) GetPlayer.RotateAround(PlayerUtils.GetVRCPlayer().transform.position, -GetPlayer.right, Config.Instance.RotationSpeed * Config.Instance.FlightSpeed * Time.deltaTime);
                    if (Input.GetKey(KeyCode.J)) GetPlayer.RotateAround(PlayerUtils.GetVRCPlayer().transform.position, GetPlayer.forward, Config.Instance.RotationSpeed * Config.Instance.FlightSpeed * Time.deltaTime);
                    if (Input.GetKey(KeyCode.L)) GetPlayer.RotateAround(PlayerUtils.GetVRCPlayer().transform.position, -GetPlayer.forward, Config.Instance.RotationSpeed * Config.Instance.FlightSpeed * Time.deltaTime);

                    Rotation.AlineTrackingToPlayer();
                }
            }
        }
    }
    public static class Rotation
    {
        private static MethodInfo AlineTrackingToPlayerMethod;
        internal delegate void AlineTrackingDelegate();
        internal static AlineTrackingDelegate AlineTrackingToPlayer;
        internal static AlineTrackingDelegate GetAlineTrackingDelegate
        {
            get
            {
                if (AlineTrackingToPlayerMethod == null)
                {
                    AlineTrackingToPlayerMethod = typeof(VRCPlayer).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).First(m => m.ReturnType == typeof(void)
                        && m.GetParameters().Length == 0 && m.Name.IndexOf("PDM", StringComparison.OrdinalIgnoreCase) == -1 && m.XRefScanForMethod("get_Transform", null)
                        && m.XRefScanForMethod(null, nameof(VRCTrackingManager)) && m.XRefScanForMethod(null, nameof(InputStateController)));
                }
                return (AlineTrackingDelegate)Delegate.CreateDelegate(typeof(AlineTrackingDelegate), PlayerUtils.GetVRCPlayer(), AlineTrackingToPlayerMethod);
            }
        }
        internal static void ToggleRotation(bool State)
        {
            try
            {
                if (State && AlineTrackingToPlayer == null) AlineTrackingToPlayer = Rotation.GetAlineTrackingDelegate;
                else
                {
                    Quaternion localRotation = PlayerUtils.GetVRCPlayer().transform.localRotation;
                    PlayerUtils.GetVRCPlayer().transform.localRotation = new Quaternion(0f, localRotation.y, 0f, localRotation.w);
                }
            } catch (Exception e)
            {
                SetConsoleColor.WriteEmbeddedColorLine("Something Went Wrong with rotation: " + e.Message, SetConsoleColor.ConsoleLogType.Error);
                State = false;
            }
            if (State) return;
            AlineTrackingToPlayer?.Invoke();
        }
    }
}
