using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 25f;
    public float sensY = 25f;
    public Transform orientation;

    [Header("Tontura por exaustão")]
    [Range(0f, 1f)] public float exhaustionEffectStrength = 1f;
    private PlayerStamina stamina;
    private PlayerStaminaSimple simpleStamina;
    private Camera viewCamera;
    private VHSFilterSettings vhs;
    private bool exhaustionTriggered;
    private float exhaustionIntensity;
    private float exhaustionPhase;
    private float appliedFovOffset;

    // Mantém a escala dos valores já serializados nas cenas, usando 60 FPS como referência.
    // Mouse X/Y já representam deslocamento por quadro: não multiplicar por deltaTime.
    private const float LegacySensitivityScale = 1f / 60f;
    private float rotationX;
    private float rotationY;
    private bool acceptingLook;
    private bool applicationFocused = true;

    private void Start()
    {
        stamina = FindFirstObjectByType<PlayerStamina>();
        simpleStamina = FindFirstObjectByType<PlayerStaminaSimple>();
        viewCamera = GetComponent<Camera>();
        if (viewCamera != null) vhs = viewCamera.GetComponent<VHSFilterSettings>();
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
        bool playing = acceptingLook && applicationFocused && !GameInterface.BlocksInput
            && Cursor.lockState == CursorLockMode.Locked && !Cursor.visible;
        RestoreExhaustionVisuals();
        if (!playing)
        {
            // Nenhum balanço ou pós-efeito sobre livros, pausa ou resultados.
            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
            return;
        }
        float energy = stamina != null ? stamina.GetStaminaPercent()
            : simpleStamina != null ? simpleStamina.GetStaminaPercent() : 1f;
        UpdateExhaustion(energy, Time.deltaTime);
        float amount = exhaustionIntensity * exhaustionEffectStrength;
        float roll = Mathf.Sin(exhaustionPhase * 1.7f) * 2.4f * amount;
        float pitch = Mathf.Sin(exhaustionPhase * 1.1f) * .7f * amount;
        float yaw = Mathf.Sin(exhaustionPhase * .8f) * .8f * amount;
        // Deslocamento visual apenas: não altera a direção de movimento nem acumula rotação.
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f) * Quaternion.Euler(pitch, yaw, roll);
        if (viewCamera != null)
        {
            appliedFovOffset = Mathf.Sin(exhaustionPhase * 2f) * 2.5f * amount;
            viewCamera.fieldOfView += appliedFovOffset;
        }
        if (vhs != null) vhs.ExhaustionIntensity = amount;
    }

    private void UpdateExhaustion(float energy, float deltaTime)
    {
        if (energy <= 0f) exhaustionTriggered = true;
        else if (energy >= 1f) exhaustionTriggered = false;
        float target = exhaustionTriggered ? 1f - Mathf.Clamp01(energy) : 0f;
        exhaustionIntensity = Mathf.MoveTowards(exhaustionIntensity, target, deltaTime * 1.5f);
        exhaustionPhase += deltaTime;
    }

    private void RestoreExhaustionVisuals()
    {
        if (viewCamera != null && appliedFovOffset != 0f)
            viewCamera.fieldOfView -= appliedFovOffset;
        appliedFovOffset = 0f;
        if (vhs != null) vhs.ExhaustionIntensity = 0f;
    }

    private void OnDisable()
    {
        RestoreExhaustionVisuals();
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        exhaustionTriggered = false;
        exhaustionIntensity = 0f;
        acceptingLook = false;
    }
}
