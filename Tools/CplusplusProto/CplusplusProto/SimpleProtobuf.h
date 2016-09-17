#ifndef _PROTOCAL_SERIALIZE_H_
#define _PROTOCAL_SERIALIZE_H_

#include <string>
#include <vector>

typedef unsigned char byte;

enum eProtobufType
{
	SERIALIZE = 0x01,
	DESERIALIZE = 0x02,
};

class SimpleProtobuf
{
public:
	byte*		buffer;
	int			position;
	int			bufferSize;
	byte*		flag;
	int			flagSize;
	int			type;

public:
	SimpleProtobuf()
	{
		buffer = NULL;
		position = 0;
		bufferSize = 0;
		flag = NULL;
		flagSize = 0;
		type = 0;
	}

	~SimpleProtobuf()
	{
		if (buffer != NULL && ((type & SERIALIZE) != 0))
		{
			delete [] buffer;
			buffer = NULL;
		}
	}

	void Serialize(int size)
	{
		buffer = new byte[size];
		bufferSize = size;
		type = type | SERIALIZE;
	}

	void DeSerialize(byte* buffer,int length)
	{
		this->buffer = buffer;
		this->bufferSize = length;
		type = type | DESERIALIZE;
	}

	int SerializeDataLength(int length)
	{
		for (int i = 0; i < sizeof(int); i++)
		{
			buffer[position++] = (length >> (8 * i)) & 0xff;
		}

		return 0;
	}

	int DeSerializeDataLength()
	{
		int length = 0;
		if (position > bufferSize - 1)
		{
			return length;
		}

		for (int i = 0; i < sizeof(int); i++)
		{
			length |= buffer[position++] << (8 * i);
		}

		return length;
	}

	// 
	int SerializeOptional()
	{
		for (int i = flagSize - 1; i >= 0; i--)
		{
			if ((flag[i] | 0x00) != 0)
			{
				for (int j = 0; j <= i; j++)
				{
					byte b = flag[j];
					if (i != j)
						b |= 0x80;
					buffer[position++] = b;
				}
				break;
			}
		}

		return 0;
	}

	//serialize value type
	int SerializeInt32(int val)
	{
		long long  v = (long long)val;
		v = (v << 1) ^ (v >> 63);
		if (v > 0)
		{
			while (v > 0)
			{
				char b = v & 0x7f;
				v = v >> 7;
				if (v > 0) b |= 0x80;
				buffer[position++] = b;
			}
		}
		else
		{
			buffer[position++] = 0;
		}	

		return 0;
	}

	int SerializeInt64(long long v)
	{
		v = (v << 1) ^ (v >> 63);
		if (v > 0)
		{
			while (v > 0)
			{
				char b = v & 0x7f;
				v = v >> 7;
				if (v > 0) b |= 0x80;
				buffer[position++] = b;
			}
		}
		else
		{
			buffer[position++] = 0;
		}

		return 0;
	}

	int SerializeFloat(float v)
	{
		int* val = (int*)&v;
		int i;
		for (i = 0; i < 4; i++)
		{
			buffer[position++] = *val >> 8 * i;
		}

		return 0;
	}

	int SerializeString(std::string v)
	{
		int len = strlen(v.c_str());
		SerializeInt32(len);

		for (int i = 0; i < len; i++)
		{
			buffer[position + i] = v[i];
		}

		position += len;

		return 0;
	}

	int SerializeBool(bool v)
	{
		buffer[position++] = v ? 1 : 0;

		return 0;
	}

	// array value type
	int SerializeInt32Array(std::vector<int> v)
	{
		int length = v.size();
		SerializeInt32(length);
		for (int i = 0; i < length; i++)
		{
			SerializeInt32(v[i]);
		}

		return 0;
	}

	int SerializeInt64Array(std::vector<long long> v)
	{
		int length = v.size();
		SerializeInt32(length);
		for (int i = 0; i < length; i++)
		{
			SerializeInt64(v[i]);
		}

		return 0;
	}

	int SerializeFloatArray(std::vector<float> v)
	{
		int length = v.size();
		SerializeInt32(length);
		for (int i = 0; i < length; i++)
		{
			SerializeFloat(v[i]);
		}

		return 0;
	}

	int SerializeBoolArray(std::vector<bool> v)
	{
		int length = v.size();
		SerializeInt32(length);
		for (int i = 0; i < length; i++)
		{
			SerializeBool(v[i]);
		}

		return 0;
	}

	int SerializeStringArray(std::vector<std::string> v)
	{
		int length = v.size();
		SerializeInt32(length);
		for (int i = 0; i < length; i++)
		{
			SerializeString(v[i]);
		}

		return 0;
	}

	/*
	int SerializeBytes(byte* v, int length)
	{
		SerializeInt32(length);
		for (int i = 0; i < length; i++)
		{
			buffer[position++] = v[i];
		}

		return 0;
	}*/

	bool HasOptionalFlag(int index)
	{
		bool has = false;
		int row = index / 7;
		if (row < flagSize)
		{
			int col = index % 7;
			byte b = flag[row];
			has = ((b & (0x01 << col)) != 0);
		}

		return has;
	}

