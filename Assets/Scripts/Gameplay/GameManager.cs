using Goji.Gameplay.Player;
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
				{
					Destroy(value);
					return;
				}

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
		PlayerController player;

		[SerializeField]
		GameObject winText;

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
			winText.SetActive(false);
			gameOverText.SetActive(false);
		}

		void Update()
		{
			if (player.IsDead)
				gameOverText.SetActive(true);

			if (snake.IsDead)
				winText.SetActive(true);

			// Reload the scene if the player presses R
			if ((snake.IsDead || player.IsDead) && UnityEngine.Input.GetKeyDown(KeyCode.R))
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);

			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
				SceneManager.LoadScene("StartScreenScene");
		}
		#endregion
	}
}