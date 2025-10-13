using UnityEngine;

[ExecuteInEditMode]
public class SnapToTerrain : MonoBehaviour
{
    public Terrain terrain;

    void Update()
    {
        if (!terrain) return;
        Vector3 pos = transform.position;
        float height = terrain.SampleHeight(pos) + terrain.GetPosition().y;
        transform.position = new Vector3(pos.x, height, pos.z);
    }
}
