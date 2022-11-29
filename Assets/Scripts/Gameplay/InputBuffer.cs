using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct InputElement 
{
    [FieldOffset(0)]
    public Int32 timeStamp; //4 bytes
    [FieldOffset(4)]
    public KeyCode key;     //4 bytes
}

public enum InputBufferState
{
    InputCollector, InputReceiver
}


public class InputFrame 
{
    public InputElement[] _inputInFrame;
    int DelayInput = 0;
    public InputFrame(KeyCode[] allowedKeys,int delay=0)
    {
        _inputInFrame = new InputElement[allowedKeys.Length];
        this.DelayInput = delay;
        for (int i = 0; i < allowedKeys.Length; i++)
        {
            
            if (Input.GetKey(allowedKeys[i]))
            {
                KeyCode keyCode = allowedKeys[i];
                _inputInFrame[i].key = keyCode;
            }
            _inputInFrame[i].timeStamp = FrameLimiter.FramesInPlay + DelayInput;
        }
    }

    public InputFrame(InputElement[] elements)
    {
        this._inputInFrame = elements;
    }

   

}
public class InputBuffer : MonoBehaviour
{

    //TODO make buffer only accpets keys that do sth in the game
    public KeyCode[] allowedKeysArr;

    // public readonly object bufferLock = new object();
    public InputBufferState bufferState;
    //public uint MaxInputs;
    //private InputElement[] _inputElements;

   
    [SerializeField]
    public int DelayInput;

    public Queue<InputFrame> BufferedInput { get; set; }

    public float DebugOutputTime;

    //private Queue<InputElement> _keyDownBuffer;
    //Refreshed buffer after seconds if new key has not been received the meantime
    //public uint MaxKeyDownInputs;

    public float RefreshBufferAfter;
    // Start is called before the first frame update

    public int LowestFrameStamp  { 
        get { 
            return this.BufferedInput.ToArray()[0]._inputInFrame[0].timeStamp; 
        } }
    public int HighestFrameStamp { get {return LowestFrameStamp + DelayInput; } }


    void Start()
    {
        allowedKeysArr = new KeyCode[] { KeyCode.Space, KeyCode.S, KeyCode.A, KeyCode.D };
        StartCoroutine(DebugPrintAfter());
        StartCoroutine(DeleteBufferAter());
        //_inputElements = new InputElement[5];
        BufferedInput = new Queue<InputFrame>();
        // _keyDownBuffer = new Queue<InputElement>();

        //AddKey(KeyCode.None);
        //if (bufferState == InputBufferState.InputCollector)
        //{
        //    for (int i = 0; i < DelayInput; i++)
        //    {
        //        AddNewFrame();
        //    }
        //}
       



        ////TODO priority is important
       
        //if (bufferState == InputBufferState.InputReceiver)
        //{
        //    //TODO check if event delegate is still active

        //    Listener.Instance.OnReceive += SetBuffer;
        //}
        //Debug.LogError($"InputBuffer {this.GetInstanceID()} is an active object ");

    }

    // Update is called once per frame
    void Update()
    {
        if (ClientData.IsPaused)
        {
            return;
        }
        if (bufferState == InputBufferState.InputCollector)
        {
            AddNewFrame();
       
        }
        else
        {
            AddNewFrame(new InputFrame(NetworkGamePacket.LastReceivedGamePacket.InputElements));
        }

    }

    public InputFrame LastFrame { get; set; }
  

    void AddNewFrame(InputFrame inputFrame=null) 
    {
        if (inputFrame==null)
        {
            inputFrame = new InputFrame(allowedKeysArr,DelayInput);
        }
        if (BufferedInput.Count > DelayInput)
        {
            BufferedInput.Dequeue();
        }
        BufferedInput.Enqueue(inputFrame);
        LastFrame = inputFrame;

    }
    public InputElement[] GetFirstFrame() 
    {
        return this.BufferedInput.Peek()._inputInFrame;
    }
    //public void SetBuffer() 
    //{
    //    var elements = NetworkGamePacket.LastReceivedGamePacket.InputElements;
    //    this._inputElements.Clear();
    //    for (int i = 0; i < elements.Length; i++)
    //    {
    //        var el = elements[i];
    //        this._inputElements.Enqueue(el);
    //    }
    //}

    //public void SetBuffer()
    //{
    //    this._inputElements = NetworkGamePacket.LastReceivedGamePacket.InputElements;
    //}



    IEnumerator DebugPrintAfter()
    {
        while (true)
        {
            yield return new WaitForSeconds(DebugOutputTime);
           // DebugPrint();
        }
     
    }

    IEnumerator DeleteBufferAter()
    {
        yield return new WaitForSeconds(RefreshBufferAfter);
        //while (true)
        //{
        //    yield return new WaitForSeconds(RefreshBufferAfter);
        //    this._keyDownBuffer = new Queue<InputElement>();
        //}

    }

    public void DebugPrint() 
    {
        //string strKeyDownBuff = "Key Down Buffer";
        //foreach (var el in this._keyDownBuffer)
        //{
        //    strKeyDownBuff += el.key.ToString();
        //}

        string strAllInputBuff="All Input Buffer";
        foreach (var inputFrame in this.BufferedInput)
        {
            foreach (var el in inputFrame._inputInFrame)
            {
                strAllInputBuff += el.key.ToString();
            }
        }
        // Debug.LogError(_inputElements.Count);
        //  Debug.LogError(strKeyDownBuff);
        Debug.LogError(strAllInputBuff);
    }

    //public void AddKeyDown(KeyCode k)
    //{
    //    this._keyDownBuffer.Enqueue(new InputElement { key = k, timeStamp = FrameLimiter.FramesInPlay });
    //    if (_keyDownBuffer.Count > MaxKeyDownInputs)
    //    {
    //        this._keyDownBuffer.Dequeue();
    //    }
    //}
    //public void AddKeyDown(KeyCode k)
    //{
    //    this._keyDownBuffer.Enqueue(new InputElement { key=k, timeStamp=FrameLimiter.FramesInPlay});
    //    if (_keyDownBuffer.Count > MaxKeyDownInputs)
    //    {
    //        this._keyDownBuffer.Dequeue();
    //    }
    //}


    //public void ToggleKey(int index) 
    //{
    //    KeyCode keyCode = allowedKeysArr[index];
    //    _inputElements[index] = (new InputElement { key = keyCode, timeStamp = FrameLimiter.FramesInPlay + DelayInput });
    //        }


    //public InputElement[] GetBuffer()
    //{
    //    return this._inputElements;
    //}

    //public Queue<InputElement> GetBuffer() {
    //        return this._inputElements;
    // }
    //public void SetBuffer(Queue<InputElement> b) {
    //        this._inputElements = b;
    //}

    //public Queue<InputElement> GetKeyDownBuffer() { return this._keyDownBuffer; }

}
