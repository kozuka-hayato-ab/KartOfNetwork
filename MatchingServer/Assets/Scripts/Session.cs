using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public abstract class Session<T>
	where T : ITransport, new()
{

	// リスニングソケット.
	protected Socket				m_listener = null;

	protected int					m_port = 0;

	// 現在の接続ID.
	protected int					m_nodeIndex = 0;
	
	protected Dictionary<int, T>	m_transports = new Dictionary<int, T>();

	//
	// スレッド関連のメンバ変数.
	//

	protected bool					m_threadLoop = false;
	
	protected Thread				m_thread = null;


	// 
	protected System.Object 		m_transportLock = new System.Object();

	// 
	protected System.Object 		m_nodeIndexLock = new System.Object();
	
	// サーバフラグ.	
	protected bool	 				m_isServer = false;

	//
	protected int 					m_mtu = 1400;


	// イベント通知のデリゲート.
	public delegate void 			EventHandler(int node, NetEventState state);
	// イベントハンドラー.
	protected EventHandler			m_handler;
	
	// 
	~Session() 
	{
		Disconnect();
	}
	
	
	public bool StartServer(int port, int connectionMax)
	{
		// リスニングソケットを生成します.
		bool ret = CreateListener(port, connectionMax);
		if (ret == false) {
			Debug.Log("Starat server failed.");
			return false;
		}

		//
		if (m_threadLoop == false) {
			CreateThread();
		}

		m_port = port;
		m_isServer = true;
		
		return true;
	}
	
	public void StopServer()
	{
		m_isServer = false;

		DestroyThread();

		DestroyListener();

		Debug.Log("Server stopped.");
	}



	// 
	protected bool CreateThread()
	{
		Debug.Log("CreateThread called.");

		// 受信処理のスレッド起動.
		try {
			m_thread = new Thread(new ThreadStart(ThreadDispatch));
			m_threadLoop = true;
			m_thread.Start();
		}
		catch {
			return false;
		}


		Debug.Log("Thread launched.");

		return true;
	}

	protected bool DestroyThread()
	{
		Debug.Log("DestroyThread called.");

		if (m_threadLoop == true) {
			// 
			m_threadLoop = false;

			if (m_thread != null) {
				// 受信処理スレッド終了.
				m_thread.Join();
				// 受信処理スレッド破棄.
				m_thread = null;
			}
		}

		return true;
	}

	//
	protected int JoinSession(Socket socket)
	{
		// セッションに参加.
		T transport = new T();

		if (socket != null) {
			// ソケットの設定をします.
			transport.Initialize(socket);
		}

		return JoinSession(transport);
	}

	protected int JoinSession(T transport)
	{
		int node = -1;
		lock (m_nodeIndexLock) {
			node = m_nodeIndex;
			++m_nodeIndex;
		}
		
		transport.SetNodeId(node);
		
		// イベントの通知を受ける関数を登録します.
		transport.RegisterEventHandler(OnEventHandling);
		
		try {
			lock (m_transportLock) {
				m_transports.Add(node, transport);
			}
		}
		catch { 
			return -1;
		}
		
		return node;
	}


	// 
	protected bool LeaveSession(int node)
	{
		if (node < 0) {
			return false;	
		}

		if (m_transports.ContainsKey(node) == false) {
			return false;
		}
					
		T transport = (T) m_transports[node];
		if (transport == null) {
			return false;
		}

		lock (m_transportLock) {
			// Transportの破棄
			transport.Terminate();

			m_transports.Remove(node);
		}

		return true;
	}

	public bool IsServer()
	{
		return m_isServer;
	}

	// 
	public int GetNodeNum()
	{
		return m_transports.Count;
	}
	
	public IPEndPoint GetLocalEndPoint(int node)
	{
		if (m_transports.ContainsKey(node) == false) {
			return default(IPEndPoint);
		}
		
		IPEndPoint ep;
		T transport = m_transports[node];
		ep = transport.GetLocalEndPoint();
		
		return ep;
	}

	public IPEndPoint GetRemoteEndPoint(int node)
	{
		if (m_transports.ContainsKey(node) == false) {
			return default(IPEndPoint);
		}

		IPEndPoint ep;
		T transport = m_transports[node];
		ep = transport.GetRemoteEndPoint();

		return ep;
	}

	// 接続要求監視.
	int FindTransoprt(IPEndPoint sender)
	{
		foreach (int node in m_transports.Keys) {
			T transport = m_transports[node];
			IPEndPoint ep = transport.GetLocalEndPoint();
			if (ep.Address.ToString() == sender.Address.ToString()) {
				return node;
			}
		}
		
		return -1;
	}

	//
	public virtual void ThreadDispatch()
	{	
		
		string str = "ThreadDispatch:" + m_threadLoop.ToString();
		Debug.Log(str);
		
		while (m_threadLoop) {
			// 接続要求監視.
			AcceptClient();
			
			// セッション内のノードの送受信処理.
			Dispatch();
			
			// 他のスレッドへ処理を譲る.
			Thread.Sleep(5);		
		}
		
		Debug.Log("Thread end.");
	}


	public virtual int Connect(string address, int port)
	{
		Debug.Log("Connect call");

		if (m_threadLoop == false) {
			Debug.Log("CreateThread");
			CreateThread();
		}
	
		int node = -1;
		bool ret = false;
		try {
			Debug.Log("transport Connect");
			T transport = new T();
			ret = transport.Connect(address, port);
			if (ret) {
				node = JoinSession(transport);
				Debug.Log("JoinSession node:" + node);
				// 同一端末で実行する際にポート番号で送信元を判別するあためのポート番号を設定.
				transport.SetServerPort(m_port);
			}
		}
		catch {
			Debug.Log("Connect fail.[exception]");
		}

		if (m_handler != null) {
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = (ret)? NetEventResult.Success : NetEventResult.Failure;
			m_handler(node, state);
		}

		return node;
	}

	public virtual bool Disconnect(int node)
	{
		if (node < 0) {
			return false;
		}

		T transport = m_transports[node];
		if (transport != null) {
			transport.Disconnect();
			LeaveSession(node);
		}

		if (m_handler != null) {
			NetEventState state = new NetEventState();
			state.type = NetEventType.Disconnect;
			state.result = NetEventResult.Success;
			m_handler(node, state);
		}

		return true;
	}

	public virtual bool Disconnect()
	{
		// スレッドの停止
		DestroyThread();
		
		// 接続中のTransportを切断する.
		lock (m_transportLock) {
			foreach (T trans in m_transports.Values) {
				trans.Disconnect();
				trans.Terminate();
			}
		}

		return true;
	}

	//
	public virtual int Send(int node, byte[] data, int size)
	{
		if (node < 0) {
			return -1;
		}

		int sendSize = 0;
		try {
			T transport = (T)m_transports[node];
			sendSize = transport.Send(data, size);
		}
		catch {
			return -1;
		}

		return sendSize;	
	}
	
	//
	public virtual int Receive(int node, ref byte[] buffer)
	{
		if (node < 0) {
			return -1;
		}

		int recvSize = 0;
		try { 
			T transport = m_transports[node];
			recvSize = transport.Receive(ref buffer, buffer.Length);
		}
		catch {
			return -1;
		}

		return recvSize;
	}

	//
	public virtual void Dispatch()
	{
		Dictionary<int, T> transports = new Dictionary<int, T>(m_transports);
		
		// 送信処理.
		foreach (T trans in transports.Values) {
			trans.Dispatch();
		}

		// 受信処理.
		DispatchReceive();

	}

	//
	protected virtual void DispatchReceive()
	{
		// リスニングソケットで一括受信したデータを各トランスポートへ振り分ける.
	}

	// イベント通知関数登録.
	public void RegisterEventHandler(EventHandler handler)
	{
		m_handler += handler;
	}
	
	// イベント通知関数削除.
	public void UnregisterEventHandler(EventHandler handler)
	{
		m_handler -= handler;
	}


	// 
	public virtual void OnEventHandling(ITransport itransport, NetEventState state)
	{
		int node = itransport.GetNodeId();

		string str = "SignalNotification[" + node + "] :" + state.type.ToString() + " state:" + state.ToString();
		Debug.Log(str);

		do {
			if (m_transports.ContainsKey(node) == false) {
				// 見つからなかった.
				string msg = "NodeId[" + node + "] is not founded.";
				Debug.Log(msg);
				break;
			}

			switch (state.type) {
			case NetEventType.Connect:
				break;

			case NetEventType.Disconnect:
				LeaveSession(node);
				break;
			}
		} while (false);

		// イベント通知関数が登録されていたらコールバックします.
		if (m_handler != null) {
			m_handler(node, state);
		}
	}


	public abstract bool	CreateListener(int port, int connectionMax);
	
	
	public abstract bool 	DestroyListener();
	
	
	public abstract void	AcceptClient();
	
}

