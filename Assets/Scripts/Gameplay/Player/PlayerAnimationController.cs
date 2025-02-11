using System;
using UnityEngine;

namespace Goji.Gameplay.Player
{
	public class PlayerAnimationController : MonoBehaviour
	{
		#region Properties
		private PlayerController Player { get; set; }
		private Animator Animator { get; set; }
		#endregion

		#region Fields
		private bool _groundedThisFrame;
		private bool _groundedLastFrame;
		#endregion

		#region Methods
		private void Awake()
		{
			// Get components
			Player = transform.parent.GetComponent<PlayerController>();
			Animator = GetComponent<Animator>();
		}

		private void Start()
		{
			// Subscribe to player event handler
			Player.OnPlayerJump += OnPlayerJump;
			Player.OnPlayerHitGround += OnPlayerHitGround;
		}

		private void OnPlayerJump()
		{
			Animator.SetTrigger("Jump");
		}

		private void OnPlayerHitGround()
		{
			Animator.SetTrigger("HitGround");
		}
		#endregion
	}
}