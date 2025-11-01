using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [Header("Walk Settings - Estilo Terror Clássico")]
    public float walkAmplitude = 0.15f; // Intensidade do balanço vertical
    public float walkFrequency = 3f;    // Velocidade do balanço (quanto maior, mais rápido)

    [Header("Idle Settings")]
    public float idleAmplitude = 0.02f; // Respiração sutil
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
        // Não processar head bob se o cursor estiver visível (em menu/quiz)
        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
        {
            // Suavemente retornar para a posição inicial
            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition,
                startPos,
                Time.deltaTime * 5f
            );

            // NÃO resetar rotação - deixar o PlayerCam controlar
            return;
        }

        if (IsMoving())
        {
            // Movimento clássico de terror: apenas vertical (cima/baixo)
            float bobY = Mathf.Sin(Time.time * walkFrequency) * walkAmplitude;

            // Apenas movimento Y, sem X (lateral) e sem Z
            cameraHolder.localPosition = startPos + new Vector3(0f, bobY, 0f);

            // NÃO TOCAR na rotação - deixar o PlayerCam controlar
        }
        else
        {
            // Idle: movimento sutil vertical
            float idleY = Mathf.Sin(Time.time * idleFrequency) * idleAmplitude;

            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition,
                startPos + new Vector3(0f, idleY, 0f),
                Time.deltaTime * 2f
            );

            // NÃO TOCAR na rotação - deixar o PlayerCam controlar
        }
    }

    bool IsMoving()
    {
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        return flatVel.magnitude > 0.1f;
    }
}
