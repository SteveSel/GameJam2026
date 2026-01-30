using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static GameManager Instance;

    public int health = 3;
    public int dayInteractions = 2; // Puntos de investigación por día
    
    public enum GamePhase { Day, Night }
    public GamePhase currentPhase = GamePhase.Day;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
    }

    public void HandleCardClick(Card card) 
    {
        if (currentPhase == GamePhase.Day) 
        {
            // En este prototipo, al hacer clic "ejecutamos" la sentencia
            ExecuteSentence(card);
        }
    }

    void ExecuteSentence(Card card) 
    {
        // Lógica de daño según tu diseño de la Game Jam
        if (card.role.side == RoleData.Side.Villager) health -= 2;
        else if (card.role.side == RoleData.Side.Neutral) health -= 1;
        
        card.Die();
        CheckGameState();
    }

    void CheckGameState() 
    {
        Debug.Log("Vidas del Juez: " + health);
        if (health <= 0) 
        {
            Debug.Log("GAME OVER: El Juez ha sucumbido.");
            // Aquí llamarías a tu script MainMenu para volver al inicio
        }
    }
}