using UnityEngine;

/// <summary>
/// Script que marca quando o player visitou o andar2
/// Coloque este script em qualquer GameObject do andar2
/// </summary>
public class SceneVisitMarker : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Nome desta cena")]
    public string sceneName = "andar2";

    void Start()
    {
        // Registrar que visitou esta cena
        if (sceneName == "andar2")
        {
            DoorGameEnd.SetVisitedRequiredScene();
            Debug.Log($"✅ {sceneName} visitada! Porta de conclusão desbloqueada!");
        }
    }
}
