using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public Vector3 rotationVector = new(0, 1, 0);

    void Update()
    {
        transform.Rotate(rotationVector, rotationSpeed * Time.deltaTime);
    }
}
