using UnityEngine;

namespace Goji.Input
{
	public class InputHandler : MonoBehaviour
	{
		#region Properties
		public static InputHandler Instance 
		{ 
			get => _instance;
			private set 
			{
				if (_instance != null)
					return;

				_instance = value;
			}
		}

		public InputMap InputMap { get; private set; }
		public PlayerInput PlayerInput { get; private set; }
		#endregion

		#region Fields
		private static InputHandler _instance;
		#endregion

		#region Methods
		private void Awake()
		{
			Instance = this;
			InputMap = new InputMap();
			PlayerInput = new PlayerInput(InputMap);
		}

		private void OnEnable()
		{
			InputMap.Enable();
		}

		private void OnDisable()
		{
			InputMap.Disable();
		}
		#endregion
	}
}