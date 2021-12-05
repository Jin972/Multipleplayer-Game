using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    [Header("Player")]
    [SerializeField]
    float moveSpeed = 6f;
    float speed;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    Vector3 playerVelocity;
    private float _rotationVelocity;
    private float _targetRotation = 0.0f;
    float SpeedChangeRate = 10f;

    [Header("Ground")]
    [SerializeField]
    bool isGrounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;
    [SerializeField]
    float height = 1f; //the height that character can jump
    [SerializeField]
    float gravityAcceleration = -15f;
    float _verticalVelocity;
    bool isJump = true;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField]
    Transform CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField]
    float TopClamp = 70f;
    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField]
    float BottomClamp = -30f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();
        Move();
        JumpAndGravity();        
    }

    private void LateUpdate()
    {
        CameraRotation();
        
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);  
    }

    void CameraRotation()
    {
        _cinemachineTargetYaw += Input.GetAxis("Mouse X")*200*Time.deltaTime;
        _cinemachineTargetPitch += Input.GetAxis("Mouse Y")*200*Time.deltaTime;

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, -360f, 360f);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void Move()
    {        
        float targetSpeed = moveSpeed;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        playerVelocity = new Vector3(horizontal, 0.0f, vertical).normalized;

        if (playerVelocity.magnitude == 0) targetSpeed = 0;
        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * playerVelocity.magnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        if (playerVelocity.magnitude >= 0.1f)
        {
            _targetRotation = Mathf.Atan2(playerVelocity.x, playerVelocity.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);    
        }
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (isGrounded)
        {
            isJump = true;

            if (_verticalVelocity < 0f)
                _verticalVelocity = 0f;

            if (Input.GetButtonDown("Jump") && isJump)
            {
                _verticalVelocity = Mathf.Sqrt(height * -3 * gravityAcceleration);
            }
            _verticalVelocity += gravityAcceleration * Time.deltaTime; 
        }
        else
        {
            isJump = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y, transform.position.z), GroundedRadius);
    }
}
