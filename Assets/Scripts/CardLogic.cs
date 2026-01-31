using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardLogic : MonoBehaviour
{
    [Header("Identidad")]
    public RoleType realRole;
    public RoleType disguisedRole;
    public int cardID;

    [Header("Estado")]
    public bool isRevealed = false;
    public bool isDead = false;

    [Header("Referencias UI")]
    public Image roleIcon;
    public GameObject backCover;
    public TextMeshProUGUI idText;

    public Button myButton;

    public void SetupCard(RoleType assignedRole, int newID)
    {
        realRole = assignedRole;
        cardID = newID;

        if (idText != null) idText.text = "#" + cardID.ToString();

        if (IsDemon(realRole))
        {
            disguisedRole = GetRandomVillagerRole();
        }
        else
        {
            disguisedRole = realRole;
        }

        Sprite disguiseSprite = GameManager.Instance.GetSpriteForRole(disguisedRole);
        roleIcon.sprite = disguiseSprite;

        backCover.SetActive(true);
        isRevealed = false;
    }

    bool IsDemon(RoleType role)
    {
        return role == RoleType.Imp;
    }

    RoleType GetRandomVillagerRole()
    {
        int r = Random.Range(0, 3);
        if (r == 0) return RoleType.Healer;
        if (r == 1) return RoleType.Scribe;
        else return RoleType.Investigator;
    }

    public void OnClickCard()
    {
        Debug.Log("Has clicado en la carta #" + cardID + " que es realmente: " + realRole);

        if (isDead || GameManager.Instance.currentAP <= 0) return;

        if (!isRevealed)
        {
            RevealCard();
        }
        else
        {
            UseAbility();
        }

    }

    void RevealCard()
    {
        isRevealed = true;
        backCover.SetActive(false); // Se ve el disguisedRole
        GameManager.Instance.UseAP(1);

        GameManager.Instance.LogInfo("Has revelado una carta. Parece un " + disguisedRole);
    }

    void UseAbility()
    {
        GameManager.Instance.UseAP(1);

        switch (disguisedRole)
        {
            case RoleType.Healer:
                TryToHeal();
                break;

            case RoleType.Scribe:
                TryToCountDemons();
                break;

            case RoleType.Investigator:
                TryToInvestigate();
                break;

                //AGregar mas roles
        }
    }

    void TryToHeal()
    {
        if (IsDemon(realRole))
        {
            GameManager.Instance.playerLives--;
            GameManager.Instance.LogInfo("¡¡Usaste el healer y te quitó 1HP!!");
        }
        else
        {
            GameManager.Instance.playerLives++;
            GameManager.Instance.LogInfo("¡¡Usaste el healer y te sumó 1HP!!");
        }
    }

    void TryToCountDemons()
    {
        int realCount = GameManager.Instance.CountRealDemons();

        if (IsDemon(realRole))
        {
            int fakeCount = realCount + (Random.Range(0, 2) == 0 ? 1 : -1);
            GameManager.Instance.LogInfo($"Scribe(Falso): Veo exactamente {fakeCount} demonios en la mesa");
        }
        else
        {
            GameManager.Instance.LogInfo($"Scribe(Real): Veo exactamente {realCount} demonios en la mesa");
        }
    }

    void TryToInvestigate()
    {
        int totalCards = GameManager.Instance.totalCardsInGame;
        int targetID = cardID;
        if (totalCards > 1)
        {
            while (targetID == cardID)
            {
                targetID = Random.Range(1, totalCards + 1);
            }
        }

        CardLogic targetCard = GameManager.Instance.GetCardByID(targetID);
        
        if (targetCard == null) return;

        bool targetIsDemon = IsDemon(targetCard.realRole);
        bool amILying = IsDemon(realRole);

        string veredicto = "";

        if (amILying)
        {
            veredicto = targetIsDemon ? "HUMANO" : "DEMONIO";
            GameManager.Instance.LogInfo($"Sheriff (Falso): 'La Carta #{targetID} es un {veredicto}'");
        }
        else
        {
            veredicto = targetIsDemon ? "DEMONIO" : "HUMANO";
            GameManager.Instance.LogInfo($"Sheriff (Real): 'La Carta #{targetID} es un {veredicto}'");
        }
    }
}