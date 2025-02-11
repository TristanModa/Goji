using Goji.Input;
using System;
using UnityEngine;

namespace Goji.Gameplay.Player
{
	public class PlayerController : MonoBehaviour
	{
		#region Properties
		/// <summary>
		/// The source of input for the player
		/// </summary>
		private PlayerInput PlayerInput { get; set; }
		/// <summary>
		/// The player's rigidbody
		/// </summary>
		private Rigidbody2D Rigidbody2D { get; set; }

		/// <summary>
		/// The current velocity of the player
		/// </summary>
		private Vector2 Velocity 
		{
			get => Rigidbody2D.linearVelocity;
			set
			{
				Rigidbody2D.linearVelocity = value;
			}
		}

		/// <summary>
		/// The player's acceleration per frame
		/// </summary>
		private float AccelerationPerFrame => maxMovementSpeed / accelerationFrames;
		/// <summary>
		/// The player's deceleration per frame
		/// </summary>
		private float DecelerationPerFrame => maxMovementSpeed / decelerationFrames;

		/// <summary>
		/// The amount the player's Y velocity should be decreased by per frame
		/// </summary>
		private float CurrentGravityScale => IsJumping ? upwardGravityScale : downwardGravityScale;

		/// <summary>
		/// Whether the player is touching the ground this frame
		/// </summary>
		private bool IsGrounded { get; set; }

		/// <summary>
		/// Whether the player is currently approaching max speed
		/// </summary>
		private bool IsAccelerating 
		{ 
			get 
			{ 
				return 
					PlayerInput.MovementDirection != 0 && 
					Mathf.Sign(PlayerInput.MovementDirection) == Mathf.Sign(Velocity.x);
			}
		}
		/// <summary>
		/// Whether the player is able to jump
		/// </summary>
		private bool CanJump => (IsGrounded || CoyoteTimeActive) && !IsJumping;
		
		/// <summary>
		/// Whether the player is currently jumping
		/// </summary>
		private bool IsJumping { get; set; }

		/// <summary>
		/// The number of physics steps that have elapsed since the player started in the scene
		/// </summary>
		private int FrameCount { get; set; }

		/// <summary>
		/// Whether the player currently has a jump buffered
		/// </summary>
		private bool JumpBuffered => (_jumpBufferStartFrame + jumpBufferFrames >= FrameCount) && !IsJumping;
		/// <summary>
		/// Whether the player can jump while airborne
		/// </summary>
		private bool CoyoteTimeActive => _coyoteTimeStartFrame + coyoteTimeFrames >= FrameCount;
		#endregion

		#region Events
		public event Action OnPlayerHitGround;
		public event Action OnPlayerJump;
		#endregion

		#region Settings
		[Header("Physics")]
		[SerializeField]
		private float upwardGravityScale;
		[SerializeField]
		private float downwardGravityScale;

		[Header("Movement")]
		[SerializeField]
		private float maxMovementSpeed;
		[SerializeField]
		private int accelerationFrames;
		[SerializeField]
		private int decelerationFrames;

		[Space(20), SerializeField]
		private float jumpHeight;

		[Header("Collision")]
		[SerializeField]
		private LayerMask groundLayer;

		[Space(20), SerializeField]
		private Vector2 groundCheckOffset;
		[SerializeField]
		private Vector2 groundCheckSize;

		[Space(20), SerializeField]
		private float cornerNudgeCheckLength;
		[SerializeField]
		private float cornerNudgeCheckOffset;

		[Header("Player Assist")]
		[SerializeField]
		private int jumpBufferFrames;
		[SerializeField]
		private int coyoteTimeFrames;
		#endregion

		#region Fields
		private bool _wasGroundedLastFrame;

		private int _jumpBufferStartFrame;
		private int _coyoteTimeStartFrame;
		#endregion

		#region Methods
		private void Awake()
		{
			// Set initial timer values
			_jumpBufferStartFrame = int.MinValue;
			_coyoteTimeStartFrame = int.MinValue;

			// Get the player's rigidbody
			Rigidbody2D = GetComponent<Rigidbody2D>();
		}

		private void Start()
		{
			// Set the player input
			PlayerInput = InputHandler.Instance.PlayerInput;
		}

		private void FixedUpdate()
		{
			// Get player input for the frame
			PlayerInput.UpdateButtonStates();

			CheckCollision();

			UpdateTimers();
			UpdateVelocity();
			PerformCornerAdjustments();

			_wasGroundedLastFrame = IsGrounded;
		}

		/// <summary>
		/// Updates states related to collision
		/// </summary>
		private void CheckCollision()
		{
			// Check if the player is grounded
			IsGrounded = 
				Physics2D.OverlapBox(
					transform.position + (Vector3)groundCheckOffset,
					groundCheckSize,
					0, groundLayer);
			
			// Invoke the ground
			if (IsGrounded && !_wasGroundedLastFrame)
			{
				OnPlayerHitGround?.Invoke();
			}
		}

