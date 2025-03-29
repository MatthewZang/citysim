using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 100;
    public int mapHeight = 100;
    public float tileSize = 1f;
    
    [Header("Map Visuals")]
    public GameObject tilePrefab;
    public Material grassMaterial;
    public Material waterMaterial;
    public Material roadMaterial;

    private GameObject[,] mapTiles;

    private void Start()
    {
        mapTiles = new GameObject[mapWidth, mapHeight];
        GenerateMap();
    }

    private void GenerateMap()
    {
        // Create map parent object
        GameObject mapHolder = new GameObject("Map");
        
        // Generate basic grid of tiles
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                // Calculate position
                Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);
                
                // Create tile
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.parent = mapHolder.transform;
                
                // Set default material (grass)
                tile.GetComponent<MeshRenderer>().material = grassMaterial;
                
                // Store reference
                mapTiles[x, z] = tile;
            }
        }

        // Center the map
        mapHolder.transform.position = new Vector3(-mapWidth * tileSize / 2f, 0, -mapHeight * tileSize / 2f);
    }

    public void SetTileType(int x, int z, TileType type)
    {
        if (x < 0 || x >= mapWidth || z < 0 || z >= mapHeight)
            return;

        Material material = type switch
        {
            TileType.Grass => grassMaterial,
            TileType.Water => waterMaterial,
            TileType.Road => roadMaterial,
            _ => grassMaterial
        };

        mapTiles[x, z].GetComponent<MeshRenderer>().material = material;
    }
}

public enum TileType
{
    Grass,
    Water,
    Road
} 