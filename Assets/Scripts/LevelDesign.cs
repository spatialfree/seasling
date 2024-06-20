using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDesign : MonoBehaviour
{
	[Tooltip("what to spawn | how many | nextStep | modifier")]
	public Vector4[] lvlDesign;

	[Header("References")]
	public Design design;
	public GameObject[] enemies;

	private bool started = false;
	private float timer;

	private void Start()
	{
		design.stage = 0;
		design.currentEnemies = 0;

		if (lvlDesign.Length == 0)
		{
			// End
			Destroy(this);
		}
	}

	private void Update()
	{
		Vector4 stage = lvlDesign[design.stage];
		Vector3Int stageInt = new Vector3Int(Mathf.RoundToInt(stage.x), Mathf.RoundToInt(stage.y), Mathf.RoundToInt(stage.z));

		if (!started)
		{
			// Convert to int where needed
			for (int i = 0; i < stageInt.y; i++)
			{
				Vector3 spawnPos = Random.rotation * Vector3.forward * design.spawnRadius;
				Quaternion spawnRot = Quaternion.identity;
				Instantiate(enemies[stageInt.x], spawnPos, spawnRot, this.transform);
				design.currentEnemies++;
			}

			started = true;
			timer = Time.time + stage.w;
		}
		else
		{
			// stage progression modifier
			if (stageInt.z == 0)
				NextStage();

			if (stageInt.z == 1 && Time.time > timer)
				NextStage();

			if (stageInt.z == 2 && design.currentEnemies <= stage.w)
				NextStage();

			if (stageInt.z == 3)
			{
				// Empty, future modifiers?
				NextStage();
			}
		}
	}

	void NextStage()
	{
		design.stage++;
		started = false;

		if (design.stage + 1 > lvlDesign.Length)
		{
			// End
			Destroy(this);
		}
	}
}
