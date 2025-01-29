using UnityEngine;
using UnityEngine.SceneManagement;

namespace Goji.Gameplay
{
	public class GameManager : MonoBehaviour
	{
		#region Properties
		public static GameManager Instance 
		{ 
			get => _instance; 
			private set
			{
				// Avoid setting the singleton if it is not null
				if (_instance != null)
					return;

				_instance = value;
			}
		}

		public RectInt MapBounds => new RectInt(Vector2Int.zero - mapSize / 2, mapSize);
		#endregion

		#region Fields
		private static GameManager _instance;

		[SerializeField]
		private Vector2Int mapSize;

		[SerializeField]
		SnakeController snake;

		[SerializeField]
		GameObject gameOverText;
		#endregion

		#region Methods
		private void Awake()
		{
			Instance = this;
		}

		void Start()
		{
			gameOverText.SetActive(false);
		}

		void Update()
		{
			if (snake.IsDead)
				gameOverText.SetActive(true);

			// Reload the scene if the player presses R
			//if (snake.IsDead && Input.GetKeyDown(KeyCode.R))
				//SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		#endregion
	}
}