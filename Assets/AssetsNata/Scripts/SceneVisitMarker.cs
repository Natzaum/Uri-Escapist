using UnityEngine;

/// <summary>
/// Script que marca quando o player visitou a cena_ruan
/// Coloque este script em qualquer GameObject da cena_ruan
/// </summary>
public class SceneVisitMarker : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Nome desta cena")]
    public string sceneName = "cena_ruan";

    void Start()
    {
        // Registrar que visitou esta cena
        if (sceneName == "cena_ruan")
        {
            DoorGameEnd.SetVisitedRequiredScene();
            Debug.Log($"✅ {sceneName} visitada! Porta de conclusão desbloqueada!");
        }
    }
}
