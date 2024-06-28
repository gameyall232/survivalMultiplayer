using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

public class PlayerInventory : NetworkBehaviour
{
	public const int MAX_WEIGHT = 100;
	[SerializeField] ClientInput input;

	public NetworkList<ItemSlot> inventory { get; private set; }
	public uint selectionIndex;
	
	public ItemSlot selection => inventory[(int)selectionIndex];
	public uint weight
	{
		get
		{
			uint w = 0;
			foreach (ItemSlot itemSlot in inventory)
			{
				w += itemSlot.description.weight * itemSlot.quantity;
			}
			return w;
		}
	}
	
	public delegate void InventoryChangedHandler(ItemSlot activeSlot);
	public event InventoryChangedHandler InventoryChanged;

	[SerializeField] PlayerInteract interact;
	[SerializeField] PlayerVisuals characterVisuals;

	void Awake()
	{
		inventory = new NetworkList<ItemSlot>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	}

	public override void OnNetworkSpawn()
	{
		if (IsOwner)
		{
			input.Primary.Drop.performed += _ => RequestDrop();
			input.Primary.Scroll.performed += ctx => 
			{
				uint activeIndex = ctx.ReadValue<float>() > 0 
					? (uint)((selectionIndex + 1) % inventory.Count)
					: (uint)((selectionIndex + inventory.Count - 1) % inventory.Count);
	
				InvokeInventoryChangedRPC(activeIndex);
			};
			for (int i = 0; i < input.hotbarActions.Length; i++)
			{
				uint index = (uint)i; // Create copy for lambda
				input.hotbarActions[i].Enable();
				input.hotbarActions[i].performed += _ =>
				{
					if (index < inventory.Count)
					{
						InvokeInventoryChangedRPC(index);
					}
				};
			}
		}
		
		inventory.OnListChanged += (NetworkListEvent<ItemSlot> ctx) =>
		{
			selectionIndex = ctx.Index > inventory.Count - 1 ?
			(uint)ctx.Index - 1 :
			(uint)ctx.Index;

			InventoryChanged?.Invoke(selection);
		};

		if (IsServer)
		{
			inventory.Add(ItemSlot.none);
		}
	}

	public bool Add(Item item)
	{
		if (weight + item.itemDescription.weight > MAX_WEIGHT) { return false; }
		
		if (item.dataTag.Value.Key != DataKeyType.None)
		{
			// Try to stack to existing slots
			for (int i = 0; i < inventory.Count; i++)
			{
				ItemSlot slot = inventory[i];
				if (item.itemDescription.itemType == slot.itemType)
				{
					slot.quantity++;
					inventory[i] = slot;
					return true;
				}
			}
		}
		// Try to place in new slot
		if (inventory.Count < 10)
		{
			inventory.Add(new ItemSlot(item));
			return true;
		}

		return false;
	}

	void RequestDrop()
	{
		if (selectionIndex == 0) { return; }
		if (selectionIndex >= inventory.Count) { return; }

		if (selection.itemType != ItemType.None)
		{
			ItemInteractionManager.Instance.RequestDropRPC(selectionIndex, this);
		}
	}

	public ItemSlot DropSelection()
	{
		if (IsServer)
		{
			ItemSlot itemSlot = selection;
			itemSlot.quantity--;
			if (itemSlot.quantity <= 0)
			{
				inventory.RemoveAt((int)selectionIndex);
			}
			else
			{
				inventory[(int)selectionIndex] = itemSlot;
			}
			
			return itemSlot;
		}
		else
		{
			throw new InvalidOperationException("DropSelection should not be called outside of the server.");
		}
	}

	public void ModifySlot(int index, ItemSlot itemSlot)
	{
		if (itemSlot.DataKey == DataKeyType.Durability)
		{
			if (itemSlot.dataValue <= 0)
			{
				inventory.RemoveAt(index);
			}
			else
			{
				inventory[index] = itemSlot;
			}
		}
	}

	// Use with no target
	public void UseHolding(bool alt)
	{
		if (!alt)
		{
			// noraml usage
			switch (selection.itemType)
			{
				case ItemType.Blueprint:
					if (selection.DataKey == DataKeyType.WorkbenchId)
					{
						characterVisuals.StartPlacing(StructureManager.Instance.workbenchDescriptions[(int)selection.dataValue]);
					}
					break;

				default:
					Debug.Log("No standalone use case found for " + selection.itemType.ToString());
					break;
			}
		}
		else
		{
			// alt usage
			switch (selection.itemType)
			{
				default:
					Debug.Log("No alternate standalone use case found for " + selection.itemType.ToString());
					break;
			}
		}
	}
	// Use with an item target
	public void UseHolding(NetworkBehaviourReference target, bool alt)
	{
		ItemInteractionManager.Instance.UseItemRPC(selectionIndex, this, target);
	}
	// Use with a stucture target
	public void UseHolding(Structure structure, bool alt)
	{
		StructureManager.Instance.InteractStructureRPC(selectionIndex, this, structure.structureId);
	}

	[Rpc(SendTo.Everyone)]
	void InvokeInventoryChangedRPC(uint activeIndex)
	{
		selectionIndex = activeIndex;
		InventoryChanged?.Invoke(selection);
	}

	public void PrintContents()
	{
		string m = "Printing User " + OwnerClientId + "'s Inventory: \n";
		for (int i = 0; i < inventory.Count; i++)
		{
			ItemSlot slot = inventory[i];
			m += "Slot " + i + ": " + slot.itemType.ToString() + "; " + slot.quantity + "\n";	
		}
		Debug.Log(m);
	}
}

public struct ItemSlot : INetworkSerializable, IEquatable<ItemSlot>
{
	public ItemType itemType;
	public uint quantity;
	
	DataKeyType dataKey;
	public DataKeyType DataKey { get { return dataKey; } }
	public float dataValue;
	
	public ItemDescription description => ItemInteractionManager.ItemDescriptions[(int)itemType];

	public static ItemSlot none => new ItemSlot(ItemType.None, 1);

	public ItemSlot(Item item)
	{
		itemType = item.itemDescription.itemType;
		quantity = 1;
		dataKey = item.dataTag.Value.Key;
		dataValue = item.dataTag.Value.data;
	}

	ItemSlot(ItemType t, uint q)
	{
		itemType = t;
		quantity = q;
		dataKey = DataKeyType.None;
		dataValue = 0;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref itemType);
		serializer.SerializeValue(ref quantity);
		serializer.SerializeValue(ref dataKey);
		serializer.SerializeValue(ref dataValue);
	}

	public bool Equals(ItemSlot other)
	{
		return itemType == other.itemType
			&& quantity == other.quantity
			&& dataKey == other.dataKey
			&& dataValue == other.dataValue;
	}

	public override bool Equals(object obj)
	{
		return obj is ItemSlot other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = itemType.GetHashCode();
			hashCode = (hashCode * 397) ^ (int)quantity;
			hashCode = (hashCode * 397) ^ dataKey.GetHashCode();
			hashCode = (hashCode * 397) ^ dataValue.GetHashCode();
			return hashCode;
		}
	}
}