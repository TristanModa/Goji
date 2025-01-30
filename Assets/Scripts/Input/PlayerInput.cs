using UnityEngine;
using UnityEngine.InputSystem;

namespace Goji.Input
{
	public class PlayerInput
	{
		#region Properties
		private InputMap InputMap { get; set; }

		public float MovementDirection => InputMap.Player.Move.ReadValue<float>();
		public ButtonState Jump { get; private set; }
		#endregion


		#region Constructors
		public PlayerInput(InputMap inputMap)
		{
			this.InputMap = inputMap;

			Jump = new ButtonState();
		}

		public void UpdateButtonStates()
		{
			Jump.UpdateState(InputMap.Player.Jump.IsPressed());
		}
		#endregion
	}
}