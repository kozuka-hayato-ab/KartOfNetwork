using System.Collections;
using System.Net;
using System.Net.Sockets;


// イベント通知のデリゲート.
public delegate void 	EventHandler(ITransport transport, NetEventState state);


public interface ITransport
{
	// Use this for initialization
	bool		Initialize(Socket socket);

	//
	bool		Terminate();

	int			GetNodeId();

	void		SetNodeId(int node);

	IPEndPoint	GetLocalEndPoint();

	IPEndPoint	GetRemoteEndPoint();

	//
	int			Send(byte[] data, int size);
	
	//
	int			Receive(ref byte[] buffer, int size);

	//
	bool		Connect(string ipAddress, int port);
	
	// 
	void		Disconnect();
	
	// 
	void		Dispatch();

	//
	//void		SetReceiveData(byte[] data, int size);

	//
	void		RegisterEventHandler(EventHandler handler);

	//
	void		UnregisterEventHandler(EventHandler handler);


	// 同一端末で実行する際にポート番号で送信元を判別するあためにキープアライブ用の.
	// ポート番号を設定します.
	void 		SetServerPort(int port);
}

