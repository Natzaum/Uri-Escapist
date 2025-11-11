using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f; // Stamina gasta por segundo ao correr
    public float staminaRegenRate = 10f; // Stamina recuperada por segundo
    public float regenDelay = 2f; // Delay antes de começar a regenerar

    [Header("Speed Settings")]
    [Tooltip("Velocidade ao caminhar (ajuste aqui no Inspector)")]
    public float walkSpeed = 7f;
    
    [Tooltip("Velocidade ao correr (ajuste aqui no Inspector)")]
    public float sprintSpeed = 12f;

    [Header("UI")]
    public Image staminaBar;
    public GameObject staminaBarObject;
    public bool alwaysShowBar = false; // Marcar para sempre mostrar a barra (debug)

    private PlayerMove playerMove;
    private float timeSinceLastSprint = 0f;
    private bool isSprinting = false;
    private bool isExhausted = false;

    void Start()
    {
        currentStamina = maxStamina;
        playerMove = GetComponent<PlayerMove>();

        // NÃO pegar velocidade do PlayerMove - usar os valores do Inspector!
        // Os valores walkSpeed e sprintSpeed são definidos aqui no Inspector

        // Esconder barra se estiver cheia (a menos que alwaysShowBar esteja ativo)
        if (staminaBarObject != null && !alwaysShowBar)
            staminaBarObject.SetActive(false);
        else if (staminaBarObject != null && alwaysShowBar)
            staminaBarObject.SetActive(true);
            
        Debug.Log("✓ PlayerStamina iniciado");
        Debug.Log($"  Walk Speed: {walkSpeed} | Sprint Speed: {sprintSpeed}");
        Debug.Log("  💡 Ajuste walkSpeed e sprintSpeed no Inspector!");
    }

    void Update()
    {
        // Só processar se o cursor estiver travado (jogando)
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        HandleSprint();
        RegenerateStamina();
        UpdateUI();
    }

    void HandleSprint()
    {
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        // Verificar se pode correr
        if (wantsToSprint && isMoving && currentStamina > 0 && !isExhausted)
        {
            // CORRENDO
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            timeSinceLastSprint = 0f;

            // Atualizar velocidade do player
            if (playerMove != null)
            {
                playerMove.moveSpeed = sprintSpeed;
            }

            // Mostrar barra de stamina
            if (staminaBarObject != null && !staminaBarObject.activeSelf)
            {
                staminaBarObject.SetActive(true);
                Debug.Log("Barra de stamina ativada!");
            }

            // Se stamina acabar, ficar exausto
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
                Debug.Log("😓 Sem stamina! Aguarde recuperar...");
            }
        }
        else
        {
            // ANDANDO
            isSprinting = false;

            // Voltar para velocidade de caminhada
            if (playerMove != null)
            {
                playerMove.moveSpeed = walkSpeed;
            }

            timeSinceLastSprint += Time.deltaTime;
        }
    }

    void RegenerateStamina()
    {
        // Só regenerar após o delay
        if (!isSprinting && timeSinceLastSprint >= regenDelay)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                isExhausted = false;

                // Esconder barra quando cheia (a menos que alwaysShowBar esteja ativo)
                if (staminaBarObject != null && currentStamina >= maxStamina && !alwaysShowBar)
                    staminaBarObject.SetActive(false);
            }
        }
    }

    void UpdateUI()
    {
        if (staminaBar != null)
        {
            float fillValue = currentStamina / maxStamina;
            staminaBar.fillAmount = fillValue;

            // Mudar cor baseado na stamina
            if (currentStamina <= 20f)
            {
                staminaBar.color = Color.red; // Crítico
            }
            else if (currentStamina <= 50f)
            {
                staminaBar.color = Color.yellow; // Baixo
            }
            else
            {
                staminaBar.color = Color.green; // OK
            }
        }
        else
        {
            Debug.LogWarning("Stamina Bar (Image) não está atribuído!");
        }
    }

    // Método público para verificar se está correndo
    public bool IsSprinting()
    {
        return isSprinting;
    }

    // Método público para verificar stamina
    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }

    // Debug visual
    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 100, 300, 20), $"Stamina: {currentStamina:F1}/{maxStamina}");
            GUI.Label(new Rect(10, 120, 300, 20), $"Sprint: {isSprinting} | Exausto: {isExhausted}");
        }
    }
}
