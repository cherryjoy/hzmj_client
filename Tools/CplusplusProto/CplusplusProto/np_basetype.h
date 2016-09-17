#ifndef _NP_BASETYPE_H_
#define _NP_BASETYPE_H_

#include <string>
#include "SimpleProtobuf.h"

//class PBTest;
//class PBVector3;

// 所以的枚举都预留0为默认值
enum ChatMessageType
{
	System = 1,
	WORLD = 2,
	GANG = 3,
	PRIVATE = 4,
	TOGM = 5,
	FROMGM = 6,
};

class PBVector3 : public SimpleProtobuf
{
// 成员变量
private:
	float	x_;
	float	y_;
	float	z_;
	long long count_; //option

public:
// 构造、析构函数
	PBVector3()
	{
		flag = new byte[1];
		flagSize = 1;
		memset(flag, 0, flagSize);

		x_ = 0;
		y_ = 0;
		z_ = 0;
		count_ = 0;
	}

	~PBVector3()
	{
		if (flag != NULL)
		{
			delete [] flag;
			flag = NULL;
		}
	}

// 成员函数
	float x()
	{
		return x_;	
	}

	void set_x(float value)
	{
		x_ = value;
	}

	float y()
	{
		return y_;
	}

	void set_y(float value)
	{
		y_ = value;
	}

	float z()
	{
		return z_;
	}

	void set_z(float value)
	{
		z_ = value;
	}

	long long count()
	{
		return count_;
	}

	void set_count(long long value)
	{
		count_ = value;
		byte b = flag[0];
		b |= 0x01 << 0;
		flag[0] = b;
	}

// 序列化、反序列化
	void Serialize()
	{
		SimpleProtobuf::Serialize(GetSize());

		int fieldCount = 3;
		SerializeInt32(fieldCount);
		SerializeOptional();
		SerializeFloat(x_);
		SerializeFloat(y_);
		SerializeFloat(z_);
		if (HasOptionalFlag(0))
		{
			SerializeInt64(count_);
		}
	}

	void DeSerialize(byte* buffer,int length)
	{
		SimpleProtobuf::DeSerialize(buffer, length);

		int fieldCount = DeSerializeInt32();
		DeSerializeOptional();
		x_ = DeSerializeFloat();
		y_ = DeSerializeFloat();
		z_ = DeSerializeFloat();
		if (HasOptionalFlag(0))
		{
			count_ = DeSerializeInt64();
		}
	}

// 计算大小
	int GetSize()
	{
		int size = 0;
		size += 28;
		return size;
	}

};

class PBTest : public SimpleProtobuf
{
private:
	int intv_;
	long long longv_;
	float floatv_;
	std::string stringv_;
	bool boolv_;
	PBVector3 vectorv_;
	ChatMessageType enumv_;
	ChatMessageType enumvop_;
	int intvop_;
	long long longvop_;
	float floatvop_;
	std::string stringvop_;
	bool boolvop_;
	PBVector3 vectorvop_;
	std::vector<int> intvre_;
	std::vector<long long> longvre_;
	std::vector<float> floatvre_;
	std::vector<std::string> stringvre_;
	std::vector<bool> boolvre_;
	std::vector<PBVector3> vectorvre_;
	std::vector<ChatMessageType> enumvre_;
	//byte* data_;
public:
	PBTest()
	{
		flag = new byte[1];
		flagSize = 1;
		memset(flag, 0, flagSize);

		intv_ = 0;
		longv_ = 0;
		floatv_ = 0;
		stringv_ = "";
		boolv_ = false;
		//vectorv_;
		enumv_ = (ChatMessageType)0;
		enumvop_ = (ChatMessageType)0;
		intvop_ = 0;
		longvop_ = 0;
		floatvop_ = 0;
		stringvop_ = "";
		boolvop_ = false;
		//vectorvop_;
		//intvre_;
		//longvre_;
		//floatvre_;
		//stringvre_;
		//boolvre_;
		//vectorvre_;
		//enumvre_;
	}

	~PBTest()
	{
		if (flag != NULL)
		{
			delete [] flag;
			flag = NULL;
		}
	}

	// 成员函数
	int intv()
	{
		return intv_;
	}

	void set_intv(int value)
	{
		intv_ = value;
	}

	long long longv()
	{
		return longv_;
	}

	void set_longv(long long value)
	{
		longv_ = value;
	}

	float floatv()
	{
		return floatv_;
	}

	void set_floatv(float value)
	{
		floatv_ = value;
	}

	std::string stringv()
	{
		return stringv_;
	}

	void set_stringv(std::string value)
	{
		stringv_ = value;
	}

	bool boolv()
	{
		return boolv_;
	}

