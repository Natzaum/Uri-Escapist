using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverUI;
    private bool isShowingGameOver = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    void LateUpdate()
    {
        if (!isShowingGameOver)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowGameOver()
    {
        Debug.Log("💀 ShowGameOver chamado!");
        isShowingGameOver = true;
        
        // Fechar quiz se estiver aberto
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.ForceCloseQuiz();
            Debug.Log("✓ Quiz fechado pelo Game Over");
        }
        
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Debug.Log("✓ Game Over UI ativada");
        }
        else
        {
            Debug.LogError("⚠️ Game Over UI não está atribuída!");
        }

        Time.timeScale = 0f;
        Debug.Log("⏸️ Jogo pausado para mostrar a tela de retry");

        DisablePlayerControls();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        Debug.Log("✓ Cursor liberado para clicar em Retry");
    }

    public void Retry()
    {
        Debug.Log("🔄 Retry pressionado!");
        isShowingGameOver = false;
        
        // Garantir que o tempo volte ao normal ANTES de recarregar
        Time.timeScale = 1f;

        // Fechar quiz se ainda estiver aberto
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.ForceCloseQuiz();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ReloadScene();
    }

    void ReloadScene()
    {
        Debug.Log("🔄 Recarregando cena agora...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Sair do jogo!");
        Application.Quit();
    }

    void DisablePlayerControls()
    {
        DisableBehavioursByTypeName("PlayerMove");
        DisableBehavioursByTypeName("PlayerCam");
        DisableBehavioursByTypeName("PlayerStamina");
        DisableBehavioursByTypeName("CameraHeadBob");
    }

    void DisableBehavioursByTypeName(string typeName)
    {
        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour != null && behaviour.GetType().Name == typeName)
            {
                behaviour.enabled = false;
            }
        }
    }
}
