using UnityEngine;

public class DetectObstacles : MonoBehaviour
{

    public bool obstacleDetected = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Collides with obstacle");
            obstacleDetected = true;
        }
        else{
            obstacleDetected = false;
        }
    }
}
