using UnityEngine;

namespace Server.client
{
    public class Player : MonoBehaviour
    {
        public uint tick;
        public int id;
        public string username;
        public InputManager inputManager;

        [Header("Constants")] [SerializeField] private CharacterController controller;
        [SerializeField] public float speed = 15f;
        [SerializeField] public float walkSpeed = 15f;
        [SerializeField] private float gravity = 9.8f;
        [SerializeField] private float terminalVelocity = 9.8f;

        [Header("References")] [SerializeField]
        private LayerMask groundMask;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform ceilingCheck;

        [Header("Movement Settings")] [SerializeField]
        private float jumpHeight;

        [SerializeField] private float jumpTime;
        public float playerGravity;
        public Vector3 jumpingForce;
        public Vector3 jumpingVelocity;

        [Header("Checks")] public bool isGrounded;
        private Vector2 _horizontalMovementInput;
        private Vector3 _initialPlayerScale;

        [Header("InitialValues")] private float _initialSpeed;
        private bool _isCrouching;
        private bool _isHeadBlocked;
        private bool _isWalking;
        private Vector3 _movementVelocity;

        public void Awake()
        {
            inputManager = GetComponent<InputManager>();
            inputManager.HorizontalMovement += ctx => _horizontalMovementInput = ctx;
            inputManager.Jump += _ => Jump();
            inputManager.Walk += ctx => _isWalking = ctx;
            inputManager.Crouch += ctx => _isCrouching = ctx;
            inputManager.Tick += ctx => tick = ctx;
            var transform1 = transform;
            _initialPlayerScale = transform1.localScale;
            _initialSpeed = speed;
            transform1.position = new Vector3(0f, 2f, 0f);
        }

        public void Update()
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.55f, groundMask);
            _isHeadBlocked = Physics.CheckSphere(ceilingCheck.position, 0.55f, groundMask);
            CalculateGroundMovement();
            CalculateJump();
            Walk();
            Crouch();
        }

        public void FixedUpdate()
        {
            tick++;
            ServerSend.PlayerState(id, this);
        }

        public void Initialize(int id, string username)
        {
            this.id = id;
            this.username = username;
        }

        private void CalculateGroundMovement()
        {
            if (playerGravity > terminalVelocity && jumpingVelocity.y < 0.1f) playerGravity -= gravity * Time.deltaTime;
            if (playerGravity < 0.01f && isGrounded)
            {
                var horizontalMovement = _horizontalMovementInput.x * speed;
                var verticalMovement = _horizontalMovementInput.y * speed;
                _movementVelocity = new Vector3(horizontalMovement, 0f, verticalMovement);
                _movementVelocity = transform.TransformDirection(_movementVelocity);
                playerGravity = -0.01f;
            }
            else
            {
                var horizontalMovement = _horizontalMovementInput.x * 8f * Time.deltaTime;
                var verticalMovement = _horizontalMovementInput.y * 8f * Time.deltaTime;
                var airVelocity = new Vector3(horizontalMovement, 0f, verticalMovement);
                airVelocity = transform.TransformDirection(airVelocity);
                if (airVelocity.magnitude < 0.1f)
                    _movementVelocity += airVelocity;
            }

            _movementVelocity.y = playerGravity;
            _movementVelocity += jumpingVelocity;
            controller.Move(_movementVelocity * Time.deltaTime);
        }

        private void CalculateJump()
        {
            jumpingVelocity = Vector3.SmoothDamp(jumpingVelocity, Vector3.zero, ref jumpingForce, jumpTime);
        }

        private void Jump()
        {
            if (isGrounded)
            {
                jumpingVelocity = Vector3.up * jumpHeight;
                playerGravity = 0f;
            }
        }

        private void Walk()
        {
            if (_isWalking)
            {
                speed = walkSpeed;
                return;
            }

            speed = _initialSpeed;
        }

        private void Crouch()
        {
            if (_isCrouching)
            {
                var crouchedHeight = _initialPlayerScale;
                crouchedHeight.y /= 2f;
                transform.localScale = crouchedHeight;
                return;
            }

            transform.localScale = _initialPlayerScale;
        }
    }
}