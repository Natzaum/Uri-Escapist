using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    public float walkAmplitude = 0.2f;
    public float walkFrequency = 4f;
    public float tiltAngle = 2f;
    public float idleAmplitude = 0.02f;
    public float idleFrequency = 1f;

    public Transform cameraHolder;
    public Rigidbody playerRb;

    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        if (cameraHolder == null) cameraHolder = transform;
        startPos = cameraHolder.localPosition;
        startRot = cameraHolder.localRotation;
    }

    void Update()
    {
        if (IsMoving())
        {
            float bobX = Mathf.Sin(Time.time * walkFrequency) * walkAmplitude;
            float bobY = Mathf.Cos(Time.time * walkFrequency * 2f) * walkAmplitude * 0.5f;
            float tilt = Mathf.Sin(Time.time * walkFrequency) * tiltAngle;
            cameraHolder.localPosition = startPos + new Vector3(bobX, bobY, 0f);
            cameraHolder.localRotation = startRot * Quaternion.Euler(0f, 0f, tilt);
        }
        else
        {
            float idleY = Mathf.Sin(Time.time * idleFrequency) * idleAmplitude;
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, startPos + new Vector3(0f, idleY, 0f), Time.deltaTime * 2f);
            cameraHolder.localRotation = Quaternion.Lerp(cameraHolder.localRotation, startRot, Time.deltaTime * 2f);
        }
    }

    bool IsMoving()
    {
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        return flatVel.magnitude > 0.1f;
    }
}
