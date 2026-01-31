using UnityEngine;
using UnityEngine.UI;

public class CardLogic : MonoBehaviour
{

    public RoleType myRole;
    public bool isRevealed = false;
    public bool isDead = false;

    
    public Image roleIcon;
    public GameObject backCover;
    public Button myButton;


    public void SetupCard(RoleType assignedRole)
    {
        myRole = assignedRole;
        Sprite roleSprite = GameManager.Instance.GetSpriteForRole(myRole);

        if (roleSprite != null)
        {
            roleIcon.sprite = roleSprite;
        }

        backCover.SetActive(true);
        isRevealed = false;
    }

    public void OnClickCard()
    {
        Debug.Log("Has clicado en: " + myRole);
        
        if (isRevealed || isDead || GameManager.Instance.currentAP <= 0) return;

        RevealCard();
    }

    void RevealCard()
    {
        isRevealed = true;
        backCover.SetActive(false); // Quitamos la tapa -> Se ve el roleIcon

        // Gastamos 1 punto de acci�n
        GameManager.Instance.UseAP(1);

        Debug.Log("Revelado: " + myRole);
    }
}