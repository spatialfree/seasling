using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	[Header("Variables")]
	public Vector2 number;
	public Vector2 radius;

	[Header("References")]
	public GameObject toSpawn;

	private void Start()
	{
		float numberOf = Random.Range(number.x, number.y);

		for (float i = 0; i < numberOf; i++)
		{
			Vector3 spawnPos = Random.rotation * Vector3.forward;
			spawnPos *= Random.Range(radius.x, radius.y);

			Instantiate(toSpawn, spawnPos + transform.position, Random.rotation, this.transform);
		}
	}
}
