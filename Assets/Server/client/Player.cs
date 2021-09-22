using System.Buffers;
using UnityEngine;

namespace Server.client
{
    public class Player : MonoBehaviour
    {
        public int id;
        public string username;
        public byte[] controls;
        public InputManager inputManager;

        [Header("Constants")]
        [SerializeField] private CharacterController controller;
        [SerializeField] public float speed = 15f;
        [SerializeField] public float walkSpeed = 15f;
        [SerializeField] private float gravity = 9.8f;
        [SerializeField] private float terminalVelocity = 9.8f;

        [Header("References")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform ceilingCheck;

        [Header("Movement Settings")]
        [SerializeField] private float jumpHeight;
        [SerializeField] private float jumpTime;
        private Vector2 horizontalMovementInput;
        private Vector3 movementVelocity;
        public float playerGravity;
        public Vector3 jumpingForce;
        public Vector3 jumpingVelocity;

        [Header("Checks")]
        public bool isGrounded;
        private bool isCrouching;
        private bool isWalking;
        private bool isHeadBlocked;

        [Header("InitialValues")]
        private float initialSpeed;
        private Vector3 initialPlayerScale;

        public void Initialize(int id, string username)
        {
            this.id = id;
            this.username = username;
        }

        public void Awake()
        {
            inputManager = new InputManager();
            inputManager.HorizontalMovement += ctx =>  horizontalMovementInput = ctx;
            inputManager.YRotation += ctx => { var currentRotation = transform.localRotation; currentRotation.y = ctx; transform.localRotation = currentRotation; };
            inputManager.Jump += _ => Jump();
            inputManager.Walk += ctx => isWalking = ctx;
            inputManager.Crouch += ctx => isCrouching = ctx;
            initialPlayerScale = transform.localScale;
            initialSpeed = speed;
            transform.position = new Vector3(0f, 2f, 0f);
        }

        public void Update()
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.55f, groundMask);
            isHeadBlocked = Physics.CheckSphere(ceilingCheck.position, 0.55f, groundMask);
            CalculateGroundMovement();
            CalculateJump();
            Walk();
            Crouch();
        }

        public void FixedUpdate()
        {
            ServerSend.PlayerState(id, this);
        }

        private void CalculateGroundMovement()
        {
            if (playerGravity > terminalVelocity && jumpingVelocity.y < 0.1f)
            {
                playerGravity -= gravity * Time.deltaTime;
            }
            if (playerGravity < 0.01f && isGrounded)
            {
                var horizontalMovement = horizontalMovementInput.x * speed;
                var verticalMovement = horizontalMovementInput.y * speed;
                movementVelocity = new Vector3(horizontalMovement, 0f, verticalMovement);
                movementVelocity = transform.TransformDirection(movementVelocity);
                playerGravity = -0.01f;
            }
            else
            {
                var horizontalMovement = horizontalMovementInput.x * 8f * Time.deltaTime;
                var verticalMovement = horizontalMovementInput.y * 8f * Time.deltaTime;
                var airVelocity = new Vector3(horizontalMovement, 0f, verticalMovement);
                airVelocity = transform.TransformDirection(airVelocity);
                if (airVelocity.magnitude < 0.1f)
                    movementVelocity += airVelocity;
            }
            movementVelocity.y = playerGravity;
            movementVelocity += jumpingVelocity;
            controller.Move(movementVelocity * Time.deltaTime);
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

        public void Walk()
        {
            if (isWalking)
            {
                speed = walkSpeed;
                return;
            }
            else
            {
                speed = initialSpeed;
            }
        }

        public void Crouch()
        {
            if (isCrouching)
            {
                Vector3 crouchedHeight = initialPlayerScale;
                crouchedHeight.y /= 2f;
                transform.localScale = crouchedHeight;
                return;
            }
            else
            {
                transform.localScale = initialPlayerScale;
            }
        }
    }
}
