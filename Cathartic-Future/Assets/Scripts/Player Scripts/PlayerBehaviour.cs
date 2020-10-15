using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Controlador Principal del Personaje
/// </summary>
public class PlayerBehaviour : MonoBehaviour
{
    #region Variables Inicializables
    [Header("Object References")]
    [Tooltip("Controlador del jugador (Player Controller)")]
    [SerializeField] CharacterController controller;
    [Tooltip("Animator Controller del personaje (para sus animaciones)")]
    [SerializeField] Animator anim;
    [Tooltip("Animator auxiliar para el personaje (para el control de los eventos como el salto)")]
    [SerializeField] Animator animatorController;
    [Tooltip("Referencia a la componente Transform de la cámara")]
    [SerializeField] Transform cameraPos;
    [Tooltip("Audio Source del personaje (Para reproducir sonidos)")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("Parte inferior de la ropa del personaje (Animada con físicas)")]
    [SerializeField] MassSpring2D cama;

    public BoxCollider collider;
    public float weight = 2f;
    public Vector3 force;

    [Header("Shot Sound")]
    [Tooltip("Sonido de disparo")]
    [SerializeField] AudioClip shotSound;
    [Tooltip("Volumen del sonido de disparo")]
    [SerializeField] float shotSoundVolume;
    [Tooltip("Audio Clip")]
    [SerializeField] float shot_SpacialBlend;

    [Header("Frase 1")]
    [Tooltip("Audio Clip")]
    [SerializeField] AudioClip Frase1_Clip;
    [Tooltip("Audio Clip")]
    [SerializeField] float F1_Volume;
    [Tooltip("Audio Clip")]
    [SerializeField] float F1_SpacialBlend;

    [Header("Frase 2")]
    [Tooltip("Audio Clip")]
    [SerializeField] AudioClip Frase2_Clip;
    [Tooltip("Audio Clip")]
    [SerializeField] float F2_Volume;
    [Tooltip("Audio Clip")]
    [SerializeField] float F2_SpacialBlend;

    [Header("Variables")]
    [Tooltip("Velocidad del personaje al andar")]
    [SerializeField] float walkSpeed = 2f;
    [Tooltip("Velocidad del personaje al correr")]
    [SerializeField] float runSpeed = 4f;
    [Tooltip("Velocidad del personaje al andar agachado")]
    [SerializeField] float crouchSpeed = 1f;
    [Tooltip("Fuerza de la gravedad sobre EL PERSONAJE")]
    [SerializeField] float gravity = 20f;
    [Tooltip("Tiempo máximo de caída")]
    [SerializeField] float maxFallingTimer = 0.4f;
    [Tooltip("Fuerza del salto (Determina la altura del salto)")]
    [SerializeField] float jumpForce = 20;
    [Tooltip("Determina si se desea ocultar el ratón de la escena")]
    [SerializeField] bool hideCursor = true;
    [Tooltip("Estado del personaje")]
    [SerializeField] PlayerState playerState;

    [Header("Camera")]
    [Tooltip("Alturas de la cámara virtual (3 estados)")]
    [SerializeField] float[] heights;
    [Tooltip("Radios de la cámara virtual (3 estados)")]
    [SerializeField] float[] radius;
    [Tooltip("Referencia a la cámara virtual")]
    [SerializeField] CinemachineFreeLook freelook;
    #endregion

    #region Variables Privadas
    private float currentSpeed = 0f;                    // Velocidad actual del personaje
    private float speedSmoothVelocity = 0.1f;           // Velocidad en el suavizado de la animación
    private float speedSmoothTime = 0.1f;               // Tiempo que tarda el suavizado
    private float rotationSpeed = 0.1f;                 // Velocidad de rotación del personaje
    private float fallingTimer = 0;                     // Contador que determina si el personaje está cayendo o no
    private float timerForWashing = 6f;                 // Contador que determina cuándo el personaje ejecuta la animación
                                                        // de secarse la frente
    private float timerForEndWashing = 2f;              // Tiempo que tarda el personaje en secarse la frente
    private float jumpCoolDown = 0;                     // Tiempo entre salto y salto
    private bool runJump = false;                       // Determina si el personaje al saltar ejecuta el salto corriendo o no
    private bool canMove = true;                        // Determina si el personaje se puede mover
    private bool canMove2 = true;                       // Determina si el personaje se puede mover
    private bool canJump2 = true;                       // Determina si el personaje puede saltar
    private bool isFalling = false;                     // Determina si el personaje está cayendo
    private bool canJump = true;                        // Determina si el personaje puede saltar
    private bool isFallingG = true;                     // Determina si el personaje está cayendo
    private bool canWash = true;                        // Determina si el personaje puede secarse la frente
    private bool isDead = false;                        // Determina si el personaje está muerto
    private Vector3 gravityVector = Vector3.zero;       // Vector de la gravedad
    #endregion

