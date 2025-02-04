using Goji.Input;
using System;
using UnityEngine;

namespace Goji.Gameplay 
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
		private bool IsGrounded 
		{
			get 
			{
				return Physics2D.OverlapBox(
					transform.position + (Vector3)groundCheckOffset,
					groundCheckSize,
					0, groundLayer);
			}
		}
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
		[SerializeField]
		private Vector2 groundCheckOffset;
		[SerializeField]
		private Vector2 groundCheckSize;

		[Header("Player Assist")]
		[SerializeField]
		private int jumpBufferFrames;
		[SerializeField]
		private int coyoteTimeFrames;
		#endregion

		#region Fields
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

			UpdateTimers();
			UpdateVelocity();	
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
				verticalVelocity = Mathf.Sqrt(2 * CurrentGravityScale * jumpHeight);
			}
			
			// Return the result
			return verticalVelocity;
		}

		private void OnDrawGizmos()
		{
			#region Ground Check
			Gizmos.color = IsGrounded ? Color.green : Color.white;
			Gizmos.DrawCube(transform.position + (Vector3)groundCheckOffset, groundCheckSize);
			#endregion
		}
		#endregion
	}
}