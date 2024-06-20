using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
	public bool leftHand;

	[Header("References")]
	public Design design;

	private void OnTriggerEnter(Collider col)
	{
		// hand animations wont close to a fist when pressing but more of a ready to grab something posture it also gives the shrimp acceleration that decays with distance to player

		if (leftHand)
			Grab(OVRInput.Controller.RTouch, col);
		else
			Grab(OVRInput.Controller.LTouch, col);
	}

	void Grab(OVRInput.Controller controller, Collider col)
	{
		if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
		{
			// Grip check relative velocity of the object you wish to grab

			// On grab disengage their individual velocities but keep their values
			// weight them against each etc. etc. link the final combined velocity to both of them
			//design.netVelocity = ;
		}
	}
}
