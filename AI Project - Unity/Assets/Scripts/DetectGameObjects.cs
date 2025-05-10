using System.Linq.Expressions;
using UnityEngine;

public class DetectGameObjects : MonoBehaviour
{
    public bool isGrounded;
    public bool obstacaleHit;
    public bool wallHit;
    public bool targetReached;

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded=false;
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            wallHit = false;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            obstacaleHit = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            wallHit = true;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            obstacaleHit = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            targetReached = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            targetReached = false;
        }
    }
}
