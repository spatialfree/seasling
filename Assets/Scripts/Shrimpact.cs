using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Shrimpact : MonoBehaviour
{
	[Header("Referencese")]
	public Design design;
	public ParticleSystem ps;

	Vector3[] positionQueue;

	private void Start()
	{
		positionQueue = new Vector3[0];	
	}

	private void Update()
	{
		int queueLength = positionQueue.Length;
		if (queueLength > 0)
		{
			transform.position = positionQueue[--queueLength];
			//Play Particle effect
			ps.Emit(1);

			Array.Resize(ref positionQueue, positionQueue.Length - 1);
		}

		if (Input.GetKeyDown("space"))
		{
			QueuePos(Vector3.zero);
		}
	}

	public void QueuePos(Vector3 pos)
	{;
		//Vector3[] temp = new Vector3[index + 1];
		Array.Resize(ref positionQueue, positionQueue.Length + 1);
		positionQueue[positionQueue.Length - 1] = pos;
	}
}
