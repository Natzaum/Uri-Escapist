using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 25f;
    public float sensY = 25f;
    public Transform orientation;

    // Mantém a escala dos valores já serializados nas cenas, usando 60 FPS como referência.
    // Mouse X/Y já representam deslocamento por quadro: não multiplicar por deltaTime.
    private const float LegacySensitivityScale = 1f / 60f;
    private float rotationX;
    private float rotationY;
    private bool acceptingLook;
    private bool applicationFocused = true;

    private void Start()
    {
        rotationX = transform.eulerAngles.y;
        rotationY = Mathf.Clamp(Mathf.DeltaAngle(0f, transform.eulerAngles.x), -90f, 90f);
        if (!GameInterface.BlocksInput)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnEnable() => acceptingLook = false;

    private void OnApplicationFocus(bool focused)
    {
        applicationFocused = focused;
        acceptingLook = false;
    }

    private void Update()
    {
        if (!applicationFocused || GameInterface.BlocksInput ||
            Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
        {
            acceptingLook = false;
            return;
        }

        // O primeiro delta após recapturar o cursor pode incluir movimento feito fora do jogo.
        if (!acceptingLook)
        {
            acceptingLook = true;
            return;
        }

        ApplyLookDelta(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));
    }

    private void ApplyLookDelta(Vector2 delta)
    {
        float scale = GamePreferences.Sensitivity * LegacySensitivityScale;
        rotationX = Mathf.Repeat(rotationX + delta.x * sensX * scale, 360f);
        rotationY = Mathf.Clamp(rotationY - delta.y * sensY * scale, -90f, 90f);
        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0f, rotationX, 0f);
    }

    private void LateUpdate()
    {
        if (!acceptingLook || !applicationFocused || GameInterface.BlocksInput) return;
        // Aplicar a visão depois dos Updates evita depender da ordem do movimento do jogador.
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }
}
