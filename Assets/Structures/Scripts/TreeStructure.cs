using UnityEngine;

public class TreeStructure : ResourceNode
{
	public override void Harvest()
	{
		durability--;
		
		if (durability <= 0)
		{
			// TODO: REPLACE WITH THE BIG LOGG
			ItemInteractionManager.SpawnNew(ItemType.Log, transform.position + Vector3.up);
			ItemInteractionManager.SpawnNew(ItemType.Log, transform.position + Vector3.up);
			ItemInteractionManager.SpawnNew(ItemType.Log, transform.position + Vector3.up);
			StructureManager.DespawnStructure(structureId);
		}
	}
}