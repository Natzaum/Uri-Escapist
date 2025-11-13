using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu Principal do jogo Uri Escapist
/// Com opção de Modo Pesadelo (Always Chase ativado)
/// Script para usar com Canvas criado manualmente no Inspector
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button exitButton;
    public Toggle nightmareToggle;
    public TextMeshProUGUI titleText;

    private static bool isNightmareMode = false;

    void Start()
    {
        // Encontrar elementos se não foram atribuídos no Inspector
        if (playButton == null)
            playButton = FindObjectOfType<Button>();

        if (exitButton == null)
            exitButton = FindObjectOfType<Button>();

        if (nightmareToggle == null)
            nightmareToggle = FindObjectOfType<Toggle>();

        if (titleText == null)
            titleText = FindObjectOfType<TextMeshProUGUI>();

        // Configurar listeners
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);

        if (nightmareToggle != null)
        {
            nightmareToggle.isOn = isNightmareMode;
            nightmareToggle.onValueChanged.AddListener(OnNightmareToggleChanged);
        }

        Time.timeScale = 1f; // Garantir que o jogo não está pausado

        Debug.Log("✅ Menu Principal carregado!");
        Debug.Log($"🌙 Modo Pesadelo atual: {isNightmareMode}");
    }

    void OnPlayButtonClicked()
    {
        Debug.Log($"🎮 Iniciando jogo... Modo Pesadelo: {isNightmareMode}");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainScene");
    }

    void OnExitButtonClicked()
    {
        Debug.Log("👋 Saindo do jogo...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void OnNightmareToggleChanged(bool isOn)
    {
        isNightmareMode = isOn;
        Debug.Log($"🌙 Modo Pesadelo: {(isNightmareMode ? "✅ ATIVADO" : "❌ DESATIVADO")}");
    }

    public static bool IsNightmareMode()
    {
        return isNightmareMode;
    }

    public static void ResetMenu()
    {
        isNightmareMode = false;
    }
}
