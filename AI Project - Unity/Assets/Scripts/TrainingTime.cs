using UnityEngine;
using UnityEngine.Rendering;

public class TrainingTime : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private float traingTimer = 45.0f;
    private float timeCopy;

    void Start()
    {
        timeCopy = traingTimer;
    }

    // Update is called once per frame
    void Update()
    {
        traingTimer -= Time.deltaTime;

    }

    public bool HasExpired()
    {
        if (traingTimer < 0)
            return true;
        return false;
    }

    public float ResetTime()
    {
        float remainingTime = traingTimer;
        traingTimer = timeCopy;
        Debug.Log($"Remaining Time: {remainingTime} seconds");
        return remainingTime;
    }
}
