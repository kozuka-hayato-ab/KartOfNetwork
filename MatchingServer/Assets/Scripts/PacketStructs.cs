using UnityEngine;
using System;
using System.Collections;
using System.Net;




public enum TransportRequest
{
	Connect = 0,

	Disconnect,

	UserData,

}




public enum PacketId
{
	// マッチング用パケット.
	MatchingRequest = 0,		// マッチング要求パケット.
	MatchingResponse, 			// マッチング応答パケット.
	SearchRoomResponse, 		// 部屋検索応答.
	StartSessionNotify, 		// ゲーム開始通知.

	// ゲーム用パケット.
	Equip,						// 初期装備情報.
	GameSyncInfo,				// ゲーム前同期情報.
	CharacterData,				// キャラクター座標情報.
	AttackData,					// キャラクター攻撃情報.
	ItemData,					// アイテム取得/破棄情報
	UseItem,					// アイテム使用情報.
	DoorState,					// ドアの状態.
	MovingRoom,					// 部屋移動情報.
	HpData,						// HP通知.
	DamageData,					// ホストへダメージ通知.
	DamageNotify,				// 全端末へダメージ量を通知.
	MonsterData,				// モンスターの発生.
	Summon,						// 召喚獣情報.
	BossDirectAttack,			// ボス直接攻撃.
	BossRangeAttack,			// ボス範囲攻撃.
	Prize,						// ご褒美取得情報.
	PrizeResult,				// ご褒美取得結果.
	ChatMessage,				// チャットメッセージ.

	Max,
}



public enum MatchingRequestId
{
	CreateRoom = 0,
	JoinRoom,
	StartSession,
	SearchRoom,

	Max,
}

public enum MatchingResult 
{
	Success = 0,


	RoomIsFull,
	MemberIsFull,
	RoomIsGone,

}

public struct PacketHeader
{
	// パケット種別.
	public PacketId 	packetId;
}

//
// マッチングリクエスト.
//
public struct MatchingRequest
{
	public int					version;	// パケットID.
	public MatchingRequestId	request;	// リクエスト内容.
	public int 					roomId;		// リクエスト要求ルームID.
	public string				name;		// 作成ルーム名.
	public int					level;		// レベル分けの指定.
	
	public const int roomNameLength = 32;	// ルーム名の長さ.
}

//
// マッチングレスポンス.
//
public struct MatchingResponse
{
	// リクエストの結果.
	public MatchingResult		result;
	
	// リクエスト内容.
	public MatchingRequestId	request;

	// レスポンスルームID.
	public int 					roomId;

	// 
	public string			 	name;

	// 参加人数.
	public int					members;

	// ルーム名の長さ.
	public const int roomNameLength = 32;
}

//
// ルーム情報.
//
public struct RoomInfo
{
	// リクエスト要求ルームID.
	public int 					roomId;
	
	// 作成ルーム名.
	public string				name;

	//
	public int					members;

	// ルーム名の長さ.
	public const int roomNameLength = 32;
}

//
// ルーム検索結果.
//
public struct SearchRoomResponse
{
	// 検索した部屋の数.
	public int			roomNum;

	// 部屋情報.
	public RoomInfo[]	rooms;
}

//
// 接続先情報.
//
public struct EndPointData
{
	public string		ipAddress;
	
	public int 			port;

	// IPアドレスの長さ.
	public const int ipAddressLength = 32;
}

//
// セッション情報.
//
public struct SessionData
{
	public MatchingResult 	result;

	public int				playerId;		// 同一の端末で動作させるときに使用します.

	public int				members;

	public EndPointData[]	endPoints;
}


//
//
// ゲーム用パケットデータ定義.
//
//


//
// ゲーム前の同期情報.
//
public struct CharEquipment
{
	public int			globalId;	// キャラクターのグローバルID.
	public string		itemId;		// 選択した武器情報.

	public const int 	itemNameLength = 32;	// 武器名の長さ.
}

//
// 全員分の同期情報.
//
public struct GameSyncInfo
{
	public int				seed;		// 同期する乱数の種.
	public CharEquipment[]	items;		// 同期する装備情報.
}


//
// アイテム取得情報.
//
public struct ItemData
{
	public string 		itemId;		// アイテム識別子.
	public int			state;		// アイテムの取得状態.
	public string 		ownerId;	// 所有者ID.

	public const int 	itemNameLength = 32;		// アイテム名の長さ.
	public const int 	charactorNameLength = 64;	// キャラクターIDの長さ.
}


