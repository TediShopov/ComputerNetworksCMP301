using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Size = 9)]
public struct InputElement 
{
    [FieldOffset(0)]
    public bool isProcessed; //1 byte
    [FieldOffset(1)]
    public Int32 timeStamp; //4 bytes
    [FieldOffset(5)]
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

    public float RefreshBufferAfter;
    // Start is called before the first frame update



    void Start()
    {
       
        _inputElements = new Queue<InputElement>();
       
        _keyDownBuffer = new Queue<InputElement>();
        for (int i = 0; i < 10; i++)
        {
            AddKey(KeyCode.None);
        }
       

      
        //TODO priority is important
        allowedKeysArr = new KeyCode[] { KeyCode.Space, KeyCode.S, KeyCode.A,  KeyCode.D  };
        StartCoroutine(DebugPrintAfter());
        StartCoroutine(DeleteBufferAter());
        if (bufferState == InputBufferState.InputReceiver)
        {
            //TODO check if event delegate is still active

             Listener.Instance.OnReceive += SetBuffer;
        }
        Debug.LogError($"InputBuffer {this.GetInstanceID()} is an active object ");

    }

    // Update is called once per frame
    void Update()
    {
        
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
           
    }
    public void SetBuffer() 
    {
        //lock (bufferLock)
        // {
        var elements = Listener.Instance.ReceivedBuffer.InputElements;
        //int receivedHash= Listener.Instance.ReceivedBuffer.portNum;
        //if (receivedHash == ClientData.Instance.PlayerHash)
        //{
        //    return;
        //}
        this._inputElements.Clear();
        for (int i = 0; i < elements.Length; i++)
        {
            var el = elements[i];
            el.isProcessed = false;
            this._inputElements.Enqueue(el);
        }

        //DebugPrint();
        //  }

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
        this._keyDownBuffer.Enqueue(new InputElement { key=k, timeStamp=FrameLimiter.FramesInPlay, isProcessed=false });
        if (_keyDownBuffer.Count > MaxKeyDownInputs)
        {
            this._keyDownBuffer.Dequeue();
        }
    }



    public void AddKey(KeyCode k) 
    {

        //lock (bufferLock)
        //{
            this._inputElements.Enqueue(new InputElement { key = k, timeStamp = FrameLimiter.FramesInPlay, isProcessed = false });
            if (_inputElements.Count > MaxInputs)
            {
                this._inputElements.Dequeue();
            }
       // }
       
    }

    public Queue<InputElement> GetBuffer() {
       // lock (bufferLock)
       // {
            return this._inputElements;
        //}
     }
    public void SetBuffer(Queue<InputElement> b) {
       // lock (bufferLock)
       // {
            this._inputElements = b;
        //}
    
    }

    public Queue<InputElement> GetKeyDownBuffer() { return this._keyDownBuffer; }

}
