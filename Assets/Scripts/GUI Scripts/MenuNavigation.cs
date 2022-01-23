using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour {

    private void Start()
    {
		Screen.SetResolution(1280, 720, false);
    }

    public void MainMenu()
	{
		//Application.LoadLevel("menu");
		SceneManager.LoadScene("menu");
	}

	public void Quit()
	{
		Application.Quit();
	}
	
	public void Play()
	{
		//Application.LoadLevel("game");
		SceneManager.LoadScene("game");
	}
	
	public void HighScores()
	{
		//Application.LoadLevel("scores");
		SceneManager.LoadScene("scores");

	}

    public void Credits()
    {
        //Application.LoadLevel("credits");
		SceneManager.LoadScene("credits");
	}

	public void SourceCode()
	{
		Application.OpenURL("https://github.com/vilbeyli/Pacman-Clone/");
	}
}
