using Goji.Input;
using UnityEngine;

namespace Goji.Gameplay 
{
	public class PlayerController : MonoBehaviour
	{
		#region Properties
		private PlayerInput PlayerInput { get; set; }
		private Rigidbody2D Rigidbody2D { get; set; }

		private Vector2 Velocity { get; set; }

		private bool IsGrounded { get; set; }
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
			UpdatePlayerStates();
			UpdatePlayerVelocity();
		}

		private void UpdatePlayerStates()
		{
			IsGrounded = Physics2D.OverlapBox(
				transform.position + (Vector3)groundCheckOffset,
				groundCheckSize,
				0, groundLayer);
		}

		private void UpdatePlayerVelocity()
		{
			// Get the rigidbody's velocity after the previous physics step
			Velocity = Rigidbody2D.linearVelocity;

			// Apply player movement
			float desiredMovementDirection = PlayerInput.MovementDirection;
			Velocity = new Vector2(desiredMovementDirection * maxMovementSpeed, Velocity.y);

			// Apply gravity
			Velocity += gravityScale * Time.fixedDeltaTime * Vector2.down;

			// Set the rigidbody's velocity for this physics step
			Rigidbody2D.linearVelocity = Velocity;
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