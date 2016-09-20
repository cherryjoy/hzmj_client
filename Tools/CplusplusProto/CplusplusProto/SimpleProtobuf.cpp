#include "SimpleProtobuf.h"

byte* SimpleProtobuf::buffer = NULL;
byte* SimpleProtobuf::buffer_De = NULL;
int SimpleProtobuf::position = 0;
int SimpleProtobuf::bufferSize = 0;
int SimpleProtobuf::bufferSize_De = 0;

SimpleProtobuf::SimpleProtobuf()
{

}

SimpleProtobuf::~SimpleProtobuf()
{

}

int SimpleProtobuf::SerializeStart(int size)
{
	if (buffer == NULL)
	{
		buffer = (byte *)realloc(NULL, 4);
		bufferSize = 4;
	}

	if (size > bufferSize)
	{
		buffer = (byte *)realloc(buffer, size);
		bufferSize = size;
	}

	position = 0;
	
	return 0;
}

int SimpleProtobuf::DeSerializeStart(byte* buf, int len)
{
	printf("DeSerializeStart len: %d\n", len);
	for (int i = 0; i < len; ++i)
	{
		printf("byte %d: %d\n", i, (int)(buf[i]));
	}

	buffer_De = buf;
	bufferSize_De = len;

	position = 0;

	return 0;
}

int SimpleProtobuf::Serialize(SimpleProtobuf *proto)
{
	SerializeStart(proto->GetSize());
	proto->Serialize();

	return 0;
}

int SimpleProtobuf::DeSerialize(SimpleProtobuf *proto, byte* buf, int len)
{
	DeSerializeStart(buf, len);
	proto->DeSerialize();

	return 0;
}

int SimpleProtobuf::SerializeDataLength(int length)
{
	for (unsigned int i = 0; i < sizeof(int); i++)
	{
		buffer[position++] = (length >> (8 * i)) & 0xff;
	}

	return 0;
}

int SimpleProtobuf::DeSerializeDataLength()
{
	int length = 0;
	if (position > bufferSize_De - 1)
	{
		return length;
	}

	for (unsigned int i = 0; i < sizeof(int); i++)
	{
		length |= buffer_De[position++] << (8 * i);
	}

	return length;
}

int SimpleProtobuf::SerializeOptional(byte* flag, int flagSize)
{
	int startPos = position;

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

	if (startPos == position)
	{
		buffer[position++] = 0x00;
	}

	return 0;
}

int SimpleProtobuf::DeSerializeOptional(byte* flag, int& flagSize)
{
	int offset = 0;
	int startPos = position;
	while ((buffer_De[position] & 0x80) != 0)
	{
		position++;
		offset++;
	}

	offset++;
	position++;
	if (flag != NULL)
	{
		memcpy(flag, buffer_De + startPos, offset);
		flagSize = offset;
	}

	return 0;
}

int SimpleProtobuf::SerializeInt32(const int& val)
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

int SimpleProtobuf::SerializeInt64(const long long& val)
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

int SimpleProtobuf::SerializeFloat(const float& v)
{
	int* val = (int*)&v;
	int i;
	for (i = 0; i < 4; i++)
	{
		buffer[position++] = *val >> 8 * i;
	}

	return 0;
}

int SimpleProtobuf::SerializeString(const std::string& v)
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

int SimpleProtobuf::SerializeBool(const bool& v)
{
	buffer[position++] = v ? 1 : 0;

	return 0;
}

int SimpleProtobuf::SerializeInt32Array(const std::vector<int>& v)
{
	int length = v.size();
	SerializeInt32(length);
	for (int i = 0; i < length; i++)
	{
		SerializeInt32(v[i]);
	}

	return 0;
}

int SimpleProtobuf::SerializeInt64Array(const std::vector<long long>& v)
{
	int length = v.size();
	SerializeInt32(length);
	for (int i = 0; i < length; i++)
	{
		SerializeInt64(v[i]);
	}

	return 0;
}

