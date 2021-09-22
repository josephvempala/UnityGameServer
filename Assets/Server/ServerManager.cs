using Server.client;
using UnityEngine;

internal class ServerManager : MonoBehaviour
{
    [SerializeField]
    private int maximum_clients;
    [SerializeField]
    private int port;

    public static ServerManager instance;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        Application.targetFrameRate = 999;
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }

    private void Start()
    {
        Server.Server.Start(maximum_clients, port);
        Debug.Log("Server Started");
    }

    private void OnApplicationQuit()
    {
        Server.Server.Stop();
        Debug.Log("Server Stopped");
    }
}