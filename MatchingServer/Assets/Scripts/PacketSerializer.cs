using UnityEngine;
using System.Collections;


public class HeaderSerializer : Serializer
{
	public bool Serialize(PacketHeader data)
	{
	// 既存のデータをクリアします.
		Clear();
		
		// 各要素を順番にシリアライズします.
		bool ret = true;
		ret &= Serialize((int)data.packetId);

		if (ret == false) {
			return false;
		}

		return true;	
	}
	
	
	public bool Deserialize(ref PacketHeader serialized)
	{
		// デシリアライズするデータを設定します.
		bool ret = (GetDataSize() > 0)? true : false;
		if (ret == false) {
			return false;
		}
		
		// データの要素ごとにデシリアライズします.
		int packetId = 0;
		ret &= Deserialize(ref packetId);
		serialized.packetId = (PacketId)packetId;

		return ret;
	}	
}
