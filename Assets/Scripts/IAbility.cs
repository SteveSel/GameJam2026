public interface IAbility
{
    // Qué pasa cuando el jugador usa la habilidad (Día)
    void ExecuteActiveAbility(Card target = null);
    
    // Qué pasa automáticamente (Noche)
    void ExecuteNightAbility();
}