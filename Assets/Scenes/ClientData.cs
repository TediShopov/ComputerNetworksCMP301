using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{
    static public int PlayerHash { get; set; }

    static public bool IsClientInitiator { get; set; } = false;

   static private bool _pause;
 
    static public bool Pause
    {
        get { return _pause; }
        set {
           
            _pause = value;
        }
    }

    static public bool CharacterIndex { get; set; }

    static public bool SoloPlay = false;

    static public bool TwoWayConnectionEstablished() {

        if (SocketComunication.Receiver==null || SocketComunication.Sender==null)
        {
            return false;
        }
        return SocketComunication.Receiver.Connected &&
       SocketComunication.Sender.Connected;
    }

    static public readonly KeyCode[] AllowedKeys = { KeyCode.Space, KeyCode.S, KeyCode.A, KeyCode.D };
    static public readonly Dictionary<KeyCode,int> AllowedKeysIndex = new Dictionary<KeyCode, int>
        { {KeyCode.Space, 0 },
        {KeyCode.S, 1 }, 
        {KeyCode.A, 2 },
        {KeyCode.D, 3 } };


}