//
// キャラクター座標情報.
//
public struct CharacterCoord
{
	public float	x;		// キャラクターのx座標.
	public float	z;		// キャラクターのz座標.
	
	public CharacterCoord(float x, float z)
	{
		this.x = x;
		this.z = z;
	}
	public Vector3	ToVector3()
	{
		return(new Vector3(this.x, 0.0f, this.z));
	}
	public static CharacterCoord	FromVector3(Vector3 v)
	{
		return(new CharacterCoord(v.x, v.z));
	}
	
	public static CharacterCoord	Lerp(CharacterCoord c0, CharacterCoord c1, float rate)
	{
		CharacterCoord	c = new CharacterCoord();
		
		c.x = Mathf.Lerp(c0.x, c1.x, rate);
		c.z = Mathf.Lerp(c0.z, c1.z, rate);
		
		return(c);
	}
}

//
// キャラクターの移動情報.
//
public struct CharacterData
{
	public string 			characterId;	// キャラクターID.
	public int 				index;			// 位置座標のインデックス.
	public int				dataNum;		// 座標データ数.
	public CharacterCoord[]	coordinates;	// 座標データ.

	public const int 		characterNameLength = 64;	// キャラクターIDの長さ.
}

//
// キャラクターの攻撃情報.
//
public struct AttackData
{
	public string		characterId;		// キャラクターID.
	
	public const int 	characterNameLength = 64;	// キャラクターIDの長さ.
}

//
// モンスターのリスポーン情報.
//
public struct MonsterData
{
	public string		lairId;			// モンスター名.
	
	public const int 	monsterNameLength = 64;	// モンスター名の長さ.
}


//
// ダメージ量の情報.
//
public struct DamageData
{
	public string 			target;			// 攻撃されたキャラクターID.
	public int	 			attacker;		// 攻撃したアカウントID.
	public float			damage;			// ダメージ量.

	public const int 		characterNameLength = 64;	// キャラクターIDの長さ.
}

//
// キャラクターHPの情報.
//
public struct HpData
{
	public string 			characterId;	// キャラクターID.
	public float			hp;				// HP.
	
	public const int 		characterNameLength = 64;	// キャラクターIDの長さ.
}

//
// ドーナツに入った状態の情報.
//
public struct CharDoorState
{
	public int			globalId;		// グローバルID.
	public string		keyId;			// 鍵ID.
	public bool			isInTrigger;	// ドーナツ上にいる状態.
	public bool			hasKey;			// ガギを持っているか.

	public const int 	keyNameLength = 64;	// 鍵IDの長さ.
}


//
// ルーム移動通知.
//
public struct MovingRoom
{
	public string		keyId;				// カギID.

	public const int 	keyNameLength = 32;	// カギ名の長さ.
}

//
// アイテム使用情報.
//
public struct ItemUseData
{
	public int		itemFavor;	// アイテムの効果.
	public string	targetId;	// 効果を発動するキャラクターID.
	public string	userId;		// アイエムを使用するキャラクターID.

	public int		itemCategory;	// アイテムの効果の種類.

	public const int characterNameLength = 64;	// キャラクターIDの長さ.
}

//
// 召喚獣の出現情報.
//
public struct SummonData
{
	public string		summon;					// 召喚獣の種類.

	public const int 	summonNameLength = 32;	// 召喚獣名の長さ.
}


//
// ボス攻撃情報.
//

// 直接攻撃.
public struct BossDirectAttack
{
	public string		target;		// ターゲットのキャラクター.
	public float		power;		// 攻撃力.

	public const int 	characterNameLength = 64;	// キャラクターIDの長さ.
}

// 範囲攻撃.
public struct BossRangeAttack
{
	public float	power;		// 攻撃力.
	public float	range;		// 範囲.
}


//
// ご褒美ケーキ情報.
//
public struct PrizeData
{
	public string		characterId;	// キャラクターID.
	public int			cakeNum;		// ケーキの数.

	public const int 	characterNameLength = 64;	// キャラクターIDの長さ.
}

//
// ご褒美ケーキの結果情報.
//
public struct PrizeResultData
{
	public int 		cakeDataNum;	// ケーキデータ数.
	public int []	cakeNum;		// 食べてケーキ数.
}


//
// チャットメッセージ.
//
public struct ChatMessage
{
	public string		characterId; // キャラクターID.
	public string		message;	 // チャットメッセージ.
	
	public const int 	characterNameLength = 64;	// キャラクターIDの長さ.
	public const int	messageLength = 64;
}

