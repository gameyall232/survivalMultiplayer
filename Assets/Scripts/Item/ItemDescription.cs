using UnityEngine;

[CreateAssetMenu(fileName = "ItemDescription", menuName = "ScriptableObjects/Item Description", order = 1)]
public class ItemDescription : ScriptableObject
{
	public GameObject prefab;
	public Sprite icon;
	
	public ItemType itemType;
	public uint weight;

	[Header("Data Tag")]
	public DataKeyType dataKey;
	public float dataValue;
}