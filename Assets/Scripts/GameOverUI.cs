using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Referencias Internas")]
    public Image overlayImage;
    public TextMeshProUGUI titleText;

    public void ShowGameOver(bool victory)
    {
        gameObject.SetActive(true);

        if (overlayImage != null)
        {
            overlayImage.color = victory 
                ? new Color(0f, 0.6f, 0f, 0.85f)
                : new Color(0.6f, 0f, 0f, 0.85f);
        }

        if (titleText != null)
        {
            titleText.text = victory ? "¡VICTORIA!" : "DERROTA...";
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("LevelSelector");
    }
}