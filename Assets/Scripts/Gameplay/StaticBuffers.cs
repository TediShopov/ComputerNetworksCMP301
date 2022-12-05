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


        Instance = this;
    }

}

