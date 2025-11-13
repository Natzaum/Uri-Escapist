using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script que aplica configurações do Modo Pesadelo ao inimigo
/// Ativa Always Chase Player se o Modo Pesadelo estiver ativado
/// Executa automaticamente quando a cena de jogo é carregada
/// </summary>
public class NightmareModeManager : MonoBehaviour
{
    public EnemyAI enemyAI;

    void Start()
    {
        // Se o Modo Pesadelo não está ativado, não faz nada
        if (!MenuPrincipal.IsNightmareMode())
        {
            Debug.Log("ℹ️ Modo Pesadelo desativado");
            return;
        }

        // Encontrar o inimigo na cena
        if (enemyAI == null)
            enemyAI = GetComponent<EnemyAI>();

        if (enemyAI == null)
            enemyAI = FindObjectOfType<EnemyAI>();

        // Ativar Modo Pesadelo
        if (enemyAI != null)
        {
            ActivateNightmareMode();
        }
        else
        {
            Debug.LogWarning("⚠️ Nenhum EnemyAI encontrado na cena!");
        }
    }

    void ActivateNightmareMode()
    {
        enemyAI.alwaysChasePlayer = true;
        Debug.Log("🌙 MODO PESADELO ATIVADO! Inimigo sempre perseguindo!");
    }

    // Reset quando voltar ao menu
    public static void ResetNightmareMode()
    {
        MenuPrincipal.ResetMenu();
    }
}
