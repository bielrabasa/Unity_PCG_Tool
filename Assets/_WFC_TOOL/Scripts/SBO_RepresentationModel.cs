using System;
using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    [CreateAssetMenu(fileName = "New RepresentationModel", menuName = "WFC_Tool/Representation Model")]
    public class SBO_RepresentationModel : ScriptableObject
    {
        public SBO_TileSet tileSet;
        private Vector3Int gridSize = Vector3Int.one;
        [SerializeField] private TileInfo[] tiles;
        public event Action OnModelChanged;

        public Vector3Int GridSize
        {
            get => gridSize;
            set
            {
                if (gridSize != value)
                {
                    ResizeGrid(value);
                }
            }
        }
        

        private void OnEnable()
        {
            //TODO: Erase, only testing
            if (tiles == null)
            {
                Debug.LogWarning("Initialising Grid in Representation Model because tiles array is null.");
                InitializeGrid();
            }
            else if (tiles.Length != gridSize.x * gridSize.y * gridSize.z)
            {
                Debug.LogWarning("Initialising Grid in Representation Model because tiles array size is different than gridSize. " +
                    "GridSize: " + gridSize + "  tilesArrayLength: " + tiles.Length);
                InitializeGrid();
            }

            /*
            
            if (tiles == null || tiles.Length != gridSize.x * gridSize.y * gridSize.z)
            {
                InitializeGrid();
            }
            
             */
        }

        private void InitializeGrid()
        {
            tiles = new TileInfo[gridSize.x * gridSize.y * gridSize.z];

            // Opcional: Inicializar con valores por defecto
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new TileInfo(0); // ID 0 = vacío
            }

            EditorUtility.SetDirty(this); // Marcar como modificado en el editor
        }

        private int Index(int x, int y, int z) => x + gridSize.x * (y + gridSize.y * z);

        public TileInfo GetTile(int x, int y, int z)
        {
            if (x >= gridSize.x || y >= gridSize.y || z >= gridSize.z || x < 0 || y < 0 || z < 0)
            {
                Debug.LogWarning("Representation Model: Trying to acess a value out of the grid size.");
            }

            return tiles[Index(x, y, z)];
        }

        public void SetTile(int x, int y, int z, TileInfo tile)
        {
            tiles[Index(x, y, z)] = tile;
            
            NotifyModelChanges();
        }

        public void ResizeGrid(Vector3Int newSize)
        {
            TileInfo[] newTiles = new TileInfo[newSize.x * newSize.y * newSize.z];

            for (int x = 0; x < Mathf.Min(gridSize.x, newSize.x); x++)
            {
                for (int y = 0; y < Mathf.Min(gridSize.y, newSize.y); y++)
                {
                    for (int z = 0; z < Mathf.Min(gridSize.z, newSize.z); z++)
                    {
                        // Copiar datos de la grid antigua a la nueva
                        newTiles[x + newSize.x * (y + newSize.y * z)] = GetTile(x, y, z);
                    }
                }
            }

            // Asignar la nueva grid y actualizar el tamaño
            tiles = newTiles;
            gridSize = newSize;
            EditorUtility.SetDirty(this); // Guardar cambios en el editor

            NotifyModelChanges();
        }

        private void NotifyModelChanges()
        {
            OnModelChanged?.Invoke();
        }
    }

}