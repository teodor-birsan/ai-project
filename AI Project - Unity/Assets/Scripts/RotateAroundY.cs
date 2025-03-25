using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Unity.VisualScripting;
using System.Linq.Expressions;

public class RotateAroundY : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public Vector3 rotationVector = new(0, 1, 0);

    void Update()
    {
        transform.Rotate(rotationVector, rotationSpeed * Time.deltaTime);
    }
}
