using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
	[SerializeField] PlayerInteract interact;
	[SerializeField] PlayerInventory inventory;
	
	[SerializeField] Transform hand;

	// Blueprints
	[SerializeField] Material blueprintMaterial;
	bool placing;
	GameObject blueprintGO;

	void Update()
	{
		if (placing)
		{
			
		}
	}

	public void UpdateHolding(ItemType activeSlot)
	{
		if (hand.childCount > 0)
		{
			for (int i = 0; i < hand.childCount; i++)
			{
				Destroy(hand.GetChild(i).gameObject);
			}
		}

		ItemDescription description = ItemInteractionManager.ItemDescriptions[(int)activeSlot];
		GameObject model = description.prefab.transform.GetChild(0).gameObject;
		Transform newModel = Instantiate(model, hand).transform;

		newModel.localPosition = Vector3.zero;
		newModel.localEulerAngles = Vector3.zero;	
	}
	
	public void StartPlacing(StructureDescription structureDescription)
	{
		blueprintGO = Instantiate(structureDescription.prefab);
		Material structMat = blueprintGO.GetComponent<Renderer>().material;
		blueprintGO.GetComponent<Renderer>().material = blueprintMaterial;
		Material blueprintMat = blueprintGO.GetComponent<Renderer>().material;
		blueprintMat.SetTexture("_Albedo", structMat.GetTexture("_BaseMap"));
		blueprintMat.SetTexture("_Normal", structMat.GetTexture("_BumpMap"));
		blueprintMat.SetTexture("_Metallic", structMat.GetTexture("_MetallicGlossMap"));
		blueprintMat.SetTexture("_Smoothness", structMat.GetTexture("_OcclusionMap"));
		placing = true;
	}
	
	public void StopPlacing()
	{
		Destroy(blueprintGO);
		placing = false;
	}
	
	public void ResetVisuals()
	{
		StopPlacing();
	}
}