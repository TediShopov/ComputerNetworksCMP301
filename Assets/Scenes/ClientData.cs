using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{
    static public int PlayerHash { get; set; }

    static public bool IsClientInitiator { get; set; } = false;

   static private bool _pause;

   static  public bool Pause
    {
        get { return _pause; }
        set {
           
            _pause = value;
        }
    }


    static public bool TwoWayConnectionEstablished() {
        return SocketComunication.Receiver.Connected &&
       SocketComunication.Sender.Connected;
    }
   

}
