using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCore : MonoBehaviour
{
	[Header("Variables")]
	public float health = 10;
	public float turnStrength = 5;
	public float swimStrength = 1;
	public float maxVel = 3;

	[Header("References")]
	public Design design;
	public Rigidbody rb;

	float distFactor;

	private void Update()
	{
		if (health < 0)
		{
			foreach (Transform child in transform)
			{
				GlowShrimp glowShrimp = child.GetComponent<GlowShrimp>();
				if (glowShrimp != null)
				{
					glowShrimp.Drop();
				}
			}

			design.currentEnemies--;
			Destroy(this.gameObject);
		}

		distFactor = Mathf.Clamp01(Vector3.Distance(transform.position, design.playerPos) / design.dashRange);
	}

	private void FixedUpdate()
	{
		// Scale swimSpeed(increase) and rotation speed(slow) with proximity to player
		rb.rotation = Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(design.playerPos - transform.position), 0.5f * Time.deltaTime * turnStrength * distFactor * distFactor);
		rb.AddRelativeForce(Vector3.forward * swimStrength * (1.1f - (distFactor * distFactor)));

		rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVel);
	}
}
