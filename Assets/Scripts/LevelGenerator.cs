using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public enum RoleType
{
    Imp,      
    Lucifer,  
    Mammon,
    Healer,   
    Investigator, 
    Jester,
    Scribe,
    Queen
}

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuración Visual")]
    public GameObject cardPrefab;
    public Transform gridContainer;

    [Header("Nivel Actual")]
    public int totalCards = 9;
    public List<RoleType> specialRoles;

    [Tooltip("Filler roles")]
    public List<RoleType> fillerRoles;

    void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        List<RoleType> deck = new List<RoleType>();

        deck.AddRange(specialRoles);

        int remainingSlots = totalCards - deck.Count;

        for (int i = 0; i < remainingSlots; i++)
        {
            if (fillerRoles.Count > 0)
            {
                RoleType randomVillager = fillerRoles[Random.Range(0, fillerRoles.Count)];
                deck.Add(randomVillager);
            }
            else
            {
                Debug.LogError("La lista 'Filler Roles' está vacía en el Inspector");
            }
        }

        Shuffle(deck);

        foreach (RoleType role in deck)
        {
            GameObject newCardObj = Instantiate(cardPrefab, gridContainer);

            CardLogic cardScript = newCardObj.GetComponent<CardLogic>();
            cardScript.SetupCard(role);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

}
