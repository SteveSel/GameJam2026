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
        isTransitioning = true;
        LogInfo("Cae la noche...");

        if (fadePanel != null) fadePanel.blocksRaycasts = true;

        // 1. FADE OUT (Pantalla negra)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; 
        }
        if (fadePanel != null) fadePanel.alpha = 1f;

        // 2. CAMBIO DE ESTADO EN EL ANIMATOR (Pantalla en negro)
        yield return new WaitForSeconds(0.2f); 
        
        // <--- AQUÍ ESTÁ EL CAMBIO PRINCIPAL --->
        if (backgroundAnimator != null)
        {
            // Activamos el booleano que configuramos en el paso 3
            backgroundAnimator.SetBool("esNoche", true);
        }
        else
        {
            Debug.LogWarning("ERROR: No has asignado el 'Background Animator' en el Inspector del GameManager.");
        }
        // <------------------------------------->

        // 3. FADE IN (Vuelve la imagen y ya estará reproduciéndose la animación)
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
        LogInfo("Es de noche. Ten cuidado.");
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