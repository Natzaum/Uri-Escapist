using System.Collections;
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

    [Header("Sound")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.12f;
    [Range(0f, 1f)]
    public float playFeedbackVolume = 0.45f;

    private static bool isNightmareMode = false;
    private bool isStartingGame;

    void Start()
    {
        MenuAmbientAudio ambientAudio = GetComponent<MenuAmbientAudio>();
        if (ambientAudio == null)
            ambientAudio = gameObject.AddComponent<MenuAmbientAudio>();

        ambientAudio.Initialize(ambientVolume, playFeedbackVolume);

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
        if (isStartingGame)
            return;

        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        isStartingGame = true;
        if (playButton != null)
            playButton.interactable = false;

        Debug.Log($"🎮 Iniciando jogo... Modo Pesadelo: {isNightmareMode}");
        Time.timeScale = 1f;

        MenuAmbientAudio ambientAudio = GetComponent<MenuAmbientAudio>();
        if (ambientAudio != null)
        {
            ambientAudio.PlayStartFeedback();
            yield return ambientAudio.FadeOut(0.75f);
        }

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
