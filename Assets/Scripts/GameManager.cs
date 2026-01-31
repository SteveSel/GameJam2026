using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necesario para IEnumerator
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
    public int maxLives = 3 ;
    public int totalCardsInGame = 9;

    public bool isExecutionMode = false;
    private bool isTransitioning = false; // Para evitar clics durante el cambio

    [Header("UI References")]
    public TextMeshProUGUI apText;
    public TextMeshProUGUI livesText;
    public Image executionButtonImage;
    public TextMeshProUGUI infoLog;

    [Header("Transición Día/Noche")]
    [Header("Transición Día/Noche")]
    public CanvasGroup fadePanel;
    public Animator backgroundAnimator;
    public float fadeDuration = 1.0f;

    public void LogInfo(string message)
    {
        infoLog.text += "\n> " + message;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
        // Aseguramos que el panel empieza transparente
        if (fadePanel != null) 
        {
            fadePanel.alpha = 0;
            fadePanel.blocksRaycasts = false;
        }
    }

    public void ToggleExecutionMode()
    {
        if (isTransitioning) return; // No permitir cambios durante la transición

        isExecutionMode = !isExecutionMode;

        if (executionButtonImage != null)
        {
            executionButtonImage.color = isExecutionMode ? Color.red : Color.white;
        }

        if(isExecutionMode) LogInfo("Execution mode active");
        else LogInfo("Investigation mode active");
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
            Debug.Log("¡SE ACABÓ EL DÍA! -> Iniciando transición a Noche Animada...");
            StartCoroutine(TransitionToNightRoutine());
        }
    }

    
    
    IEnumerator TransitionToNightRoutine()
    {
        // BLOQUEO TOTAL AL JUGADOR
        isTransitioning = true;
        if (fadePanel != null) fadePanel.blocksRaycasts = true; 

        LogInfo("Cae la noche...");

        // ---------------------------------------------
        // FASE 1: IR A DORMIR (Día -> Negro)
        // ---------------------------------------------
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; 
        }
        if (fadePanel != null) fadePanel.alpha = 1f;

        // CAMBIO VISUAL A NOCHE
        if (backgroundAnimator != null) backgroundAnimator.SetBool("esNoche", true);
        
        yield return new WaitForSeconds(0.5f); // Pequeña pausa en negro total

        // ---------------------------------------------
        // FASE 2: MOSTRAR LA NOCHE (Negro -> Noche visible)
        // ---------------------------------------------
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        // ---------------------------------------------
        // FASE 3: ESPERA DE 5 SEGUNDOS (El jugador mira, no toca)
        // ---------------------------------------------
        LogInfo("Es de noche... Los demonios acechan.");
        yield return new WaitForSeconds(5.0f); 

        // ---------------------------------------------
        // FASE 4: AMANECER (Noche -> Negro)
        // ---------------------------------------------
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        if (fadePanel != null) fadePanel.alpha = 1f;

        // CAMBIO VISUAL A DÍA y RESET DE JUEGO
        if (backgroundAnimator != null) backgroundAnimator.SetBool("esNoche", false);
        
        // ¡IMPORTANTE! RECUPERAR LOS PUNTOS DE ACCIÓN (AP)
        currentAP = maxAP; 
        UpdateUI();
        LogInfo("Amanece un nuevo día.");

        yield return new WaitForSeconds(0.5f); // Pequeña pausa en negro total

        // ---------------------------------------------
        // FASE 5: VOLVER A JUGAR (Negro -> Día visible)
        // ---------------------------------------------
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        
        // DESBLOQUEO DEL JUGADOR
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

        if (playerLives <= 0)
        {
            LogInfo("GAME OVER");
        }
    }

    public int CountRealDemons()
    {
        CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);
        int count = 0;
        foreach (CardLogic card in allCards)
        {
            if (card.realRole == RoleType.Imp) count++;
        }
        return count;
    }
}

public enum RoleType
{
    Imp,
    Healer,
    Scribe,
    Investigator
}