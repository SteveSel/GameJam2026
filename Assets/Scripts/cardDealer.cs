using UnityEngine;
using System.Collections.Generic;

public class CardDealer : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject prefabCarta;      
    public Transform contenedorCartas; 

    [Header("Forma de la Elipse")]
    public float radioX = 500f; // Aumenta esto para separar horizontalmente
    public float radioY = 150f; // Altura del arco
    public Vector2 centroCirculo = new Vector2(0, -100); 

    [Header("Distribución (Ángulos)")]
    // 180 a 0 crea un arco superior completo de Izquierda a Derecha
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

        for (int i = 0; i < totalCartas; i++)
        {
            // 1. Crear y Configurar
            RoleVisualData data = rolesDisponibles[Random.Range(0, rolesDisponibles.Count)];
            GameObject nuevaCarta = Instantiate(prefabCarta, contenedorCartas);
            
            CardLogic logic = nuevaCarta.GetComponent<CardLogic>();
            if (logic != null) logic.SetupCard(data.role);

            // 2. MATEMÁTICA ELÍPTICA
            float t = (float)i / (totalCartas - 1); // Distribuye de principio a fin
            float anguloRad = Mathf.Lerp(anguloInicio, anguloFin, t) * Mathf.Deg2Rad;

            // Usamos radioX para el ancho y radioY para el alto
            float x = Mathf.Cos(anguloRad) * radioX + centroCirculo.x;
            float y = Mathf.Sin(anguloRad) * radioY + centroCirculo.y;

            nuevaCarta.transform.localPosition = new Vector3(x, y, 0);
            
            // Sin rotación
            nuevaCarta.transform.localRotation = Quaternion.identity;
            
            nuevaCarta.name = $"Carta_{i}";
        }
    }
}