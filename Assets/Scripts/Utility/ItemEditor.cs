using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
	public override void OnInspectorGUI()
	{
		Item item = (Item)target;
		
		EditorGUILayout.LabelField("Data Tag");
		
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.TextField("Data Key", item.dataTag.Value.Key.ToString());
		EditorGUILayout.FloatField("Data Value", item.dataTag.Value.data);
		EditorGUILayout.EndVertical();
		
		DrawDefaultInspector();
	}
}
#endif