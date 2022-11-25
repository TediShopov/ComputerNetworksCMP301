using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{
    static public ClientData Instance { get; set; }
    public int PlayerHash { get; set; }

    public bool IsClientInitiator { get; set; }
    public bool IsPaused { get; set; }
    public bool TwoWayConnectionEstablished() {
        return Listener.Instance.IsReceiverConnected() &&
Sender.Instance.IsSenderConnected();
    }
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        IsPaused = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        IsClientInitiator = false;
        PlayerHash = Random.Range(0, 65550);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            IsPaused = !IsPaused;
            Debug.LogError($"Paused = {IsPaused}");
        }
    }


}
