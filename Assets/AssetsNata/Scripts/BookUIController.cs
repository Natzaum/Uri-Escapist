using UnityEngine;
using TMPro;
using System.Collections;

public class BookUIController : MonoBehaviour
{
    [Header("Textos UI")]
    public TMP_Text objectiveText;
    public GameObject exitPrompt; // "Pressione ESC para sair"

    private void Start()
    {
        if (exitPrompt != null)
            exitPrompt.SetActive(false);
    }

    private void Update()
    {
        if (BookManager.Instance != null)
        {
            // Atualizar texto de objetivo
            if (objectiveText != null)
            {
                int collected = BookManager.Instance.GetBooksCollected();
                int required = BookManager.Instance.minBooksToWin;
                int errors = BookManager.Instance.GetErrors();
                int maxErrors = BookManager.Instance.maxErrors;

                objectiveText.text = $"Objetivo: {collected}/{required} livros corretos\n" +
                                   $"Erros: {errors}/{maxErrors + 1}";
            }

            // Mostrar prompt de saída se atingiu o objetivo
            if (BookManager.Instance.CanProgress())
            {
                if (exitPrompt != null && !exitPrompt.activeSelf)
                {
                    exitPrompt.SetActive(true);
                    Debug.Log("✓ Objetivo atingido! Você pode sair agora.");
                }
            }
        }
    }
}
