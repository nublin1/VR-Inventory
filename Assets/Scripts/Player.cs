using UnityEngine.Serialization;
using NaughtyAttributes;
using UnityEngine;
using CustomAttributes;
using Input;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{

    [CustomHeader("Player")]
    public bool ignoreMe;

    [Tooltip("The object that represents the forward direction movement, usually should be set as the camera or a tracked controller")]
    public Transform forwardFollow;
    [Tooltip("The tracked headCamera object")]
    public Camera headCamera;
    public Transform trackingContainer;
    [Header("Hands")]
    [SerializeField] Hand l_hand;
    [SerializeField] Hand r_hand;    

    // Movemant
    [ToggleHeader("Movement")]
    public bool useMovement = true;
    [EnableIf("useMovement"), FormerlySerializedAs("moveSpeed")]
    [Tooltip("Movement speed when isGrounded")]
    public float maxMoveSpeed = 1.5f;
    [EnableIf("useMovement")]
    [Tooltip("Movement acceleration when isGrounded")]
    public float moveAcceleration = 10f;
    [EnableIf("useMovement")]
    [Tooltip("Movement acceleration when isGrounded")]
    public float groundedDrag = 4f;
    [EnableIf("useMovement")]
    public float heightSmoothSpeed = 20f;
    [EnableIf("useMovement")]
    [SerializeField] LayerMask groundMask;
    [Header("MoveInput")]
    [SerializeField] InputActionReference moveAction;
    

    // Turn
    [ToggleHeader("Snap Turning")]
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

    Vector3 lastUpdatePosition;
    float lastUpdateTime;    

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        bodyCapsule = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        lastUpdatePosition = transform.position;       
    }

    void Update()
    {
        if (useMovement)
        {
            UpdateTurn(Time.deltaTime);
        }        
    }

    void FixedUpdate()
    {
        if (useMovement)
        {
            CheckGround();
            UpdateRigidbody();
            //UpdateTurn(Time.fixedDeltaTime);
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

        // Moves velocity towards desired movement direction
        if (move != Vector3.zero && isGrounded == true)
            rigidBody.velocity = Vector3.MoveTowards(rigidBody.velocity, move * maxMoveSpeed, moveAcceleration * Time.fixedDeltaTime);

        // This will keep velocity if consistent when moving while falling
        if (rigidBody.useGravity)
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, yVel, rigidBody.velocity.z);

        //
        lastUpdateTime = Time.realtimeSinceStartup;
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

                lastUpdatePosition = transform.position;
                lastUpdateTime = Time.realtimeSinceStartup;

                turnReset = false;
            }
        }
        else
        {
            //if (turnReset)
            {
                var turn_angle = 0f;
                if (turnVector.x > turnDeadzone || turnVector.x < -turnDeadzone)
                    turn_angle = smoothTurnSpeed * turnVector.x;                

                var target_Rot = trackingContainer.rotation * Quaternion.Euler(0, turn_angle, 0);
                trackingContainer.rotation = Quaternion.Lerp(trackingContainer.rotation, target_Rot, deltaTime);

                lastUpdatePosition = transform.position;
                lastUpdateTime = Time.realtimeSinceStartup;
            }
        }

        if (turnVector.x == 0 && turnVector.y == 0)
            turnReset = true;
    }
}
