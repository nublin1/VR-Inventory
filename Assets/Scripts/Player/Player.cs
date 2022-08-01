using UnityEngine.Serialization;
using NaughtyAttributes;
using UnityEngine;
using Input;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Player")]   
    [Tooltip("The object that represents the forward direction movement, usually should be set as the camera or a tracked controller")]
    public Transform forwardFollow;
    [Tooltip("The tracked headCamera object")]
    public Camera headCamera;
    public Transform trackingContainer;
    [Header("Hands")]
    [SerializeField] Hand l_hand;
    [SerializeField] Hand r_hand;

    // Movemant
    [Header("Movement")]
    public bool enableMovement = true;
    [ShowIf("enableMovement"), FormerlySerializedAs("moveSpeed")]
    [Tooltip("Movement speed when isGrounded")]
    public float maxMoveSpeed = 1.5f;
    [ShowIf("enableMovement")]
    [Tooltip("Movement acceleration when isGrounded")]
    public float moveAcceleration = 10f;
    [ShowIf("enableMovement")]
    public float heightSmoothSpeed = 20f;
    [ShowIf("enableMovement")]
    [SerializeField] LayerMask groundMask;
    [ShowIf("enableMovement")]
    [Header("MoveInput")]
    [SerializeField] InputActionReference moveAction;

    // Turn
    [Header("Snap Turning")]
    [Tooltip("Whether or not to use snap turning or smooth turning"), Min(0)]
    public bool snapTurning = true;
    [Tooltip("turn speed when not using snap turning - if snap turning, represents angle per snap")]
    [ShowIf("snapTurning")]
    public float snapTurnAngle = 30f;
    [HideIf("snapTurning")]
    public float smoothTurnSpeed = 10f;
    public float turnDeadzone = 0.4f;
    [Header("TurnInput")]
    [SerializeField] InputActionReference turnAction;


    Rigidbody rigidBody;
    CapsuleCollider bodyCapsule;

    bool isGrounded = true;
    bool turnReset = true;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        bodyCapsule = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        if (enableMovement)
        {
            UpdateTurn(Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (enableMovement)
        {
            CheckGround();
            UpdateRigidbody();
        }
    }

    protected virtual void CheckGround()
    {
        isGrounded = Physics.CheckSphere(gameObject.transform.position, bodyCapsule.radius, groundMask);
    }

    protected virtual void UpdateRigidbody()
    {
        var move = CalculateMovementDirection();
        var yVel = rigidBody.velocity.y;

        // Moves velocity towards
        if (move != Vector3.zero && isGrounded == true)
            rigidBody.velocity = Vector3.MoveTowards(rigidBody.velocity, move * maxMoveSpeed, moveAcceleration * Time.fixedDeltaTime);

        // Keep velocity if consistent when moving while falling
        if (rigidBody.useGravity)
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, yVel, rigidBody.velocity.z);

    }

    Vector3 CalculateMovementDirection()
    {
        Vector2 moveVector = moveAction.action.ReadValue<Vector2>();
        Vector3 moveDirection = Vector3.zero;
        moveDirection += Vector3.ProjectOnPlane(forwardFollow.transform.right, transform.up).normalized * moveVector.x;
        moveDirection += Vector3.ProjectOnPlane(forwardFollow.transform.forward, transform.up).normalized * moveVector.y;

        //If necessary, clamp movement vector to magnitude of 1f;
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        return moveDirection;
    }

    protected virtual void UpdateTurn(float deltaTime)
    {
        Vector2 turnVector = turnAction.action.ReadValue<Vector2>();

        if (snapTurning)
        {
            if ((Mathf.Abs(turnVector.x) > turnDeadzone || Mathf.Abs(turnVector.y) > turnDeadzone) && turnReset)
            {
                var angle = 0.0f;
                if (turnVector.x > turnDeadzone)
                    angle = snapTurnAngle;
                if (turnVector.x < -turnDeadzone)
                    angle = -snapTurnAngle;
                if (turnVector.y < -turnDeadzone)
                    angle = 180.0f;

                var targetPos = transform.position - headCamera.transform.position; targetPos.y = 0;
                trackingContainer.position += targetPos;
                trackingContainer.RotateAround(transform.position, Vector3.up, angle);

                turnReset = false;
            }
        }
        else
        {
            var turn_angle = 0f;
            if (turnVector.x > turnDeadzone || turnVector.x < -turnDeadzone)
                turn_angle = smoothTurnSpeed * turnVector.x;

            var target_Rot = trackingContainer.rotation * Quaternion.Euler(0, turn_angle, 0);
            trackingContainer.rotation = Quaternion.Lerp(trackingContainer.rotation, target_Rot, deltaTime);

        }

        if (turnVector.x == 0 && turnVector.y == 0)
            turnReset = true;
    }
}
