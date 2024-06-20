using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowShrimp : MonoBehaviour
{
	[Header("References")]
	public Design design;
	public Rigidbody rb;
	public GameObject shrTrail;
	public Transform shrimpMesh;
	public float shrMag;

	public enum ShrimpState { idle, hold, release, embed };
	public ShrimpState shrState;

	Transform spawner;
	Vector3 vel;
	Vector3 rotVel;
	Vector3 newDir;
	float turnTiming;
	float swimTiming;
	float orbitRate;
	float shrimpID;
	Vector3 orbitPoint;
	float orbitSpacing = 1;
	Vector3 oldMeshPos;
	public bool leftie;
	float speedValue;
	float smoothAccel;
	Color emitColor;

	//MaterialPropertyBlock props;
	//public MeshRenderer rend;
	int playSpeed;
	int shdrColor;

	private void Start()
	{
		spawner = transform.parent;
		shrState = ShrimpState.idle;
		shrTrail.SetActive(false);
		orbitRate = Random.Range(design.shrOrbitSpeed / 4, design.shrOrbitSpeed);
		rb.drag = design.shrDrag;

		if (Random.value > 0.5f)
			orbitRate *= -1;
		
		// Randomize scale
		transform.localScale *= 1 - (Random.value * 0.5f);

		// Instanced Shader shit
		/* props = new MaterialPropertyBlock();
		playSpeed = Shader.PropertyToID("_PlaySpeed");
		shdrColor = Shader.PropertyToID("_EmissionColor");
		emitColor = Color.cyan;
		rend.GetPropertyBlock(props);
		props.SetFloat("_OffsetPlay", Random.value);
		rend.SetPropertyBlock(props); */
	}

	private void FixedUpdate()
	{
		if (shrState == ShrimpState.idle)
		{
			// Swim around the spawn point (orbit)
			if (Vector3.Distance(transform.position, transform.parent.position) > 1)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.parent.position - transform.position), 0.5f * Time.deltaTime * 0.25f);
			}
			else if (Time.time > turnTiming)
			{
				Vector3 randomDir = Random.rotation * Vector3.forward;
				rb.AddTorque(randomDir * Random.value);
				turnTiming = Time.time + (Random.value * 1.5f);
			}

			if (Time.time > swimTiming)
			{
				speedValue = Random.value + 0.5f;
				rb.AddRelativeForce(Vector3.forward * design.shrSwim * speedValue, ForceMode.Impulse);
				swimTiming = Time.time + 0.1f + (Random.value * design.shrSwimRate);
			}

			// Pull and transition
			float lDist = Vector3.Distance(transform.position, design.lSpring);
			float rDist = Vector3.Distance(transform.position, design.rSpring);

			PullIntoSnap(lDist, design.lShrimp, design.lSpring, design.lStick, true);
			PullIntoSnap(rDist, design.rShrimp, design.rSpring, design.rStick, false);

			// Custom Physics
			//rotVel *= 1 - (3 * Time.deltaTime);
			//transform.rotation *= Quaternion.Euler(rotVel * design.shrRotate * Time.deltaTime);

			//vel *= 1 - (design.shrDrag * Time.deltaTime);
			//vel = Vector3.ClampMagnitude(vel, design.shrMaxVel);
			//transform.position += vel * Time.deltaTime;

			rb.velocity = Vector3.ClampMagnitude(rb.velocity, design.shrMaxVel);
		}
	}

	private void Update()
	{	
		if (shrState == ShrimpState.hold)
		{
			if (leftie)
				Hold(design.lShrimp, design.lSpring, design.llSpring, design.shrLColorShift);
			else
				Hold(design.rShrimp, design.rSpring, design.rrSpring, design.shrRColorShift);

			// springVel = (orbitPoint - transform.position) / Time.deltaTime;

			// Orbit
			transform.rotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * orbitRate);
			transform.position = orbitPoint;

			newDir = shrimpMesh.position - oldMeshPos;
			shrMag = (newDir / Time.deltaTime).magnitude;

			if (newDir != Vector3.zero)
				shrimpMesh.rotation = Quaternion.Lerp(shrimpMesh.rotation, Quaternion.LookRotation(newDir), 0.5f * Time.deltaTime * 30 * (1 + newDir.magnitude));

			float orbitMod = 0.25f * Mathf.Lerp(1, 0.25f, orbitSpacing);
			shrimpMesh.localPosition = Vector3.right * Mathf.Lerp(shrimpMesh.localPosition.x, orbitMod + 0.05f, 0.5f * Time.deltaTime * 1.666f);
			oldMeshPos = shrimpMesh.position;

			shrTrail.GetComponent<TrailRenderer>().time = 0.1f + (0.5f * orbitSpacing);

			if (leftie && design.lStick == 0 || !leftie && design.rStick == 0)
			{
				HandRelease();
			}
		}
		else if (shrState == ShrimpState.release)
		{
			// convert to sqrMagnitude?
			float velMag = rb.velocity.magnitude;
			shrMag = velMag;

			if (velMag > design.shrMinVel)
			{
				emitColor = Color.Lerp(Color.magenta, Color.red, velMag / design.shrMaxVel);
			}
			else
			{
				emitColor = Color.cyan;
				shrTrail.SetActive(false);
				shrState = ShrimpState.idle;
			}
		}

		// Animate
			// note set speedValue to a value when coasting around the player
		/* rend.GetPropertyBlock(props);
		speedValue = Mathf.Clamp01(speedValue - Time.deltaTime);
		smoothAccel = Mathf.Lerp(smoothAccel, speedValue, 0.5f * Time.deltaTime * 30);
		props.SetFloat(playSpeed, smoothAccel * 0.25f);
		props.SetColor(shdrColor, emitColor);
		rend.SetPropertyBlock(props);
		props.SetColor(shdrColor, emitColor); */
	}

	private void OnCollisionEnter(Collision col)
	{
		EnemyCore enemyCore = col.transform.GetComponent<EnemyCore>();

		if (enemyCore != null)
		{
			if (shrMag > design.shrMinVel)
			{
				enemyCore.health -= design.shrDamageMult * shrMag;
				transform.parent.GetChild(0).GetComponent<Shrimpact>().QueuePos(col.contacts[0].point);

				// embed into the creature until it dies
				UpdateID();
				shrState = GlowShrimp.ShrimpState.embed;
				transform.GetChild(0).GetComponent<SphereCollider>().enabled = false;
				transform.SetParent(col.transform);

				shrTrail.SetActive(false);
				rb.isKinematic = true;
				Destroy(rb);

				// increase depth
					//transform.position += transform.rotation * (Vector3.forward * 0.01f);
			}
			else
			{
				HandRelease();
			}
		}
	}

	void HandRelease()
	{
		Reset();

		rb.isKinematic = false;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = newDir / Time.deltaTime;

		UpdateID();
		shrState = ShrimpState.release;
	}

	void UpdateID()
	{
		if (shrState == GlowShrimp.ShrimpState.hold)
		{
			if (leftie)
				design.lShrimp--;
			else
				design.rShrimp--;

			shrimpID = 0;
		}
	}

	void Reset()
	{
		transform.position = shrimpMesh.position;
		transform.rotation = shrimpMesh.rotation;
		shrimpMesh.localPosition = Vector3.zero;
		shrimpMesh.localRotation = Quaternion.identity;
	}

	public void Drop()
	{
		transform.SetParent(spawner);
		rb = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
		Reset();
		rb.useGravity = false;
		rb.angularDrag = 3;
		rb.drag = design.shrDrag;
		rb.isKinematic = false;
		emitColor = Color.cyan;
		transform.GetChild(0).GetComponent<SphereCollider>().enabled = true;
		shrState = ShrimpState.idle;
	}

	void PullIntoSnap(float dist, int shrimpInHand, Vector3 spring, float input, bool leftHand)
	{
		if (dist < design.shrPullRange && shrimpInHand < design.shrimpPerHand && input != 0)
		{
			rb.AddForce((transform.position - spring) * (design.shrPullRange - dist) * design.shrPullForce);

			if (dist < design.shrSnapRange)
			{
				shrState = ShrimpState.hold;
				rb.isKinematic = true;
				leftie = leftHand;
				transform.rotation = Random.rotation;
				transform.position = orbitPoint = spring;
				shrTrail.SetActive(true);

				if (leftHand)
					shrimpID = ++design.lShrimp;
				else
					shrimpID = ++design.rShrimp;
			}
		}
	}

	void Hold(int shrimpInHand, Vector3 firstSpring, Vector3 secondSpring, Color colorShift)
	{
		orbitSpacing = Mathf.Lerp(orbitSpacing, shrimpID / shrimpInHand, 0.5f * Time.deltaTime);
		orbitPoint = Vector3.Lerp(secondSpring, firstSpring, orbitSpacing);
		emitColor = colorShift;
	}
}
