using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SnakeScene");
    }

    public void StarLegacyGame()
    {
        SceneManager.LoadScene("LegacySnakeScene");
    }

    public void StartCredits() 
    {
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame() 
    {
        Application.Quit();
    }
}
