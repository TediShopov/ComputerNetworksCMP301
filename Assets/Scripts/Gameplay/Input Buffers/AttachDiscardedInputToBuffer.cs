using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachDiscardedInputToBuffer : MonoBehaviour
{

    [SerializeField]
    public InputBuffer CaptureBuffer;
    [SerializeField]
    public InputBuffer TargetBuffer;
    // Start is called before the first frame update
    void Start()
    {
        CaptureBuffer.OnInputFrameDiscarded += (InputFrame f) => { TargetBuffer.AddNewFrame(f); };
    }
}
