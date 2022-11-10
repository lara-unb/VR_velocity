using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UdpEuler : MonoBehaviour
{
    [HideInInspector] public bool isTxStarted = false;

    [SerializeField] string IP = "127.0.0.1"; // local host
    int rxPort = 8000; // port to receive data from Python on
    int txPort = 8001; // port to send data to Python on

    
    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    //String Received
    //public static string[] textArray;
    [SerializeField] private float y_data;
    [SerializeField] private int steps= 0;
    [SerializeField] private float timer = 0.0f;
    [SerializeField] private bool ascending = true;
    public float velocity = 0.0f;
    private bool timerStart = false;
    private float prevTime = 0.0f;

    void Awake()
    {
        // Create remote endpoint  
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);

        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Initialize (seen in comments window)
        print("UDP Comms Initialised");

        //StartCoroutine(SendDataCoroutine()); // Added to show sending data from Unity to Python via UDP

        
    }

    private void Update()
    {
        
        // Timer Starts on Enter. 
        if (Input.GetKey(KeyCode.KeypadEnter)){
            timerStart = true;
        }
        if (timerStart == true)
        {
            timer += Time.deltaTime;
        }
                

    }


    // Receive data, update packets received
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                
                y_data = float.Parse(text);
                //Debug.Log(y_data);

                // Calculating number of steps     
                
                if (y_data > 0 && ascending == false)
                {
                    ascending = true;
                    steps++;

                    //float period = timer - prevTime;
                    //prevTime = timer;
                    //Debug.Log(period);
                }
                if (y_data < 0 && ascending == true)
                {                    
                    ascending = false;
                    steps++;

                    float period = timer - prevTime;
                    velocity = (1 / period);
                    prevTime = timer;
                    Debug.Log(period);
                }

                
                //print(text);
                ProcessInput(text);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }



    private void ProcessInput(string input)
    {
        // PROCESS INPUT RECEIVED STRING HERE

        if (!isTxStarted) // First data arrived so tx started
        {
            isTxStarted = true;
        }
    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
        

    IEnumerator SendDataCoroutine() //  Added to show sending data from Unity to Python via UDP
    {
        while (true)
        {
            SendData("Sent from Unity: " + y_data);

            yield return new WaitForSeconds(1f);
        }
    }

    public void SendData(string message) // Use to send data to Python
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }


}