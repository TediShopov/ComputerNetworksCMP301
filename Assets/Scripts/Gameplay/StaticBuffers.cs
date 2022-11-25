using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class StaticBuffers : MonoBehaviour
{
    [SerializeField]
     public InputBuffer PlayerBuffer;
    [SerializeField]
     public InputBuffer EnemyBuffer;

    public static StaticBuffers Instance;
    void Start()
    {
        if (ClientData.Instance.IsClientInitiator)
        {
            Sender.Instance.toggleSend = false;
            Listener.Instance.toggleReceiving = true;


        }
        else
        {
            Sender.Instance.toggleSend = true;
            Listener.Instance.toggleReceiving = true;
        }

        Instance = this;
    }

}

