using UnityEngine;

[CreateAssetMenu(fileName = "NuevoRol", menuName = "Juego/Rol")]
public class RoleData : ScriptableObject 
{
    public string roleName;
    public enum Side { Villager, Demon, Neutral }
    public Side side;
    
    [TextArea]
    public string description;

    // Comportamiento base para la deducción
    public bool liesAlways; 
}