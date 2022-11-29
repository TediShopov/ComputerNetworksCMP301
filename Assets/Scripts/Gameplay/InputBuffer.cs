using System;
using System.Collections;
using System.Collections.Generic;
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
public class InputBuffer : MonoBehaviour
{

    //TODO make buffer only accpets keys that do sth in the game
    public KeyCode[] allowedKeysArr;

    // public readonly object bufferLock = new object();
    public InputBufferState bufferState;
    public uint MaxInputs;

    public float DebugOutputTime;
    private Queue<InputElement> _inputElements;

    private Queue<InputElement> _keyDownBuffer;
    //Refreshed buffer after seconds if new key has not been received the meantime
    public uint MaxKeyDownInputs;
    public int DelayInput;

    public float RefreshBufferAfter;
    // Start is called before the first frame update

    public int LowestFrameStamp  { get { return this._inputElements.ToArray()[0].timeStamp; } }
    public int HighestFrameStamp { get { return this._inputElements.ToArray()[_inputElements.Count-1].timeStamp; } }

    void Start()
    {
       
        _inputElements = new Queue<InputElement>();
       
        _keyDownBuffer = new Queue<InputElement>();
        for (int i = 0; i < 10; i++)
        {
            AddKey(KeyCode.None);
        }



        ////TODO priority is important
        allowedKeysArr = new KeyCode[] { KeyCode.Space, KeyCode.S, KeyCode.A, KeyCode.D };
        StartCoroutine(DebugPrintAfter());
        StartCoroutine(DeleteBufferAter());
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

            bool _noAllowedKeyPressed = true;
            foreach (var el in allowedKeysArr)
            {
                if (Input.GetKeyDown(el))
                {
                    AddKeyDown(el);
                }
                if (Input.GetKey(el))
                {
                    AddKey(el);
                    _noAllowedKeyPressed = false;
                }
            }


            if (_noAllowedKeyPressed)
            {
                AddKey(KeyCode.None);
            }

        }
        else
        {
            SetBuffer();
        }

    }
    public void SetBuffer() 
    {
        var elements = NetworkGamePacket.LastReceivedGamePacket.InputElements;
        this._inputElements.Clear();
        for (int i = 0; i < elements.Length; i++)
        {
            var el = elements[i];
            this._inputElements.Enqueue(el);
        }
    }

   

    IEnumerator DebugPrintAfter()
    {
        while (true)
        {
            yield return new WaitForSeconds(DebugOutputTime);
            //DebugPrint();
        }
     
    }

    IEnumerator DeleteBufferAter()
    {
        while (true)
        {
            yield return new WaitForSeconds(RefreshBufferAfter);
            this._keyDownBuffer = new Queue<InputElement>();
        }

    }

    public void DebugPrint() 
    {
        string strKeyDownBuff = "Key Down Buffer";
        foreach (var el in this._keyDownBuffer)
        {
            strKeyDownBuff += el.key.ToString();
        }

        string strAllInputBuff="All Input Buffer";
        foreach (var el in this._inputElements)
        {
            strAllInputBuff += el.key.ToString();
        }
        // Debug.LogError(_inputElements.Count);
        //  Debug.LogError(strKeyDownBuff);
        Debug.LogError(strAllInputBuff);
    }

    public void AddKeyDown(KeyCode k)
    {
        this._keyDownBuffer.Enqueue(new InputElement { key=k, timeStamp=FrameLimiter.FramesInPlay});
        if (_keyDownBuffer.Count > MaxKeyDownInputs)
        {
            this._keyDownBuffer.Dequeue();
        }
    }



    public void AddKey(KeyCode k) 
    {
            this._inputElements.Enqueue(new InputElement { key = k, timeStamp = FrameLimiter.FramesInPlay+ DelayInput });
            if (_inputElements.Count > MaxInputs)
            {
                this._inputElements.Dequeue();
            }
      
    }

    public Queue<InputElement> GetBuffer() {
            return this._inputElements;
     }
    public void SetBuffer(Queue<InputElement> b) {
            this._inputElements = b;
    }

    public Queue<InputElement> GetKeyDownBuffer() { return this._keyDownBuffer; }

}