int SimpleProtobuf::SerializeFloatArray(const std::vector<float>& v)
{
	int length = v.size();
	SerializeInt32(length);
	for (int i = 0; i < length; i++)
	{
		SerializeFloat(v[i]);
	}

	return 0;
}

int SimpleProtobuf::SerializeBoolArray(const std::vector<bool>& v)
{
	int length = v.size();
	SerializeInt32(length);
	for (int i = 0; i < length; i++)
	{
		SerializeBool(v[i]);
	}

	return 0;
}

int SimpleProtobuf::SerializeStringArray(const std::vector<std::string>& v)
{
	int length = v.size();
	SerializeInt32(length);
	for (int i = 0; i < length; i++)
	{
		SerializeString(v[i]);
	}

	return 0;
}

bool SimpleProtobuf::HasOptionalFlag(int index, byte* flag, int flagSize)
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

int SimpleProtobuf::DeSerializeInt32()
{
	if (position > bufferSize_De - 1)
	{
		return 0;
	}

	long long v = 0;
	int offset = 0;
	int val;
	while ((buffer_De[position] & 0x80) != 0)
	{
		v |= (long long)(buffer_De[position++] & 0x7f) << offset;
		offset += 7;
	}

	v |= (long long)(buffer_De[position++] & 0x7f) << offset;
	val = (int)(v >> 1) ^ (int)(-(v & 1));
	return val;
}

long long SimpleProtobuf::DeSerializeInt64()
{
	if (position > bufferSize_De - 1)
	{
		return 0;
	}

	long long v = 0;
	int offset = 0;
	long long val;

	while ((buffer_De[position] & 0x80) != 0)
	{
		v |= (long long)(buffer_De[position++] & 0x7f) << offset;
		offset += 7;
	}

	v |= (long long)(buffer_De[position++] & 0x7f) << offset;
	val = (v >> 1) ^ (-(v & 1));
	return val;
}

float SimpleProtobuf::DeSerializeFloat()
{
	if (position > bufferSize_De - 1)
	{
		return 0;
	}

	int n = buffer_De[position] | buffer_De[position + 1] << 8 | buffer_De[position + 2] << 16 | buffer_De[position + 3] << 24;
	position += 4;
	return  *(float*)&n;
}

std::string SimpleProtobuf::DeSerializeString()
{
	if (position > bufferSize_De - 1)
	{
		return "";
	}

	int len = DeSerializeInt32();
	char* v = new char[len + 1]; // 应该可以去掉这个new
	for (int i = 0; i < len + 1; i++)
	{
		v[i] = 0;
	}

	for (int i = 0; i < len; i++)
	{
		v[i] = buffer_De[position + i];
	}

	position += len;

	std::string value(v);

	delete[] v;
	return value;
}

bool SimpleProtobuf::DeSerializeBool()
{
	if (position > bufferSize_De - 1)
	{
		return false;
	}

	return (buffer_De[position++] & 0xff) != 0;
}

std::vector<int> SimpleProtobuf::DeSerializeInt32Array()
{
	std::vector<int> value;
	if (position >= bufferSize_De - 1)
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

std::vector<long long> SimpleProtobuf::DeSerializeInt64Array()
{
	std::vector<long long> value;
	if (position >= bufferSize_De - 1)
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

std::vector<float> SimpleProtobuf::DeSerializeFloatArray()
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

std::vector<bool> SimpleProtobuf::DeSerializeBoolArray()
{
	std::vector<bool> value;
	if (position >= bufferSize_De - 1)
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

std::vector<std::string> SimpleProtobuf::DeSerializeStringArray()
{
	std::vector<std::string> value;
	if (position >= bufferSize_De - 1)
	{
		return value;
	}

	int len = DeSerializeInt32();
	if (len <= 0)
	{
		return value;
	}

	value.resize(len);
	std::string v = "";
	for (int i = 0; i < len; i++)
	{
		v = DeSerializeString();
		value[i] = v;
	}

	return value;
}

int SimpleProtobuf::GetStrActualSize(int size)
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