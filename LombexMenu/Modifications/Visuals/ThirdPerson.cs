using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using LombexMenu.Configuration;
using Utils;

namespace Modifications.Visuals
{
    public static class ThirdPerson
    {
		private static GameObject TPCameraBack;
		private static GameObject TPCameraFront;
		private static GameObject referenceCamera;
		private static float zoomOffset;
		public static int CameraSetup;
		private static void Initialize()
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			UnityEngine.Object.Destroy(gameObject.GetComponent<MeshRenderer>());
			ThirdPerson.referenceCamera = GameObject.Find("Camera (eye)");
			if (ThirdPerson.referenceCamera != null)
			{
				gameObject.transform.localScale = ThirdPerson.referenceCamera.transform.localScale;
				Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
				rigidbody.isKinematic = true;
				rigidbody.useGravity = false;
				if (gameObject.GetComponent<Collider>()) gameObject.GetComponent<Collider>().enabled = false;
				gameObject.GetComponent<Renderer>().enabled = false;
				gameObject.AddComponent<Camera>();
				GameObject gameObject2 = ThirdPerson.referenceCamera;
				gameObject.transform.parent = gameObject2.transform;
				gameObject.transform.rotation = gameObject2.transform.rotation;
				gameObject.transform.position = gameObject2.transform.position;
				gameObject.transform.position -= gameObject.transform.forward * 2f;
				gameObject2.GetComponent<Camera>().enabled = false;
				gameObject.GetComponent<Camera>().fieldOfView = 75f;
				gameObject.GetComponent<Camera>().nearClipPlane /= 4f;
				ThirdPerson.TPCameraBack = gameObject;
				GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				UnityEngine.Object.Destroy(gameObject3.GetComponent<MeshRenderer>());
				gameObject3.transform.localScale = ThirdPerson.referenceCamera.transform.localScale;
				Rigidbody rigidbody2 = gameObject3.AddComponent<Rigidbody>();
				rigidbody2.isKinematic = true;
				rigidbody2.useGravity = false;
				if (gameObject3.GetComponent<Collider>()) gameObject3.GetComponent<Collider>().enabled = false;
				gameObject3.GetComponent<Renderer>().enabled = false;
				gameObject3.AddComponent<Camera>();
				gameObject3.transform.parent = gameObject2.transform;
				gameObject3.transform.rotation = gameObject2.transform.rotation;
				gameObject3.transform.Rotate(0f, 180f, 0f);
				gameObject3.transform.position = gameObject2.transform.position;
				gameObject3.transform.position += -gameObject3.transform.forward * 2f;
				gameObject2.GetComponent<Camera>().enabled = false;
				gameObject3.GetComponent<Camera>().fieldOfView = 75f;
				gameObject3.GetComponent<Camera>().nearClipPlane /= 4f;
				ThirdPerson.TPCameraFront = gameObject3;
				ThirdPerson.TPCameraBack.GetComponent<Camera>().enabled = false;
				ThirdPerson.TPCameraFront.GetComponent<Camera>().enabled = false;
				GameObject.Find("Camera (eye)").GetComponent<Camera>().enabled = true;
				MelonCoroutines.Start(ThirdPerson.Loop());
			}
		}
		private static IEnumerator Loop()
		{
			while (true)
			{
				if (ThirdPerson.TPCameraBack != null && ThirdPerson.TPCameraFront != null)
				{
					if (ThirdPerson.CameraSetup == 0)
					{
						ThirdPerson.TPCameraBack.GetComponent<Camera>().enabled = false;
						ThirdPerson.TPCameraFront.GetComponent<Camera>().enabled = false;
					}
					else if (ThirdPerson.CameraSetup == 1)
					{
						ThirdPerson.TPCameraBack.GetComponent<Camera>().enabled = true;
						ThirdPerson.TPCameraFront.GetComponent<Camera>().enabled = false;
					}
					else if (ThirdPerson.CameraSetup == 2)
					{
						ThirdPerson.TPCameraBack.GetComponent<Camera>().enabled = false;
						ThirdPerson.TPCameraFront.GetComponent<Camera>().enabled = true;
					}
					if (ThirdPerson.CameraSetup != 0)
					{
						if (Input.GetKeyDown(KeyCode.Escape)) ThirdPerson.CameraSetup = 0;
						float axis = Input.GetAxis("Mouse ScrollWheel");
						if (axis > 0f)
						{
							ThirdPerson.TPCameraBack.transform.position += ThirdPerson.TPCameraBack.transform.forward * 0.1f;
							ThirdPerson.TPCameraFront.transform.position -= ThirdPerson.TPCameraBack.transform.forward * 0.1f;
							ThirdPerson.zoomOffset += 0.1f;
						}
						else if (axis < 0f)
						{
							ThirdPerson.TPCameraBack.transform.position -= ThirdPerson.TPCameraBack.transform.forward * 0.1f;
							ThirdPerson.TPCameraFront.transform.position += ThirdPerson.TPCameraBack.transform.forward * 0.1f;
							ThirdPerson.zoomOffset -= 0.1f;
						}
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		private static IEnumerator SetupThirdPerson()
        {
			while (ReferenceEquals(GameObject.Find("Camera (eye)"), null)) yield return new WaitForEndOfFrame();
			ThirdPerson.Initialize();
			yield break;
        }
		public static void Start() => MelonCoroutines.Start(ThirdPerson.SetupThirdPerson());
	}
	public static class FOVChanger
    {
		private static float FieldOfView = Config.Instance.FieldOfView;
		public static void Update()
        {
			if (PlayerUtils.GetVRCPlayer() != null && Camera.main != null && ThirdPerson.CameraSetup == 0)
            {
				if (!Application.isFocused)
                {
					FOVChanger.FieldOfView = Config.Instance.FieldOfView;
					GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/ReticleParent")?.SetActive(true);
                }
				if (Utilities.GetAxis("Mouse ScrollWheel", true, false) < 0f) FOVChanger.FieldOfView += 1f;
				if (Utilities.GetAxis("Mouse ScrollWheel", true, false) > 0f) FOVChanger.FieldOfView -= 1f;
				if (GetMouseButton(2, true, false)) FOVChanger.FieldOfView = Config.Instance.FieldOfView;
				Camera.main.fieldOfView = FOVChanger.FieldOfView;
            }
        }
		private static bool GetMouseButton(int Button, bool Control = false, bool Shift = false)
        {
			bool flag = !Control;
			bool flag2 = !Shift;
			bool flag3 = Control && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
			if (flag3) flag = true;
			bool flag4 = Shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			if (flag4) flag2 = true;
			return flag && flag2 && Input.GetMouseButtonDown(Button);
		}
	}
}
