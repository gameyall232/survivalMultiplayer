using UnityEngine;

public class WorldGenerationManager : MonoBehaviour
{
	public static WorldGenerationManager Instance { get; private set;}
	
	void Awake()
	{
		Instance = this;
	}
	
	public void Initialize()
	{
		for (int i = 0; i < 20; i++)
		{
			float range = 20;
			Vector3 randomPos = new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
			StructureManager.SpawnStructure(StructureType.Tree, randomPos);
		}		
	}
}