	void set_boolv(bool value)
	{
		boolv_ = value;
	}

	PBVector3 vectorv()
	{
		return vectorv_;
	}

	void set_vectorv(PBVector3 value)
	{
		vectorv_ = value;
	}
	
	ChatMessageType enumv()
	{
		return enumv_;
	}

	void set_enumv(ChatMessageType value)
	{
		enumv_ = value;
	}

	ChatMessageType enumvop()
	{
		return enumvop_;
	}

	void set_enumvop(ChatMessageType value)
	{
		enumvop_ = value;
		byte b = flag[0];
		b |= 0x01 << 0;
		flag[0] = b;
	}

	int intvop()
	{
		return intvop_;
	}

	void set_intvop(int value)
	{
		intvop_ = value;
		byte b = flag[0];
		b |= 0x01 << 1;
		flag[0] = b;
	}

	long long longvop()
	{
		return longvop_;
	}

	void set_longvop(long long value)
	{
		longvop_ = value;
		byte b = flag[0];
		b |= 0x01 << 2;
		flag[0] = b;
	}

	float floatvop()
	{
		return floatvop_;
	}

	void set_floatvop(float value)
	{
		floatvop_ = value;
		byte b = flag[0];
		b |= 0x01 << 3;
		flag[0] = b;
	}

	std::string stringvop()
	{
		return stringvop_;
	}

	void set_stringvop(std::string value)
	{
		stringvop_ = value;
		byte b = flag[0];
		b |= 0x01 << 4;
		flag[0] = b;
	}

	bool boolvop()
	{
		return boolvop_;
	}

	void set_boolvop(bool value)
	{
		boolvop_ = value;
		byte b = flag[0];
		b |= 0x01 << 5;
		flag[0] = b;
	}

	PBVector3 vectorvop()
	{
		return vectorvop_;
	}

	void set_vectorvop(PBVector3 value)
	{
		vectorvop_ = value;
		byte b = flag[0];
		b |= 0x01 << 6;
		flag[0] = b;
	}

	std::vector<int> intvre()
	{
		return intvre_;
	}

	void set_intvre(std::vector<int> value)
	{
		intvre_ = value;
	}

	std::vector<long long> longvre()
	{
		return longvre_;
	}

	void set_longvre(std::vector<long long> value)
	{
		longvre_ = value;
	}

	std::vector<float> floatvre()
	{
		return floatvre_;
	}

	void set_floatvre(std::vector<float> value)
	{
		floatvre_ = value;
	}

	std::vector<std::string> stringvre()
	{
		return stringvre_;
	}

	void set_stringvre(std::vector<std::string> value)
	{
		stringvre_ = value;
	}

	std::vector<bool> boolvre()
	{
		return boolvre_;
	}

	void set_boolvre(std::vector<bool> value)
	{
		boolvre_ = value;
	}

	std::vector<PBVector3> vectorvre()
	{
		return vectorvre_;
	}

	void set_vectorvre(std::vector<PBVector3> value)
	{
		vectorvre_ = value;
	}

	std::vector<ChatMessageType> enumvre()
	{
		return enumvre_;
	}

	void set_enumvre(std::vector<ChatMessageType> value)
	{
		enumvre_ = value;
	}

	void Serialize()
	{
		SimpleProtobuf::Serialize(GetSize());

		int fieldCount = 22;
		SerializeInt32(fieldCount);
		SerializeOptional();
		SerializeInt32(intv_);
		SerializeInt64(longv_);
		SerializeFloat(floatv_);
		SerializeString(stringv_);
		SerializeBool(boolv_);
		{
			int waitPos = position;
			position += 4;
			vectorv_.Serialize();
			int maxPosNow = position;
			int realSize = maxPosNow - (waitPos + 4);
			position = waitPos;
			SerializeDataLength(realSize);
			position = maxPosNow;
		}

		SerializeInt32(enumv_);
		if (HasOptionalFlag(0))
		{
			SerializeInt32(enumvop_);
		}

		if (HasOptionalFlag(1))
		{
			SerializeInt32(intvop_);	
		}

		if (HasOptionalFlag(2))
		{
			SerializeInt64(longvop_);
		}

		if (HasOptionalFlag(3))
		{
			SerializeFloat(floatvop_);
		}

		if (HasOptionalFlag(4))
		{
			SerializeString(stringvop_);
		}

		if (HasOptionalFlag(5))
		{
			SerializeBool(boolvop_);
		}

		if (HasOptionalFlag(6))
		{
			{
				int waitPos = position;
				position += 4;
				vectorvop_.Serialize();
				int maxPosNow = position;
				int realSize = maxPosNow - (waitPos + 4);
				position = waitPos;
				SerializeDataLength(realSize);
				position = maxPosNow;
			}
		}

		SerializeInt32Array(intvre_);
		SerializeInt64Array(longvre_);
		SerializeFloatArray(floatvre_);
		SerializeStringArray(stringvre_);
		SerializeBoolArray(boolvre_);
		{
			int count = vectorvre_.size();
			SerializeInt32(count);
			for (int i = 0; i < count; ++i)
			{
				{
					int waitPos = position;
					position += 4;
					vectorvop_.Serialize();
					int maxPosNow = position;
					int realSize = maxPosNow - (waitPos + 4);
					position = waitPos;
					SerializeDataLength(realSize);
					position = maxPosNow;
				}
			}
		}
		{
			int count = enumvre_.size();
			SerializeInt32(count);
			if (count > 0)
			{
				std::vector<int> vec(count);
				for (int i = 0; i < count; ++i)
				{
					vec.push_back(enumvre_[i]);
				}

				SerializeInt32Array(vec);
			}
		}
	}

