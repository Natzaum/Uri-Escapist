using UnityEngine;
using TMPro;

/// <summary>
/// Script de cronômetro que conta regressivamente
/// Se o tempo acabar, chama InstantGameOver do BookManager
/// </summary>
public class TimerManager : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Tempo limite em segundos")]
    public float timeLimit = 300f; // 5 minutos padrão
    
    [Tooltip("Se marcado, o cronômetro começa ativo")]
    public bool startActive = true;

    [Header("UI")]
    [Tooltip("TextMeshPro para mostrar o tempo (ex: 4:32)")]
    public TextMeshProUGUI timerDisplay;
    
    [Tooltip("Cor quando tempo está ok")]
    public Color normalColor = Color.white;
    
    [Tooltip("Cor quando tempo está acabando (< 60s)")]
    public Color warningColor = Color.yellow;
    
    [Tooltip("Cor quando crítico (< 10s)")]
    public Color criticalColor = Color.red;

    [Header("Audio (Opcional)")]
    [Tooltip("Som de alerta quando tempo acaba")]
    public AudioClip warningSound;
    private AudioSource audioSource;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private float timeRemaining;
    private bool isRunning = false;
    private bool hasEnded = false;
    private float lastWarningTime = 0f;

    public static TimerManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && warningSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        timeRemaining = timeLimit;
        isRunning = startActive;

        if (showDebugInfo)
        {
            Debug.Log($"⏱️ TimerManager iniciado!");
            Debug.Log($"   Tempo limite: {FormatTime(timeLimit)}");
            Debug.Log($"   Status: {(isRunning ? "ATIVO" : "PARADO")}");
        }

        UpdateDisplay();
    }

    void Update()
    {
        if (!isRunning || hasEnded)
            return;

        // Decrementar tempo
        timeRemaining -= Time.deltaTime;

        // Verificar se acabou
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            OnTimeUp();
        }

        // Verificar alertas
        CheckAlerts();

        // Atualizar display
        UpdateDisplay();
    }

    void CheckAlerts()
    {
        // Alerta a cada segundo no período crítico
        if (timeRemaining < 10f && timeRemaining > 0f)
        {
            if (Time.time - lastWarningTime > 1f)
            {
                if (warningSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(warningSound);
                }
                lastWarningTime = Time.time;
                Debug.LogWarning($"⏱️ AVISO: {Mathf.Ceil(timeRemaining)}s restantes!");
            }
        }
    }

    void OnTimeUp()
    {
        hasEnded = true;
        isRunning = false;

        Debug.LogError("💀 TEMPO ACABOU! Inimigo vindo infinitamente!");

        // Verificar se tem BookManager
        if (BookManager.Instance != null)
        {
            // Incrementar erros para ativar o comportamento de inimigo infinito
            BookManager.Instance.OnTimeUp();
        }
        else
        {
            Debug.LogError("❌ BookManager.Instance não encontrado!");
        }
    }

    void UpdateDisplay()
    {
        if (timerDisplay == null)
            return;

        // Formatar tempo
        string timeText = FormatTime(timeRemaining);
        timerDisplay.text = timeText;

        // Mudar cor conforme tempo
        if (timeRemaining < 10f)
            timerDisplay.color = criticalColor; // Vermelho: crítico
        else if (timeRemaining < 60f)
            timerDisplay.color = warningColor; // Amarelo: aviso
        else
            timerDisplay.color = normalColor; // Branco: ok
    }

    string FormatTime(float seconds)
    {
        int mins = (int)(seconds / 60f);
        int secs = (int)(seconds % 60f);
        return $"{mins}:{secs:D2}";
    }

    // Métodos públicos para controlar
    public void StartTimer()
    {
        if (!hasEnded)
        {
            isRunning = true;
            Debug.Log("▶️ Timer iniciado");
        }
    }

    public void PauseTimer()
    {
        isRunning = false;
        Debug.Log("⏸️ Timer pausado");
    }

    public void ResumeTimer()
    {
        if (!hasEnded)
        {
            isRunning = true;
            Debug.Log("▶️ Timer retomado");
        }
    }

    public void ResetTimer()
    {
        timeRemaining = timeLimit;
        hasEnded = false;
        isRunning = startActive;
        Debug.Log("🔄 Timer resetado");
        UpdateDisplay();
    }

    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
        Debug.Log($"⏱️ +{seconds}s adicionados! Total: {FormatTime(timeRemaining)}");
        UpdateDisplay();
    }

    public void SetTimeLimit(float newLimit)
    {
        timeLimit = newLimit;
        timeRemaining = newLimit;
        Debug.Log($"⏱️ Novo limite: {FormatTime(timeLimit)}");
        UpdateDisplay();
    }

    // Getters
    public float GetTimeRemaining() => timeRemaining;
    public float GetTimePercentage() => timeRemaining / timeLimit;
    public bool IsRunning() => isRunning;
    public bool HasEnded() => hasEnded;
    public string GetFormattedTime() => FormatTime(timeRemaining);
}