		/// <summary>
		/// Updates the player's jump buffer and coyote time
		/// </summary>
		private void UpdateTimers()
		{
			// Jump buffer
			if (PlayerInput.Jump.Pressed && !IsGrounded)
				_jumpBufferStartFrame = FrameCount;

			// Coyote time
			if (IsGrounded)
				_coyoteTimeStartFrame = FrameCount;

			// Increment the frame count for timers
			FrameCount++;
		}

		/// <summary>
		/// Updates the player's velocity for this frame
		/// </summary>
		private void UpdateVelocity()
		{
			// Calculate the horizontal and vertical components of the velocity
			float horizontalVelocity = CalculateHorizontalVelocity();
			float verticalVelocity = CalculateVerticalVelocity();

			// Set the player's velocity for this frame
			Velocity = new Vector2(horizontalVelocity, verticalVelocity);
		}

		/// <summary>
		/// Calculates the horizontal velocity of the player for this frame
		/// </summary>
		/// <returns>The calculated velocity</returns>
		private float CalculateHorizontalVelocity()
		{
			// Calculate the player's target velocity
			float targetVelocity = PlayerInput.MovementDirection * maxMovementSpeed;

			// Calculate the player's speed change for this frame
			float speedChange = IsAccelerating ? AccelerationPerFrame : DecelerationPerFrame;

			// Move the current velocity towards the target velocity
			float horizontalVelocity = Mathf.MoveTowards(Velocity.x, targetVelocity, speedChange);

			// Return the result
			return horizontalVelocity;
		}

		/// <summary>
		/// Calculates the vertical velocity of the player for this frame
		/// </summary>
		/// <returns>The calculated velocity</returns>
		private float CalculateVerticalVelocity()
		{
			// Get the current vertical velocity
			float verticalVelocity = Velocity.y;

			// End the player's jump if it should end
			if (IsJumping && (!PlayerInput.Jump.Held || Velocity.y <= 0))
				IsJumping = false;

			// Apply gravity
			verticalVelocity -= (CurrentGravityScale * Time.fixedDeltaTime);
			
			// Apply a jump force if the player jumped
			if ((PlayerInput.Jump.Pressed || JumpBuffered) && CanJump)
			{
				IsJumping = true;
				OnPlayerJump?.Invoke();
				verticalVelocity = Mathf.Sqrt(2 * CurrentGravityScale * jumpHeight);
			}
			
			// Return the result
			return verticalVelocity;
		}

		/// <summary>
		/// Adjusts the player's position if jumping into the corner of a platform
		/// </summary>
		private void PerformCornerAdjustments()
		{
			// Return if the player's velocity is zero
			if (Velocity.y <= 0)
				return;

			// Perform ceiling corner checks
			bool ceilingCenterCheck = 
				Physics2D.Raycast(
					transform.position, 
					Vector2.up, 
					cornerNudgeCheckLength, 
					groundLayer);
			bool ceilingLeftCheck = 
				Physics2D.Raycast(
					transform.position + Vector3.left * cornerNudgeCheckOffset, 
					Vector2.up, 
					cornerNudgeCheckLength, 
					groundLayer);
			bool ceilingRightCheck =
				Physics2D.Raycast(
					transform.position + Vector3.right * cornerNudgeCheckOffset,
					Vector2.up,
					cornerNudgeCheckLength,
					groundLayer);

			// Get the direction to adjust the player
			int direction = (ceilingLeftCheck) ? -1 : 1;

			// Check if a correction is required
			bool correctionRequired =
				!ceilingCenterCheck && (ceilingLeftCheck ^ ceilingRightCheck) &&
				Mathf.Sign(direction) != Mathf.Sign(Velocity.x);

			// Return if the correction is unneeded
			if (!correctionRequired)
				return;

			// Get the distance to adjust the player by
			float distanceFromCorner = Physics2D.Raycast(
				transform.position + cornerNudgeCheckLength * Vector3.up,
				Vector2.right * direction,
				cornerNudgeCheckOffset,
				groundLayer).distance;
			float distanceToMove = -(cornerNudgeCheckOffset - distanceFromCorner + 0.01f);

			// Update the player's position
			transform.position += distanceToMove * direction * Vector3.right;
		}

		private void OnDrawGizmos()
		{
			#region Ground Check
			Gizmos.color = IsGrounded ? Color.green : Color.white;
			Gizmos.DrawCube(transform.position + (Vector3)groundCheckOffset, groundCheckSize);
			#endregion

			#region Corner Nudge Check
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.position + Vector3.up * cornerNudgeCheckLength);
			Gizmos.DrawLine(
				transform.position + Vector3.right * cornerNudgeCheckOffset, 
				transform.position + Vector3.right * cornerNudgeCheckOffset + Vector3.up * cornerNudgeCheckLength);
			Gizmos.DrawLine(
				transform.position + Vector3.left * cornerNudgeCheckOffset,
				transform.position + Vector3.left * cornerNudgeCheckOffset + Vector3.up * cornerNudgeCheckLength);
			#endregion
		}
		#endregion
	}
}