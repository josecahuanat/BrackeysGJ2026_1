using UnityEngine;

public class FogPuzzleManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform spawn1; // Inicial
    [SerializeField] private Transform spawn2; // Después de completar rep1
    [SerializeField] private Transform spawn3; // Después de completar rep2
    [SerializeField] private Transform spawn4; // Después de completar rep3
    [SerializeField] private GameObject player;
    
    [Header("Rep Tombs (Auto-populated)")]
    private Tomb[] rep1Tombs;
    private Tomb[] rep2Tombs;
    private Tomb[] rep3Tombs;
    private Tomb[] rep4Tombs;
    
    private int currentLevel = 1; // Repetición actual (1-4)
    
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        // Buscar automáticamente las tumbas de cada rep
        FindTombsInMap();
        
        // Inicializar todas las tumbas (apagarlas)
        InitializeTombs();
        
        // Suscribirse a todos los eventos de las tumbas
        SubscribeToTombs(rep1Tombs);
        SubscribeToTombs(rep2Tombs);
        SubscribeToTombs(rep3Tombs);
        SubscribeToTombs(rep4Tombs);
        
        // Mostrar info de la repetición inicial
        ShowRepetitionInfo();
    }
    
    private void FindTombsInMap()
    {
        GameObject map = GameObject.Find("Map");
        if (map == null)
        {
            Debug.LogError("No se encontró el GameObject 'Map'");
            return;
        }
        
        // Buscar cada rep y obtener sus tumbas
        Transform rep1 = map.transform.Find("rep1");
        Transform rep2 = map.transform.Find("rep2");
        Transform rep3 = map.transform.Find("rep3");
        Transform rep4 = map.transform.Find("rep4");
        
        rep1Tombs = rep1 != null ? rep1.GetComponentsInChildren<Tomb>() : new Tomb[0];
        rep2Tombs = rep2 != null ? rep2.GetComponentsInChildren<Tomb>() : new Tomb[0];
        rep3Tombs = rep3 != null ? rep3.GetComponentsInChildren<Tomb>() : new Tomb[0];
        rep4Tombs = rep4 != null ? rep4.GetComponentsInChildren<Tomb>() : new Tomb[0];
    }
    
    private void InitializeTombs()
    {
        InitializeTombArray(rep1Tombs);
        InitializeTombArray(rep2Tombs);
        InitializeTombArray(rep3Tombs);
        InitializeTombArray(rep4Tombs);
    }
    
    private void InitializeTombArray(Tomb[] tombs)
    {
        if (tombs == null) return;
        
        foreach (Tomb tomb in tombs)
        {
            if (tomb != null)
            {
                tomb.Initialize();
            }
        }
    }
    
    private void SubscribeToTombs(Tomb[] tombs)
    {
        if (tombs == null) return;
        
        foreach (Tomb tomb in tombs)
        {
            if (tomb != null)
            {
                tomb.OnTombInteracted += HandleTombInteraction;
            }
        }
    }
    
    private void HandleTombInteraction()
    {
        CheckLevelComplete();
    }
    
    private void CheckLevelComplete()
    {
        Tomb[] currentLevelTombs = GetCurrentLevelTombs();
        
        if (currentLevelTombs == null || currentLevelTombs.Length == 0)
        {
            return;
        }
        
        // Contar cuántas están encendidas
        int litCount = 0;
        foreach (Tomb tomb in currentLevelTombs)
        {
            if (tomb != null && tomb.IsLit)
            {
                litCount++;
            }
        }
        
        // Si todas están encendidas, avanzar a la siguiente repetición
        if (litCount == currentLevelTombs.Length)
        {
            CompleteLevel();
        }
    }
    
    private Tomb[] GetCurrentLevelTombs()
    {
        switch (currentLevel)
        {
            case 1: return rep1Tombs;
            case 2: return rep2Tombs;
            case 3: return rep3Tombs;
            case 4: return rep4Tombs;
            default: return null;
        }
    }
    
    private void CompleteLevel()
    {
        Debug.Log($"<color=green>¡Repetición {currentLevel} completada!</color>");
        
        // Si completó la repetición 4, terminar el puzzle
        if (currentLevel == 4)
        {
            Debug.Log("<color=cyan>¡¡¡PUZZLE COMPLETADO!!!</color>");
            currentLevel++;
            return;
        }
        
        currentLevel++;
        
        // Teletransportar al siguiente spawn
        Transform nextSpawn = GetNextSpawn();
        if (nextSpawn != null)
        {
            TeleportPlayer(nextSpawn);
            ShowRepetitionInfo();
        }
    }
    
    private Transform GetNextSpawn()
    {
        switch (currentLevel)
        {
            case 2: return spawn2;
            case 3: return spawn3;
            case 4: return spawn4;
            default: return null;
        }
    }
    
    private void ShowRepetitionInfo()
    {
        Tomb[] currentTombs = GetCurrentLevelTombs();
        if (currentTombs != null)
        {
            Debug.Log($"<color=yellow>Repetición {currentLevel} - Enciende {currentTombs.Length} velas</color>");
        }
    }
    
    private void TeleportPlayer(Transform spawnPoint)
    {
        if (player == null || spawnPoint == null) return;
        
        CharacterController controller = player.GetComponent<CharacterController>();
        
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
            controller.enabled = true;
        }
        else
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }
    }
}
