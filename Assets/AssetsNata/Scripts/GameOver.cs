using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverUI;

    void Start()
    {
        // No começo do jogo, trava o cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // garante que o tempo está rodando
        Time.timeScale = 1f;

        // garante que o painel está oculto
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // pausa o jogo
        Time.timeScale = 0f;

        // libera o cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Retry()
    {
        // volta o tempo ao normal
        Time.timeScale = 1f;

        // trava o cursor novamente
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // recarrega a cena
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Sair do jogo!");
        Application.Quit();
    }
}