    #region Variables de Solo Lectura
    private readonly float MAX_TIMER_FOR_WASHING = 6f;            // Tiempo máximo que puede tardar el personaje en ejecutar la 
                                                                  // animación de secarse la frente
    private readonly CinemachineFreeLook.Orbit[] ORIGINAL_ORBITS; // Orbitas originales de la cámara virtual
    #endregion

    #region Otras Variables
    [HideInInspector] public bool endedJump = false;   // Determina si el salto ha concluido
    [HideInInspector] public bool isImpulsing = false; // Determina si el personaje se está impulsando (salto básico)
    #endregion

    // Estados del personaje
    enum PlayerState
    {
        Walking = 0,
        Falling = 1,
        Running = 2,
        Jumping = 3,
        Crouching = 4
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        // Si el valor del bool que determina si se oculta o no es "true",
        // se oculta el ratón
        if (hideCursor)
        {
            Cursor.visible = false; // Se oculta el ratón
        }

        // Se recorren las órbitas de la cámara y se inicializan
        for (int i = 0; i < freelook.m_Orbits.Length; i++)
        {
            freelook.m_Orbits[i].m_Height = heights[i]; // Alturas
            freelook.m_Orbits[i].m_Radius = radius[i];  // Radios
        }
    }

    private void FixedUpdate()
    {
        
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Se calcula el movimiento del personaje
        Move();
        
        // Se comprueba si el personaje se debe secar la frente
        if(timerForWashing <= 0)
        {
            // Se comprueba si el personaje PUEDE secarse la frente
            if (!anim.GetBool("IsWashing") && canWash)
            {
                canWash = false;                 // No se puede secar la frente durante la animación
                anim.SetBool("IsWashing", true); // Se ejecuta la animación de Secarse la Frente
                
            }
            
            // Si el tiempo para terminar la animación ha concluido
            if(timerForEndWashing <= 0)
            {
                // Se reinicia el primer contador
                timerForWashing = MAX_TIMER_FOR_WASHING + 6f;
                // Se reinicia el segundo contador
                timerForEndWashing = 2;
                // Se reinicia el bool
                canWash = true;
                // Se termina la animación
                anim.SetBool("IsWashing", false);
            }
            else 
            {
                // No se puede secar la frente durante la animación
                canWash = false;
                // Se actualiza el contador para terminar la animación
                timerForEndWashing -= Time.deltaTime;
            }
        }
        else
        {
            // Se actualiza el contador para ejecutar la animación
            timerForWashing -= Time.deltaTime;
        }

        // Input del teclado
        Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 forward = cameraPos.forward; // Dirección frontal
        Vector3 right = cameraPos.right;     // Dirección lateral

        // Click derecho del ratón para apuntar estando quieto
        if (Input.GetKey(KeyCode.Mouse1) && movementInput == Vector2.zero)
        {
            anim.SetBool("IsAiming", true); // Se ejecuta la animación de apuntado
            canMove2 = false;               // El jugador no se puede mover mientras apunta
        }
        else
        {
            anim.SetBool("IsAiming", false); // Concluye la animación de apuntado
            canMove2 = true;                 // El jugador se puede mover tras terminar de apuntar
        }

        
        // Si el jugador se encuentra apuntando el arma, al presionar el click
        // izquierdo del ratón, este dispara
        if (Input.GetKeyDown(KeyCode.Mouse0) && !canMove2 )
        {
            Shoot();
        }

        // Al soltar el click izquierdo concluye la acción de disparar
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            anim.SetBool("IsShooting", false);
        }

        // Si se presiona la tecla "1" estando quieto, se ejecuta un gesto
        if (Input.GetKeyDown(KeyCode.Alpha1) && canMove2)
        {
            anim.SetTrigger("Gesto1");
        }

