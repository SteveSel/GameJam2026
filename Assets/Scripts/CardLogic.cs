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

        if (IsDemon(realRole)) disguisedRole = GetRandomVillagerRole();
        else disguisedRole = realRole;

        if (GameManager.Instance != null)
        {
            Sprite disguiseSprite = GameManager.Instance.GetSpriteForRole(disguisedRole);
            if (roleIcon != null) roleIcon.sprite = disguiseSprite;
        }

        if (backCover != null) backCover.SetActive(true);
        
        isRevealed = false;
        isDead = false;
        
        if (myButton != null) myButton.image.color = Color.white;
        if (roleIcon != null) roleIcon.color = Color.white;
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
        if (isDead || GameManager.Instance.currentAP <= 0) return;

        if (GameManager.Instance.isExecutionMode)
        {
            ExecuteThisCard();
        }
        else
        {
            if (!isRevealed)
            {
                RevealCard();
            }
            else
            {
                UseAbility();
            }
        }
    }

    public void ExecuteThisCard()
    {
        isDead = true;
        isRevealed = true;          
        backCover.SetActive(false); 

        if (myButton != null) myButton.image.color = Color.gray;
        if (roleIcon != null) roleIcon.color = Color.gray;

        GameManager.Instance.ToggleExecutionMode(false);

        if (IsDemon(realRole))
        {
            GameManager.Instance.LogInfo($"Has ejecutado al DEMONIO (#{cardID}).");
        }
        else
        {
            GameManager.Instance.playerLives -= 2;
            GameManager.Instance.LogInfo($"Has ejecutado a un INOCENTE (#{cardID}).");
        }

        GameManager.Instance.UpdateUI();
    }

    public void RevealCard()
    {
        isRevealed = true;
        backCover.SetActive(false); 
        
        GameManager.Instance.UseAP(1);
        
        GameManager.Instance.LogInfo($"Investigación: La carta #{cardID} parece {disguisedRole}");
    }

    public void UseAbility()
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
        }
    }

    void TryToHeal()
    {
        if (IsDemon(realRole))
        {
            GameManager.Instance.playerLives--;
            GameManager.Instance.LogInfo("Healer (Falso): Te ha envenenado (-1 HP)");
        }
        else
        {
            // No puedes curarte por encima del máximo
            if (GameManager.Instance.playerLives < GameManager.Instance.maxLives)
            {
                GameManager.Instance.playerLives++;
                GameManager.Instance.LogInfo("Healer (Real): Te ha curado (+1 HP)");
            }
            else
            {
                GameManager.Instance.LogInfo("Healer (Real): Ya estás a tope de vida.");
            }
        }
        GameManager.Instance.UpdateUI();
    }

    void TryToCountDemons()
    {
        int realCount = GameManager.Instance.CountRealDemons();

        if (IsDemon(realRole))
        {
            int fakeCount = realCount + (Random.Range(0, 2) == 0 ? 1 : -1);
            // Evitar negativos
            if (fakeCount < 0) fakeCount = 0; 
            
            GameManager.Instance.LogInfo($"Scribe (Falso): 'Detecto {fakeCount} presencias oscuras...'");
        }
        else
        {
            GameManager.Instance.LogInfo($"Scribe (Real): 'Detecto exactamente {realCount} presencias oscuras.'");
        }
    }

    void TryToInvestigate()
    {
        int totalCards = GameManager.Instance.totalCardsInGame;
        int targetID = cardID;
        int intentos = 0;
        if (totalCards > 1)
        {
            while (targetID == cardID && intentos < 100)
            {
                targetID = Random.Range(1, totalCards + 1);
                intentos++;
            }
        }

        CardLogic targetCard = GameManager.Instance.GetCardByID(targetID);
        
        if (targetCard == null || targetCard.isDead) 
        {
            GameManager.Instance.LogInfo("Sheriff: 'No encuentro a nadie sospechoso...'");
            return;
        }

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