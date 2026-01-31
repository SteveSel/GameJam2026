using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("modeSelection");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void LevelSelector()
    {
        SceneManager.LoadScene("LevelSelector");
    }

    public void Custom()
    {
        SceneManager.LoadScene("Custom1");
    }

    public void MainPlay()
    {
        SceneManager.LoadScene("NightTimeScene");
    }

    public void Load() 
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}