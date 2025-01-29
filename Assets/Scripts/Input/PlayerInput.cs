using UnityEngine;

namespace Goji.Input
{
	public class PlayerInput
	{
		#region Properties
		private InputMap InputMap { get; set; }

		public float MovementDirection => InputMap.Player.Move.ReadValue<float>();
		#endregion

		#region Constructors
		public PlayerInput(InputMap inputMap)
		{
			this.InputMap = inputMap;
		}
		#endregion
	}
}