using UnityEngine;
using UnityEngine.SceneManagement;

namespace Goji.Legacy
{
	public class GameManagerLegacy : MonoBehaviour
	{
		[SerializeField]
		SnakeControllerLegacy snake;

		[SerializeField]
		GameObject gameOverText;

		void Start()
		{
			gameOverText.SetActive(false);
		}

		void Update()
		{
			if (snake.IsDead)
				gameOverText.SetActive(true);

			if (Input.GetKeyDown(KeyCode.Escape))
				SceneManager.LoadScene("StartScreenScene");

			// Reload the scene if the player presses R
			//if (snake.IsDead && Input.GetKeyDown(KeyCode.R))
			//	SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}