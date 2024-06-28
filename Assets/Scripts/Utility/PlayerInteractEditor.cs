using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerInteract))]
public class PlayerInteractEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// Draw the default inspector
		DrawDefaultInspector();

		PlayerInteract plrInteract = (PlayerInteract)target;

		EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);

		if (plrInteract.worldSelection != null)
		{
			for (int i = 0; i < plrInteract.worldSelection.Count; i++)
			{
				ISelectable selection = plrInteract.worldSelection[i];
				if (selection is Item item)
				{
					EditorGUILayout.LabelField($"{i}: Item: {item.gameObject.name}");
				}
				else if (selection is Structure structure)
				{
					EditorGUILayout.LabelField($"{i}: Structure: {structure.gameObject.name}");
				}
			}
		}
		else
		{
			EditorGUILayout.LabelField("worldSelection is null.");
		}
	}
}
#endif