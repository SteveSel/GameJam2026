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

    public bool isExecutionMode = false;
    private bool isTransitioning = false;

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
        if (infoLog != null) infoLog.text = "> " + message;

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

        // --- FASE 1: IR A DORMIR ---
        LogInfo("Cae la noche..."); // Esto mostrará el texto grande también
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

        // --- FASE 3: NOCHE (5 Segundos) ---
        // Aquí podrías poner un mensaje de "Turno de los Demonios" si quisieras
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
        currentAP = maxAP; 
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

        if (playerLives <= 0)
        {
            // Puedes crear una función específica de Game Over si quieres
            ShowOnScreenMessage("GAME OVER");
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