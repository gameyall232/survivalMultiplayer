using UnityEngine;

public class WetClayBrickItem : Item
{
	const float TIME_TO_DRY = 5f; // For testing only
	
	float timeToDry = TIME_TO_DRY;
	
	void Update()
	{
		if (IsServer)
		{
			timeToDry -= Time.deltaTime;

			if (timeToDry <= 0)
			{
				ItemInteractionManager.SpawnNew(ItemType.ClayBrick, transform.position);
				ServerDestroy();
			}
		}
	}
}