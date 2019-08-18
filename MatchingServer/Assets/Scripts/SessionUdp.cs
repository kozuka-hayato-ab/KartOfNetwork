using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

public class SessionUDP : Session<TransportUDP>
{

	// 
	public override bool CreateListener(int port, int connectionMax)
	{
		try {
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_listener.Bind(new IPEndPoint(IPAddress.Any, port));

			string str = "Create UDP Listener " + "(Port:" + port + ")"; 
			Debug.Log(str);
		}
		catch {
			return false;
		}
		
		
		return true;
	}
	
	//
	public override bool DestroyListener()
	{
		if (m_listener == null) {
			return false;
		}

		m_listener.Close();
		m_listener = null;
		
		return true;
	}	
	
	public override void AcceptClient() 
	{
	}

	//
	protected override void DispatchReceive()
	{
		// リスニングソケットで一括受信したデータを各トランスポートへ振り分ける.
		if (m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) {
			byte[] buffer = new byte[m_mtu];
			IPEndPoint address = new IPEndPoint(IPAddress.Any, 0);
			EndPoint endPoint =(EndPoint) address;
			
			int recvSize = m_listener.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

			int node = -1;
			// 同一端末で実行する際にポート番号で送信元を判別するあためにキープアライブの.
			// パケットにIPアドレスとポート番号を取り出します.
			string str = System.Text.Encoding.UTF8.GetString(buffer).Trim('\0');
			if (str.Contains(TransportUDP.m_requestData)) {
				string[] strArray = str.Split(':');
				IPEndPoint ep = new IPEndPoint(IPAddress.Parse(strArray[0]), int.Parse(strArray[1]));
				node = getNodeFromEndPoint(ep, true);
			}
			else {
				node = getNodeFromEndPoint((IPEndPoint) endPoint, false);
			}

			//Debug.Log("remote:" + ((IPEndPoint) endPoint).Address.ToString() + " port:" + ((IPEndPoint) endPoint).Port);
			//Debug.Log("remote node:" + node); 

			if (node >= 0) {
				TransportUDP transport = m_transports[node];
				transport.SetReceiveData(buffer, recvSize, (IPEndPoint) endPoint);
			}
		}
	}

	
	// EndPointからノード番号を取得.
	private int getNodeFromEndPoint(IPEndPoint endPoint, bool keepAlive)
	{
		foreach (int node in m_transports.Keys) {
			TransportUDP transport = m_transports[node];

			IPEndPoint transportEp = (keepAlive)? transport.GetLocalEndPoint() : transport.GetRemoteEndPoint();
			if (transportEp != null) {
				//Debug.Log("NodeFromEP recv:" + ((IPEndPoint) endPoint).Address.ToString() + " transport:" + transportEp.Address.ToString());
				if (
					transportEp.Port == endPoint.Port &&
					transportEp.Address.ToString() == endPoint.Address.ToString()
				    ) {
					return node;
				}
			}
		}
		
		return -1;
	}
}

