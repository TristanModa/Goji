using Goji.Input;
using System;
using UnityEngine;

namespace Goji.Gameplay 
{
	public class PlayerController : MonoBehaviour
	{
		#region Properties
		private PlayerInput PlayerInput { get; set; }
		private Rigidbody2D Rigidbody2D { get; set; }

		private Vector2 Velocity 
		{
			get => Rigidbody2D.linearVelocity;
			set
			{
				Rigidbody2D.linearVelocity = value;
			}
		}

		private float AccelerationPerFrame => maxMovementSpeed / accelerationFrames;
		private float DecelerationPerFrame => maxMovementSpeed / decelerationFrames;

		private bool IsGrounded { get; set; }
		private bool IsJumping { get; set; }
		#endregion

		#region Fields
		[Header("Physics")]
		[SerializeField]
		private float gravityScale;
		[SerializeField]
		private float maxFallSpeed;

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
		#endregion

		#region Methods
		private void Awake()
		{
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
			PlayerInput.UpdateButtonStates();
			UpdatePlayerStates();
			UpdatePlayerVelocity();
		}

		private void UpdatePlayerStates()
		{
			// Perform the ground check
			IsGrounded = Physics2D.OverlapBox(
				transform.position + (Vector3)groundCheckOffset,
				groundCheckSize,
				0, groundLayer);

			if (IsJumping && Velocity.y < 0)
			{
				IsJumping = false;
			}
		}

		private void UpdatePlayerVelocity()
		{
			// Calculate the horizontal and vertical components of the velocity
			float horizontalVelocity = CalculateHorizontalVelocity();
			float verticalVelocity = CalculateVerticalVelocity();

			// Set the player's velocity for this frame
			Velocity = new Vector2(horizontalVelocity, verticalVelocity);
		}

		private float CalculateHorizontalVelocity()
		{
			// Calculate the player's target velocity
			float targetVelocity = PlayerInput.MovementDirection * maxMovementSpeed;

			// Calculate the player's speed change for this frame
			float speedChange = 0;
			if (targetVelocity != 0 && Mathf.Sign(targetVelocity) == Mathf.Sign(Velocity.x))
				speedChange = AccelerationPerFrame;
			else
				speedChange = DecelerationPerFrame;

			// Move the current velocity towards the target velocity
			float horizontalVelocity = Mathf.MoveTowards(Velocity.x, targetVelocity, speedChange);

			// Return the result
			return horizontalVelocity;
		}

		private float CalculateVerticalVelocity()
		{
			// Get the current vertical velocity
			float verticalVelocity = Velocity.y;


			
			// Apply a jump force if the player jumped
			if (PlayerInput.Jump.Pressed && IsGrounded && !IsJumping)
			{
				IsJumping = true;
				verticalVelocity = Mathf.Sqrt(2 * gravityScale * jumpHeight);
			}
			
			// Apply gravity
			verticalVelocity -= (gravityScale * Time.fixedDeltaTime);

			Debug.Log(transform.position.y);

			// Return the result
			return verticalVelocity;
		}

		private void OnDrawGizmos()
		{
			// Draw ground check
			Gizmos.color = IsGrounded ? Color.green : Color.white;
			Gizmos.DrawCube(transform.position + (Vector3)groundCheckOffset, groundCheckSize);
		}
		#endregion
	}
}