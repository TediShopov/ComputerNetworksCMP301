using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class StaticBuffers : MonoBehaviour
{
    [SerializeField]
    public GameObject Player;
    [SerializeField]
    public GameObject Enemy;

    [SerializeField]
    public GameObject PlayerRB;
    [SerializeField]
    public GameObject EnemyRB;

    private InputBuffer _serializedPlayerBuffer=null;
     public InputBuffer PlayerBuffer { get 
        {
            return _serializedPlayerBuffer; 
        } }
    private InputBuffer _serializedEnemyBuffer=null;

    //Do not use unity API here
    public InputBuffer EnemyBuffer { get 
        {
            return _serializedEnemyBuffer;
        }
    }

    public void RenewBuffers() 
    {
        _serializedPlayerBuffer = Player.GetComponent<FighterController>().InputBuffer;
        _serializedEnemyBuffer = Enemy.GetComponent<FighterController>().InputBuffer;
    }

    public static StaticBuffers Instance;
    void Awake()
    {
        Instance = this;
        RenewBuffers();
    }

}

