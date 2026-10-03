using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverUI;
    private bool isShowingGameOver;

    private void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (isShowingGameOver || GameInterface.WorldPaused) return;
        isShowingGameOver = true;
        if (gameOverUI != null) gameOverUI.SetActive(false);
        GameInterface.Instance.ShowResult(false);
    }

    public void Retry() => GameInterface.Instance.LoadScene(SceneManager.GetActiveScene().name);
    public void QuitGame() => GameInterface.Instance.LoadScene("UriMenu");
}
