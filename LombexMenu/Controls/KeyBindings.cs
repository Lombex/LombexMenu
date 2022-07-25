using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;
using LombexMenu.Configuration;
using Modifications.Movement;
using Modifications.Misc;
using Modifications.Visuals;
using ReMod.Core.VRChat;

namespace Controls
{
    public static class KeyBindings
    {
        public static void OnKeyPress()
        {
            if (!Input.anyKeyDown) return;
            if (PlayerUtils.GetVRCPlayer() != null && Event.current.control)
            {
                if (Input.GetKeyDown(KeyCode.KeypadMultiply)) Utilities.RestartGame();
                if (Input.GetKeyDown(KeyCode.Delete)) Utilities.DeleteAllPortals();
                if (Input.GetKeyDown(KeyCode.F))
                {
                    StaticConfig.Flight = !StaticConfig.Flight;
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
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    StaticConfig.PortableMirror = !StaticConfig.PortableMirror;
                    PortableMirror.GetMirror.SetActive(StaticConfig.PortableMirror);
                    Vector3 Position = PlayerUtils.GetVRCPlayer().transform.position + PlayerUtils.GetVRCPlayer().transform.forward;
                    Position.y += 1f;
                    PortableMirror.GetMirror.transform.position = Position;
                    PortableMirror.GetMirror.transform.rotation = PlayerUtils.GetVRCPlayer().transform.rotation;
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    StaticConfig.PortableBall = !StaticConfig.PortableBall;
                    PortableBall.GetPortableBall.SetActive(StaticConfig.PortableBall);
                    Vector3 pos = PlayerUtils.GetLocalViewPoint() + PlayerUtils.GetVRCPlayer().transform.forward;
                    PortableBall.GetPortableBall.transform.position = pos;
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    ThirdPerson.CameraSetup++;
                    if (ThirdPerson.CameraSetup > 2) ThirdPerson.CameraSetup = 0;
                }
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    StaticConfig.IKFreeze = !StaticConfig.IKFreeze;
                    Utilities.CloneLocalAvatar();
                    SetConsoleColor.WriteEmbeddedColorLine(StaticConfig.IKFreeze ? "IKFreeze Has Been Activated!" : "IKFreeze Has Been Deactivated!", SetConsoleColor.ConsoleLogType.Message);                   
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    StaticConfig.EarrapeMic = !StaticConfig.EarrapeMic;
                    SetConsoleColor.WriteEmbeddedColorLine(StaticConfig.EarrapeMic ? "EarrapeMic Has Been Activated!" : "EarrapeMic Has Been Deactivated!", SetConsoleColor.ConsoleLogType.Message);
                    if (StaticConfig.EarrapeMic) PlayerUtils.GetVRCPlayer().prop_Player_0.prop_USpeaker_0.field_Private_Single_1 = float.MaxValue;
                    else PlayerUtils.GetVRCPlayer().prop_Player_0.prop_USpeaker_0.field_Private_Single_1 = 1f;
                }
                // temp
                if (Input.GetKeyDown(KeyCode.L)) LombexMenu.UI.CustomNameplates.CreateRankNameplate(LombexMenu.UI.CustomNameplates.Rank.Admin, PlayerUtils.GetVRCPlayer().prop_Player_0);
                if (Input.GetKeyDown(KeyCode.Mouse0)) PlayerUtils.GetVRCPlayer().transform.position += PlayerUtils.GetVRCPlayer().transform.forward * 1f;
            }
            if (PlayerUtils.GetVRCPlayer() != null && Config.Instance.InfiniteJump)
            {
                if (VRCInputManager.Method_Public_Static_VRCInput_String_0("Jump").prop_Boolean_2 && !PlayerUtils.GetVRCPlayer().field_Private_VRCPlayerApi_0.IsPlayerGrounded())
                {
                    Vector3 velocity = VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Private_VRCPlayerApi_0.GetVelocity();
                    velocity.y = PlayerUtils.GetVRCPlayer().field_Private_VRCPlayerApi_0.GetJumpImpulse();
                    PlayerUtils.GetVRCPlayer().field_Private_VRCPlayerApi_0.SetVelocity(velocity);
                }
            }
        }
    }
}
