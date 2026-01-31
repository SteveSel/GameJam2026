using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum GamePhase
{
    Day,
    Night
}

[System.Serializable]
public struct RoleVisualData
{
    public RoleType role;
    public Sprite artwork;
    public string description;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración de Roles")]
    public List<RoleVisualData> roleLibrary; 

    [Header("Estado del Juego")]
    public GamePhase currentPhase = GamePhase.Day;
    public int dayCount = 1;
    public int currentAP = 3;
    public int maxAP = 3;
    public int playerLives = 3;
    public int maxLives = 3;
    public int totalCardsInGame = 9;
    public bool isGameOver = false;

    public bool isExecutionMode = false;
    private bool isTransitioning = false;

    public bool isInvestigating = false;
    public CardLogic currentInvestigator;

    [Header("UI References")]
    public TextMeshProUGUI apText;
    public TextMeshProUGUI livesText;
    public Image executionButtonImage;
    public TextMeshProUGUI infoLog; // El log pequeño (historial)

    [Header("Notificaciones en Pantalla")]
    public TextMeshProUGUI bigMessageText;       // ARRASTRA AQUÍ TU NUEVO TEXTO
    public CanvasGroup bigMessageCanvasGroup;    // ARRASTRA AQUÍ EL CANVAS GROUP DEL TEXTO
    public float messageDuration = 2.0f;         // Cuanto tiempo se queda el mensaje en pantalla

    [Header("Transición Día/Noche")]
    public CanvasGroup fadePanel;
    public Animator backgroundAnimator;
    public float fadeDuration = 1.0f;

    [Header("Game Over UI")]
    public GameObject gameOverPanel ;
    public TextMeshProUGUI resultTitle ;
    public TextMeshProUGUI statsSummary ;

    // Variable para controlar la corrutina de texto y que no se solapen bruscamente
    private Coroutine currentMessageRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
        
        // Configuración inicial de UI
        if (fadePanel != null) 
        {
            fadePanel.alpha = 0;
            fadePanel.blocksRaycasts = false;
        }
        if (bigMessageCanvasGroup != null)
        {
            bigMessageCanvasGroup.alpha = 0; // Texto invisible al inicio
            bigMessageCanvasGroup.blocksRaycasts = false;
        }

