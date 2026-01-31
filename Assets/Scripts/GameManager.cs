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
    public CanvasGroup fadePanel;          // Arrastra aquí el panel negro con CanvasGroup
    public SpriteRenderer backgroundObject; // Arrastra aquí el objeto del fondo
    public Sprite nightSprite;             // Arrastra aquí la imagen 'nightTimescene'
    public float fadeDuration = 1.0f;      // Tiempo que tarda en oscurecerse

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
        UpdateUI(); // Actualizamos texto antes de chequear el 0

        if (currentAP <= 0)
        {
            currentAP = 0;
            Debug.Log("¡SE ACABÓ EL DÍA! -> Iniciando transición a Noche...");
            
            // INICIAMOS LA TRANSICIÓN
            StartCoroutine(TransitionToNightRoutine());
        }
    }

    public void startDay()
    {
        currentPhase = GamePhase.Day;
        currentAP = maxAP;

        if (nextDayButton != null)
            nextDayButton.gameObject.SetActive(false);

        LogInfo("¡Comienza el día " + dayCount + "! Tienes " + currentAP + " AP.");
        UpdateUI();
    }

    public void EndDay()
    {
        if (currentPhase == GamePhase.Night) return ;
        currentPhase = GamePhase.Night;


        if (nextDayButton != null)
            nextDayButton.gameObject.SetActive(true);

        LogInfo("¡Ha terminado el día " + dayCount + "! Prepárate para la noche.");
        UpdateUI();
    }

    public void OnNextDayButtonPressed()
    {
        dayCount++;
        startDay();
    }
    
    // --- NUEVA RUTINA DE TRANSICIÓN ---
    IEnumerator TransitionToNightRoutine()
    {
        isTransitioning = true;
        LogInfo("Cae la noche...");

        // 1. Bloqueamos interacciones
        if (fadePanel != null) fadePanel.blocksRaycasts = true;

        // 2. FADE OUT (Pantalla se vuelve negra)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; // Esperar al siguiente frame
        }
        if (fadePanel != null) fadePanel.alpha = 1f;

        // 3. CAMBIO DE FONDO (En la oscuridad total)
        yield return new WaitForSeconds(0.5f); // Pequeña pausa en negro
        
        if (backgroundObject != null && nightSprite != null)
        {
            backgroundObject.sprite = nightSprite;
        }
        else
        {
            Debug.LogWarning("Falta asignar el BackgroundObject o el NightSprite en el Inspector del GameManager");
        }

        // Aquí podrías añadir lógica extra de la fase de noche (resetear AP, turno enemigos, etc.)
        // ResetAPForNight(); 

        // 4. FADE IN (Vuelve la imagen con el fondo nuevo)
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
            fadePanel.blocksRaycasts = false; // Desbloqueamos interacciones
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