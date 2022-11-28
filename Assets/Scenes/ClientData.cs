using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{
    static public int PlayerHash { get; set; }

    static public bool IsClientInitiator { get; set; } = false;
    static public bool IsPaused { get; set; }
    static public bool TwoWayConnectionEstablished() {
        return Listener.Instance.IsReceiverConnected() &&
        Sender.Instance.IsSenderConnected();
    }
    //void Awake()
    //{
    //    DontDestroyOnLoad(this.gameObject);
    //    IsPaused = false;
    //}
    //// Start is called before the first frame update
    //void Start()
    //{
    //    IsClientInitiator = false;
    //    PlayerHash = Random.Range(0, 65550);

    //}

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.P))
    //    {
    //        IsPaused = !IsPaused;
    //        Debug.LogError($"Paused = {IsPaused}");
    //    }
    //}


}
