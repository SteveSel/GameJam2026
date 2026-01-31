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
    public int currentAP = 3;
    public int maxAP = 3;
    public int playerLives = 3;
    public int totalCardsInGame = 9;

    [Header("UI References")]
    public TextMeshProUGUI apText;
    public TextMeshProUGUI livesText;

    [Header("UI Textos")]
    public TextMeshProUGUI infoLog;

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
        UpdateUI();
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
    
    void UpdateUI()
    {
        if (apText != null) apText.text = "AP: " + currentAP;
        if (livesText != null) livesText.text = "Vidas: " + playerLives;
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
