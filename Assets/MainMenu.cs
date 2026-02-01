using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("LevelSelector");
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

    public void LoadLvl1()
    {
        SceneManager.LoadScene("Lvl1");
    }

    public void LoadLvl2()
    {
        SceneManager.LoadScene("Lvl2");
    }

    public void LoadLvl3()
    {
        SceneManager.LoadScene("Lvl3");
    }

    public void LoadLvl4()
    {
        SceneManager.LoadScene("Lvl4");
    }

    public void LoadLvl5()
    {
        SceneManager.LoadScene("Lvl5");
    }

    public void LoadLvl6()
    {
        SceneManager.LoadScene("Lvl6");
    }

    public void LoadLvl7()
    {
        SceneManager.LoadScene("Lvl7");
    }

    public void LoadLvl8()
    {
        SceneManager.LoadScene("Lvl8");
    }

    public void LoadLvl9()
    {
        SceneManager.LoadScene("Lvl9");
    }

    public void LoadLvl10()
    {
        SceneManager.LoadScene("Lvl10");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}