using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SnakeScene");
    }

    public void StartLegacyGame()
    {
        SceneManager.LoadScene("LegacySnakeScene");
    }

    public void StartCredits() 
    {
        SceneManager.LoadScene("CreditsScene");
    }

	public void StartChangelog()
	{
		SceneManager.LoadScene("ChangelogScene");
	}

	public void QuitGame() 
    {
        Application.Quit();
    }
}
