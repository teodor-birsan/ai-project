using UnityEngine;

public class DefineSpawnArea : MonoBehaviour
{
    private MeshCollider mesh;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        mesh = GetComponent<MeshCollider>();
    }
    public Vector3 SpawnAreaBounds()
    {
        var bounds = mesh.bounds;
        return bounds.extents;
    }
}
