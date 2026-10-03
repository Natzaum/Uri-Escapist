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
    public static string GameMode { get; private set; } = "normal";
    private readonly string[] difficultyModes = { "facil", "normal", "dificil" };
    private readonly string[] difficultyLabels = { "Fácil", "Normal", "Difícil" };
    private Button[] difficultyButtons;

    private void CreateDifficultySelector()
    {
        if (playButton == null) return;
        difficultyButtons = new Button[3];
        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            int index = i;
            Button button = Instantiate(playButton, playButton.transform.parent);
            button.name = "Dificuldade_" + difficultyModes[i];
            // Replace the event as cloned buttons can contain persistent Inspector listeners.
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => {
                if (isStartingGame) return;
                GameMode = difficultyModes[index];
                RefreshDifficultySelector();
            });
            RectTransform rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2((i - 1) * 170f, -110f);
            rect.sizeDelta = new Vector2(160f, 44f);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) {
                label.enableAutoSizing = true;
                label.fontSizeMin = 14;
                label.fontSizeMax = 24;
            }
            difficultyButtons[i] = button;
        }
        TMP_Text source = playButton.GetComponentInChildren<TMP_Text>();
        if (source != null)
        {
            TMP_Text heading = Instantiate(source, playButton.transform.parent);
            heading.name = "DificuldadeTitulo";
            heading.text = "Dificuldade das perguntas";
            heading.fontSize = 20;
            heading.raycastTarget = false;
            RectTransform rect = heading.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -65f);
            rect.sizeDelta = new Vector2(520f, 35f);
        }
        RefreshDifficultySelector();
    }

    private void RefreshDifficultySelector()
    {
        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            TMP_Text label = difficultyButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = (GameMode == difficultyModes[i] ? "• " : "") + difficultyLabels[i];
        }
    }

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

        CreateDifficultySelector();

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        if (difficultyButtons != null)
            foreach (Button button in difficultyButtons) button.interactable = false;
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