	void DeSerialize(byte* buffer,int length)
	{
		SimpleProtobuf::DeSerialize(buffer, length);

		int fieldCount = DeSerializeInt32();
		DeSerializeOptional();
		intv_ = DeSerializeInt32();
		longv_ = DeSerializeInt64();
		floatv_ = DeSerializeFloat();
		stringv_ = DeSerializeString();
		boolv_ = DeSerializeBool();
		{
			int vectorvSize = DeSerializeDataLength();
			int vectorvStartPos = position;
			vectorv_.DeSerialize(buffer + position, vectorvSize);
			position = vectorvSize + vectorvStartPos;
		}
		enumv_ = (ChatMessageType)DeSerializeInt32();
		if (HasOptionalFlag(0))
		{
			enumvop_ = (ChatMessageType)DeSerializeInt32();
		}

		if (HasOptionalFlag(1))
		{
			intvop_ = DeSerializeInt32();
		}

		if (HasOptionalFlag(2))
		{
			longvop_ = DeSerializeInt64();
		}

		if (HasOptionalFlag(3))
		{
			floatvop_ = DeSerializeFloat();
		}

		if (HasOptionalFlag(4))
		{
			stringvop_ = DeSerializeString();
		}

		if (HasOptionalFlag(5))
		{
			boolvop_ = DeSerializeBool();
		}

		if (HasOptionalFlag(6))
		{
			{
				int vectorvSize = DeSerializeDataLength();
				int vectorvStartPos = position;
				vectorvop_.DeSerialize(buffer + position, vectorvSize);
				position = vectorvSize + vectorvStartPos;
			}
		}

		intvre_ = DeSerializeInt32Array();
		longvre_ = DeSerializeInt64Array();
		floatvre_ = DeSerializeFloatArray();
		stringvre_ = DeSerializeStringArray();
		boolvre_ = DeSerializeBoolArray();
		{
			int count = DeSerializeInt32();
			if (count > 0)
			{
				vectorvre_.resize(count);
				for (int i = 0; i < count; ++i)
				{
					int vectorvSize = DeSerializeDataLength();
					int vectorvStartPos = position;
					vectorvre_[i].DeSerialize(buffer + position, vectorvSize);
					position = vectorvSize + vectorvStartPos;
				}
			}
		}
		{
			int count = DeSerializeInt32();
			if (count > 0)
			{
				enumvre_.resize(count);
				for (int i = 0; i < count; ++i)
				{
					enumvre_[i] = (ChatMessageType)DeSerializeInt32();
				}
			}
		}
	}

	int GetSize()
	{
		int size = 0;
		size += GetStrActualSize(strlen(stringv_.c_str()));
		size += vectorv_.GetSize();
		size += GetStrActualSize(strlen(stringvop_.c_str()));
		size += vectorvop_.GetSize();
		for (unsigned int i = 0; i < intvre_.size(); i++) 
		{
			size += 4;
		}
		for (unsigned int i = 0; i < longvre_.size(); i++)
		{
			size += 8;
		}
		for (unsigned int i = 0; i < floatvre_.size(); i++)
		{
			size += 4;
		}
		for (unsigned int i = 0; i < stringvre_.size(); i++)
		{
			size += GetStrActualSize(strlen(stringvre_[i].c_str()));
		}
		for (unsigned int i = 0; i < boolvre_.size(); i++)
		{
			size += 1;
		}
		for (unsigned int i = 0; i < vectorvre_.size(); i++)
		{
			size += vectorvre_[i].GetSize();
		}
		for (unsigned int i = 0; i < enumvre_.size(); i++)
		{
			size += 4;
		}

		size += 77;
		return size;
	}
};

#endif