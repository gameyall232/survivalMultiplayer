using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemThumbnailGenerator : MonoBehaviour
{
	private static string prefabsFolderPath = "Assets/Items/Prefabs"; // Path to the folder containing prefabs
	private static string thumbnailsFolderPath = "Assets/Items/Icons"; // Path to the folder where thumbnails will be saved
	private static int thumbnailWidth = 256;
	private static int thumbnailHeight = 256;

	[MenuItem("Tools/Generate Item Thumbnails")]
	public static void GenerateThumbnails()
	{
		// Ensure the thumbnails folder exists
		if (!Directory.Exists(thumbnailsFolderPath))
		{
			Directory.CreateDirectory(thumbnailsFolderPath);
		}

		// Get all prefab files in the specified folder
		string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { prefabsFolderPath });

		foreach (string guid in prefabGUIDs)
		{
			string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

			if (prefab != null)
			{
				// Create a render texture for the thumbnail
				RenderTexture renderTexture = new RenderTexture(thumbnailWidth, thumbnailHeight, 24);
				Texture2D thumbnailTexture = new Texture2D(thumbnailWidth, thumbnailHeight, TextureFormat.RGBA32, false);

				// Set up a temporary camera to render the prefab
				GameObject cameraGameObject = new GameObject("ThumbnailCamera");
				Camera camera = cameraGameObject.AddComponent<Camera>();
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = Color.clear;
				camera.orthographic = true;
				camera.orthographicSize = 1;
				camera.targetTexture = renderTexture;

				// Create a temporary instance of the prefab
				GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				instance.transform.position = Vector3.zero;
				Bounds bounds = CalculateBounds(instance);

				// Adjust the camera to fit the prefab
				camera.transform.position = bounds.center - Vector3.forward * 10;
				camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);

				// Render the prefab to the texture
				camera.Render();

				// Read the rendered texture into the Texture2D
				RenderTexture.active = renderTexture;
				thumbnailTexture.ReadPixels(new Rect(0, 0, thumbnailWidth, thumbnailHeight), 0, 0);
				thumbnailTexture.Apply();

				// Clean up
				RenderTexture.active = null;
				camera.targetTexture = null;
				DestroyImmediate(renderTexture);
				DestroyImmediate(cameraGameObject);
				DestroyImmediate(instance);

				// Save the texture as a PNG file
				byte[] pngData = thumbnailTexture.EncodeToPNG();
				string thumbnailPath = Path.Combine(thumbnailsFolderPath, prefab.name + "Icon.png");
				File.WriteAllBytes(thumbnailPath, pngData);

				// Import the generated thumbnail so it shows up in the project
				AssetDatabase.ImportAsset(thumbnailPath);
				Debug.Log("Generated thumbnail for: " + prefab.name);
			}
		}

		AssetDatabase.Refresh();
	}

	private static Bounds CalculateBounds(GameObject go)
	{
		var renderers = go.GetComponentsInChildren<Renderer>();
		var bounds = new Bounds(go.transform.position, Vector3.zero);
		foreach (var renderer in renderers)
		{
			bounds.Encapsulate(renderer.bounds);
		}
		return bounds;
	}
}
