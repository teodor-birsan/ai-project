using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Video;
using UnityEngine.Rendering;
using System.Collections.Generic;


public class PathAgent : Agent
{
    private Rigidbody rBody;
    private DetectObstacles detectObstacles; // Referinta la componenta RigidBody a agentului
    public Transform Target; // Referinta la componenta Transform a target-ului (sfarsitul platformei)
    [SerializeField] float movementForce;
    [SerializeField] float jumpForce;

    void Start()
    {   
        rBody = GetComponent<Rigidbody>(); // Se face referinta la componenta RigidBody
        detectObstacles = GetComponent<DetectObstacles>(); 
    }

    // Initializeaza si reseteaza agentul
    public override void OnEpisodeBegin()
    {
        if(transform.localPosition.y < 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.linearVelocity = Vector3.zero;
            var xPos = Random.Range(0, 6);
            var zPos = Random.Range(0, 2);
            transform.localPosition = new Vector3(xPos, 0, zPos);
        }   
    }

    // Colecteaza informatii despre agent si target
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(transform.localPosition);

        sensor.AddObservation(rBody.linearVelocity.x);
        sensor.AddObservation(rBody.linearVelocity.z);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Actiuni
        Vector3 controlSignal = Vector3.zero;
        Vector2 agentMovementXY = Vector2.zero;
        agentMovementXY.x = actions.ContinuousActions[0];
        agentMovementXY.y = actions.ContinuousActions[1];
        agentMovementXY *= movementForce;
        var jump = actions.DiscreteActions[0] * jumpForce;
        controlSignal = new Vector3(agentMovementXY.x, 0, agentMovementXY.y);
        if (detectObstacles.isGrounded && jump > 0)
        {
            controlSignal.y = jump;
            AddReward(-0.2f);
        }
        rBody.AddForce(controlSignal, ForceMode.Force);

        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

        if(distanceToTarget < 1.0f){
            AddReward(5.0f);
            EndEpisode();
        }
        else{
            if(transform.localPosition.y < 0){
                AddReward(-5.0f);
                EndEpisode();
            }
            
            if(detectObstacles.obstacleDetected){
                AddReward(-0.5f);
            }
        }
    }
}
