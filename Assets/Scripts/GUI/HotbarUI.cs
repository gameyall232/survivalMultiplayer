using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
	const float SLOT_WIDTH_PERCENT = .07f;

	PlayerInventory inventory;
	
	[SerializeField] RectTransform hotbar;
	[SerializeField] GameObject slotPrefab;
	[SerializeField] GameObject highlightPrefab;
	
	public void Initialize()
	{
		inventory = GUIManager.PlayerInventory;
		
		inventory.InventoryChanged += _ => RefreshDisplay();
		
		RefreshDisplay();
	}
	
	void RefreshDisplay()
	{
		int inventoryCount = inventory.inventory.Count;
	
		float hotbarWidth = SLOT_WIDTH_PERCENT * inventoryCount;
		hotbar.anchorMin = new Vector2(.5f - (hotbarWidth/2), 0);
		hotbar.anchorMax = new Vector2(.5f + (hotbarWidth/2), .125f);
		
		for (int i = 0; i < hotbar.childCount; i++)
		{
			Destroy(hotbar.GetChild(i).gameObject);
		}
		
		for (int i = 0; i < inventoryCount; i++)
		{
			Transform newSlot = Instantiate(slotPrefab, hotbar).transform;
			int selectedItemsIndex = (int)inventory.inventory[i].itemType;
			
			newSlot.GetChild(0).GetComponent<Image>().sprite = ItemInteractionManager.ItemDescriptions[selectedItemsIndex].icon;
			newSlot.GetChild(1).GetComponent<TMPro.TMP_Text>().text = i.ToString();
			newSlot.GetChild(2).GetComponent<TMPro.TMP_Text>().text = inventory.inventory[i].quantity.ToString();
			
			newSlot.GetComponent<RectTransform>().anchorMin = new Vector2((float)i / inventoryCount, 0);
			newSlot.GetComponent<RectTransform>().anchorMax = new Vector2((float)(i + 1) / inventoryCount, 1);
			
			if (i == inventory.selectionIndex)
			{
				Instantiate(highlightPrefab, newSlot);
			}
		}
	}
}