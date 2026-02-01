using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardLogic : MonoBehaviour, IPointerClickHandler
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
    public bool isCorruptedByBeelzebub = false;
    public bool isBlockedByBelphegor = false;

    [Header("Referencias UI")]
    public Image roleIcon;
    public GameObject backCover;
    public Animator coverAnimator; 
    public TextMeshProUGUI idText;
    public Button myButton;

    [Header("Player Marks")]
    public GameObject markSuspect; 
    public GameObject markGood;    
    public GameObject markEvil;    
     
    private int currentMarkState = 0;

    public void SetupCard(RoleType assignedRole, int newID)
    {
        realRole = assignedRole;
        cardID = newID;

        if (idText != null) idText.text = "#" + cardID.ToString();

        if (IsDemon(realRole) || assignedRole == RoleType.Jester || assignedRole == RoleType.Doomsayer) disguisedRole = GetRandomVillagerRole();
        else disguisedRole = realRole;

        if (GameManager.Instance != null)
        {
            Sprite disguiseSprite = GameManager.Instance.GetSpriteForRole(disguisedRole);
            if (roleIcon != null) roleIcon.sprite = disguiseSprite;
        }

        if (backCover != null) 
        {
            backCover.SetActive(true);
            if (coverAnimator != null) coverAnimator.Rebind();
        }
        
        isRevealed = false;
        isDead = false;
        isUsed = false;
        markedForDeath = false;

        if (myButton != null) myButton.image.color = Color.white;
        if (roleIcon != null) roleIcon.color = Color.white;
    }

    bool IsDemon(RoleType role)
    {
        return role == RoleType.Imp || role == RoleType.Lucifer || role == RoleType.Mammon || role == RoleType.Asmodeus
            || role == RoleType.Satan || role == RoleType.Beelzebub || role == RoleType.Belphegor || role == RoleType.Leviathan;
    }

    public bool CheckIfCorrupted()
    {
        if (IsDemon(realRole) || isCorruptedByBeelzebub || realRole == RoleType.Jester || realRole == RoleType.Doomsayer) return true;

        int leftID = GameManager.Instance.GetCircularID(cardID, -1);
        int rightID = GameManager.Instance.GetCircularID(cardID, 1);

        int[] neighbors = { leftID, rightID };

        foreach (int id in neighbors)
        {
            CardLogic neighbor = GameManager.Instance.GetCardByID(id);
            if (neighbor != null && neighbor.realRole == RoleType.Asmodeus)
            {
                return true;
            }
        }
        return false; 
    }

    RoleType GetRandomVillagerRole()
    {
        int r = Random.Range(0, 10);
        if (r == 0) return RoleType.Healer;
        if (r == 1) return RoleType.Scribe;
        if (r == 2) return RoleType.Queen;
        if (r == 3) return RoleType.Medium;
        if (r == 4) return RoleType.Monk;
        if (r == 5) return RoleType.Gravekeeper;
        if (r == 6) return RoleType.Hunter;
        if (r == 7) return RoleType.Diplomat;
        if (r == 8) return RoleType.Lazy;
        else return RoleType.Torchbearer;
    }
    
    public void OnClickCard()
    {
        if (GameManager.Instance.isGameOver) return;

        if (isBlockedByBelphegor)
        {
            if (CheckIfBelphegorIsAliveAround())
            {
                GameManager.Instance.LogInfo("Esta carta está sumida en la PEREZA (Bloqueada por Belphegor)...");
                return;
            }
            else
            {
                isBlockedByBelphegor = false;
                if (myButton != null) myButton.interactable = true;
                if (roleIcon != null) roleIcon.color = Color.white;
                GameManager.Instance.LogInfo("¡La influencia de Belphegor ha desaparecido!");
            }
        }

        if (GameManager.Instance.isInvestigating)
        {
            if (GameManager.Instance.currentInvestigator == this)
            {
                GameManager.Instance.LogInfo("Investigator: 'Cancelando investigación...'");
                GameManager.Instance.isInvestigating = false;
                GameManager.Instance.currentInvestigator = null;
                return;
            }

            if (realRole == RoleType.Satan && !isDead)
            {
                GameManager.Instance.playerLives--;
                GameManager.Instance.LogInfo("¡Has tocado a SATANÁS! Te quemas la mano (-1 HP).");
                GameManager.Instance.UpdateUI();
            }

            GameManager.Instance.ProcessInvestigationTarget(this);
            return;
        }

        if (isDead || GameManager.Instance.currentAP <= 0) return;

        if (GameManager.Instance.isExecutionMode)
        {
            GameManager.Instance.playSFX(GameManager.Instance.killSound);
            ExecuteThisCard();
        }
        else
        {
            if (realRole == RoleType.Satan)
            {
                GameManager.Instance.playerLives--;
                GameManager.Instance.LogInfo("¡Has invocado a SATANÁS! (-1 HP).");
                GameManager.Instance.UpdateUI();
            }

            if (!isRevealed)
            {
                GameManager.Instance.playSFX(GameManager.Instance.flipSound);
                RevealCard();
            }
            else
            {
                if(!isUsed) UseAbility();
            }
        }
    }

    bool CheckIfBelphegorIsAliveAround()
    {
        int leftID = GameManager.Instance.GetCircularID(cardID, -1);
        int rightID = GameManager.Instance.GetCircularID(cardID, 1);
        int[] neighbors = { leftID, rightID };

        foreach (int id in neighbors)
        {
            CardLogic n = GameManager.Instance.GetCardByID(id);
            if (n != null && n.realRole == RoleType.Belphegor && !n.isDead) return true;
        }
        return false;
    }

    IEnumerator BurnRoutine()
    {
        if (coverAnimator != null)
        {
            coverAnimator.SetTrigger("burn");
            
            yield return new WaitForSeconds(2.0f); 
            
            if (backCover != null) backCover.SetActive(false);
        }
        else
        {
            if (backCover != null) backCover.SetActive(false);
        }
    }

    public void ExecuteThisCard()
    {
        isDead = true;
        isRevealed = true;          
        
        Sprite trueSprite = GameManager.Instance.GetSpriteForRole(realRole);
        if (roleIcon != null) roleIcon.sprite = trueSprite;

        StartCoroutine(BurnRoutine());

        if (myButton != null) {
            myButton.image.color = Color.gray;
            myButton.interactable = true ;
        }
        if (roleIcon != null) roleIcon.color = Color.gray;

        if (IsDemon(realRole))
        {
            GameManager.Instance.LogInfo($"¡JUSTICIA! Ejecutaste a la carta #{cardID}. ERA UN DEMONIO.");
            if (roleIcon != null) roleIcon.color = new Color(1f, 0.5f, 0.5f);
            GameManager.Instance.CheckWinCondition();
        }
        else
        {
            switch (realRole)
            {
                case RoleType.Jester:
                    GameManager.Instance.LogInfo("¡JESTER! Te ha gastado una broma final. (-1 AP Máximo permanente).");
                    GameManager.Instance.permanentAPPenalty++;
                    break;

                case RoleType.Doomsayer:
                    GameManager.Instance.LogInfo("¡DOOMSAYER! Su muerte invoca la oscuridad inmediata.");
                    GameManager.Instance.playerLives -= 2;
                    GameManager.Instance.SkipToNight();
                    break;

                case RoleType.Lazy:
                    GameManager.Instance.LogInfo("¡Has matado al VAGO! Era inútil, pero inocente. (-1 HP).");
                    GameManager.Instance.playerLives -= 1;
                    break;

                case RoleType.Amnesiac:
                default:
                    GameManager.Instance.playerLives -= 2;
                    GameManager.Instance.LogInfo($"¡ERROR! Has ejecutado a un INOCENTE (#{cardID}).");
                    break;
            }
        }

        GameManager.Instance.UpdateUI();
        GameManager.Instance.ToggleExecutionMode();
    }

    public void DieSilently()
    {
        isDead = true;
        isRevealed = true;

        Sprite trueSprite = GameManager.Instance.GetSpriteForRole(realRole);
        if (roleIcon != null)
        {
            roleIcon.sprite = trueSprite;
            roleIcon.color = Color.gray;
        }

        StartCoroutine(BurnRoutine());

        if (myButton != null) myButton.image.color = Color.gray;
    }

    public void RevealCard()
    {
        isRevealed = true;
        
        StartCoroutine(BurnRoutine());
        
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
            case RoleType.Jester:
                GameManager.Instance.LogInfo("Jester: *Hace malabares y se ríe*");
                isUsed = true;
                break;
            case RoleType.Doomsayer:
                GameManager.Instance.LogInfo("Doomsayer: 'El fin se acerca...'");
                isUsed = true;
                break;
            case RoleType.Amnesiac:
                GameManager.Instance.LogInfo("Amnesiac: '¿Quién soy? No recuerdo nada...'");
                isUsed = true;
                break;
            case RoleType.Lazy:
                GameManager.Instance.LogInfo("Lazy: 'Zzz...'");
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
            string lie = targetIsDemon ? "un VILLAGER" : "un DEMONIO";
            GameManager.Instance.LogInfo($"Queen: 'Mi intuición real dice que #{targetID} es {lie}'");
        }
        else
        {
            string truth = targetIsDemon ? "un DEMONIO" : "un VILLAGER";
            GameManager.Instance.LogInfo($"Quees: 'Declaro que la carta #{targetID} es {truth}'");
        }
    }

    void TryToMedium()
    {
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

            for (int i = 0; i < innocentIDs.Count; i++)
            {
                int temp = innocentIDs[i];
                int r = Random.Range(i, innocentIDs.Count);
                innocentIDs[i] = innocentIDs[r];
                innocentIDs[r] = temp;
            }

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
                chosenIDs.Add(demons[Random.Range(0, demons.Count)]); 
                chosenIDs.Add(villagers[Random.Range(0, villagers.Count)]); 
                chosenIDs.Add(villagers[Random.Range(0, villagers.Count)]); 
            }
            else
            {
                GameManager.Instance.LogInfo("Medium: 'Los espíritus están confusos... (No encuentro el patrón 1 Demonio + 2 Aldeanos)'");
                return;
            }
        }

        if (chosenIDs.Count >= 3)
        {
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
        if (CheckIfCorrupted())
        {
            GameManager.Instance.LogInfo("Torchbearer: 'La luz brilla con fuerza...'");
            return;
        }

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
            DieSilently();
            GameManager.Instance.UpdateUI();
        }
        else
        {
            GameManager.Instance.LogInfo("Torchbearer: 'La luz brilla con fuerza...'");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (GameManager.Instance.isGameOver) return;

            CycleMark();
        }
    }

    void CycleMark()
    {
        currentMarkState++;
        if (currentMarkState > 3) currentMarkState = 0; 

        UpdateMarkVisuals();
    }

    void UpdateMarkVisuals()
    {
        
        if (markSuspect != null) markSuspect.SetActive(false);
        if (markGood != null) markGood.SetActive(false);
        if (markEvil != null) markEvil.SetActive(false);

       
        switch (currentMarkState)
        {
            case 1:
                if (markSuspect != null) markSuspect.SetActive(true);
                break;
            case 2:
                if (markGood != null) markGood.SetActive(true);
                break;
            case 3:
                if (markEvil != null) markEvil.SetActive(true);
                break;
        }
    }
}