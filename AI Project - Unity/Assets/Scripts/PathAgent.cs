using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Convert = System.Convert;

public class PathAgent : Agent
{
    public GameObject spawnArea;
    private Vector3 spawnAreaBounds;
    private Rigidbody rBody;
    private DetectGameObjects detectScript;
    public Transform target;
    [SerializeField] float movementSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float rotationAngle;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        detectScript = GetComponent<DetectGameObjects>();
        spawnAreaBounds = spawnArea.GetComponent<DefineSpawnArea>().SpawnAreaBounds();
    }

    public override void OnEpisodeBegin()
    {
        //float xBound = Random.Range(spawnArea.gameObject.transform.localPosition.x - spawnAreaBounds.x,
        //                            spawnArea.gameObject.transform.localPosition.x + spawnAreaBounds.x);
        //float zBound = Random.Range(spawnArea.gameObject.transform.localPosition.z - spawnAreaBounds.z,
        //                            spawnArea.gameObject.transform.localPosition.z + spawnAreaBounds.z);
        //transform.localPosition = new Vector3(xBound, spawnArea.transform.localPosition.y, zBound);
        Vector3 spawnCenter = spawnArea.transform.position;
        Vector3 spawnAreaSize = spawnAreaBounds;

        float xOffset = Random.Range(-spawnAreaSize.x, spawnAreaSize.x);
        float zOffset = Random.Range(-spawnAreaSize.z, spawnAreaSize.z);

        // Calculate local offset within the spawn area, then convert to world space
        Vector3 localOffset = new Vector3(xOffset, 0, zOffset);
        Vector3 spawnPosition = spawnArea.transform.TransformPoint(localOffset);

        transform.localPosition = transform.parent.InverseTransformPoint(spawnPosition); // keep local positioning

        SetReward(0f);
        rBody.linearVelocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;

        detectScript.targetReached = false;
        detectScript.wallHit = false;
        detectScript.obstacaleHit = false;
        detectScript.isGrounded = false;
    }

    // Colecteaza informatii despre agent si target
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);

        sensor.AddObservation(rBody.linearVelocity.x);
        sensor.AddObservation(rBody.linearVelocity.z);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Agent Inputs
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotatationVariable = actions.ContinuousActions[2];
        Vector3 agentMovementVector = new Vector3(moveX, 0, moveZ);
        var jumpVariable = actions.DiscreteActions[0];

        // Movement Function Calls
        RotateAgent(rotatationVariable);
        MoveAgent(agentMovementVector);
        JumpMethod(jumpForce * jumpVariable);

        // Rewards and Penalties
        if (detectScript.targetReached)
        {
            AddReward(5f);
            EndEpisode();
            Debug.Log("Target has been reached!");
        }

        if (transform.localPosition.y < 0) 
        {
            AddReward(-2f);
            EndEpisode();
            Debug.Log("Agent has fallen off the map!");
        }

        if (detectScript.wallHit)
        {
            AddReward(-0.3f);
            Debug.Log("Agent has hit a wall.");
        }

        if (!detectScript.isGrounded)
        {
            AddReward(-0.1f);
        }

        if (detectScript.obstacaleHit)
        {
            AddReward(-0.5f);
            Debug.Log("Agent has hit an obstacle.");
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
    private void MoveAgent(Vector3 inputVector)
    {
        if (inputVector.sqrMagnitude == 0)
            return;

        Vector3 moveDirection = transform.TransformDirection(inputVector.normalized);
        rBody.AddForce(moveDirection * movementSpeed, ForceMode.Acceleration);
    }

    private void JumpMethod(float jumpForce)
    {
        if (detectScript.isGrounded)
        {
            rBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void RotateAgent(float rotationInput)
    {
        // rotationInput is expected to be in range [-1, 1]
        float rotationStep = rotationInput * rotationAngle * Time.fixedDeltaTime;

        Quaternion deltaRotation = Quaternion.Euler(0f, rotationStep, 0f);
        rBody.MoveRotation(rBody.rotation * deltaRotation);
    }

}
