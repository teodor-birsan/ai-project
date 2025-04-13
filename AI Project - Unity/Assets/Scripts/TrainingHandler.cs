using UnityEngine;

public class TrainingHandler : MonoBehaviour
{
    public int xTiles;
    public int zTiles;
    public float xBound;
    public float zBound;
    public float spawnBoundX;
    public readonly float spawnBoundZ = 3f;
    public readonly Vector3 tileSize = new Vector3(3f, 0.25f, 3f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xBound = xTiles * tileSize.x;
        zBound = zTiles * tileSize.z;
        spawnBoundX = xBound;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
