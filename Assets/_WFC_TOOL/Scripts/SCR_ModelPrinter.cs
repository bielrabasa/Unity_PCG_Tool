using PCG_Tool;
using UnityEditor;
using UnityEngine;

public class SCR_ModelPrinter : MonoBehaviour
{
    [SerializeField] private bool generateOnStart = true;

    public SBO_RepresentationModel model;
    public SBO_TileSet tileSet;
    public Transform parentTransform;

    void Start()
    {
        if (generateOnStart) Generate();
    }

    public void Generate()
    {
        PrePrint();
        PrintModel();
    }

    public void ClearTiles()
    {
        if (parentTransform != null) DestroyImmediate(parentTransform.gameObject);
    }

    private void PrePrint()
    {
        if (model == null || model.tileSet == null)
        {
            Debug.LogError("ModelPrinter: RepresentationModel or TileSet is missing.");
            return;
        }

        tileSet = model.tileSet;
    }

    private void PrintModel()
    {
        if (model == null || model.tileSet == null)
        {
            Debug.LogError("ModelPrinter: RepresentationModel or TileSet is missing.");
            return;
        }

        // Create parent
        ClearTiles();
        GameObject parentObject = new GameObject("Generated Tiles");
        parentTransform = parentObject.transform;
        Undo.RegisterCreatedObjectUndo(parentObject, "Create Tiles Parent");

        // Print
        for (int x = 0; x < model.gridSize.x; x++)
        {
            for (int y = 0; y < model.gridSize.y; y++)
            {
                for (int z = 0; z < model.gridSize.z; z++)
                {
                    TileInfo tileInfo = model.GetTile(x, y, z);
                    if (tileInfo.id == 0) continue;  // ID 0 is empty space

                    GameObject prefab = tileSet.GetPrefab(tileInfo.id);
                    if (prefab == null)
                    {
                        Debug.Log("ModelPrinter: Null item detected.");
                        continue;
                    }

                    Vector3 position = new Vector3(x * tileSet.tileSize.x, y * tileSet.tileSize.y, z * tileSet.tileSize.z);
                    GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    instance.transform.position = position;
                    instance.transform.rotation = tileInfo.GetRotation();
                    instance.transform.localScale = tileInfo.GetMirroredScale();
                    instance.transform.parent = parentTransform;
                }
            }
        }

        Debug.Log("ModelPrinter: Generation Complete.");
    }
}
