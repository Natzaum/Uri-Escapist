using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [Header("Walk Settings")]
    public float walkAmplitude = 0.2f;
    public float walkFrequency = 4f;
    public float tiltAngle = 2f;
    public bool enableLateralBob = false;

    [Header("Idle Settings")]
    public float idleAmplitude = 0.02f;
    public float idleFrequency = 1f;

    public Transform cameraHolder;
    public Rigidbody playerRb;

    private Vector3 startPos;

    void Start()
    {
        if (cameraHolder == null)
            cameraHolder = transform;
        startPos = cameraHolder.localPosition;
    }

    void Update()
    {
        if (IsMoving())
        {
            float bobY = Mathf.Cos(Time.time * walkFrequency * 2f) * walkAmplitude * 0.5f;

            float bobX = 0f;
            if (enableLateralBob)
                bobX = Mathf.Sin(Time.time * walkFrequency) * (walkAmplitude * 0.2f); // bem sutil

            float tilt = Mathf.Sin(Time.time * walkFrequency) * tiltAngle;

            cameraHolder.localPosition = startPos + new Vector3(bobX, bobY, 0f);

            cameraHolder.localRotation = Quaternion.Euler(
                cameraHolder.localRotation.eulerAngles + new Vector3(0f, 0f, tilt)
            );
        }
        else
        {
            float idleY = Mathf.Sin(Time.time * idleFrequency) * idleAmplitude;

            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition,
                startPos + new Vector3(0f, idleY, 0f),
                Time.deltaTime * 2f
            );

            cameraHolder.localRotation = Quaternion.Lerp(
                cameraHolder.localRotation,
                Quaternion.Euler(
                    0f,
                    cameraHolder.localRotation.eulerAngles.y,
                    cameraHolder.localRotation.eulerAngles.z
                ),
                Time.deltaTime * 2f
            );
        }
    }

    bool IsMoving()
    {
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        return flatVel.magnitude > 0.1f;
    }
}
