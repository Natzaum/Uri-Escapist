using UnityEngine;

/// <summary>
/// GUIA DE BALANCEAMENTO - Player vs Enemy
/// 
/// VELOCIDADES ATUAIS:
/// ==================
/// Player (normal):     7 m/s
/// Player (sprint):    12 m/s
/// Enemy (patrol):      0.2 m/s (base antes de escala)
/// Enemy (chase):       2 m/s (base antes de escala)
/// 
/// PROBLEMA IDENTIFICADO:
/// ====================
/// 1. Player é MUITO mais rápido que o inimigo:
///    - Normal: 7 m/s vs 0.2 m/s patrol = 35x mais rápido!
///    - Sprint: 12 m/s vs 2 m/s chase = 6x mais rápido
///    - Inimigo nunca consegue pegar o player
///
/// 2. Com auto-scale (x8):
///    - Enemy patrol: 0.2 * 8 = 1.6 m/s
///    - Enemy chase:  2 * 8 = 16 m/s
///    - Player normal: 7 m/s (sem escala!)
///    - Desequilibrado!
///
/// RECOMENDAÇÃO DE BALANCEAMENTO:
/// ==============================
/// 
/// OPÇÃO 1: Aumentar velocidade do inimigo
/// ----------------------------------------
/// Enemy patrol:  0.5 m/s → ~4 m/s (próximo do player)
/// Enemy chase:   2 m/s → ~8 m/s (mais rápido que player normal, mas não sprint)
/// 
/// OPÇÃO 2: Diminuir velocidade do player
/// ----------------------------------------
/// Player normal: 7 m/s → 5 m/s
/// Player sprint: 12 m/s → 8 m/s
/// 
/// OPÇÃO 3: BALANCEAMENTO IDEAL (Recomendado)
/// -------------------------------------------
/// Player (normal):     5 m/s
/// Player (sprint):     10 m/s
/// Enemy (patrol):      3 m/s (com escala: 3 * 8 = 24 m/s)
/// Enemy (chase):       6 m/s (com escala: 6 * 8 = 48 m/s)
/// Enemy (always chase):7 m/s (com escala: 7 * 8 = 56 m/s)
/// 
/// DINÂMICA DE JOGO COM ESSAS VELOCIDADES:
/// ========================================
/// 1. Patrulha Normal (patrol=3, player normal=5):
///    - Player consegue escapar correndo
///    - Inimigo consegue aproximar se não correr
///    - Dinâmica: "Fuja antes de ficar perto"
///
/// 2. Chase Normal (chase=6, player normal=5):
///    - Inimigo é LEVEMENTE mais rápido
///    - Player DEVE sprintar para fugir
///    - Stamina é crítica!
///
/// 3. Sprint (sprint=10, chase=6):
///    - Player consegue manter distância ou aumentar
///    - Mas stamina se esgota
///    - Estratégia: "Corre mas economiza stamina"
///
/// 4. Always Chase (always=7, sprint=10):
///    - Player consegue fugir se sprintar
///    - Mas perseguição é intensa
///    - Final: "Luta contra o tempo e stamina"
///
/// CONFIGURAÇÃO RECOMENDADA NO INSPECTOR:
/// ======================================
/// 
/// PlayerMove.cs:
/// - moveSpeed = 5 (era 7)
/// 
/// PlayerStamina.cs:
/// - walkSpeed = 5 (era 7)
/// - sprintSpeed = 10 (era 12)
/// 
/// EnemyAI.cs:
/// - patrolSpeed = 0.375 (deixar assim, com escala fica 3)
/// - chaseSpeed = 0.75 (deixar assim, com escala fica 6)
/// - extendedDetectionRange = 20
/// 
/// BookManager.cs:
/// - baseChaseSpeed = 0.75
/// - basePatrolSpeed = 0.375
/// - finalChaseSpeed = 8.75 (0.75 * 8 + 2.75 de aumentos)
/// 
/// TESTE A DINÂMICA:
/// ================
/// 1. Ande normalmente (5 m/s) e fuja do inimigo patrulhando (3 m/s)
///    → Deve conseguir fugir facilmente caminhando
///
/// 2. Em chase (6 m/s) enquanto caminha (5 m/s)
///    → Inimigo está logo atrás, sprinte é obrigatório
///
/// 3. Sprinte (10 m/s) longe do chase (6 m/s)
///    → Consegue escapar, mas stamina acaba rápido
///
/// 4. Sem stamina, volte a caminhar (5 m/s) vs chase (6 m/s)
///    → Vai ser pego! Muito tenso!
///    → Dinâmica: "Escapei mas usei stamina, agora estou em perigo"
/// </summary>
public class BalancingGuide : MonoBehaviour
{
    // Este arquivo é apenas documentação
    // Não precisa de código funcional
}
