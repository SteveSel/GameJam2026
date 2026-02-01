using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuideEntry : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI roleNameText;
    public TextMeshProUGUI descriptionText;
    public Image backgroundPanel;

    public void Setup(RoleVisualData data)
    {
        if (iconImage != null) iconImage.sprite = data.artwork;
        if (roleNameText != null) roleNameText.text = data.role.ToString();
        if (descriptionText != null) descriptionText.text = data.description;

        if (backgroundPanel != null)
        {
            string r = data.role.ToString();
            if (GameManager.Instance.IsDemon(data.role))
                backgroundPanel.color = new Color(0.8f, 0.3f, 0.3f, 0.5f); // Rojo suave
            else if (r == "Jester" || r == "Doomsayer" || r == "Amnesiac" || r == "Lazy")
                backgroundPanel.color = new Color(0.6f, 0.3f, 0.8f, 0.5f); // Morado (Caos)
            else
                backgroundPanel.color = new Color(0.3f, 0.8f, 0.3f, 0.5f); // Verde (Villager)
        }
    }
}