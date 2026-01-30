using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Controlador 3D básico con movimiento, cámara, salto, dash y parry.
/// Requiere: CharacterController en el mismo GameObject, InputSystem_Actions con mapa Player.
/// Asigna la cámara en el Inspector para movimiento relativo a cámara.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CharacterController3D : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] InputActionAsset inputActions;

    [Header("Movimiento")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float rotationSmoothTime = 0.1f;

    [Header("Salto")]
    [SerializeField] float jumpForce = 6f;
    [SerializeField] float gravity = -18f;
    [Tooltip("Radio de la esfera que detecta el suelo (debajo del personaje).")]
    [SerializeField] float groundCheckRadius = 0.25f;
    [Tooltip("Solo detecta colisiones en estos layers. Pon el suelo en un layer (ej: Ground) y asígnelo aquí para no detectarte a ti mismo.")]
    [SerializeField] LayerMask groundMask = -1;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 15f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 1f;

    [Header("Parry")]
    [SerializeField] float parryWindowDuration = 0.3f;
    [SerializeField] float parryCooldown = 1.2f;

    CharacterController _controller;
    InputAction _moveAction;
    InputAction _lookAction;
    InputAction _jumpAction;
    InputAction _sprintAction;
    InputAction _dashAction;
    InputAction _parryAction;

    Vector3 _velocity;
    float _rotationVelocityY;
    float _dashTimer;
    float _dashCooldownRemaining;
    float _parryWindowRemaining;
    float _parryCooldownRemaining;
    bool _isGrounded;
    bool _isDashing;
    bool _isParrying;
    bool _jumpConsumed;

    /// <summary>True mientras la ventana de parry está activa (puedes recibir un ataque para parry).</summary>
    public bool IsParryWindowActive => _isParrying && _parryWindowRemaining > 0f;

    /// <summary>Llamar desde el sistema de daño cuando un ataque es parado.</summary>
    public event Action OnParrySuccess;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = Camera.main != null ? Camera.main.transform : null;
        BindInput();
    }

    void BindInput()
    {
        if (inputActions == null) return;
        var map = inputActions.FindActionMap("Player");
        if (map == null) return;

        _moveAction = map.FindAction("Move");
        _lookAction = map.FindAction("Look");
        _jumpAction = map.FindAction("Jump");
        _sprintAction = map.FindAction("Sprint");
        _dashAction = map.FindAction("Dash");
        _parryAction = map.FindAction("Parry");
        map.Enable();
    }

    void Update()
    {
        CheckGrounded();

        if (_isDashing)
        {
            UpdateDash();
            return;
        }

        if (_parryWindowRemaining > 0f)
            _parryWindowRemaining -= Time.deltaTime;

        if (_parryCooldownRemaining > 0f)
            _parryCooldownRemaining -= Time.deltaTime;

        if (_dashCooldownRemaining > 0f)
            _dashCooldownRemaining -= Time.deltaTime;

        HandleParry();
        HandleDash();
        HandleMove();
        HandleJump();
        ApplyGravity();
        _controller.Move(_velocity * Time.deltaTime);
    }

    void CheckGrounded()
    {
        // Esfera justo debajo del pie del capsule para no colisionar con nosotros mismos
        Vector3 foot = transform.position + _controller.center + Vector3.down * (_controller.height * 0.5f + groundCheckRadius);
        _isGrounded = Physics.CheckSphere(foot, groundCheckRadius, groundMask);

        if (_isGrounded)
        {
            _jumpConsumed = false;
            if (_velocity.y <= 0f)
                _velocity.y = -2f;
        }
    }

    void HandleMove()
    {
        Vector2 moveInput = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        bool sprint = _sprintAction?.IsPressed() ?? false;
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);

        Vector3 direction = Vector3.zero;
        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            direction = (forward * moveInput.y + right * moveInput.x).normalized;
        }
        else
        {
            direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        }

        if (direction.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationVelocityY, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            _velocity.x = direction.x * speed;
            _velocity.z = direction.z * speed;
        }
        else
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
    }

    void HandleJump()
    {
        if (!(_jumpAction?.WasPressedThisFrame() ?? false)) return;
        if (!_isGrounded || _jumpConsumed) return;
        _velocity.y = jumpForce;
        _jumpConsumed = true;
    }

    void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
    }

    void HandleDash()
    {
        if (_dashCooldownRemaining > 0f || _isDashing) return;
        if (!(_dashAction?.WasPressedThisFrame() ?? false)) return;

        _dashCooldownRemaining = dashCooldown;
        _dashTimer = dashDuration;
        _isDashing = true;

        Vector3 dir = transform.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        dir.Normalize();
        _velocity = dir * dashSpeed;
        _velocity.y = 0f;
    }

    void UpdateDash()
    {
        _dashTimer -= Time.deltaTime;
        if (_dashTimer <= 0f)
        {
            _isDashing = false;
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
        else
            _controller.Move(_velocity * Time.deltaTime);
    }

    void HandleParry()
    {
        if (!(_parryAction?.WasPressedThisFrame() ?? false)) return;
        if (_parryCooldownRemaining > 0f || _parryWindowRemaining > 0f) return;

        _parryCooldownRemaining = parryCooldown;
        _parryWindowRemaining = parryWindowDuration;
        _isParrying = true;
    }

    /// <summary>
    /// Llamar cuando un ataque es bloqueado durante la ventana de parry.
    /// Ejemplo: desde un script de daño que detecte IsParryWindowActive.
    /// </summary>
    public void NotifyParrySuccess()
    {
        if (IsParryWindowActive)
        {
            _parryWindowRemaining = 0f;
            _isParrying = false;
            OnParrySuccess?.Invoke();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_controller == null) _controller = GetComponent<CharacterController>();
        if (_controller == null) return;
        Vector3 foot = transform.position + _controller.center + Vector3.down * (_controller.height * 0.5f + groundCheckRadius);
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(foot, groundCheckRadius);
    }
}
