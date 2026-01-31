using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardDealer : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public GameObject prefabCarta;
    public Transform contenedorCartas; 

    [Header("Configuración de Nivel (Mazo)")]
    public int totalCartas = 9;
    [Tooltip("Roles que SIEMPRE aparecerán")]
    public List<RoleType> rolesObligatorios; // Antes 'specialRoles'
    [Tooltip("Roles para rellenar los huecos que falten")]
    public List<RoleType> rolesRelleno;      // Antes 'fillerRoles'

    [Header("Configuración Visual")]
    [Range(0.1f, 1.5f)] public float escalaCarta = 0.4f; 
    
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
        if (rolesRelleno.Count == 0) 
        {
            Debug.LogError("CardDealer: No has asignado 'Roles de Relleno' en el Inspector.");
            return;
        }

        foreach (Transform child in contenedorCartas) Destroy(child.gameObject);

        List<RoleType> mazo = new List<RoleType>();

        mazo.AddRange(rolesObligatorios);

        int huecosFaltantes = totalCartas - mazo.Count;
        for (int i = 0; i < huecosFaltantes; i++)
        {
            RoleType rolRandom = rolesRelleno[Random.Range(0, rolesRelleno.Count)];
            mazo.Add(rolRandom);
        }

        Barajar(mazo);

        GameManager.Instance.totalCardsInGame = mazo.Count;

        for (int i = 0; i < mazo.Count; i++)
        {
            // Crear objeto
            GameObject nuevaCarta = Instantiate(prefabCarta, contenedorCartas);
            
            // Setup Lógica (Usando el rol del mazo barajado)
            CardLogic logic = nuevaCarta.GetComponent<CardLogic>();
            if (logic != null) 
            {
                // Pasamos el Rol Específico y su ID (1 al 9)
                logic.SetupCard(mazo[i], i + 1);
            }

            // C. Posicionamiento Matemático
            bool esCirculoCompleto = Mathf.Abs(anguloInicio - anguloFin) >= 360f;

            float t;
            if (mazo.Count > 1)
            {
                if (esCirculoCompleto)
                {
                    t = (float)i / mazo.Count; 
                }
                else
                {
                    t = (float)i / (mazo.Count - 1); 
                }
            }
            else
            {
                t = 0.5f;
            }

            float anguloRad = Mathf.Lerp(anguloInicio, anguloFin, t) * Mathf.Deg2Rad;

            float x = Mathf.Cos(anguloRad) * radioX + centroCirculo.x;
            float y = Mathf.Sin(anguloRad) * radioY + centroCirculo.y;

            nuevaCarta.transform.localPosition = new Vector3(x, y, 0);
            
            nuevaCarta.transform.localScale = Vector3.one * escalaCarta;
            nuevaCarta.transform.localRotation = Quaternion.identity;
            
            nuevaCarta.name = $"Carta_{i + 1}_{mazo[i]}";
        }

        StartCoroutine(InitDemonsRoutine());

       
    }
    
    IEnumerator InitDemonsRoutine()
            {
                yield return null; // Espera 1 frame
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ApplyStartOfGameDemonEffects();
                }
            }
    
    // Función auxiliar para barajar listas
    void Barajar<T>(List<T> list)
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