using UnityEngine;

public class TerrainFlipper : MonoBehaviour
{
    public Terrain terrain;
    public bool flipX = false;
    public bool flipZ = false;

    [ContextMenu("Flip Terrain")]
    public void FlipTerrain()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }

        if (terrain == null)
        {
            Debug.LogError("No Terrain assigned!");
            return;
        }

        TerrainData data = terrain.terrainData;
        int width = data.heightmapResolution;
        int height = data.heightmapResolution;

        float[,] heights = data.GetHeights(0, 0, width, height);
        float[,] flipped = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int newX = flipX ? width - 1 - x : x;
                int newZ = flipZ ? height - 1 - z : z;
                flipped[newX, newZ] = heights[x, z];
            }
        }

        data.SetHeights(0, 0, flipped);
        Debug.Log("✅ Terrain flipped successfully!");
    }
}
