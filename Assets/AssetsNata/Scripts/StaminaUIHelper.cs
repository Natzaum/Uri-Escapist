using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * INSTRUÇÕES PARA CRIAR A UI DE STAMINA:
 * 
 * 1. Na Hierarchy, crie:
 *    Canvas
 *    └── StaminaBar (GameObject vazio)
 *        ├── Background (Image)
 *        │   └── Fill (Image) ← Esta é a barra que vai encher/esvaziar
 *        └── StaminaText (TextMeshPro) [OPCIONAL]
 * 
 * 2. Configurar Background:
 *    - Anchor: Bottom-Center
 *    - Width: 200, Height: 20
 *    - Position Y: 50 (do fundo)
 *    - Color: Preto semi-transparente
 *    - Image Type: Sliced
 * 
 * 3. Configurar Fill:
 *    - Image Type: Filled
 *    - Fill Method: Horizontal
 *    - Fill Origin: Left
 *    - Color: Verde
 *    - Anchor: Stretch (preencher o pai)
 * 
 * 4. No Player, adicionar o script PlayerStamina:
 *    - Stamina Bar: Arraste o componente Image do "Fill"
 *    - Stamina Bar Object: Arraste o GameObject "StaminaBar"
 * 
 * 5. Ajustar valores no Inspector:
 *    - Max Stamina: 100
 *    - Stamina Drain Rate: 20 (gasta em 5 segundos)
 *    - Stamina Regen Rate: 10 (recupera em 10 segundos)
 *    - Regen Delay: 2 (espera 2s para começar a regenerar)
 *    - Walk Speed: 7
 *    - Sprint Speed: 12
 */

public class StaminaUIHelper : MonoBehaviour
{
    // Este script é apenas para documentação
    // Não precisa ser adicionado a nenhum objeto
}
