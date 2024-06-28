using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StructureManager : NetworkBehaviour
{
	public static StructureManager Instance { get; private set; }

	public static uint lastId = 0;
	public static uint NewId() => lastId++;

	[field: SerializeField] public StructureDescription[] resourceNodeDescriptions { get; private set; }
	[field: SerializeField] public StructureDescription[] workbenchDescriptions { get; private set; }

	Dictionary<uint, Structure> spawnedStructures;

	void Awake()
	{
		Instance = this;
	}

	public override void OnNetworkSpawn()
	{
		if (IsServer)
		{
			spawnedStructures = new();
			WorldGenerationManager.Instance.Initialize();
		}
		else
		{
			// Do NOT initialize structures dictionary
		}
	}

	#region Spawn Structure
	public static void SpawnStructure<T>(T structureType, Vector3 worldPos) where T : Enum
	{
		if (NetworkManager.Singleton.IsServer)
		{
			if (structureType is ResourceNodeType resourceNodeType)
			{
				Instance.AddStructureRPC(resourceNodeType, worldPos);
			}
			else if (structureType is WorkbenchType workbenchType)
			{
				Instance.AddStructureRPC(workbenchType, worldPos);
			}
			else
			{
				throw new ArgumentException("Type must be ResourceNodeType or WorkbenchType");
			}
		}
		else
		{
			Debug.LogWarning("Do not attempt to spawn structures outside of the server");
		}
	}

	[Rpc(SendTo.Everyone)]
	public void AddStructureRPC(ResourceNodeType resourceNodeType, Vector3 worldPos)
	{
		Structure newResourceNode = Instantiate(resourceNodeDescriptions[(int)resourceNodeType].prefab).GetComponent<Structure>();
		newResourceNode.structureId = NewId();
		spawnedStructures.Add(newResourceNode.structureId, newResourceNode);

		newResourceNode.transform.position = worldPos;
	}
	[Rpc(SendTo.Everyone)]
	public void AddStructureRPC(WorkbenchType workbenchType, Vector3 worldPos)
	{
		Structure newWorkbench = Instantiate(workbenchDescriptions[(int)workbenchType].prefab).GetComponent<Structure>();
		newWorkbench.structureId = NewId();
		spawnedStructures.Add(newWorkbench.structureId, newWorkbench);

		newWorkbench.transform.position = worldPos;
	}
	#endregion Spawn Structure

	#region Despawn Structure
	public static void DespawnStructure(uint structureId)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Instance.DespawnStructureRPC(structureId);
		}
		else
		{
			Debug.LogWarning("Do not attempt to despawn structures outside of the server");
		}
	}

	[Rpc(SendTo.Everyone)]
	public void DespawnStructureRPC(uint structureId)
	{
		Destroy(spawnedStructures[structureId].gameObject);
		spawnedStructures.Remove(structureId);
	}
	#endregion Despawn Structure

	#region Item-Structure Interaction
	[Rpc(SendTo.Server)]
	public void InteractStructureRPC(uint inventorySelectionIndex, NetworkBehaviourReference plrInventoryNBR, uint structureId)
	{
		if (!plrInventoryNBR.TryGet(out NetworkBehaviour plrInventoryNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
		if (plrInventoryNB is not PlayerInventory playerInventory) { Debug.LogWarning("Input type does not match PlayerInventory"); return; }
		ItemSlot itemSlot = playerInventory.inventory[(int)inventorySelectionIndex];

		Structure selectedStructure = spawnedStructures[structureId];
		if (selectedStructure is ResourceNode resourceNode)
		{
			switch (itemSlot.description.itemType)
			{
				case ItemType.Axe:
					itemSlot.dataValue -= 1;
					playerInventory.ModifySlot((int)inventorySelectionIndex, itemSlot);
					resourceNode.Harvest();
					break;
			}
		}
		else if (selectedStructure is Workbench workbench)
		{
			throw new NotImplementedException();
		}
	}
	#endregion Item-Structure Interaction
}

public abstract class Structure : MonoBehaviour, ISelectable
{
	public uint structureId;
	public uint durability;

	[SerializeField] Material[] modelMaterials;
	[SerializeField] Renderer[] renderers;

	public void Highlight(bool on)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				Material[] newMats = renderers[i].materials;

				if (newMats.Length > 1)
				{
					newMats[1] = on ? ItemInteractionManager.Instance.outlineMaterial : modelMaterials[i];
					renderers[i].materials = newMats;
				}
			}
		}
	}
}