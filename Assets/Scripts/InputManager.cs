using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	[Header("References")]
	public Design design;
	
	private void Update()
	{
		design.lStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).y;
		design.rStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;

		if (design.lStick != 0 && design.lShrimp != 0)
			design.lTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
		else
			design.lTrigger = 0;

		if (design.rStick != 0 && design.rShrimp != 0)
			design.rTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
		else
			design.rTrigger = 0;
	}
}
