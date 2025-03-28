using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Video;
using UnityEngine.Rendering;


public class PathAgent : Agent
{
    Rigidbody rBody; // Referinta la componenta RigidBody a agentului
    public Transform Target; // Referinta la componenta Transform a target-ului (sfarsitul platformei)
    public float movementForce = 10f;

    void Start()
    {
        rBody = GetComponent<Rigidbody>(); // Se face referinta la componenta RigidBody
    }

    // Initializeaza si reseteaza agentul
    public override void OnEpisodeBegin()
    {
        if(transform.localPosition.y < 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.linearVelocity = Vector3.zero;
            transform.localPosition = new Vector3(2.5f, 0.86f, 3f);
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
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        // TODO: agentul nu site sa sara peste obstacole
        rBody.AddForce(controlSignal * movementForce);

        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

        if(distanceToTarget < 1.0f){
            SetReward(1.0f);
            EndEpisode();
        }
        else{
            if(transform.localPosition.y < 0){
                EndEpisode();
            }
            // TODO: cod pentru detectarea coliziunii cu un obstacol 
        }
    }
}
