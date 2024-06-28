using UnityEngine;

[CreateAssetMenu(fileName = "StructureDescription", menuName = "ScriptableObjects/Structure Description", order = 2)]
public class StructureDescription : ScriptableObject
{
	public GameObject prefab;

	public int maxDurability;
}