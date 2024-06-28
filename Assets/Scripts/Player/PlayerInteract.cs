using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerInteract : NetworkBehaviour
{
	public List<ISelectable> worldSelection { get; private set; }

	public PlayerInventory inventory;
	[SerializeField] PlayerVisuals visuals;

	[SerializeField] ClientInput input;
	[SerializeField] Camera cam;
	[SerializeField] float reachDistance;

	[SerializeField] LayerMask selectableMask;
	[SerializeField] LayerMask entityMask; // Use in the future
	public static Vector3 handOffset = new Vector3(0.25f, 1, 0.75f);

	void Start()
	{
		if (IsClient && IsOwner)
		{
			worldSelection = new List<ISelectable>();
			input.Primary.Craft.performed += _ => Craft();
			input.Primary.Mouse.performed += _ => SelectWhereLooking();
			input.Primary.Multiselect.performed += ctx =>
			{
				if ((int)ctx.ReadValue<float>() == 0)
				{
					ClearSelection();
					SelectWhereLooking();
				}
			};
			input.Primary.Interact.performed += _ => { if (Cursor.lockState == CursorLockMode.Locked) { Interact(false); } };
			input.Primary.AltInteract.performed += _ => { if (Cursor.lockState == CursorLockMode.Locked) { Interact(true); } };
			input.Primary.Pickup.performed += _ => Pickup();
			input.Primary.ToggleMouse.performed += _ => SpawnMenuGUI.Instance.ToggleMouse();
		}
	}

	public override void OnNetworkSpawn()
	{
		name = "Player" + OwnerClientId;
		inventory.InventoryChanged += selection => visuals.UpdateHolding(selection.itemType);

		if (!IsOwner)
		{
			enabled = false;
			input.enabled = false;
		}
		else
		{
			transform.GetChild(1).GetComponent<AudioListener>().enabled = true;
			GetComponent<PlayerController>().enabled = true;
			GUIManager.Instance.Initialize(this);
		}
	}

	void Update()
	{
		if (input.Primary.Move.IsPressed())
		{
			SelectWhereLooking();
		}
	}

	void Interact(bool alt)
	{
		if (IsOwner)
		{
			SelectWhereLooking();
			if (worldSelection.Count == 1)
			{
				if (worldSelection[0] is Item targetItem)
				{
					inventory.UseHolding(targetItem, alt);
				}
				else if (worldSelection[0] is Structure targetStructure)
				{
					inventory.UseHolding(targetStructure, alt);
				}
			}
			else if (worldSelection.Count == 0 && inventory.selection.itemType != ItemType.None)
			{
				inventory.UseHolding(alt);
			}
		}
	}

	void Pickup()
	{
		SelectWhereLooking();
		if (worldSelection.Count == 1)
		{
			if (worldSelection[0] is Item target)
			{
				ItemInteractionManager.Instance.RequestPickupRPC(target, this);
				worldSelection.Remove(target);
			}
		}
	}

	void Craft()
	{
		if (worldSelection.Count < 2) { return; }

		NetworkBehaviourReference[] itemNBRs = new NetworkBehaviourReference[worldSelection.Count];
		for (int i = 0; i < worldSelection.Count; i++)
		{
			if (worldSelection[i] is not Item) { return; }

			itemNBRs[i] = worldSelection[i] as Item;
		}

		Vector3 craftPosition = Vector3.zero;
		foreach (ISelectable item in worldSelection)
		{
			craftPosition += (item as Item).transform.position;
		}
		craftPosition /= worldSelection.Count;

		if (ItemInteractionManager.Instance.RequestCraft(craftPosition, itemNBRs))
		{
			// Craft approved locally
			ClearSelection();
		}
	}

	void SelectWhereLooking()
	{
		if (IsClient)
		{
			bool multiselect = input.Primary.Multiselect.IsPressed();
			if (DetectSelectable(out ISelectable lookedAt))
			{
				if (!multiselect)
				{
					if (worldSelection.Count == 0 || worldSelection[0] != lookedAt)
					{
						ClearSelection();
						worldSelection.Add(lookedAt);
						lookedAt.Highlight(true);
					}
				}
				else
				{
					if (!worldSelection.Contains(lookedAt))
					{
						worldSelection.Add(lookedAt);
						lookedAt.Highlight(true);
					}
				}
			}
			else
			{
				if (!multiselect)
				{
					ClearSelection();
				}
			}
		}
	}

	bool DetectSelectable(out ISelectable selected)
	{
		selected = null;

		Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
		if (!Physics.Raycast(ray, out RaycastHit hitInfo, reachDistance, selectableMask))
		{
			return false;
		}

		if (!hitInfo.collider.transform.TryGetComponent(out selected))
		{
			return false;
		}

		if (selected is Item item)
		{
			if (item.CanPickup.Value != true)
			{
				return false;
			}
		}

		selected = hitInfo.collider.transform.GetComponent<ISelectable>();
		return true;
	}

	void ClearSelection()
	{
		worldSelection.ForEach(x =>
		{
			if (x != null)
			{
				x.Highlight(false);
			}
		});
		worldSelection.Clear();
	}
}

public interface ISelectable
{
	public void Highlight(bool on);
}