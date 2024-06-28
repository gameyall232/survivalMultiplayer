using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerInventory))]
public class PlayerInventoryEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		PlayerInventory playerInventory = (PlayerInventory)target;

		if (EditorApplication.isPlaying && playerInventory.IsSpawned)
		{
			EditorGUILayout.LabelField("Inventory");
			foreach (var itemSlot in playerInventory?.inventory)
			{
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.TextField("Item Type", itemSlot.itemType.ToString());
				EditorGUILayout.IntField("Quantity", (int)itemSlot.quantity);
				EditorGUILayout.TextField("Data Key", itemSlot.DataKey.ToString());
				EditorGUILayout.FloatField("Data Value", itemSlot.dataValue);
				EditorGUILayout.EndVertical();
			}
		}
		else
		{
			EditorGUILayout.HelpBox("Enter Play Mode to view the NetworkList.", MessageType.Info);
		}
	}
}
#endif