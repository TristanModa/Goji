using UnityEngine;

namespace Goji.Input
{
	public class ButtonState
	{
		#region Fields
		public bool Pressed { get; private set; }
		public bool Held { get; private set; }
		public bool Released { get; private set; }
		#endregion

		#region Methods
		public void UpdateState(bool buttonDown)
		{
			Pressed = false;
			Released = false;
			
			// Check if the button was pressed this frame
			if (buttonDown && !Held)
			{
				Pressed = true;
				Held = true;
			}

			// Check if the button was released this frame
			if (!buttonDown && Held)
			{
				Released = true;
				Held = false;
			}
		}

		public override string ToString()
		{
			return $"Input (Pressed: {Pressed}, Held: {Held}, Released {Released})";
		}
		#endregion
	}
}