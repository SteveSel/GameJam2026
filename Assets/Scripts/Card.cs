using UnityEngine;

public class Card : MonoBehaviour 
{
    public RoleData role; 
    public bool isRevealed = false;
    public bool isDead = false;

    // Estados alterados por habilidades de demonios
    public bool isCorrupted = false; // Beelzebub
    public bool isLocked = false;    // Belphegor

    private void OnMouseDown() 
    {
        // Solo interactuamos si la carta no ha sido ejecutada
        if (!isDead && GameManager.Instance != null) 
        {
            GameManager.Instance.HandleCardClick(this);
        }
    }

    public void Die() 
    {
        isDead = true;
        Debug.Log(role.roleName + " ha sido eliminado por el Juez.");
        // Desactivamos el objeto para representar la muerte
        gameObject.SetActive(false); 
    }
}