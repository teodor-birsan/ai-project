using UnityEngine;

public class DetectObstacles : MonoBehaviour
{

    public bool obstacleDetected = false;
    public bool isGrounded;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isGrounded = true;
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
        if (!collision.gameObject.CompareTag("Floor"))
        {
            Debug.Log("Agent is not on the ground");
            isGrounded = false;
        }
        else
        {
            isGrounded = true;
        }
    }
}
