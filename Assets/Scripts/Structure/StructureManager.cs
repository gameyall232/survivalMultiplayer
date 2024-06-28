using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum StructureType
{
	OreDeposit,
	Tree,
	WoodworkingTable
}

public class StructureManager : NetworkBehaviour
{
	public static StructureManager Instance { get; private set; }

	public static uint lastId = 0;
	public static uint NewId() => lastId++;

	[field: SerializeField] public StructureDescription[] structureDescriptions { get; private set; }

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
			spawnedStructures = new();
			RequestLoadStructuresRPC(NetworkManager.LocalClientId);
		}
	}

	#region Spawn Structure
	public static void SpawnStructure(StructureType structureType, Vector3 worldPos)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Instance.AddStructureRPC(structureType, worldPos);
		}
		else
		{
			Debug.LogWarning("Do not attempt to spawn structures outside of the server");
		}
	}

	[Rpc(SendTo.Everyone)]
	public void AddStructureRPC(StructureType structureType, Vector3 worldPos)
	{
		Structure newResourceNode = Instantiate(structureDescriptions[(int)structureType].prefab).GetComponent<Structure>();
		newResourceNode.structureId = NewId();
		spawnedStructures.Add(newResourceNode.structureId, newResourceNode);

		newResourceNode.transform.position = worldPos;
	}
	#endregion Spawn Structure

	#region Synchronize Structures
	[Rpc(SendTo.Server)]
	public void RequestLoadStructuresRPC(ulong newClientId)
	{
		LoadStructuresRPC(new StructurePackage(spawnedStructures), RpcTarget.Single(newClientId, RpcTargetUse.Temp));
	}

	[Rpc(SendTo.SpecifiedInParams)]
	public void LoadStructuresRPC(StructurePackage structurePackage, RpcParams rpcParams)
	{
		BaseRpcTarget target = RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Persistent);
		
		for (int i = 0; i < structurePackage.structureIds.Length; i++)
		{
			LoadStructure(
				structurePackage.structureIds[i],
				structurePackage.structureTypes[i], 
				structurePackage.positions[i],
				structurePackage.durabilities[i]);
		}

		target.Dispose();
	}
	
	void LoadStructure(uint structureId, StructureType structureType, Vector3 position, uint durability)
	{
	// 	Structure newResourceNode = Instantiate(structureDescriptions[(int)structureType].prefab).GetComponent<Structure>();
	// 	newResourceNode.structureId = NewId();
	// 	spawnedStructures.Add(newResourceNode.structureId, newResourceNode);

	// 	newResourceNode.transform.position = worldPos;
	}

	public struct StructurePackage : INetworkSerializable
	{
		public uint[] structureIds;
		public StructureType[] structureTypes;
		public Vector3[] positions;
		public uint[] durabilities;

		public StructurePackage(Dictionary<uint, Structure> structuresDict)
		{
			int iterator = 0;
			int length = structuresDict.Count;
			structureIds = new uint[length];
			structureTypes = new StructureType[length];
			positions = new Vector3[length];
			durabilities = new uint[length];

			foreach (KeyValuePair<uint, Structure> kvp in structuresDict)
			{
				structureIds[iterator] = kvp.Key;
				structureTypes[iterator] = kvp.Value.structureType;
				positions[iterator] = kvp.Value.transform.position;
				durabilities[iterator] = kvp.Value.GetComponent<Structure>().durability;
				iterator++;
			}
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref structureIds);
			serializer.SerializeValue(ref positions);
			serializer.SerializeValue(ref durabilities);
		}
	}
	#endregion Synchronize Structures

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
	public StructureType structureType;
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