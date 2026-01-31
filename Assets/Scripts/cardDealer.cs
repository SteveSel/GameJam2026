using UnityEngine;
using System.Collections.Generic;

public class CardDealer : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject prefabCarta;      
    public Transform contenedorCartas; 

    [Header("Configuración de Tamaño")]
    [Range(0.1f, 1.5f)] public float escalaCarta = 0.4f; // <--- ¡AQUÍ ESTÁ LA SOLUCIÓN!

    [Header("Forma de la Elipse")]
    public float radioX = 500f; 
    public float radioY = 150f; 
    public Vector2 centroCirculo = new Vector2(0, -100); 

    [Header("Distribución (Ángulos)")]
    [Range(0, 360)] public float anguloInicio = 180f; 
    [Range(0, 360)] public float anguloFin = 0f;

    void Start()
    {
        RepartirCartas();
    }

    public void RepartirCartas()
    {
        if (GameManager.Instance == null) return;
        List<RoleVisualData> rolesDisponibles = GameManager.Instance.roleLibrary;
        if (rolesDisponibles.Count == 0) return;

        // Limpiar cartas previas
        foreach (Transform child in contenedorCartas) Destroy(child.gameObject);

        int totalCartas = 9;
        GameManager.Instance.totalCardsInGame = totalCartas; 

        for (int i = 0; i < totalCartas; i++)
        {
            // 1. Crear
            RoleVisualData data = rolesDisponibles[Random.Range(0, rolesDisponibles.Count)];
            GameObject nuevaCarta = Instantiate(prefabCarta, contenedorCartas);
            
            // 2. Setup Lógica
            CardLogic logic = nuevaCarta.GetComponent<CardLogic>();
            if (logic != null) logic.SetupCard(data.role, i + 1);

            // 3. Posición
            float t = (float)i / (totalCartas - 1); 
            float anguloRad = Mathf.Lerp(anguloInicio, anguloFin, t) * Mathf.Deg2Rad;

            float x = Mathf.Cos(anguloRad) * radioX + centroCirculo.x;
            float y = Mathf.Sin(anguloRad) * radioY + centroCirculo.y;

            nuevaCarta.transform.localPosition = new Vector3(x, y, 0);
            
            // 4. ESCALA Y ROTACIÓN (Aquí aplicamos el cambio)
            nuevaCarta.transform.localScale = Vector3.one * escalaCarta; // Multiplica (1,1,1) por tu valor (ej. 0.4)
            nuevaCarta.transform.localRotation = Quaternion.identity;
            
            nuevaCarta.name = $"Carta_{i + 1}";
        }
    }
}