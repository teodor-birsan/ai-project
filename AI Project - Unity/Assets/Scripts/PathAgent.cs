using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Convert = System.Convert;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PathAgent : Agent
{
    public GameObject spawnArea;
    public TrainingTime trainingTime;
    private Vector3 spawnAreaBounds;
    private CharacterController controller;
    public List<GameObject> targetAreas;
    [SerializeField] private int collectedTargets;
    [SerializeField] float movementSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float rotationAngle;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool obstacaleHit;
    [SerializeField] private bool wallHit;
    [SerializeField] private bool targetReached;
    private Vector3 linearVelocity;
    private Vector3 angularVelocity;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 movementVelocity;

    void Start()
    {
        spawnAreaBounds = spawnArea.GetComponent<DefineSpawnArea>().SpawnAreaBounds();
        controller = GetComponent<CharacterController>();
    }

    public override void OnEpisodeBegin()
    {
        InitializeTargets();
        collectedTargets = 0;
        Vector3 spawnCenter = spawnArea.transform.position;
        Vector3 spawnAreaSize = spawnAreaBounds;

        float xOffset = Random.Range(-spawnAreaSize.x, spawnAreaSize.x);
        float zOffset = Random.Range(-spawnAreaSize.z, spawnAreaSize.z);

        // Calculate local offset within the spawn area, then convert to world space
        Vector3 localOffset = new Vector3(xOffset, 0.75f, zOffset);
        Vector3 spawnPosition = spawnArea.transform.TransformPoint(localOffset);

        transform.localPosition = transform.parent.InverseTransformPoint(spawnPosition); // keep local positioning
        transform.localRotation = Quaternion.Euler(Vector3.zero);

        linearVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;

        SetReward(0f);
        trainingTime.ResetTime();

        targetReached = false;
    }

    // Colecteaza informatii despre agent si target
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(linearVelocity);
        sensor.AddObservation(angularVelocity);

        foreach(var area in targetAreas)
        {
            var target = area.GetComponent<TargetChild>().target.transform;
            sensor.AddObservation(target.localPosition);
            sensor.AddObservation(target.localPosition - transform.localPosition);
            sensor.AddObservation(Vector3.Distance(transform.localPosition, target.localPosition));
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        UpdateVelocities();
        isGrounded = controller.isGrounded;
        // Agent Inputs
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotatationVariable = actions.ContinuousActions[2];
        Vector3 agentMovementVector = new Vector3(moveX, 0, moveZ);
        var jumpVariable = actions.DiscreteActions[0];

        // Horizontal Movement
        if(isGrounded && linearVelocity.y < 0)
        {
            movementVelocity.y = 0f;
            agentMovementVector = Vector3.ClampMagnitude(agentMovementVector, 1f);
        }

        //Jump
        if(jumpVariable == 1 && isGrounded)
        {
            movementVelocity.y = Mathf.Sqrt(jumpForce * -2.0f * -9.81f);
        }

        // Apply Gravity
        movementVelocity.y += -9.81f * Time.deltaTime;

        Vector3 moveDirection = transform.TransformDirection(agentMovementVector.normalized);
        Vector3 finalMove = (moveDirection * movementSpeed) + (movementVelocity.y * Vector3.up);

        controller.Move(finalMove * Time.deltaTime);

        //Rotation
        float rotationInput = actions.ContinuousActions[2]; // value in [-1, 1]
        float rotationAmount = rotationInput * rotationAngle * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationAmount);


        // Rewards and Penalties
        ProximityReward();

        // Encourage continuous movement
        if (linearVelocity.magnitude < 0.1f)
        {
            AddReward(-0.001f); // small penalty for standing still
        }


        if (targetReached)
        {
            AddReward(3f);
            Debug.Log("Target has been reached!");
            collectedTargets++;
            targetReached = false;
        }

        if (transform.localPosition.y < 0) 
        {
            AddReward(-2f);
            trainingTime.ResetTime();
            Debug.Log("Agent has fallen off the map!");
            CleanUp();
            EndEpisode();
        }

        if (wallHit)
        {
            AddReward(-0.3f);
            Debug.Log("Agent has hit a wall.");
        }

        if (!isGrounded)
        {
            AddReward(-0.1f);
        }

        if (obstacaleHit)
        {
            AddReward(-0.2f);
            Debug.Log("Agent has hit an obstacle.");
        }
        Vector3 up = transform.up;
        float tiltAngle = Vector3.Angle(up, Vector3.up);

        if (trainingTime.HasExpired())
        {
            AddReward(-2.5f);
            Debug.Log("Time has expired");
            CleanUp();
            EndEpisode();
        }

        if (tiltAngle > 30f) // You can tweak the threshold
        {
            AddReward(-0.5f);
            Debug.Log("Agent flipped. Tilt angle: " + tiltAngle);
            CleanUp();
            EndEpisode();
        }

        if(collectedTargets == targetAreas.Count)
        {
            var remainingTime = trainingTime.ResetTime();
            AddReward(7f + 0.1f * remainingTime);
            EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> movementActions = actionsOut.ContinuousActions;
        movementActions[0] = Input.GetAxis("Horizontal");
        movementActions[1] = Input.GetAxis("Vertical");

        // Rotation
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotationInput = -1f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rotationInput = 1f;
        }
        movementActions[2] = rotationInput;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        // Jump 
        discreteActions[0] = Convert.ToInt32(Input.GetAxis("Jump"));
    }
    private void CleanUp()
    {
        foreach (var area in targetAreas)
        {
            area.GetComponent<TargetChild>().target.SetActive(false);
        }
    }

    private void InitializeTargets()
    {
        foreach(var area in targetAreas)
        {
            area.GetComponent<TargetChild>().target.SetActive(true);
        }
    }

    private void ProximityReward()
    {
        float totalInverseDistance = 0f;
        float minDistance = float.MaxValue;

        foreach (var area in targetAreas)
        {
            GameObject target = area.GetComponent<TargetChild>().target;

            if (target.activeSelf)
            {
                float distance = Vector3.Distance(transform.localPosition, target.transform.localPosition);
                minDistance = Mathf.Min(minDistance, distance);

                // Add inverse distance to reward to encourage getting closer (avoid div by 0)
                totalInverseDistance += 1f / (distance + 1f);
            }
        }

        // Optional: normalize or scale reward
        float reward = totalInverseDistance * 0.01f;

        AddReward(reward);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor") && gameObject.CompareTag("Agent"))
        {
            isGrounded = false;
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

    private void UpdateVelocities()
    {
        // Linear Velocity
        linearVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;

        // Angular Velocity
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);

        if (angleInDegrees > 180f) angleInDegrees -= 360f;

        angularVelocity = axis * angleInDegrees * Mathf.Deg2Rad / Time.fixedDeltaTime;
        lastRotation = transform.rotation;
    }

}
