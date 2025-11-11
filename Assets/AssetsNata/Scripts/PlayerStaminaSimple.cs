using UnityEngine;

/// <summary>
/// Sistema de stamina SEM MODIFICAR a velocidade do player
/// A velocidade deve ser ajustada APENAS pelo Inspector em PlayerMove.moveSpeed
/// Este script apenas gerencia a barra de stamina e aplica efeitos visuais
/// </summary>
public class PlayerStaminaSimple : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    
    [Tooltip("Stamina gasta por segundo ao correr")]
    public float staminaDrainRate = 20f;
    
    [Tooltip("Stamina recuperada por segundo")]
    public float staminaRegenRate = 10f;
    
    [Tooltip("Delay antes de começar a regenerar")]
    public float regenDelay = 2f;

    [Header("UI")]
    public UnityEngine.UI.Image staminaBar;
    public GameObject staminaBarObject;

    private float timeSinceLastDrain = 0f;
    private bool isSprinting = false;

    void Start()
    {
        currentStamina = maxStamina;

        if (staminaBarObject != null)
            staminaBarObject.SetActive(false);

        Debug.Log("✓ PlayerStaminaSimple ativo - Velocidade controlada APENAS pelo Inspector");
    }

    void Update()
    {
        // Só processar se o cursor estiver travado (jogando)
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        HandleSprint();
        UpdateStaminaBar();
    }

    void HandleSprint()
    {
        bool canSprint = currentStamina > 0f;
        
        if (Input.GetKey(KeyCode.LeftShift) && canSprint)
        {
            // Iniciando ou continuando sprint
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);
            timeSinceLastDrain = 0f;

            // Mostrar barra enquanto gasta stamina
            if (staminaBarObject != null)
                staminaBarObject.SetActive(true);

            Debug.Log($"🏃 Sprinting - Stamina: {currentStamina:F1}/{maxStamina}");
        }
        else
        {
            isSprinting = false;
            timeSinceLastDrain += Time.deltaTime;

            // Regenerar stamina após delay
            if (timeSinceLastDrain >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }

            // Esconder barra quando cheia
            if (staminaBarObject != null && currentStamina >= maxStamina)
                staminaBarObject.SetActive(false);
            else if (staminaBarObject != null && currentStamina < maxStamina)
                staminaBarObject.SetActive(true);
        }
    }

    void UpdateStaminaBar()
    {
        if (staminaBar == null)
            return;

        // Atualizar preenchimento
        staminaBar.fillAmount = currentStamina / maxStamina;

        // Mudar cor conforme stamina
        if (currentStamina > 50f)
            staminaBar.color = Color.green; // Verde: OK
        else if (currentStamina > 25f)
            staminaBar.color = Color.yellow; // Amarelo: Cansado
        else
            staminaBar.color = Color.red; // Vermelho: Muito cansado
    }

    // Métodos públicos para consultar estado
    public bool IsSprinting() => isSprinting;
    public float GetStaminaPercent() => currentStamina / maxStamina;
}
