using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace Domino_Queue_Handler
{
    class TCPCom
    {

        public string Delay { get; set; }


        private TcpClient TcpClientObject;
        private TcpListener TcpListenerObject;
        private NetworkStream TcpStreamObject;

        public int TCPStatus; //0 = listening, 1 = connected, 2 = offline
        public int ID;

        public string TCPIP;
        public int TCPPort;

        private bool TCPMode; //False = Client, True = Listener
        private bool ComActive;

        public TCPCom(int SID, string SIP, int SPort, bool TCPType)
        {
            ComActive = true;
            TCPStatus = 0;
            ID = SID;
            TCPIP = SIP;
            TCPPort = SPort;
            TCPMode = TCPType;
        }

        //ComActive is meant to work as a shutdown sequence. If you want to do a "soft" shutdown you can sett ComActive to false, this will stop the ComClass from activating listening functions and so forth

        public void SetOffline()
        {
            ComActive = false;
        }

        public bool SetIP(string SIP)
        {
            TCPIP = SIP;
            return SetupCom();
        }

        public bool SetPort(int SPort)
        {
            TCPPort = SPort;
            return SetupCom();
        }

        public bool SetTCPMode(bool TCPType) //false = client mode, true = listener
        {
            TCPMode = TCPType;
            return SetupCom();
        }

        public string GetIP()
        {
            return TCPIP;
        }

        public int GetPort()
        {
            return TCPPort;
        }

        public int GetStatus()
        {
            if (ComActive == false)
            {
                TCPStatus = 2;
            }
            return TCPStatus;
        }

        public int GetID()
        {
            return ID;
        }

        public bool SetupCom()
        {
            try
            {
                StopCom();
                ComActive = true;

                if (TCPMode == true) //false = client mode, true = listener
                {
                    //TcpListenerObject = new TcpListener(IPAddress.Parse(TCPIP), TCPPort);
                    TcpListenerObject = new TcpListener(IPAddress.Any, TCPPort);
                    DebugWrite("TCPListenerObject has been successfully initialized.");
                }
                else
                {
                    //Nothing to do with TCPCLientObject as that is done within the UseCom function
                    //DebugWrite("TCPClientObject has been successfully initialized.");
                }

            }
            catch (Exception e)
            {
                DebugWrite("Error setting up basic communication in SetupCom(), cause: " + e);
                return false;
            }
            TCPStatus = 0; //Set to listening status
            return true;
        }

        //StopCom is the absolute stop for this entity. If it's called it's supposed to stop the entire communication. Setupcom must be used to reactivate it.
        public void StopCom()
        {
            try
            {
                TcpStreamObject.Close();
            }
            catch (Exception e)
            {
                DebugWrite("Error closing TcpStreamObject, cause: " + e);
            }
            if (TCPMode)
            {
                try
                {
                    TcpListenerObject.Stop();
                }
                catch (Exception e)
                {
                    DebugWrite("Error closing TcpListenerObject, cause: " + e);
                }
            }

            try
            {
                TcpClientObject.Close();
            }
            catch (Exception e)
            {
                DebugWrite("Error closing TcpLClientObject, cause: " + e);
            }
            ComActive = false;
            TCPStatus = 2;
        }

        public string UseCom(int UserTimeout = 0) //timeout = 0 means no timeout, = continous listen
        {
            string ReceivedData = "";

            if (ComActive == false)
            {
                TCPStatus = 2;
                return "";
            }

            if (TCPMode == true)
            {
                //setup host mode, return and exit upon received data.
                // TODO
                ReceivedData = TCPHostListen();
            }
            else
            {
                //setup client connection to host
                ReceivedData = TCPClientListen();

            }

            return ReceivedData;
        }

        private string TCPHostListen()
        {
            TCPStatus = 0; //Set to listening
            string ReceivedData = "";
            Byte[] bytes = new Byte[256];
            TcpClient ClientListener;

            if (ComActive == false)
            {
                DebugWrite("In TCPHostListen, ComActive is closed, exiting.");
                TCPStatus = 2;
                return "";
            }


            while (ComActive)
            {
                try
                {
                    DebugWrite("In TCPHostListen, initializing listen setup.");
                    TcpListenerObject.Start();
                    DebugWrite("In TCPHostListen, start sequence OK.");
                }
                catch (SocketException e)
                {
                    TCPStatus = 0; //Set to listening as we will exit the connection now
                    DebugWrite("Error while listening to data as host in TCPHostListen, error: " + e);
                    return "";
                }
                try
                {
                    //TODO 
                    ClientListener = TcpListenerObject.AcceptTcpClient();
                    DebugWrite("As host; Client has connected.");
                    ClientListener.ReceiveTimeout = 10000;
                    ClientListener.SendTimeout = 10000;
                    try
                    {
                        TCPStatus = 1; //Set to connected

                        TcpStreamObject = ClientListener.GetStream();

                        int i;

                        while ((i = TcpStreamObject.Read(bytes, 0, bytes.Length)) != 0 && ComActive)
                        {
                            ReceivedData = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            DebugWrite("As host; Received data: " + ReceivedData);

                            TCPStatus = 0; //Set to listening as we will exit the connection now
                            ClientListener.Close();
                            return ReceivedData;

                        }
                    }
                    catch (System.IO.IOException e)
                    {
                        DebugWrite("Error reading data as host in TCPHostListen, error: " + e);
                        SetupCom();
                    }

                }
                catch (System.InvalidOperationException e)
                {
                    DebugWrite("InvalidOperationException in TCPHostListen, error: " + e);
                }
                catch (SocketException e)
                {
                    DebugWrite("SocketException in TCPHostListen, error: " + e);
                }
                catch (Exception e)
                {
                    DebugWrite("Exception in TCPHostListen, error: " + e);
                }

            }

            TCPStatus = 0; //Set to listening as we will exit the connection now
            return "";
        }

        //System.IO.IOException

        private string TCPClientListen()
        {
            TCPStatus = 0; //Set to listening

            Byte[] data = new Byte[256];
            string ReceivedData = "";

            if (ComActive == false)
            {
                DebugWrite("In TCPHostListen, ComActive is closed, exiting.");
                TCPStatus = 2;
                return "";
            }

            try
            {
                TcpClientObject = new TcpClient(TCPIP, TCPPort);
                TCPStatus = 1; //Set to connected 

                TcpStreamObject = TcpClientObject.GetStream();

            }
            catch (SocketException e)
            {
                TCPStatus = 0; //Set to listening as we will exit the connection now
                DebugWrite("Error while listening to data as client in TCPClientListen, error: " + e);
                return "";
            }
            try
            {
                Int32 bytes = TcpStreamObject.Read(data, 0, data.Length);
                ReceivedData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);


                TcpStreamObject.Close();
                TcpClientObject.Close();
                return ReceivedData;
            }
            catch (System.IO.IOException e)
            {
                DebugWrite("Error while reading data as client in TCPClientListen, error: " + e);
            }

            TCPStatus = 0; //Set to listening as we will exit the connection now
            return "";
        }

        public string GetACC()
        {
            TCPStatus = 0; //Set to listening
            var SendData = (char)2 + "044B??" + (char)13;

            try
            {
                TcpClientObject = new TcpClient(TCPIP, TCPPort);
                TCPStatus = 1; //Set to connected 

                TcpStreamObject = TcpClientObject.GetStream();

            }
            catch (SocketException e)
            {
                TCPStatus = 0; //Set to listening as we will exit the connection now
                DebugWrite("Error while listening to data as client in TCPClientListen, error: " + e);
                return "";
            }

            try
            {
                if (TcpStreamObject != null)
                {
                    byte[] msg = Encoding.UTF8.GetBytes(SendData);
                    TcpStreamObject.Write(msg, 0, msg.Length);
                    DebugWrite("msg length: " + msg.Length);
                    DebugWrite("Following data has been sent to client from TCPSend: " + SendData);
                }
                else
                {
                    DebugWrite("Could not send data due to TCPStreamObject being null.From TCPSend.");
                    return "";
                }

            }
            catch (SocketException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, SocketException error: " + e);
                return "";
            }
            catch (ObjectDisposedException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, ObjectDisposedException error: " + e);
                return "";
            }
            catch (Exception e)
            {
                DebugWrite("Error while sending data as host in TCPSend, error: " + e);
                return "";
            }

            Byte[] data = new Byte[256];
            string ReceivedData = "";

            if (ComActive == false)
            {
                DebugWrite("In TCPHostListen, ComActive is closed, exiting.");
                TCPStatus = 2;
                return "";
            }


            try
            {
                Int32 bytes = TcpStreamObject.Read(data, 0, data.Length);
                ReceivedData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);


                TcpStreamObject.Close();
                TcpClientObject.Close();
                return ReceivedData;
            }
            catch (System.IO.IOException e)
            {
                DebugWrite("Error while reading data as client in TCPClientListen, error: " + e);
            }

            TCPStatus = 0; //Set to listening as we will exit the connection now
            return "";
        }

        public string SendCMD41(string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8, string var9)
        {
            TCPStatus = 0; //Set to listening ÄNDRAT TILL Q2
            var SendData = (char)2 + "041C1E1Q2" + (char)23 + "D" + var1 + (char)10 + var2 + (char)10 + var3 + (char)10 + var4 + (char)10 + var5 + (char)10 + var6 + (char)10 + var7 + (char)10 + var8 + (char)10 + var9 + "??" + (char)13;

            try
            {
                TcpClientObject = new TcpClient(TCPIP, TCPPort);
                TCPStatus = 1; //Set to connected 

                TcpStreamObject = TcpClientObject.GetStream();

            }
            catch (SocketException e)
            {
                TCPStatus = 0; //Set to listening as we will exit the connection now
                DebugWrite("Error while listening to data as client in TCPClientListen, error: " + e);
                return "";
            }

            try
            {
                if (TcpStreamObject != null)
                {
                    byte[] msg = Encoding.UTF8.GetBytes(SendData);
                    TcpStreamObject.Write(msg, 0, msg.Length);
                    DebugWrite("msg length: " + msg.Length);
                    DebugWrite("Following data has been sent to client from TCPSend: " + SendData);
                }
                else
                {
                    DebugWrite("Could not send data due to TCPStreamObject being null.From TCPSend.");
                    return "";
                }

            }
            catch (SocketException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, SocketException error: " + e);
                return "";
            }
            catch (ObjectDisposedException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, ObjectDisposedException error: " + e);
                return "";
            }
            catch (Exception e)
            {
                DebugWrite("Error while sending data as host in TCPSend, error: " + e);
                return "";
            }

            Byte[] data = new Byte[256];
            string ReceivedData = "";

            if (ComActive == false)
            {
                DebugWrite("In TCPHostListen, ComActive is closed, exiting.");
                TCPStatus = 2;
                return "";
            }


            try
            {
                Int32 bytes = TcpStreamObject.Read(data, 0, data.Length);
                ReceivedData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);


                TcpStreamObject.Close();
                TcpClientObject.Close();
                return ReceivedData;
            }
            catch (System.IO.IOException e)
            {
                DebugWrite("Error while reading data as client in TCPClientListen, error: " + e);
            }

            TCPStatus = 0; //Set to listening as we will exit the connection now
            return "";
        }
        public string SendError41(string var1)
        {
            TCPStatus = 0; //Set to listening, ÄNDRAT TILL Q2
            var SendData = (char)2 + "041C1E2Q2" + (char)23 + "D" + var1 + "??" + (char)13;

            try
            {
                TcpClientObject = new TcpClient(TCPIP, TCPPort);
                TCPStatus = 1; //Set to connected 

                TcpStreamObject = TcpClientObject.GetStream();

            }
            catch (SocketException e)
            {
                TCPStatus = 0; //Set to listening as we will exit the connection now
                DebugWrite("Error while listening to data as client in TCPClientListen, error: " + e);
                return "";
            }

            try
            {
                if (TcpStreamObject != null)
                {
                    byte[] msg = Encoding.UTF8.GetBytes(SendData);
                    TcpStreamObject.Write(msg, 0, msg.Length);
                    DebugWrite("msg length: " + msg.Length);
                    DebugWrite("Following data has been sent to client from TCPSend: " + SendData);
                }
                else
                {
                    DebugWrite("Could not send data due to TCPStreamObject being null.From TCPSend.");
                    return "";
                }

            }
            catch (SocketException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, SocketException error: " + e);
                return "";
            }
            catch (ObjectDisposedException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, ObjectDisposedException error: " + e);
                return "";
            }
            catch (Exception e)
            {
                DebugWrite("Error while sending data as host in TCPSend, error: " + e);
                return "";
            }

            Byte[] data = new Byte[256];
            string ReceivedData = "";

            if (ComActive == false)
            {
                DebugWrite("In TCPHostListen, ComActive is closed, exiting.");
                TCPStatus = 2;
                return "";
            }


            try
            {
                Int32 bytes = TcpStreamObject.Read(data, 0, data.Length);
                ReceivedData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);


                TcpStreamObject.Close();
                TcpClientObject.Close();
                return ReceivedData;
            }
            catch (System.IO.IOException e)
            {
                DebugWrite("Error while reading data as client in TCPClientListen, error: " + e);
            }

            TCPStatus = 0; //Set to listening as we will exit the connection now
            return "";
        }

        public bool ComSend(string SendData, System.Text.Encoding EncodingType)
        {
            return TCPSend(SendData, EncodingType);
        }

        private bool TCPSend(string SendData, System.Text.Encoding EncodingType)
        {
            bool OkSend = false;

            try
            {
                if (TcpStreamObject != null)
                {
                    byte[] msg = EncodingType.GetBytes(SendData);
                    TcpStreamObject.Write(msg, 0, msg.Length);
                    DebugWrite("msg length: " + msg.Length);
                    OkSend = true;
                    DebugWrite("Following data has been sent to client from TCPSend: " + SendData);
                }
                else
                {
                    DebugWrite("Could not send data due to TCPStreamObject being null.From TCPSend.");
                }

            }
            catch (SocketException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, SocketException error: " + e);
            }
            catch (ObjectDisposedException e)
            {
                DebugWrite("Error while sending data as host in TCPSend, ObjectDisposedException error: " + e);
            }
            catch (Exception e)
            {
                DebugWrite("Error while sending data as host in TCPSend, error: " + e);
            }

            return OkSend;
        }

        private void DebugWrite(string BugLine)
        {
#if DEBUG
            Debug.WriteLine(DateTime.Now + " - New debug text from TCPCom received for scanner ID: " + ID.ToString());
            Debug.WriteLine("                      - " + BugLine);
#endif
        }
    }
}
