using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [SerializeField] private string serverIp;
    [SerializeField] private int serverPort;

    private Socket sock;

    void OnEnable()
    {
        ConnectViaSocket();
        Application.logMessageReceived += HandleLog;
    }

    private void ConnectViaSocket()
    {
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip = IPAddress.Parse(serverIp);
        IPEndPoint ep = new IPEndPoint(ip, serverPort);
        sock.Connect(ep);
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!Application.isPlaying)
        {
            return;
        }
        byte[] packet = new byte[1024];
        using MemoryStream ms = new MemoryStream(packet);
        using BinaryWriter bw = new BinaryWriter(ms);

        switch (type)
        {
            case LogType.Error:
                bw.Write($"[{DateTime.Now.ToString("hh:mm:ss")}] Error\n[Message]\n{logString}\n[StackTrace]\n{stackTrace}\n");
                break;
            case LogType.Assert:
                bw.Write($"[{DateTime.Now.ToString("hh:mm:ss")}] Assert\n[Message]\n{logString}\n[StackTrace]\n{stackTrace}\n");
                break;
            case LogType.Warning:
                bw.Write($"[{DateTime.Now.ToString("hh:mm:ss")}] Warning\n[Message]\n{logString}\n[StackTrace]\n{stackTrace}\n");
                break;
            case LogType.Log:
                bw.Write($"[{DateTime.Now.ToString("hh:mm:ss")}] Log\n[Message]\n{logString}\n[StackTrace]\n{stackTrace}\n");
                break;
            case LogType.Exception:
                bw.Write($"[{DateTime.Now.ToString("hh:mm:ss")}] Exception[Message]\n{logString}\n[StackTrace]\n{stackTrace}\n");
                break;
            default:
                break;
        }
        sock.Send(packet);
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        sock.Dispose();
    }
}
