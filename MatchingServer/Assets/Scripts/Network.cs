using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;


public class Network : MonoBehaviour {

	private SessionTCP		m_sessionTcp = null;


	private const int 		m_headerVersion = NetConfig.SERVER_VERSION;

	private Dictionary<int, NodeInfo> m_nodes = new Dictionary<int, NodeInfo>();

	// 送受信用のパケットの最大サイズ.
	private const int		m_packetSize = 1400;
	

	public delegate			void RecvNotifier(int node, byte[] data);

	private Dictionary<int, RecvNotifier> m_notifier = new Dictionary<int, RecvNotifier>();

	// イベント通知のデリゲート.
	public delegate void 	NetEventHandler(int node, NetEventState state);
	// イベントハンドラー.
	private NetEventHandler	m_handler;


	private class NodeInfo
	{
		public int	node = 0;
	}


	void Awake()
	{
		m_sessionTcp = new SessionTCP();
		m_sessionTcp.RegisterEventHandler(ServerSignalNotification);

		DontDestroyOnLoad(gameObject);
	}


	// Update is called once per frame
	void Update()
	{
		if (IsConnected() == true) {
			byte[] packet = new byte[m_packetSize];
	
			Dictionary<int, NodeInfo> nodes = new Dictionary<int, NodeInfo>(m_nodes);

			foreach (int node in nodes.Keys) {
				// 到達保障パケットの受信をします.
				while (m_sessionTcp.Receive(node, ref packet) > 0) {
					// 受信パケットの振り分けをします.
					Receive(node, packet);
				}
			}	
		}
	}

	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit called.!");

		StopServer();
	}

	public bool StartServer(int port, int connectionMax)
	{
		Debug.Log("Start server called.!");

		// リスニングソケットを生成します.
		try {
			// 到達保障用のTCP通信を開始します.
			m_sessionTcp.StartServer(port, connectionMax);
		}
		catch {
			Debug.Log("Server fail start.!");
			return false;
		}

		Debug.Log("Server started.!");


		return true;
	}

	public void StopServer()
	{
		Debug.Log("StopServer called.");

		// サーバ起動を停止.
		if (m_sessionTcp != null) {
			m_sessionTcp.StopServer();
		}

		Debug.Log("Server stopped.");
	}

	// 
	public int Connect(string address, int port)
	{
		int node = -1;

		// 到達保障用のTCP通信を開始します.
		node = m_sessionTcp.Connect(address, port);

		return node;
	}

	public bool Disconnect(int node)
	{	
		if (m_sessionTcp != null) {
			m_sessionTcp.Disconnect(node);
		}

		return true;
	}
	
	public void RegisterReceiveNotification(PacketId id, RecvNotifier notifier)
	{
		int index = (int)id;

		m_notifier.Add(index, notifier);
	}

	//
	public bool IsConnected()
	{
#if false
		if (m_sessionTcp == null || m_sessionTcp.IsConnected() == false) {
			return false;
		}
		
		if (m_udp == null) {
			return false;
		}		
#endif
		return	true;
	}

	//
	public bool IsServer()
	{
		if (m_sessionTcp == null) {
			return false;
		}

		return	m_sessionTcp.IsServer();
	}


	public IPEndPoint GetEndPoint(int node)
	{
		if (m_sessionTcp == null) {
			return default(IPEndPoint);
		}

		return m_sessionTcp.GetRemoteEndPoint(node);
	}
	
	public int Send<T>(int node, PacketId id, IPacket<T> packet)
	{
		int sendSize = 0;
		
		if (m_sessionTcp != null) {
			// モジュールで使用するヘッダ情報を生成します.
			PacketHeader header = new PacketHeader();
			HeaderSerializer serializer = new HeaderSerializer();
					
			header.packetId = id;
	
			byte[] headerData = null;
			if (serializer.Serialize(header) == true) {
				headerData = serializer.GetSerializedData();
			}
			byte[] packetData = packet.GetData();
			
			byte[] data = new byte[headerData.Length + packetData.Length];
			
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			Buffer.BlockCopy(headerData, 0, data, 0, headerSize);
			Buffer.BlockCopy(packetData, 0, data, headerSize, packetData.Length);

			string str = "Send Packet[" +  id  + "]";

			sendSize = m_sessionTcp.Send(node, data, data.Length);
		}
		
		return sendSize;
	}

	public int SendReliable<T>(int node, IPacket<T> packet)
	{
		int sendSize = 0;
		
		if (m_sessionTcp != null) {
			// モジュールで使用するヘッダ情報を生成します.
			PacketHeader header = new PacketHeader();
			HeaderSerializer serializer = new HeaderSerializer();
			
			header.packetId = packet.GetPacketId();

			byte[] headerData = null;
			if (serializer.Serialize(header) == true) {
				headerData = serializer.GetSerializedData();
			}
			byte[] packetData = packet.GetData();
			
			byte[] data = new byte[headerData.Length + packetData.Length];
			
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			Buffer.BlockCopy(headerData, 0, data, 0, headerSize);
			Buffer.BlockCopy(packetData, 0, data, headerSize, packetData.Length);
			
			string str = "Send reliable packet[" +  header.packetId  + "]";
			
			sendSize = m_sessionTcp.Send(node, data, data.Length);
		}
		
		return sendSize;
	}

	private void Receive(int node, byte[] data)
	{
		PacketHeader header = new PacketHeader();
		HeaderSerializer serializer = new HeaderSerializer();

		serializer.SetDeserializedData(data);
		bool ret = serializer.Deserialize(ref header);
		if (ret == false) {
			Debug.Log("Invalid header data.");
			// パケットとして認識できないので破棄します.
			return;			
		}
		string str = "";
		for (int i = 0; i< 16; ++i) {
			str += data[i] + ":";
		}
		Debug.Log(str);

		int packetId = (int)header.packetId;
		if (packetId < m_notifier.Count && m_notifier[packetId] != null) {
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			byte[] packetData = new byte[data.Length - headerSize];
			Buffer.BlockCopy(data, headerSize, packetData, 0, packetData.Length);
	
			m_notifier[packetId](node, packetData);
		}
	}


	public void ServerSignalNotification(int node, NetEventState state)
	{
		string str = "Node:" + node + " type:" + state.type.ToString() + " State:" + state.ToString();
		Debug.Log("ServerSignalNotification called");
		Debug.Log(str);
		
		switch (state.type) {
		case NetEventType.Connect: {
			NodeInfo info = new NodeInfo();
			info.node = node;
			m_nodes.Add(node, info);
			if (m_handler != null) {
				m_handler(node, state);
			}
		} 	break;
			
		case NetEventType.Disconnect: {
			if (m_handler != null) {
				m_handler(node, state);
			}
			m_nodes.Remove(node);
		}	break;
			
		}
	}

	public void RegisterEventHandler(NetEventHandler handler)
	{
		m_handler = handler;
	}
}
