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
            if (_serializedPlayerBuffer==null)
            {
                Debug.LogError($"Init {_serializedPlayerBuffer}");
                _serializedPlayerBuffer = Player.GetComponent<FighterController>().InputBuffer;
            }
            return _serializedPlayerBuffer; 
        } }
    private InputBuffer _serializedEnemyBuffer=null;

    public InputBuffer EnemyBuffer { get 
        {
            if (_serializedEnemyBuffer == null)
            {
                Debug.LogError($"Init {_serializedEnemyBuffer}");
                _serializedEnemyBuffer = Enemy.GetComponent<FighterController>().InputBuffer;

            }
            return _serializedEnemyBuffer;

        }
    }

    public static StaticBuffers Instance;
    void Start()
    {
        Instance = this;
    }

}

