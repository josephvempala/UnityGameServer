using Server.client;
using UnityEngine;

internal class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;
    [SerializeField] private int maximumClients;
    [SerializeField] private int port;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this) Destroy(this);
        Application.targetFrameRate = 999;
    }

    private void Start()
    {
        Server.Server.Start(maximumClients, port);
        Debug.Log("Server Started");
    }

    private void OnApplicationQuit()
    {
        Server.Server.Stop();
        Debug.Log("Server Stopped");
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0,10,0), Quaternion.identity).GetComponent<Player>();
    }
}