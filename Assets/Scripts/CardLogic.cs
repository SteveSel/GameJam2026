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
    public bool markedForDeath = false;

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
        isUsed = false;
        markedForDeath = false;

        if (myButton != null) myButton.image.color = Color.white;
        if (roleIcon != null) roleIcon.color = Color.white;
    }

    bool IsDemon(RoleType role)
    {
        return role == RoleType.Imp || role == RoleType.Lucifer || role == RoleType.Mammon || role == RoleType.Asmodeus;
    }

    public bool CheckIfCorrupted()
    {
        if (IsDemon(realRole)) return true;

        int leftID = GameManager.Instance.GetCircularID(cardID, -1);
        int rightID = GameManager.Instance.GetCircularID(cardID, 1);

        int[] neighbors = { leftID, rightID };

        foreach (int id in neighbors)
        {
            CardLogic neighbor = GameManager.Instance.GetCardByID(id);
            // Si mi vecino existe y es ASMODEUS...
            if (neighbor != null && neighbor.realRole == RoleType.Asmodeus)
            {
                return true;
            }
        }
        return false; // Estoy sano
    }

    RoleType GetRandomVillagerRole()
    {
        int r = Random.Range(0, 9);
        if (r == 0) return RoleType.Healer;
        if (r == 1) return RoleType.Scribe;
        if (r == 2) return RoleType.Queen;
        if (r == 3) return RoleType.Medium;
        if (r == 4) return RoleType.Monk;
        if (r == 5) return RoleType.Gravekeeper;
        if (r == 6) return RoleType.Hunter;
        if (r == 7) return RoleType.Diplomat;
        else return RoleType.Torchbearer;
    }
    
    public void OnClickCard()
    {
        if (GameManager.Instance.isGameOver) return;

        Debug.Log("Has clicado en la carta #" + cardID + " que es realmente: " + realRole);

        if (GameManager.Instance.isInvestigating)
        {
            if (GameManager.Instance.currentInvestigator == this)
            {
                GameManager.Instance.LogInfo("Investigator: 'Cancelando investigación...'");
                GameManager.Instance.isInvestigating = false;
                GameManager.Instance.currentInvestigator = null;
                return;
            }

            GameManager.Instance.ProcessInvestigationTarget(this);
            return;
        }

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

    public void DieSilently()
    {
        isDead = true;
        isRevealed = true;

        if (backCover != null) backCover.SetActive(false);
        if (myButton != null) myButton.image.color = Color.gray;

        // Revelar identidad real
        Sprite trueSprite = GameManager.Instance.GetSpriteForRole(realRole);
        if (roleIcon != null)
        {
            roleIcon.sprite = trueSprite;
            roleIcon.color = Color.gray;
        }
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
        bool isTargetingRole = (disguisedRole == RoleType.Investigator || disguisedRole == RoleType.Hunter || disguisedRole == RoleType.Diplomat);
        if (!isTargetingRole)
        {
            GameManager.Instance.UseAP(1);
        }

        switch (disguisedRole)
        {
            case RoleType.Healer:
                TryToHeal();
                isUsed = true;
                break;
            case RoleType.Scribe:
                TryToCountDemons();
                isUsed = true;
                break;
            case RoleType.Queen:
                TryToQueen();
                isUsed = true;
                break;
            case RoleType.Medium:
                TryToMedium();
                isUsed = true;
                break;
            case RoleType.Monk:
                isUsed = true;
                TryToMonk();
                break;
            case RoleType.Gravekeeper:
                TryToGravekeeper();
                isUsed = true;
                break;
            case RoleType.Torchbearer:
                TryToTorchbearer();
                isUsed = true;
                break;
            case RoleType.Investigator:
            case RoleType.Diplomat:
            case RoleType.Hunter:
                if (GameManager.Instance.currentAP > 0)
                {
                    GameManager.Instance.StartInvestigation(this);
                }
                break;
        }
    }

    void TryToHeal()
    {
        if (CheckIfCorrupted())
        {
            GameManager.Instance.playerLives--;
            GameManager.Instance.LogInfo("Healer: Te ha envenenado (-1 HP)");
        }
        else
        {
            if (GameManager.Instance.playerLives < GameManager.Instance.maxLives)
            {
                GameManager.Instance.playerLives++;
                GameManager.Instance.LogInfo("Healer: Te ha curado (+1 HP)");
            }
            else
            {
                GameManager.Instance.LogInfo("Healer: Ya estás a tope de vida.");
            }
        }
        GameManager.Instance.UpdateUI();
    }

    void TryToCountDemons()
    {
        int realCount = GameManager.Instance.CountRealDemons();

        if (CheckIfCorrupted())
        {
            int fakeCount = realCount + (Random.Range(0, 2) == 0 ? 1 : -1);
            // Evitar negativos
            if (fakeCount < 0) fakeCount = 0; 
            
            GameManager.Instance.LogInfo($"Scribe: 'Detecto {fakeCount} presencias oscuras...'");
        }
        else
        {
            GameManager.Instance.LogInfo($"Scribe: 'Detecto exactamente {realCount} presencias oscuras.'");
        }
    }

    void TryToQueen()
    {
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
        bool amILying = CheckIfCorrupted();

        if (amILying)
        {
            // MIENTO: Digo lo contrario
            // Si es Demonio -> Digo "Es un Villager"
            // Si es Villager -> Digo "Es un Demonio"
            string lie = targetIsDemon ? "un VILLAGER" : "un DEMONIO";
            GameManager.Instance.LogInfo($"Queen: 'Mi intuición real dice que #{targetID} es {lie}'");
        }
        else
        {
            // VERDAD
            string truth = targetIsDemon ? "un DEMONIO" : "un VILLAGER";
            GameManager.Instance.LogInfo($"Quees: 'Declaro que la carta #{targetID} es {truth}'");
        }
    }

    void TryToMedium()
    {
        // Obtener todas las cartas vivas
        List<CardLogic> allCards = GameManager.Instance.GetAllCards();
        List<int> chosenIDs = new List<int>();

        bool amILying = CheckIfCorrupted();

        if (amILying)
        {
            List<int> innocentIDs = new List<int>();
            foreach (var c in allCards)
            {
                if (!IsDemon(c.realRole) && c.cardID != cardID) innocentIDs.Add(c.cardID);
            }

            // Mezclar
            for (int i = 0; i < innocentIDs.Count; i++)
            {
                int temp = innocentIDs[i];
                int r = Random.Range(i, innocentIDs.Count);
                innocentIDs[i] = innocentIDs[r];
                innocentIDs[r] = temp;
            }

            // Coger hasta 3
            for (int i = 0; i < 3 && i < innocentIDs.Count; i++)
            {
                chosenIDs.Add(innocentIDs[i]);
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
            // Mezclar
            for (int i = 0; i < chosenIDs.Count; i++)
            {
                int temp = chosenIDs[i];
                int r = Random.Range(0, chosenIDs.Count);
                chosenIDs[i] = chosenIDs[r];
                chosenIDs[r] = temp;
            }

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
        int leftID = GameManager.Instance.GetCircularID(cardID, -1);    
        int rightID = GameManager.Instance.GetCircularID(cardID, 1);

        int demonsFound = 0;
        int neighborsChecked = 0;

        int[] neighborIDs = { leftID, rightID };

        foreach (int id in neighborIDs)
        {
            CardLogic neighbor = GameManager.Instance.GetCardByID(id);
            
            if (neighbor != null)
            {
                neighborsChecked++;
                if (IsDemon(neighbor.realRole)) demonsFound++;
            }
        }

        bool amILying = CheckIfCorrupted();

        if (amILying)
        {
            int fakeCount = (demonsFound == 0) ? 1 : 0;
            GameManager.Instance.LogInfo($"Monk: 'De mis {neighborsChecked} vecinos, {fakeCount} mienten.'");
        }
        else
        {
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
            int targetID = GameManager.Instance.GetCircularID(cardID, offset);

            CardLogic target = GameManager.Instance.GetCardByID(targetID);

            if (target != null)
            {
                if (GameManager.Instance.IsHighRankDemon(target.realRole))
                {
                    deathNear = true;
                    break;
                }
            }
        }

        bool amILying = CheckIfCorrupted();

        if (amILying)
        {
            // Si soy malo, siempre me callo
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

    void TryToTorchbearer()
    {
        // Si soy demonio, nunca me quemo
        if (IsDemon(realRole))
        {
            GameManager.Instance.LogInfo("Torchbearer: 'La luz brilla con fuerza...'");
            return;
        }

        // Si soy bueno, miro a mis vecinos
        int leftID = GameManager.Instance.GetCircularID(cardID, -1);
        int rightID = GameManager.Instance.GetCircularID(cardID, 1);
        int[] neighbors = { leftID, rightID };

        bool demonFound = false;
        foreach (int id in neighbors)
        {
            CardLogic neighbor = GameManager.Instance.GetCardByID(id);
            if (neighbor != null && !neighbor.isDead && IsDemon(neighbor.realRole))
            {
                demonFound = true;
                break;
            }
        }

        if (demonFound)
        {
            GameManager.Instance.LogInfo("Torchbearer: '¡HAY UN DEMONIO CERCA! ¡AAARGH!' (Se quema)");
            // Se sacrifica
            DieSilently();
            GameManager.Instance.UpdateUI();
        }
        else
        {
            GameManager.Instance.LogInfo("Torchbearer: 'La luz brilla con fuerza...'");
        }
    }
}