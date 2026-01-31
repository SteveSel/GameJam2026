using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardLogic : MonoBehaviour
{
    [Header("Identidad")]
    public RoleType realRole;
    public RoleType disguisedRole;
    public int cardID;

    [Header("Estado")]
    public bool isRevealed = false;
    public bool isDead = false;
    public bool isUsed;

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
        return role == RoleType.Imp || role == RoleType.Lucifer || role == RoleType.Mammon;
    }

    RoleType GetRandomVillagerRole()
    {
        int r = Random.Range(0, 6);
        if (r == 0) return RoleType.Healer;
        if (r == 1) return RoleType.Scribe;
        if (r == 2) return RoleType.Queen;
        if (r == 3) return RoleType.Medium;
        if (r == 4) return RoleType.Monk;
        else return RoleType.Gravekeeper;
    }
    
    public void OnClickCard()
    {
        if (GameManager.Instance.isGameOver) return;

        Debug.Log("Has clicado en la carta #" + cardID + " que es realmente: " + realRole);

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
                if(!isUsed) UseAbility();
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

        Sprite trueSprite = GameManager.Instance.GetSpriteForRole(realRole);

        if (roleIcon != null)
        {
            roleIcon.sprite = trueSprite;
        }

        //Gris = muerto
        if (myButton != null) myButton.image.color = Color.gray;
        if (roleIcon != null) roleIcon.color = Color.gray;

        if (IsDemon(realRole))
        {
            GameManager.Instance.LogInfo($"¡JUSTICE! You executed card #{cardID}. IT WAS A DEMON.");
            if (roleIcon != null) roleIcon.color = new Color(1f, 0.5f, 0.5f);
            GameManager.Instance.CheckWinCondition();
        }
        else
        {
            GameManager.Instance.playerLives -= 2;
            GameManager.Instance.LogInfo($"Has ejecutado a un INOCENTE (#{cardID}).");
        }

        GameManager.Instance.UpdateUI();
        GameManager.Instance.ToggleExecutionMode();
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
                //WIP
                break;
            case RoleType.Queen:
                TryToQueen();
                break;
            case RoleType.Medium:
                TryToMedium();
                break;
            case RoleType.Monk:
                TryToMonk();
                break;
            case RoleType.Gravekeeper:
                TryToGravekeeper();
                break;

        }
        isUsed = true;
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

    void TryToQueen()
    {
        // Lógica antigua del Investigator: Dice si alguien es aldeano o no
        int totalCards = GameManager.Instance.totalCardsInGame;
        int targetID = cardID;

        // Buscar a otro que no sea yo
        if (totalCards > 1)
        {
            while (targetID == cardID) targetID = Random.Range(1, totalCards + 1);
        }

        CardLogic targetCard = GameManager.Instance.GetCardByID(targetID);
        if (targetCard == null) return;

        bool targetIsDemon = IsDemon(targetCard.realRole);
        bool amILying = IsDemon(realRole); // Si soy Queen falsa (Imp), miento

        if (amILying)
        {
            // MIENTO: Digo lo contrario
            // Si es Demonio -> Digo "Es un Villager"
            // Si es Villager -> Digo "Es un Demonio"
            string lie = targetIsDemon ? "un VILLAGER" : "un DEMONIO";
            GameManager.Instance.LogInfo($"Queen (Falsa): 'Mi intuición real dice que #{targetID} es {lie}'");
        }
        else
        {
            // VERDAD
            string truth = targetIsDemon ? "un DEMONIO" : "un VILLAGER";
            GameManager.Instance.LogInfo($"Queen (Real): 'Declaro que la carta #{targetID} es {truth}'");
        }
    }

    void TryToMedium()
    {
        // Obtener todas las cartas vivas
        List<CardLogic> allCards = GameManager.Instance.GetAllCards();
        List<int> chosenIDs = new List<int>();

        bool amILying = IsDemon(realRole);

        if (amILying)
        {
            foreach (var c in allCards)
            {
                if (!IsDemon(c.realRole) && c.cardID != cardID) chosenIDs.Add(c.cardID);
            }
        }
        else
        {
            List<int> demons = new List<int>();
            List<int> villagers = new List<int>();

            foreach (var c in allCards)
            {
                if (c.cardID == cardID) continue;
                if (IsDemon(c.realRole)) demons.Add(c.cardID);
                else villagers.Add(c.cardID);
            }

            
            if (demons.Count > 0 && villagers.Count >= 2)
            {
                chosenIDs.Add(demons[Random.Range(0, demons.Count)]); // 1 Demonio
                chosenIDs.Add(villagers[Random.Range(0, villagers.Count)]); // Aldeano 1
                chosenIDs.Add(villagers[Random.Range(0, villagers.Count)]); // Aldeano 2
                // Puede salir el mismo villager 2 veces en la lista
            }
            else
            {
                GameManager.Instance.LogInfo("Medium: 'Los espíritus están confusos... (No encuentro el patrón 1 Demonio + 2 Aldeanos)'");
                return;
            }
        }
        
        if (chosenIDs.Count >= 3)
        {
            string msg = $"Medium: '¡Siento el mal! Uno entre #{chosenIDs[0]}, #{chosenIDs[1]} y #{chosenIDs[2]} es MALVADO.'";
            GameManager.Instance.LogInfo(msg);
        }
        else
        {
            GameManager.Instance.LogInfo("Medium: 'No hay suficientes cartas para mi visión.'");
        }
    }

    void TryToMonk()
    {
        int leftID = cardID - 1;
        int rightID = cardID + 1;

        int demonsFound = 0;
        int neighborsChecked = 0;

        // Chequear izquierda
        CardLogic leftCard = GameManager.Instance.GetCardByID(leftID);
        if (leftCard != null && !leftCard.isDead)
        {
            neighborsChecked++;
            if (IsDemon(leftCard.realRole)) demonsFound++;
        }

        // Chequear derecha
        CardLogic rightCard = GameManager.Instance.GetCardByID(rightID);
        if (rightCard != null && !rightCard.isDead)
        {
            neighborsChecked++;
            if (IsDemon(rightCard.realRole)) demonsFound++;
        }

        bool amILying = IsDemon(realRole);

        if (amILying)
        {
            // MENTIRA: Decimos un número falso (por ejemplo, 0 si hay, o sumamos 1)
            int fakeCount = (demonsFound == 0) ? 1 : 0;
            GameManager.Instance.LogInfo($"Monk: 'De mis {neighborsChecked} vecinos, {fakeCount} mienten.'");
        }
        else
        {
            // VERDAD
            GameManager.Instance.LogInfo($"Monk: 'De mis {neighborsChecked} vecinos, {demonsFound} mienten.'");
        }
    }

    void TryToGravekeeper()
    {
        // Mira rango +/- 2 buscando High Rank
        bool deathNear = false;
        int[] offsets = { -2, -1, 1, 2 };

        foreach (int offset in offsets)
        {
            CardLogic target = GameManager.Instance.GetCardByID(cardID + offset);
            if (target != null && !target.isDead)
            {
                if (GameManager.Instance.IsHighRankDemon(target.realRole))
                {
                    deathNear = true;
                    break; // Ya encontramos uno, no hace falta seguir
                }
            }
        }

        bool amILying = IsDemon(realRole);

        if (amILying)
        {
            // Si soy malo, siempre me callo (para proteger a mis jefes)
            GameManager.Instance.LogInfo("Gravekeeper: '...' (Silencio sepulcral)");
        }
        else
        {
            if (deathNear)
            {
                GameManager.Instance.LogInfo("Gravekeeper: '...Siento la MUERTE cerca... (Hay un Demonio de Alto Rango cerca)'");
            }
            else
            {
                GameManager.Instance.LogInfo("Gravekeeper: '...' (Silencio sepulcral)");
            }
        }
    }

}