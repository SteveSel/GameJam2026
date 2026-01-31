using UnityEngine;
using UnityEngine.UI;   
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

    [Header("UI References")]
    public TextMeshProUGUI apText;
    public TextMeshProUGUI livesText;

    [Header("UI Textos")]
    public TextMeshProUGUI infoLog;

    // Optional
    public Button nextDayButton;


    public void LogInfo(string message)
    {
        infoLog.text = message;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
      
        startDay();
    }

    // Funci�n para buscar la imagen seg�n el rol
    public Sprite GetSpriteForRole(RoleType roleToFind)
    {
        foreach (var data in roleLibrary)
        {
            if (data.role == roleToFind)
            {
                return data.artwork;
            }
        }
        return null;
    }

    public CardLogic GetCardByID(int idToFind)
    {
        CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);

        foreach (CardLogic card in allCards)
        {
            if (card.cardID == idToFind)
            {
                return card;
            }
        }
        return null;
    }

    public void UseAP(int amount)
    {
        if (currentPhase == GamePhase.Night) return ;
        currentAP -= amount;
        if (currentAP <= 0)
        {
            currentAP = 0;
            Debug.Log("Se acabó el día! -> Pasando a fase Noche...");
            EndDay() ;
        }
        UpdateUI();
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
    
    void UpdateUI()
    {
        string phaseText = currentPhase == GamePhase.Day ? "Día" : "Noche";
        if (apText != null) apText.text = $"AP: {currentAP} | {phaseText} {dayCount}";
        if (livesText != null) livesText.text = "Lives: " + playerLives;
        
    }

    public int CountRealDemons()
    {
        CardLogic[] allCards = FindObjectsByType<CardLogic>(FindObjectsSortMode.None);
        int count = 0;

        foreach (CardLogic card in allCards)
        {
            if (card.realRole == RoleType.Imp)
            {
                count++;
            }
        }
        return count;
    }
}