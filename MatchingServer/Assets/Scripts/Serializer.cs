
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Serializer
{
	
	private MemoryStream 	m_buffer = null;
	
	private int				m_offset = 0;

	
	private Endianness		m_endianness;
	

	// エンディアン.
	public enum Endianness
	{
		BigEndian = 0,		// ビッグエンディアン.
	    LittleEndian,		// リトルエンディアン.
	}
	
	public Serializer()
	{
		// シリアライズ用バッファを作成します.
		m_buffer = new MemoryStream();

		// エンディアンを判定します.
		int val = 1;
		byte[] conv = BitConverter.GetBytes(val);
		m_endianness = (conv[0] == 1)? Endianness.LittleEndian : Endianness.BigEndian;
	}
	
	
	public byte[] GetSerializedData()
	{	
		return m_buffer.ToArray();	
	}

	
	
	public void Clear()
	{
		byte[] buffer = m_buffer.GetBuffer();
		Array.Clear(buffer, 0, buffer.Length);
		
		m_buffer.Position = 0;
		m_buffer.SetLength(0);
		m_offset = 0;
	}

	//
	// デシリアライズするデータをバッファに設定します.
	//
	public bool SetDeserializedData(byte[] data)
	{
		// 設定するバッファをクリアします.
		Clear();

		try {
			// デシリアライズするデータを設定します.
			m_buffer.Write(data, 0, data.Length);
		}
		catch {
			return false;
		}
		
		return 	true;
	}

	
	//
	// bool型のデータをシリアライズします.
	//
	protected bool Serialize(bool element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(bool));
	}
	
	//
	// char型のデータをシリアライズします.
	//
	protected bool Serialize(char element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(char));
	}
	
	//
	// float型のデータをシリアライズします.
	//
	protected bool Serialize(float element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(float));
	}
	
	//
	// double型のデータをシリアライズします.
	//
	protected bool Serialize(double element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(double));
	}	
		
	//
	// short型のデータをシリアライズします.
	//
	protected bool Serialize(short element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(short));
	}	
	
	//
	// ushort型のデータをシリアライズします.
	//
	protected bool Serialize(ushort element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(ushort));
	}		
	
	//
	// int型のデータをシリアライズします.
	//
	protected bool Serialize(int element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(int));
	}	
	
	//
	// uint型のデータをシリアライズします.
	//
	protected bool Serialize(uint element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(uint));
	}		
	
	//
	// long型のデータをシリアライズします.
	//
	protected bool Serialize(long element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(long));
	}	
	
	//
	// ulong型のデータをシリアライズします.
	//
	protected bool Serialize(ulong element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(ulong));
	}
	
	//
	// byte[]型のデータをシリアライズします.
	//
	protected bool Serialize(byte[] element, int length)
	{
		// byte列はデータの塊として設定するのでエンディアン変換しない
		// ためバッファ保存先でもとに戻るようにします.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(element);	
		}

		return WriteBuffer(element, length);
	}

	//
	// string型のデータをシリアライズします.
	//
	protected bool Serialize(string element, int length)
	{
		byte[] data = new byte[length];

		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(element);
		int size = Math.Min(buffer.Length, data.Length);
		Buffer.BlockCopy(buffer, 0, data, 0, size);

		// byte列はデータの塊として設定するのでエンディアン変換しない
		// ためバッファ保存先でもとに戻るようにします.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(data);	
		}

		return WriteBuffer(data, data.Length);
	}
	
	//
	// データをbool型へデシリアライズします.
	//
	protected bool Deserialize(ref bool element)
	{
		int size = sizeof(bool);
		byte[] data = new byte[size];

		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToBoolean(data, 0);
			return true;
		}
		
		return false;
	}
	
	//
	// データをchar型へデシリアライズします.
	//
	protected bool Deserialize(ref char element)
	{
		int size = sizeof(char);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToChar(data, 0);
			return true;
		}
		
		return false;
	}
	
	
	//
	// データをfloat型へデシリアライズします.
	//
	protected bool Deserialize(ref float element)
	{
		int size = sizeof(float);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToSingle(data, 0);
			return true;
		}
		
		return false;
	}
	
	//
	// データをdouble型へデシリアライズします.
	//
	protected bool Deserialize(ref double element)
	{
		int size = sizeof(double);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToDouble(data, 0);
			return true;
		}
		
		return false;
	}	
	
	//
	// データをshort型へデシリアライズします.
	//
	protected bool Deserialize(ref short element)
	{
		int size = sizeof(short);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToInt16(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// データをushort型へデシリアライズします.
	//
	protected bool Deserialize(ref ushort element)
	{
		int size = sizeof(ushort);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToUInt16(data, 0);
			return true;
		}
		
		return false;
	}
	
	//
	// データをint型へデシリアライズします.
	//
	protected bool Deserialize(ref int element)
	{
		int size = sizeof(int);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToInt32(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// データをuint型へデシリアライズします.
	//
	protected bool Deserialize(ref uint element)
	{
		int size = sizeof(uint);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToUInt32(data, 0);
			return true;
		}
		
		return false;
	}
		
	//
	// データをlong型へデシリアライズします.
	//
	protected bool Deserialize(ref long element)
	{
		int size = sizeof(long);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToInt64(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// データをulong型へデシリアライズします.
	//
	protected bool Deserialize(ref ulong element)
	{
		int size = sizeof(ulong);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 読みだした値を変換します.
			element = BitConverter.ToUInt64(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// byte[]型のデータへデシリアライズします.
	//
	protected bool Deserialize(ref byte[] element, int length)
	{

		// 
		bool ret = ReadBuffer(ref element, length);

		// byte列はデータの塊として保存されてのでエンディアン変換しない
		// ためバッファここででもとに戻します.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(element);	
		}

		if (ret == true) {
			return true;
		}
		
		return false;
	}

	//
	// string型のデータへデシリアライズします.
	//
	protected bool Deserialize(ref string element, int length)
	{
		byte[] data = new byte[length];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// byte列はデータの塊として保存されてのでエンディアン変換しない
			// ためバッファここででもとに戻します.
			if (m_endianness == Endianness.LittleEndian) {
				Array.Reverse(data);	
			}
			string str = System.Text.Encoding.UTF8.GetString(data);
			element = str.Trim('\0');

			return true;
		}
		
		return false;
	}
	
	protected bool ReadBuffer(ref byte[] data, int size)
	{
		// 現在のオフセットからデータを読み出します.
		try {
			m_buffer.Position = m_offset;
			m_buffer.Read(data, 0, size);
			m_offset += size;
		}
		catch {
			return false;
		}
	
		// 読みだした値をホストバイトオーダーに変換します.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(data);	
		}	
		
		return true;
	}
	
	protected bool WriteBuffer(byte[] data, int size)
	{
		// 書き込む値をネットワークバイトオーダーに変換します.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(data);	
		}
	
		// 現在のオフセットからデータを書き込みます.
		try {
			m_buffer.Position = m_offset;		
			m_buffer.Write(data, 0, size);	
			m_offset += size;
		}
		catch {
			return false;
		}
		
		return true;
	}
	
	public Endianness GetEndianness()
	{
		return m_endianness;	
	}
		
	public long GetDataSize()
	{
		return m_buffer.Length;	
	}
}

