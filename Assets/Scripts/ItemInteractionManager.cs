using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public enum ItemType
{
	Axe,
	Blueprint,
	ClayBrick,
	Log,
	None,
	Rope,
	Stick,
	Stone,
	WetClayBrick,
	Wood
}

public sealed class ItemInteractionManager : NetworkBehaviour
{
	public static ItemInteractionManager Instance { get; private set;}
	static readonly Dictionary<ItemType[], ItemType[]> craftingRecipes = new Dictionary<ItemType[], ItemType[]>(new ArrayEqualityComparer<ItemType>())
	{
		{new ItemType[3]{ItemType.Rope, ItemType.Stick, ItemType.Stone}, new ItemType[1]{ItemType.Axe}}
		
	};
	[field: SerializeField] public ItemDescription[] itemDescriptions { get; private set; }
	public Material outlineMaterial;

	public delegate void SpawnedHandler();
	public event SpawnedHandler OnSpawn;

	public static ItemDescription[] ItemDescriptions => Instance.itemDescriptions;
	public static GameObject ItemPrefabs(int index) { return Instance.itemDescriptions[index].prefab; }
	
	public void Awake()
	{
		Instance = this;
	}

	public override void OnNetworkSpawn()
	{
		OnSpawn?.Invoke();
	}

	public bool RequestCraft(Vector3 craftPosition, NetworkBehaviourReference[] itemNBRs)
	{
		Item[] inputItems = new Item[itemNBRs.Length];
		
		// Validate arguments
		for (int i = 0; i < itemNBRs.Length; i++)
		{
			if (!itemNBRs[i].TryGet(out NetworkBehaviour itemNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return false; }
			if (itemNB is not Item) { Debug.LogWarning("Input type does not match Item"); return false; }
			inputItems[i] = itemNB as Item;
		}

		CraftingQuery craftingQuery = new CraftingQuery(inputItems, craftPosition);
		
		if (craftingRecipes.ContainsKey(craftingQuery.recipe))
		{
			RequestCraftRPC(craftPosition, itemNBRs);
			return true;
		}
		else
		{
			return false;
		}
	}
	
	[Rpc(SendTo.Server)]
	void RequestCraftRPC(Vector3 craftPosition, NetworkBehaviourReference[] itemNBRs)
	{
		// On Server
		Item[] inputItems = new Item[itemNBRs.Length];
		
		// Validate arguments
		for (int i = 0; i < itemNBRs.Length; i++)
		{
			if (!itemNBRs[i].TryGet(out NetworkBehaviour itemNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
			if (itemNB is not Item) { Debug.LogWarning("Input type does not match Item"); return; }
			inputItems[i] = itemNB as Item;
		}
		
		StartCoroutine(RequestCraftWrapper(inputItems, craftPosition, itemNBRs));
	}
	
	[Rpc(SendTo.Server)]
	public void RequestPickupRPC(NetworkBehaviourReference itemNBR, NetworkBehaviourReference playerNBR)
	{
		if (!itemNBR.TryGet(out NetworkBehaviour itemNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
		if (itemNB is not Item) { Debug.LogWarning("Input type does not match Item"); return; }
		
		if (!playerNBR.TryGet(out NetworkBehaviour playerNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
		if (playerNB is not PlayerInteract) { Debug.LogWarning("Player Network Behavior is not an instance of PlayerInteract"); return; }
		
		PlayerInteract player = playerNB as PlayerInteract;
		
		if (player.inventory.Add(itemNB as Item))
		{
			(itemNB as Item).ServerDestroy();
		}
	}
	
	[Rpc(SendTo.Server)]
	public void RequestDropRPC(uint hotbarIndex, NetworkBehaviourReference inventoryNBR)
	{
		if (!inventoryNBR.TryGet(out NetworkBehaviour inventoryNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
		if (inventoryNB is not PlayerInventory) { Debug.LogWarning("Input type does not match PlayerInventory"); return; }
		
		PlayerInventory plrInventory = inventoryNB as PlayerInventory;
		ItemSlot droppedItem = plrInventory.DropSelection();

		Spawn(droppedItem, plrInventory.transform.position + plrInventory.transform.forward + Vector3.up);
	}
	
	IEnumerator RequestCraftWrapper(Item[] inputItems, Vector3 craftPosition, NetworkBehaviourReference[] itemNBRs)
	{
		// On Server
		SendCraftingAnimationRPC(itemNBRs, craftPosition);
		yield return new WaitForSeconds(AnimationManager.CraftAnimationTime);
		// send animation, when its done, finalize the craft
		
		CraftRecipe(new CraftingQuery(inputItems, craftPosition));
	}
	
	void CraftRecipe(CraftingQuery craftingQuery)
	{
		if (!IsServer) { return; }
		
		if (craftingRecipes.ContainsKey(craftingQuery.recipe))
		{
			for (int i = 0; i < craftingQuery.recipeLength; i++)
			{
				craftingQuery.sortedItems[i].Damage();
			}

			foreach (ItemType item in craftingRecipes[craftingQuery.recipe])
			{
				SpawnNew(item, craftingQuery.worldPosition + RandomOffset(.25f));
			}
		}
	}

	[Rpc(SendTo.Server)]
	public void UseItemRPC(uint itemIndex, NetworkBehaviourReference inventoryNBR, NetworkBehaviourReference targetNBR)
	{
		#region Extract Data
		if (!inventoryNBR.TryGet(out NetworkBehaviour inventoryNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
		if (!(inventoryNB is PlayerInventory playerInventory)) { Debug.LogWarning("Input type does not match PlayerInventory"); return; }

		ItemSlot itemSlot = playerInventory.inventory[(int)itemIndex];

		if (!targetNBR.TryGet(out NetworkBehaviour targetNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
		if (!(targetNB is Item target)) { Debug.LogWarning("Target type was not Item"); return; }
		#endregion

		#region Handle Interaction
		Debug.LogWarning("Rewriting how to handle item-item interactions.");
		#endregion
	}

	#region Visual Connections
	[Rpc(SendTo.ClientsAndHost)]
	void SendCraftingAnimationRPC(NetworkBehaviourReference[] itemNBRs, Vector3 craftPosition)
	{
		// On All Clients
		Transform[] itemTransforms = new Transform[itemNBRs.Length];
		// Validate arguments
		for (int i = 0; i < itemNBRs.Length; i++)
		{
			if (!itemNBRs[i].TryGet(out NetworkBehaviour itemNB)) { Debug.LogWarning("Could not find attached NetworkBehavior"); return; }
			if (itemNB is not Item) { Debug.LogWarning("Input type does not match Item"); return; }
			
			itemTransforms[i] = itemNB.transform;
		}
		AnimationManager.Instance.AnimateCraft(itemTransforms, craftPosition);
	}
	#endregion

	#region Utility Functions
	static void Spawn(ItemSlot item, Vector3 worldPos)
	{
		if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("Spawn may not be called from outside the server"); return; }

		GameObject newItem = Instantiate(ItemDescriptions[(int)item.itemType].prefab);

		DataTag dataTag = new(item.DataKey, item.dataValue);
		newItem.GetComponent<Item>().initialDataTag = dataTag;
		
		newItem.transform.position = worldPos;
		newItem.GetComponent<NetworkObject>().Spawn(true);
	}
	
	public static void SpawnNew(ItemType itemType, Vector3 worldPos)
	{
		if (!NetworkManager.Singleton.IsServer) { Debug.LogWarning("SpawnNew may not be called from outside the server"); return; }
		
		ItemDescription itemDescription = ItemDescriptions[(int)itemType];
		GameObject newItem = Instantiate(itemDescription.prefab);

		Debug.Log(itemDescription.dataKey.ToString());
		DataTag dataTag = new DataTag(itemDescription.dataKey, itemDescription.dataValue);
		newItem.GetComponent<Item>().dataTag = 
			new NetworkVariable<DataTag>(dataTag, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
		newItem.transform.position = worldPos;
		
		newItem.GetComponent<NetworkObject>().Spawn(true);
	}

	static Vector3 RandomOffset(float scale)
	{
		Vector3 newVector = new(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
		newVector.Normalize();
		return newVector *= scale;
	}
	#endregion
}

public struct CraftingQuery
{
	public CraftingQuery(Item[] inputItems, Vector3 worldPos)
	{
		System.Array.Sort(inputItems, (x,y) => x.itemDescription.itemType.CompareTo(y.itemDescription.itemType));
		sortedItems = inputItems;
		worldPosition = worldPos;
	}

	public readonly Item[] sortedItems;
	public ItemType[] recipe => sortedItems.Select(item => item.itemDescription.itemType).ToArray();
	public int recipeLength => sortedItems.Length;
	
	public Vector3 worldPosition;
}

public class ArrayEqualityComparer<ItemType> : IEqualityComparer<ItemType[]>
{
	public bool Equals(ItemType[] x, ItemType[] y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		if (x.Length != y.Length) return false;
		for (int i = 0; i < x.Length; i++)
		{
			if (!EqualityComparer<ItemType>.Default.Equals(x[i], y[i])) return false;
		}
		return true;
	}

	public int GetHashCode(ItemType[] obj)
	{
		unchecked
		{
			int hash = 17;
			foreach (var item in obj)
			{
				hash = hash * 31 + (item != null ? item.GetHashCode() : 0);
			}
			return hash;
		}
	}
}