        // Mostrar Día 1 al iniciar
        ShowOnScreenMessage("Día " + dayCount);
    }

    // Esta función ahora muestra el texto en pantalla Y lo guarda en el log pequeño
    public void LogInfo(string message)
    {
        // 1. Log pequeño (Historial)
        if (infoLog != null) infoLog.text += "> " + message;

        // 2. Texto Grande en Pantalla
        ShowOnScreenMessage(message);
    }

    public void ShowOnScreenMessage(string message)
    {
        if (bigMessageText == null || bigMessageCanvasGroup == null) return;

        // Si ya hay un mensaje mostrándose, lo paramos para mostrar el nuevo
        if (currentMessageRoutine != null) StopCoroutine(currentMessageRoutine);
        
        currentMessageRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    IEnumerator ShowMessageRoutine(string msg)
    {
        bigMessageText.text = msg;

        // FADE IN (Aparición rápida)
        float t = 0;
        while(t < 0.2f)
        {
            t += Time.deltaTime;
            bigMessageCanvasGroup.alpha = Mathf.Lerp(0, 1, t / 0.2f);
            yield return null;
        }
        bigMessageCanvasGroup.alpha = 1;

        // ESPERA (Lectura)
        yield return new WaitForSeconds(messageDuration);

        // FADE OUT (Desaparición suave)
        t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            bigMessageCanvasGroup.alpha = Mathf.Lerp(1, 0, t / 0.5f);
            yield return null;
        }
        bigMessageCanvasGroup.alpha = 0;
    }

    public void ToggleExecutionMode(bool showMessage = true)
    {
        if (isTransitioning) return;

        isExecutionMode = !isExecutionMode;

        if (executionButtonImage != null)
        {
            executionButtonImage.color = isExecutionMode ? Color.red : Color.white;
        }

        if (showMessage)
        {
            if (isExecutionMode) LogInfo("MODO EJECUCIÓN ACTIVADO");
            else LogInfo("Modo Investigación");
        }
    }

    public Sprite GetSpriteForRole(RoleType roleToFind)
    {
        foreach (var data in roleLibrary)
        {
            if (data.role == roleToFind) return data.artwork;
        }
        return null;
    }

    public CardLogic GetCardByID(int idToFind)
    {
        CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);
        foreach (CardLogic card in allCards)
        {
            if (card.cardID == idToFind) return card;
        }
        return null;
    }

    public void UseAP(int amount)
    {
        if (isTransitioning) return;

        currentAP -= amount;
        UpdateUI(); 

        if (currentAP <= 0)
        {
            currentAP = 0;
            StartCoroutine(TransitionToNightRoutine());
        }
    }
    
    IEnumerator TransitionToNightRoutine()
    {
        isTransitioning = true;
        if (fadePanel != null) fadePanel.blocksRaycasts = true; 

        LogInfo("Cae la noche...");

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; 
        }
        if (fadePanel != null) fadePanel.alpha = 1f;

        if (backgroundAnimator != null) backgroundAnimator.SetBool("esNoche", true);
        
        yield return new WaitForSeconds(0.5f);

        // --- FASE 2: MOSTRAR LA NOCHE ---
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        List<CardLogic> huntersToDie = new List<CardLogic>();
        foreach (var c in GetAllCards())
        {
            if (c.markedForDeath && !c.isDead) huntersToDie.Add(c);
        }

        foreach (var h in huntersToDie)
        {
            LogInfo($"NOCHE: El Hunter (#{h.cardID}) ha sucumbido a la culpa.");
            h.DieSilently();
        }

        // MAMMON
        int mammonCount = CountSpecificRoleAlive(RoleType.Mammon);
        int apPenalty = (mammonCount > 0) ? 1 : 0;
        if (apPenalty > 0) LogInfo("NOCHE: Mammon te ha robado energía... (-1 AP mañana)");

        // LUCIFER
        int luciferCount = CountSpecificRoleAlive(RoleType.Lucifer);

        if (luciferCount > 0)
        {
            // Buscar víctimas posibles
            List<CardLogic> victims = new List<CardLogic>();
            CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);

            foreach (var c in allCards)
            {
                if (!IsHighRankDemon(c.realRole) && c.realRole != RoleType.Imp && !c.isDead)
                {
                    victims.Add(c);
                }
            }

            if (victims.Count > 0)
            {
                // Elegir y Matar
                CardLogic target = victims[Random.Range(0, victims.Count)];

                target.DieSilently();
                playerLives -= 2;

                LogInfo($"NOCHE: ¡Lucifer ha sacrificado a la carta #{target.cardID} ({target.realRole})! Pierdes 2 HP.");
            }
        }

        yield return new WaitForSeconds(2.0f);

        // --- FASE 4: AMANECER ---
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        if (fadePanel != null) fadePanel.alpha = 1f;

        // <--- AQUÍ INCREMENTAMOS EL DÍA --->
        if (backgroundAnimator != null) backgroundAnimator.SetBool("esNoche", false);
        
        dayCount++; // Subimos el contador
        currentAP = maxAP - apPenalty; 
        UpdateUI();
        
        // <--- MOSTRAMOS EL TEXTO DE NUEVO DÍA --->
        // Usamos una duración un poco más larga para el título del día
        ShowOnScreenMessage("DÍA " + dayCount); 

        yield return new WaitForSeconds(1.0f); // Pausa un poco más larga en negro para leer "Día X"

        // --- FASE 5: VOLVER A JUGAR ---
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        
        if (fadePanel != null) 
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false; 
        }

        isTransitioning = false;
    }

    public void UpdateUI()
    {
        if (apText != null) apText.text = $"AP: {currentAP} / {maxAP}";
        if (livesText != null) livesText.text = $"HP: {playerLives}";

        if (playerLives <= 0 && !isGameOver)
        {
            TriggerGameOver(false);
        }
    }

    public int CountAliveDemons()
    {
        CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);
        int count = 0;
        foreach (CardLogic card in allCards)
        {
            if ((card.realRole == RoleType.Imp || card.realRole == RoleType.Lucifer || card.realRole == RoleType.Mammon || card.realRole == RoleType.Asmodeus) && !card.isDead)
            {
                count++;
            }
        }
        return count;
    }

    public void CheckWinCondition()
    {
        int demonsLeft = CountAliveDemons();

        if (demonsLeft == 0 && !isGameOver)
        {
            TriggerGameOver(true);
        }
    }

    public void TriggerGameOver(bool victory)
    {
        isGameOver = true;

        if (executionButtonImage != null) executionButtonImage.gameObject.SetActive(false);

        if (gameOverPanel != null) {
            gameOverPanel.SetActive(true);
        
            if (resultTitle != null) {
                resultTitle.text = victory ? "¡VICTORIA!" : "DERROTA...";
                resultTitle.color = victory ? Color.green : Color.red;
            }
            if (statsSummary != null) {
                // Aquí puedes añadir más estadísticas si lo deseas
                statsSummary.text = $"Días sobrevividos: {dayCount}\n";
            }
        }
        if (victory) LogInfo("¡Has eliminado a todos los demonios!"); 
        else LogInfo("Has perdido todas tus vidas...");
    }

    public bool IsHighRankDemon(RoleType role)
    {
        return role == RoleType.Lucifer || role == RoleType.Mammon || role == RoleType.Asmodeus;
    }

    public List<CardLogic> GetAllCards()
    { 
        return new List<CardLogic>(FindObjectsByType<CardLogic>(FindObjectsSortMode.None));
    }

    public void StartInvestigation(CardLogic investigatorCard)
    {
        if (currentAP <= 0) return;

        isInvestigating = true;
        currentInvestigator = investigatorCard;

        LogInfo($"(#{investigatorCard.cardID}): 'Dime a quién selecciono...' (Haz clic en otra carta)");
    }

    public void ProcessInvestigationTarget(CardLogic targetCard)
    {
        UseAP(1);

        if (currentInvestigator != null)
        {
            currentInvestigator.isUsed = true;
        }

        RoleType roleAction = currentInvestigator.disguisedRole;
        bool amILying = currentInvestigator.CheckIfCorrupted();

        switch (roleAction)
        {
            case RoleType.Investigator:
                bool targetIsDemon = (targetCard.realRole == RoleType.Imp || IsHighRankDemon(targetCard.realRole));
                string veredicto = "";

                if (amILying) veredicto = targetIsDemon ? "HUMANO" : "DEMONIO";
                else veredicto = targetIsDemon ? "DEMONIO" : "HUMANO";

                LogInfo($"Investigator: 'He interrogado a #{targetCard.cardID}... es {veredicto}.'");
                break;

            case RoleType.Diplomat:

                bool targetSideIsDemon = (targetCard.realRole == RoleType.Imp || IsHighRankDemon(targetCard.realRole));
                bool sameTeam = !targetSideIsDemon; // Si target NO es demonio, somos del mismo equipo (Villagers)

                if (amILying) sameTeam = !sameTeam; // Miente sobre el resultado

                string msgDiplomat = sameTeam ? "SOMOS ALIADOS" : "SOMOS ENEMIGOS";
                LogInfo($"Diplomat: 'He analizado a #{targetCard.cardID}... {msgDiplomat}.'");
                break;

            case RoleType.Hunter:
                HandleHunterShot(targetCard, amILying);
                break;
        }

        isInvestigating = false;
        currentInvestigator = null;
    }

    void HandleHunterShot(CardLogic target, bool isFakeHunter)
    {
        if (isFakeHunter)
        {
            List<CardLogic> victims = new List<CardLogic>();
            foreach (var c in GetAllCards())
            {
                if (!IsHighRankDemon(c.realRole) && c.realRole != RoleType.Imp && !c.isDead)
                    victims.Add(c);
            }

            if (victims.Count > 0)
            {
                CardLogic randomVictim = victims[Random.Range(0, victims.Count)];
                LogInfo($"Hunter: '¡Ups! Se me escapó el tiro...'");
                randomVictim.ExecuteThisCard();
            }
        }
        else
        {
            bool targetIsDemon = (target.realRole == RoleType.Imp || IsHighRankDemon(target.realRole));

            if (targetIsDemon)
            {
                LogInfo($"Hunter: '¡Tiro certero! #{target.cardID} era un demonio.'");
                target.ExecuteThisCard();
                ToggleExecutionMode();
            }
            else
            {
                LogInfo($"Hunter: '¡He disparado a un inocente! (#{target.cardID}). Me siento fatal...'");
                target.ExecuteThisCard();
                ToggleExecutionMode();
                playerLives -= 1;
                playerLives -= 1;
                UpdateUI();

                // El Hunter muere mañana
                currentInvestigator.markedForDeath = true;
            }
        }
    }

    public int CountSpecificRoleAlive(RoleType role)
    {
        CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);
        int count = 0;
        foreach (CardLogic c in allCards)
        {
            if (c.realRole == role && !c.isDead) count++;
        }
        return count;
    }

    public int CountRealDemons()
    {
        CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);
        int count = 0;
        foreach (CardLogic card in allCards)
        {
            if (card.realRole == RoleType.Imp  || IsHighRankDemon(card.realRole)) count++;
        }
        return count;
    }

    public int GetCircularID(int currentID, int offset)
    {
        int zeroBasedIndex = currentID - 1;

        int targetIndex = zeroBasedIndex + offset;

        int wrappedIndex = (targetIndex % totalCardsInGame + totalCardsInGame) % totalCardsInGame;

        return wrappedIndex + 1;
    }

}

public enum RoleType
{
    // Villagers
    Healer,
    Scribe,
    Investigator,
    Queen,
    Medium,
    Monk,
    Gravekeeper,
    Hunter,
    Diplomat,
    Torchbearer,

    // Demons
    Imp,
    Lucifer,
    Mammon,
    Asmodeus
}