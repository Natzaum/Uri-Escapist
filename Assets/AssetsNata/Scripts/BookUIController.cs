using UnityEngine;
using TMPro;

// Mantém referências das cenas antigas; o objetivo agora pertence ao GameHud.
public class BookUIController : MonoBehaviour
{
    public TMP_Text objectiveText;
    public GameObject exitPrompt;
    private void Start()
    {
        if (objectiveText != null) objectiveText.enabled = false;
        if (exitPrompt != null) exitPrompt.SetActive(false);
    }
}
