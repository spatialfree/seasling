using UnityEngine;

[CreateAssetMenu]
public class Design : ScriptableObject
{
	[Header("Untitled")]
	public int stage;
	public float bgLoopLerp = 1;

	[Header("Player")]
	public float mass;
	public float maxVel;
	public float accel;
	public float fluidDynStr;
	public float hSpringStr;
	public float sSpringStr;
	public float combatMod;
	public float fallOff;
	public float shrimpPerHand;
	public float fogFallOff;
	public bool grappling;

	[Header("GlowShrimp")]
	public float shrDamageMult = 2;
	public float shrMinVel = 1;
	public float shrMaxVel = 8;
	public float shrDrag = 0.5f;
	public float shrRotate = 5;
	public float shrSwim = 0.5f;
	public float shrSwimRate = 2;
	public float shrPullRange = 1;
	public float shrPullForce = -3;
	public float shrSnapRange = 0.15f;
	public float shrOrbitSpeed = 150;
	public Color shrLColorShift;
	public Color shrRColorShift;

	[Header("Enemies")]
	public float spawnRadius = 20;
	public float dashRange = 10;

	[Header("Reference Var")]
	public int currentEnemies;

	public Vector3 playerPos;
	public Vector3 netVelocity;

	public Vector3 shrimpactPos;

	public int lShrimp;
	public Vector3 lHandPos;
	public Vector3 lSpring;
	public Vector3 lSpringVel;
	public Vector3 llSpring;

	public int rShrimp;
	public Vector3 rHandPos;
	public Vector3 rSpring;
	public Vector3 rSpringVel;
	public Vector3 rrSpring;

	[Header("Inputs")]
	public float lStick;
	public float rStick;
	public float lTrigger;
	public float rTrigger;
}
