using System.Collections;
using System.Collections.Generic;
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

     void Update()
    {
        if (Listener.Instance!=null && !Listener.Instance.IsBound )
        {
            ListenOnPort();
        }

        string statusString="";
        if (ClientData.Instance.TwoWayConnectionEstablished())
        {
            statusString += $"TWO WAY CONNECTION ESTABLISEHD";

            statusText.text = statusString;
            statusText.color = Color.green;
            StartGame();
        }
        if (Listener.Instance.IsReceiverConnected())
        {
            statusString += $" {Listener.Instance.GetReceiverEndPointNum()} -> {Listener.Instance.GetReceiverLocalPortNum()}";

        }

        if (Sender.Instance.IsSenderConnected())
        {
            statusString += $" {Sender.Instance.GetEndPointPortNum()} -> {Sender.Instance.GetLocalPortNum()}";

        }
       
    }

    public void StartGame() 
    {
        SceneManager.LoadScene(1);
    }

    public void TwoWayConnectionEstablished() 
    {

    }

    public void ListenOnPort() 
    {
        try
        {
            int portNum = int.Parse(listenOnPortField.text);
            if (Listener.Instance != null)
            {
                if (Listener.Instance.StartListeningForConnections(portNum))
                {
                    listeningOnPortText.text = "Listening on: " + (Listener.Instance.GetBoundPortString());
                    listeningOnPortText.color = Color.green;
                }
            }

        }
        catch (System.Exception)
        {
            if (Listener.Instance != null)
            {
                bool succeded = false;
                try
                {
                    succeded=Listener.Instance.StartListeningForConnections(12000);
                }
                catch (System.Exception)
                {

                    succeded=Listener.Instance.StartListeningForConnections(13000);
                }
                if (succeded)
                {
                    listeningOnPortText.text = "Listening on: " + (Listener.Instance.GetBoundPortString());
                    listeningOnPortText.color = Color.green;
                }
            }
        }
       

        


        
    }

    public void TryConnectToPort() 
    {
        if (listeningOnPortText.text!=string.Empty)
        {
            int portNum = 12;
            string connectionStr;

            try
            {
                 portNum = int.Parse(conntectToPort.text);
            }
            catch (System.Exception)
            {

                connectionStr = "";
                connectionStr += $"Connection failed!";
                statusText.text = connectionStr;
                statusText.color = Color.red;
            }
            Sender.Instance.TrtConnectToPort(portNum);
           
          


        }
    }
}
