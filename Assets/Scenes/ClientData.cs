using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{
    static public int PlayerHash { get; set; }

    static public bool IsClientInitiator { get; set; } = false;
    static public bool IsPaused { get; set; }
    static public bool TwoWayConnectionEstablished() {
        return SocketComunication.Receiver.Connected &&
       SocketComunication.Sender.Connected;
    }
   

}