        // Si se presiona la tecla "2" estando quieto y vivo, el personaje reproduce
        // la animación de muerte
        if (Input.GetKeyDown(KeyCode.Alpha2) && canMove2 && canMove && !isDead)
        {
            // Se bloquea todo tipo de movimiento
            canMove = false;
            canMove2 = false;
            // Se ejecuta la animación de muerte
            anim.SetBool("IsDead",true);
            // Al personaje se le considera muerto
            isDead = true; 
        }else if(Input.GetKeyDown(KeyCode.Alpha2) && isDead) // Se puede revivir pulsando de nuevo la tecla "2"
        {
            // El personaje ya puede moverse
            canMove = true; 
            canMove2 =  true;
            // Se anula la animación de muerte
            anim.SetBool("IsDead", false); isDead = false;
        }

        // Si se presiona la tecla "3" estando quieto, se reproduce una frase
        if (Input.GetKeyDown(KeyCode.Alpha3) && canMove2)
        {
            anim.SetTrigger("Frase1");
            //Time.timeScale = 0.3f;
            StartCoroutine(PlaySound(Frase1_Clip, F1_Volume, F1_SpacialBlend, 0.4f));
        }

        // Si se presiona la tecla "4" estando quieto, se reproduce una frase
        if (Input.GetKeyDown(KeyCode.Alpha4) && canMove2)
        {
            anim.SetTrigger("Frase2");
            //Time.timeScale = 0.3f;
            StartCoroutine(PlaySound(Frase2_Clip, F2_Volume, F2_SpacialBlend, 0.1f));
        }
    }

    IEnumerator PlaySound(AudioClip clip, float volume, float spacialBlend, float time)
    {
        yield return new WaitForSeconds(time);
        audioSource.spatialBlend = spacialBlend;
        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Función que determina el disparo del personaje
    /// </summary>
    private void Shoot()
    {
        anim.SetBool("IsShooting", true);     // Se ejecuta la animación de disparo
        audioSource.volume = shotSoundVolume; // Se ajusta el volumen del sonido del disparo
        audioSource.spatialBlend = shot_SpacialBlend;
        audioSource.PlayOneShot(shotSound);   // Se reproduce el sonido del disparo
    }

    /// <summary>
    /// Movimiento del personaje
    /// </summary>
    private void Move()
    {
        // Recibimos el input del movimiento procedente de las teclas del teclado
        Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Momiento frontal
        Vector3 forward = cameraPos.forward;

        // Momiento lateral
        Vector3 right = cameraPos.right;

        // Reseteamos los valores del forward y right
        forward.y = 0f;
        right.y = 0f;

        // Normalizamos los vectores forward y right
        forward.Normalize();
        right.Normalize();

        // Vector de la posición deseada
        Vector3 desiredMoveDirection;

        // Si el personaje puede moverse
        if (canMove && canMove2)
        {
            // Se actualiza la dirección a la que se desea avanzar en función de los inputs del teclado (A, W, S y D)
            desiredMoveDirection = (forward * movementInput.y + right * movementInput.x).normalized;
            desiredMoveDirection.Normalize(); // Se normaliza la dirección del movimiento
        }
        else
        {
            desiredMoveDirection = Vector3.zero; // La dirección del movimiento es nula
        }

        Vector3 jumpImpulse = Vector2.zero; // El impulso del salto es nulo al inicio de la iteración
        
        // Si el personaje NO SE ENCUENTRA en el suelo (o tocando el suelo)
        if (!controller.isGrounded)
        {
            // Si el personaje está saltando, se actualiza el bool que determina si está cayendo
            if(playerState == PlayerState.Jumping)
            {
                isFalling = true;
            }

            // Si el timer de la caída es menor que cero, el personaje comienza a caer y cambia de estado
            if (fallingTimer <= 0)
            {
                playerState = PlayerState.Falling;           // Cambio de estado

                gravityVector.y += gravity * Time.deltaTime; // Cálculo de la fuerza de la gravedad
            }
            else
            {
                fallingTimer -= Time.deltaTime;       // Se reduce el valor del timer
                Vector3 gravityVector = Vector3.zero; // El valor de la fuerza de la gravedad es cero
            }

            if (!isFallingG)
            {
                isFallingG = true;                    // Se activa la actualización de la caída
                Vector3 gravityVector = Vector3.zero; // El vector gravedad se vuelve cero
            }
        }
        else
        {
            isFallingG = false; // Se anula la actualización de la caída
            // Si no se están presionando las teclas Shift y C
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.C))
            {
                // Si el personaje se encontraba cayendo, se activa la animación de impacto contra el suelo
                if (isFalling)
                {
                    anim.SetBool("JumpImpact", true);
                    anim.SetBool("IsJumping", false);
                    isFalling = false;   // El personaje deja de caer
                    jumpCoolDown = 0.9f; // No se puede volver a saltar hasta que este contador sea igual a cero
                }
                playerState = PlayerState.Walking; // El estado actual del personaje es Andando
                runJump = false;                   // No puede realizar un salto corriendo si está andando
            }
            // Si se presiona la tecla Shift mientras el personaje se mueve en una dirección, se ejecuta la animación
            // y el estado de correr
            else if(Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.C) && desiredMoveDirection.magnitude > 0)
            {
                // Si el personaje se encontraba cayendo, se activa la segunda animación de impacto contra el suelo
                if (isFalling)
                { 
                    anim.SetBool("JumpImpact", true);
                    anim.SetBool("IsJumping", false);
                    isFalling = false;   // El personaje deja de caer
                    jumpCoolDown = 0.9f; // No se puede volver a saltar hasta que este contador sea igual a cero
                }
                playerState = PlayerState.Running; // El estado actual del personaje es Corriendo
            }
            // Si se presiona la tecla C mientras el personaje se mueve en una dirección, se ejecuta la animación
            // y el estado de Andar Agachado
            else if (Input.GetKey(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift) && movementInput != Vector2.zero)
            {
                playerState = PlayerState.Crouching; // El estado actual del personaje es Andar Agachado
            }
            // Si se presiona la tecla Shift mientras el personaje se encuentra quieto, el estado actual es Caminando
            else if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.C) && desiredMoveDirection.magnitude == 0)
            {
                playerState = PlayerState.Walking; // El estado actual del personaje es Andando
            }
            // Si se presiona la tecla C mientras el personaje se encuentra quieto, el estado actual es Caminando
            else if (Input.GetKey(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift) && movementInput == Vector2.zero)
            {
                playerState = PlayerState.Walking; // El estado actual del personaje es Andando
            }  
            if (!canJump2)
            {
                anim.SetBool("JumpImpact", true); // Se ejecuta la animación de impacto contra el suelo
                anim.SetBool("IsJumping", false);
                canJump2 = true;
            }
            fallingTimer = maxFallingTimer; // Se actualiza el contador de la caída
        }

        // Si el la dirección del moviento es distinta a una nula, se rota el personaje en dicha dirección
        // mediante una interpolación que la suaviza dicha rotación
        if (desiredMoveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                                                  Quaternion.LookRotation(desiredMoveDirection), 
                                                  rotationSpeed);
        }
        float targetSpeed = 0f; // Velocidad deseada

        #region CONSECUENCIAS --> ESTADOS
        switch (playerState)
        {
            case PlayerState.Walking:
                // Si el personaje se está moviendo, la cama se agita más
                if(desiredMoveDirection != Vector3.zero)
                {
                    cama.strength = 4f;
                    cama.randomStrength = 45f;
                }
                // Si no, la cama se mueve mucho menos
                else
                {
                    cama.strength = 2f;
                    cama.randomStrength = 70f;
                }

                // Si el personaje no se encuentra saltando
                if (playerState != PlayerState.Jumping)
                {
                    // Se actualiza la velocidad objetivo
                    targetSpeed = walkSpeed * movementInput.magnitude;
                    
                    // Si el personaje está quieto, se ejecuta la animación "Idle"
                    if (movementInput.magnitude == 0)
                    {
                        anim.SetBool("IsIdle", true);
                        anim.SetBool("IsWalking", false);
                        anim.SetBool("IsRunning", false);
                    }
                    // Si el personaje no está quieto, se ejecuta la animación "Walking"
                    else
                    {
                        anim.SetBool("IsCrouching", false);
                        anim.SetBool("IsWalking", true);
                        anim.SetBool("IsRunning", false);
                        anim.SetBool("IsIdle", false);
                        anim.SetBool("IsWashing", false);
                    }
                }
                break;
            case PlayerState.Running:
                // Al correr, la cama se mueve mucho más
                cama.strength = 8f;
                cama.randomStrength = 25f;

                // Se actualizan el movimiento y la animación
                targetSpeed = runSpeed * movementInput.magnitude;
                anim.SetBool("IsRunning", true);
                anim.SetBool("IsIdle", false);
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsWashing", false);
                break;
            case PlayerState.Falling:
                // Si se encuentra corriendo antes del salto, el salto es más fuerte
                if(runJump)
                    targetSpeed = runSpeed * movementInput.magnitude;
                else
                    targetSpeed = walkSpeed * movementInput.magnitude;
                break;
            case PlayerState.Crouching:
                // Se actualiza la velocidad de movimiento y la animación
                targetSpeed = crouchSpeed * movementInput.magnitude;
                anim.SetBool("IsCrouching", true);
                anim.SetBool("IsRunning", false);
                anim.SetBool("IsIdle", false);
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsWashing", false);
                break;
            default:
                // Se actualiza la velocidad a la de caminar
                targetSpeed = walkSpeed * movementInput.magnitude;
                break;
        }
        #endregion

        // Se actualiza y se suaviza la velocidad de movimiento actual
        currentSpeed = Mathf.SmoothDamp(currentSpeed, 
                                        targetSpeed, 
                                        ref speedSmoothVelocity, 
                                        speedSmoothTime);

        #region SALTO Y AGACHARSE
        // Si se presiona la tecla "Espacio" mientras no se encuentra el jugador cayendo
        if (Input.GetKeyDown(KeyCode.Space) && playerState != PlayerState.Falling )
        {
            switch (playerState)
            {
                // Tipo de salto 1
                case PlayerState.Walking:
                    if (canJump)
                    {
                        canMove = false; // No se puede mover durante el impulso
                        anim.SetBool("IsIdle", false);
                        anim.SetBool("IsWalking", false);
                        anim.SetBool("JumpImpact", false);
                        anim.SetBool("IsJumping", true);
                        anim.SetBool("IsWashing", false);
                        animatorController.Play("Jump");
                        endedJump = false;
                        canJump = false;
                    }
                    break;
                // Tipo de salto 2
                case PlayerState.Running:
                    if (canJump2)
                    {
                        anim.SetBool("IsIdle", false);
                        anim.SetBool("IsWalking", false);
                        anim.SetBool("JumpImpact", false);
                        anim.SetBool("IsJumping", true);
                        anim.SetBool("IsWashing", false);
                        canJump2 = false;
                        gravityVector.y = -jumpForce/1.1f;
                        runJump = true;
                    }
                    break;
                default:
                    break;
            }

        }
        // Si se presiona la tecla C mientras se anda, se actualiza el estado a Agachado
        else if(Input.GetKeyDown(KeyCode.C))
        {
            switch (playerState)
            {
                case PlayerState.Walking:
                    playerState = PlayerState.Crouching;
                    anim.SetBool("IsIdle", false);
                    anim.SetBool("IsWalking", false);
                    anim.SetBool("JumpImpact", false);
                    anim.SetBool("IsJumping", false);
                    anim.SetBool("IsCrouching", true);
                    break;
                default:
                    break;
            }

        }
        // Si se suelta la tecla C mientras se anda, se actualiza el estado a Caminando
        else if (Input.GetKeyUp(KeyCode.C))
        {
            switch (playerState)
            {
                case PlayerState.Walking:
                    playerState = PlayerState.Walking;
                    anim.SetBool("IsIdle", false);
                    anim.SetBool("IsWalking", false);
                    anim.SetBool("JumpImpact", false);
                    anim.SetBool("IsJumping", false);
                    anim.SetBool("IsCrouching", false);
                    break;
                default:
                    break;
            }
        }

        // Si el contador es menor a 0, el jugador puede volver a saltar
        if(jumpCoolDown <= 0 && jumpCoolDown >= -0.1f)
        {
            canJump = true;
        }

        // Se actualiza el contador que condiciona la capacidad de saltar
        jumpCoolDown -= Time.deltaTime;
        #endregion

        #region JumpTimer
        if (isImpulsing)
        {
            canJump = false;
            gravityVector.y = -jumpForce / 1f;
            playerState = PlayerState.Jumping;
        }
        if (endedJump){
            canMove = true;
        }
        #endregion

        // Se normaliza la dirección del movimiento
        desiredMoveDirection.Normalize();
        // Se mueve al personaje
        controller.Move(desiredMoveDirection*currentSpeed*Time.deltaTime);
        // Se actualiza la fuerza de la gravedad
        controller.Move(-gravityVector * Time.deltaTime);

        
    }

    /// <summary>
    /// Muerte del personaje
    /// </summary>
    public void Die()
    {
        GetComponent<CharacterController>().enabled = false;
        transform.position = new Vector3(44.98f, 74, 26);
        GetComponent<CharacterController>().enabled = true;
    }
}
