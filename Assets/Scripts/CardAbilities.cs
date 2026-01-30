using UnityEngine;
using System.Collections.Generic;

public class CardAbilities : MonoBehaviour
{
    private Card _card;

    void Awake() => _card = GetComponent<Card>();

    // Esta función la llamará el GameManager de tu amigo cuando gastes 1 AP
    public void UseAbility(Card target = null)
    {
        // Si la carta es un Demonio o está corrompida, miente
        bool lies = _card.role.side == RoleData.Side.Demon || _card.isCorrupted;

        switch (_card.role.roleName)
        {
            case "Investigator":
                InvestigatorAbility(target, lies);
                break;
            case "Scribe":
                ScribeAbility(lies);
                break;
            case "Healer":
                HealerAbility(lies);
                break;
            case "Medium":
                MediumAbility(lies);
                break;
        }
    }

    // --- LÓGICA DE LAS HABILIDADES ---

    void InvestigatorAbility(Card target, bool lies)
    {
        if (target == null) return;
        bool isDemon = target.role.side == RoleData.Side.Demon;

        if (lies) isDemon = !isDemon; // Si miente, da la info opuesta

        Debug.Log(isDemon ? "Es un Demonio" : "Es un Inocente");
        // Aquí podrías llamar a un UI para mostrar el mensaje
    }

    void ScribeAbility(bool lies)
    {
        int count = 0;
        Card[] allCards = FindObjectsOfType<Card>();
        foreach (var c in allCards) if (!c.isDead && c.role.side == RoleData.Side.Demon) count++;

        if (lies) count += Random.Range(0, 2) == 0 ? 1 : -1;

        Debug.Log("El Escriba dice que hay " + count + " demonios.");
    }

    void HealerAbility(bool lies)
    {
        if (!lies) GameManager.Instance.health += 1;
        else GameManager.Instance.health -= 1;
        
        Debug.Log("Vida actual: " + GameManager.Instance.health);
    }

    void MediumAbility(bool lies)
    {
        // Lógica: 3 cartas, una es demonio. 
        // Si miente: 3 cartas, ninguna es demonio o las 3 lo son.
        List<Card> allCards = new List<Card>(FindObjectsOfType<Card>());
        // (Aquí iría una lógica de filtrado de cartas aleatorias)
        Debug.Log("El Médium te señala 3 cartas...");
    }
}