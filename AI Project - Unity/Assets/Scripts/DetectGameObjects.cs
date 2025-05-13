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
        if (collision.gameObject.CompareTag("Floor") && gameObject.CompareTag("Agent"))
        {
            isGrounded=false;
        }
        if (collision.gameObject.CompareTag("Wall") && gameObject.CompareTag("Agent"))
        {
            wallHit = false;
        }

        if (collision.gameObject.CompareTag("Obstacle") && gameObject.CompareTag("Agent"))
        {
            obstacaleHit = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor") && gameObject.CompareTag("Agent"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Wall") && gameObject.CompareTag("Agent"))
        {
            wallHit = true;
        }

        if (collision.gameObject.CompareTag("Obstacle") && gameObject.CompareTag("Agent"))
        {
            obstacaleHit = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target") && gameObject.CompareTag("Agent"))
        {
            targetReached = true;
            other.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Target") && gameObject.CompareTag("Agent"))
        {
            targetReached = false;
        }
    }
}