	// DeSerialize
	// 
	int DeSerializeOptional()
	{
		int offset = 0;
		int startPos = position;
		while ((buffer[position] & 0x80) != 0)
		{
			position++;
			offset++;
		}

		offset++;
		position++;
		memcpy(flag, buffer + startPos, offset);
		flagSize = offset;
		
		return 0;
	}

	// deserialize value type
	int DeSerializeInt32()
	{
		if (position >= bufferSize - 1)
		{
			return 0;
		}

		long long v = 0;
		int offset = 0;
		int val;
		while ((buffer[position] & 0x80) != 0)
		{
			v |= (long long)(buffer[position++] & 0x7f) << offset;
			offset += 7;
		}

		v |= (long long)(buffer[position++] & 0x7f) << offset;
		val = (int)(v >> 1) ^ (int)(-(v & 1));
		return val;
	}

	long long DeSerializeInt64()
	{
		if (position >= bufferSize - 1)
		{
			return 0;
		}

		long long v = 0;
		int offset = 0;
		long long val;

		while ((buffer[position] & 0x80) != 0)
		{
			v |= (long long)(buffer[position++] & 0x7f) << offset;
			offset += 7;
		}

		v |= (long long)(buffer[position++] & 0x7f) << offset;
		val = (v >> 1) ^ (-(v & 1));
		return val;
	}

	float DeSerializeFloat()
	{
		if (position >= bufferSize - 1)
		{
			return 0;
		}

		int n = buffer[position] | buffer[position + 1] << 8 | buffer[position + 2] << 16 | buffer[position + 3] << 24;
		position += 4;
		return  *(float*)&n;
	}

	std::string DeSerializeString()
	{
		if (position >= bufferSize - 1)
		{
			return "";
		}

		int len = DeSerializeInt32();
		char* v = new char[len + 1];
		for (int i = 0; i < len + 1; i++)
		{
			v[i] = 0;
		}

		for (int i = 0; i < len; i++)
		{
			v[i] = buffer[position + i];
		}

		position += len;

		std::string value(v);

		delete [] v;
		return value;
	}

	bool DeSerializeBool()
	{
		if (position >= bufferSize - 1)
		{
			return false;
		}

		return (buffer[position++] & 0xff) != 0;
	}

	
	// array
	std::vector<int> DeSerializeInt32Array()
	{
		std::vector<int> value;
		if (position >= bufferSize - 1)
		{
			return value;
		}

		int len = DeSerializeInt32();
		if (len <= 0)
		{
			return value;
		}

		value.resize(len);
		int v = 0;
		for (int i = 0; i < len; i++)
		{
			v = DeSerializeInt32();
			value[i] = v;
		}

		return value;
	}

	std::vector<long long> DeSerializeInt64Array()
	{
		std::vector<long long> value;
		if (position >= bufferSize - 1)
		{
			return value;
		}

		int len = DeSerializeInt32();
		if (len <= 0)
		{
			return value;
		}

		value.resize(len);
		long long v = 0;
		for (int i = 0; i < len; i++)
		{
			v = DeSerializeInt64();
			value[i] = v;
		}

		return value;
	}

	std::vector<float> DeSerializeFloatArray()
	{
		std::vector<float> value;
		if (position >= bufferSize - 1)
		{
			return value;
		}

		int len = DeSerializeInt32();
		if (len <= 0)
		{
			return value;
		}

		value.resize(len);
		float v = 0;
		for (int i = 0; i < len; i++)
		{
			v = DeSerializeFloat();
			value[i] = v;
		}

		return value;
	}

	std::vector<bool> DeSerializeBoolArray()
	{
		std::vector<bool> value;
		if (position >= bufferSize - 1)
		{
			return value;
		}

		int len = DeSerializeInt32();
		if (len <= 0)
		{
			return value;
		}

		value.resize(len);
		bool v = 0;
		for (int i = 0; i < len; i++)
		{
			v = DeSerializeBool();
			value[i] = v;
		}

		return value;
	}

	std::vector<std::string> DeSerializeStringArray()
	{
		std::vector<std::string> value;
		if (position >= bufferSize - 1)
		{
			return value;
		}

		int len = DeSerializeInt32();
		if (len <= 0)
		{
			return value;
		}

		value.resize(len);
		std::string v = 0;
		for (int i = 0; i < len; i++)
		{
			v = DeSerializeString();
			value[i] = v;
		}

		return value;
	}

	//
	/* 需要在接口外delete(v)
	byte* DeSerializeBytes(int& size)
	{
		if (position >= bufferSize - 1)
		{
			return NULL;
		}

		int len = DeSerializeInt32();
		if (len <= 0)
		{
			return NULL;
		}

		byte* v = new byte[len];

		for (int i = 0; i < len; i++)
		{
			v[i] = buffer[position++];
		}

		size = len;
		return v;
	}*/

	int GetStrActualSize(int size)
	{
		long v = (long)size;
		v = (v << 1) ^ (v >> 31);
		if (v > 0)
		{
			while (v > 0)
			{
				v = v >> 7; size++;
			}
		}
		else
		{
			size++;
		}

		return size;
	}
};

#endif
