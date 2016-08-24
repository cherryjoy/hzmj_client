#ifndef _PROTOCAL_SERIALIZE_H_
#define _PROTOCAL_SERIALIZE_H_

#include <string>
#include <vector>
#include <string.h>

typedef unsigned char byte;

class SimpleProtobuf
{
public:
	static byte*	buffer;  // 存储序列化的字节数值
	static byte*	buffer_De; // 指向反序列化的字节数值
	static int		position;
	static int		bufferSize; // 申请的内存大小
	static int		bufferSize_De;

public:
	SimpleProtobuf();
	~SimpleProtobuf();

	// 接口
	virtual void Serialize() {}
	virtual void DeSerialize() {}
	virtual int GetSize() { return 0; }

	// 辅助函数
	static int SerializeStart(int size);
	static int DeSerializeStart(byte* buf, int len);

	static int Serialize(SimpleProtobuf *proto);
	static int DeSerialize(SimpleProtobuf *proto, byte* buf, int len);

	static int SerializeDataLength(int length);
	static int DeSerializeDataLength();

	//
	static int SerializeOptional(byte* flag, int flagSize);
	static int DeSerializeOptional(byte* flag, int& flagSize);

	//serialize value type
	static int SerializeInt32(const int& val);
	static int SerializeInt64(const long long& val);
	static int SerializeFloat(const float& v);
	static int SerializeString(const std::string& v);
	static int SerializeBool(const bool& v);
	// array value type
	static int SerializeInt32Array(const std::vector<int>& v);
	static int SerializeInt64Array(const std::vector<long long>& v);
	static int SerializeFloatArray(const std::vector<float>& v);
	static int SerializeBoolArray(const std::vector<bool>& v);
	static int SerializeStringArray(const std::vector<std::string>& v);
	static bool HasOptionalFlag(int index, byte* flag, int flagSize);

	// DeSerialize
	// deserialize value type
	static int DeSerializeInt32();
	static long long DeSerializeInt64();
	static float DeSerializeFloat();
	static std::string DeSerializeString();
	static bool DeSerializeBool();
	// array
	static std::vector<int> DeSerializeInt32Array();
	static std::vector<long long> DeSerializeInt64Array();
	static std::vector<float> DeSerializeFloatArray();
	static std::vector<bool> DeSerializeBoolArray();
	static std::vector<std::string> DeSerializeStringArray();
	static int GetStrActualSize(int size);
};

#endif
