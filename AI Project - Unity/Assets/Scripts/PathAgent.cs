using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PathAgent : Agent
{
    public GameObject agentBody;
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
        rBody = agentBody.GetComponent<Rigidbody>();
        detectScript = agentBody.GetComponent<DetectGameObjects>();
        spawnAreaBounds = spawnArea.GetComponent<DefineSpawnArea>().SpawnAreaBounds();
    }

    public override void OnEpisodeBegin()
    {
        float xBound = Random.Range(spawnArea.gameObject.transform.localPosition.x - spawnAreaBounds.x,
                                    spawnArea.gameObject.transform.localPosition.x + spawnAreaBounds.x);
        float zBound = Random.Range(spawnArea.gameObject.transform.localPosition.z - spawnAreaBounds.z,
                                    spawnArea.gameObject.transform.localPosition.z + spawnAreaBounds.z);
        transform.localPosition = new Vector3(xBound, spawnArea.transform.localPosition.y, zBound);
        SetReward(0f);
        rBody.linearVelocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
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
        Vector3 agentMovementVector = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        var jumpVariable = actions.DiscreteActions[0];
        var rotationVariable = actions.DiscreteActions[1];

        // Movement Function Calls
        RotateAgent(rotationAngle * rotationVariable);
        MoveAgent(agentMovementVector);
        JumpMethod(jumpForce * jumpVariable);

        // Rewards and Penalties
        if (detectScript.targetReached)
        {
            AddReward(5f);
            Debug.Log("Target has been reached!");
            EndEpisode();
        }

        if (transform.localPosition.y < 0) 
        {
            AddReward(-2f);
            Debug.Log("Agent has fallen off the map!");
            EndEpisode();
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

    private void MoveAgent(Vector3 inputVector)
    {
        if (inputVector.sqrMagnitude == 2)
        {
            rBody.linearVelocity = transform.TransformDirection(movementSpeed * 0.7f * Time.deltaTime * inputVector);
        }
        else
        {
            rBody.linearVelocity = transform.TransformDirection(movementSpeed * Time.deltaTime * inputVector);
        }
    }

    private void JumpMethod(float jumpForce)
    {
        if (detectScript.isGrounded)
        {
            rBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void RotateAgent(float rotationAngle)
    {
        rBody.MoveRotation(Quaternion.Euler(rotationAngle * Time.deltaTime * Vector3.up));
    }
}
