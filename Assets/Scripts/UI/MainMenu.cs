using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu: MonoBehaviour
{
    [SerializeField]
    public TMPro.TMP_Text statusText;
    [SerializeField]
    public TMPro.TMP_Text listeningOnPortText;

    [SerializeField]
    public TMPro.TMP_InputField conntectToPort;


    [SerializeField]
    public TMPro.TMP_InputField listenOnPortField;

    [SerializeField]
    TwoWayConnectionEstablisher twoWayConnection;
     void Update()
    {
        //if (SocketComunication.ConnectionListener!=null && !SocketComunication.ConnectionListener.IsBound )
        //{
        //    ListenOnPort();
        //}

        if (SocketComunication.ConnectionListener.IsBound)
        {
            IPEndPoint ip = SocketComunication.ConnectionListener.LocalEndPoint as IPEndPoint;
            listeningOnPortText.text = $"Listening on {ip.Port}"; 
            listeningOnPortText.color=Color.green;

        }

        string statusString="";
        if (ClientData.TwoWayConnectionEstablished())
        {
            statusString += $"TWO WAY CONNECTION ESTABLISEHD";

            statusText.text = statusString;
            statusText.color = Color.green;
            StartGame();
        }
        if (SocketComunication.Receiver.Connected)
        {
            IPEndPoint remoteIp = SocketComunication.Receiver.RemoteEndPoint as IPEndPoint;
            IPEndPoint localIp = SocketComunication.Receiver.LocalEndPoint as IPEndPoint;

            statusString += $" {remoteIp.Port} -> {localIp.Port}";

        }

        if (SocketComunication.Sender.Connected)
        {

            IPEndPoint remoteIp = SocketComunication.Sender.RemoteEndPoint as IPEndPoint;
            IPEndPoint localIp = SocketComunication.Sender.LocalEndPoint as IPEndPoint;


            statusString += $" {remoteIp.Port} -> {localIp.Port}";

        }
       
    }

    public void StartGame() 
    {
        SceneManager.LoadScene(1);
    }

    
   

    public void TwoWayConnection() 
    {
        if (listeningOnPortText.text!=string.Empty)
        {
            int portNum = 12;
            string connectionStr;

            try
            {
                
                 portNum = int.Parse(conntectToPort.text);
                twoWayConnection.EstablishTwoWayConnection(portNum);

            }
            catch (System.Exception)
            {

                connectionStr = "";
                connectionStr += $"Connection failed!";
                statusText.text = connectionStr;
                statusText.color = Color.red;
            }





        }
    }
}
