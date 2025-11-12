using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Velocidade de movimento do player (ajuste aqui conforme necessário)")]
    public float moveSpeed = 5f;
    public float groundDrag = 5f;
    public float stopForce = 0.9f; // Quanto mais próximo de 1, mais rápido para (0.9 = 90% de freio)

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    public Transform groundCheck; // Adicione um transform vazio como child do player para melhor detecção
    public float groundDistance = 0.4f;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.mass = 1f; // Manter massa em 1 como você prefere
        
        // Force cursor lock on start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Configurações do Rigidbody
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Criar Physics Material com zero friction
        PhysicsMaterial playerMat = new PhysicsMaterial("PlayerMat");
        playerMat.dynamicFriction = 0f;
        playerMat.staticFriction = 0f;
        playerMat.frictionCombine = PhysicsMaterialCombine.Minimum;
        playerMat.bounciness = 0f;
        
        // Aplicar ao collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.material = playerMat;
        }
        
        Debug.Log($"Player Scale: {transform.localScale.y}");
        Debug.Log($"whatIsGround Layer Mask: {whatIsGround.value}");
    }

    void Update()
    {
        // Verificação de chão simplificada e robusta
        float checkDistance = 1.5f; // Distância fixa para evitar problemas de escala
        
        if (groundCheck != null)
        {
            // Usar CheckSphere com o groundCheck
            grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);
            
            Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance, grounded ? Color.green : Color.red);
        }
        else
        {
            // Raycast simples e eficaz
            grounded = Physics.Raycast(transform.position, Vector3.down, checkDistance, whatIsGround);
            
            Debug.DrawRay(transform.position, Vector3.down * checkDistance, grounded ? Color.green : Color.red);
        }

        MyInput();
        SpeedControl();

        // Aplicar drag maior quando no chão
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            
            // Se não há input, aplicar drag extra
            if (horizontalInput == 0f && verticalInput == 0f)
            {
                rb.linearDamping = groundDrag * 2f;
            }
        }
        else
        {
            rb.linearDamping = 0f;
        }
        
        // Re-lock cursor if it gets unlocked accidentally
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void MyInput()
    {
        // Só processar input se o cursor estiver travado
        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            // Movimento quando no chão
            if (horizontalInput != 0f || verticalInput != 0f)
            {
                // Há input - aplicar força
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            }
            else
            {
                // SEM input - FREAR AGRESSIVAMENTE
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                
                // Aplicar força contrária para parar
                rb.AddForce(-flatVel * stopForce * 10f, ForceMode.Force);
                
                // Se a velocidade for muito baixa, zerar completamente
                if (flatVel.magnitude < 0.5f)
                {
                    rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                }
            }
        }
        else
        {
            // Movimento no ar com menos controle
            rb.AddForce(moveDirection.normalized * moveSpeed * 2f, ForceMode.Force);
        }
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualizar a área de detecção de chão no editor
        if (groundCheck != null)
        {
            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
        else
        {
            // Mostrar o raycast quando não há groundCheck
            float rayLength = playerHeight * 0.5f + 0.2f;
            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);
            Gizmos.DrawWireSphere(transform.position + Vector3.down * rayLength, 0.2f);
        }
    }
}