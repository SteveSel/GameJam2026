using UnityEngine;
using UnityEngine.UI;   
using System.Collections.Generic;
using TMPro;

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
    public int currentAP = 100;
    public int maxAP = 100;
    public int playerLives = 3;
    public int totalCardsInGame = 8;

    public bool isExecutionMode = false;

    [Header("UI References")]
    public TextMeshProUGUI apText;
    public TextMeshProUGUI livesText;

    public Image executionButtonImage;

    [Header("UI Textos")]
    public TextMeshProUGUI infoLog;

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
    }


    // Función para el botón de matar
    public void ToggleExecutionMode()
    {
        isExecutionMode = !isExecutionMode;

        if (executionButtonImage != null)
        {
            executionButtonImage.color = isExecutionMode ? Color.red : Color.white;
        }

        if(isExecutionMode)
        {
            LogInfo("Execution mode active");
        }
        else
        {
            LogInfo("Invest mode active");
        }
    }

    // Función para buscar la imagen según el rol
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
        currentAP -= amount;
        if (currentAP <= 0)
        {
            currentAP = 0;
            Debug.Log("¡SE ACABÓ EL DÍA! -> Pasando a fase Noche...");
            // EndDay()
        }
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (apText != null) apText.text = $"AP: {currentAP} / {maxAP}";
        if (livesText != null) livesText.text = $"HP: {playerLives}";

        if (playerLives <= 0)
        {
            LogInfo("GAME OVER");
            // Defeat Screen
        }
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

public enum RoleType
{
    Imp,
    Healer,
    Scribe,
    Investigator
}