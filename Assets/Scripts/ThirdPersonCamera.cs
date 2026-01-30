using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cámara en tercera persona que sigue al personaje y rota con Look (ratón / stick derecho).
/// Coloca este script en la cámara o en un objeto padre de la cámara.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Transform target;
    [SerializeField] InputActionAsset inputActions;

    [Header("Posición")]
    [SerializeField] float distance = 5f;
    [SerializeField] float heightOffset = 1.5f;
    [SerializeField] float positionSmoothTime = 0.1f;

    [Header("Rotación")]
    [SerializeField] float sensitivityX = 2f;
    [SerializeField] float sensitivityY = 1.2f;
    [SerializeField] float minPitch = -25f;
    [SerializeField] float maxPitch = 60f;
    [SerializeField] float rotationSmoothTime = 0.05f;

    InputAction _lookAction;
    float _currentYaw;
    float _currentPitch;
    float _yawVelocity;
    float _pitchVelocity;
    Vector3 _positionVelocity;

    void Awake()
    {
        BindInput();
        if (target != null)
        {
            Vector3 toCam = transform.position - (target.position + Vector3.up * heightOffset);
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.01f)
            {
                _currentYaw = Mathf.Atan2(toCam.x, toCam.z) * Mathf.Rad2Deg;
                _currentPitch = Mathf.Asin(Mathf.Clamp(transform.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
            }
        }
    }

    void BindInput()
    {
        if (inputActions == null) return;
        var map = inputActions.FindActionMap("Player");
        if (map == null) return;
        _lookAction = map.FindAction("Look");
        map.Enable();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 look = _lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        _currentYaw += look.x * sensitivityX;
        _currentPitch -= look.y * sensitivityY;
        _currentPitch = Mathf.Clamp(_currentPitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Vector3 targetPosition = target.position + Vector3.up * heightOffset - rotation * Vector3.forward * distance;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _positionVelocity, positionSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f - Mathf.Exp(-20f * Time.deltaTime / Mathf.Max(0.001f, rotationSmoothTime)));
    }
}
