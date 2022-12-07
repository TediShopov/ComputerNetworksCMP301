using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachDiscardedInputToBuffer : MonoBehaviour
{

    [SerializeField]
    public FighterController Fighter;
    [SerializeField]
    public InputBuffer RollbackBuffer;
    // Start is called before the first frame update
    void Start()
    {
        Fighter.OnInputProcessed += 
            (InputFrame f) => { RollbackBuffer.AddNewFrame(f); };
    }
}
