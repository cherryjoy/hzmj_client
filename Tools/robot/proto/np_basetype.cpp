#include "np_basetype.h"

// PBVector3
PBVector3::PBVector3()
{
	flagSize = 1;
	memset(flag, 0, flagSize);

	x_ = 0;
	y_ = 0;
	z_ = 0;
	count_ = 0;
}

PBVector3::~PBVector3()
{

}

float PBVector3::x()
{
	return x_;
}

void PBVector3::set_x(const float& value)
{
	x_ = value;
}

float PBVector3::y()
{
	return y_;
}

void PBVector3::set_y(const float& value)
{
	y_ = value;
}

float PBVector3::z()
{
	return z_;
}

void PBVector3::set_z(const float& value)
{
	z_ = value;
}

long long PBVector3::count()
{
	return count_;
}

void PBVector3::set_count(const long long& value)
{
	count_ = value;
	byte b = flag[0];
	b |= 0x01 << 0;
	flag[0] = b;
}

void PBVector3::DeSerialize()
{
	DeSerializeInt32();
	DeSerializeOptional(flag, flagSize);
	x_ = DeSerializeFloat();
	y_ = DeSerializeFloat();
	z_ = DeSerializeFloat();
	if (HasOptionalFlag(0, flag, flagSize))
	{
		count_ = DeSerializeInt64();
	}
}

void PBVector3::Serialize()
{
	int fieldCount = 4;
	SerializeInt32(fieldCount);
	SerializeOptional(flag, flagSize);
	SerializeFloat(x_);
	SerializeFloat(y_);
	SerializeFloat(z_);
	if (HasOptionalFlag(0, flag, flagSize))
	{
		SerializeInt64(count_);
	}
}

int PBVector3::GetSize()
{
	int size = 0;
	size += 28;
	return size;
}

// PBTest
PBTest::PBTest()
{
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

PBTest::~PBTest()
{

}

int PBTest::intv()
{
	return intv_;
}

void PBTest::set_intv(int value)
{
	intv_ = value;
}

long long PBTest::longv()
{
	return longv_;
}

void PBTest::set_longv(long long value)
{
	longv_ = value;
}

float PBTest::floatv()
{
	return floatv_;
}

void PBTest::set_floatv(float value)
{
	floatv_ = value;
}

std::string PBTest::stringv()
{
	return stringv_;
}

void PBTest::set_stringv(std::string value)
{
	stringv_ = value;
}

bool PBTest::boolv()
{
	return boolv_;
}

void PBTest::set_boolv(bool value)
{
	boolv_ = value;
}

PBVector3 PBTest::vectorv()
{
	return vectorv_;
}

void PBTest::set_vectorv(PBVector3 value)
{
	vectorv_ = value;
}

ChatMessageType PBTest::enumv()
{
	return enumv_;
}

void PBTest::set_enumv(ChatMessageType value)
{
	enumv_ = value;
}

ChatMessageType PBTest::enumvop()
{
	return enumvop_;
}

void PBTest::set_enumvop(ChatMessageType value)
{
	enumvop_ = value;
	byte b = flag[0];
	b |= 0x01 << 0;
	flag[0] = b;
}

int PBTest::intvop()
{
	return intvop_;
}

void PBTest::set_intvop(int value)
{
	intvop_ = value;
	byte b = flag[0];
	b |= 0x01 << 1;
	flag[0] = b;
}

long long PBTest::longvop()
{
	return longvop_;
}

void PBTest::set_longvop(long long value)
{
	longvop_ = value;
	byte b = flag[0];
	b |= 0x01 << 2;
	flag[0] = b;
}

float PBTest::floatvop()
{
	return floatvop_;
}

void PBTest::set_floatvop(float value)
{
	floatvop_ = value;
	byte b = flag[0];
	b |= 0x01 << 3;
	flag[0] = b;
}

std::string PBTest::stringvop()
{
	return stringvop_;
}

void PBTest::set_stringvop(std::string value)
{
	stringvop_ = value;
	byte b = flag[0];
	b |= 0x01 << 4;
	flag[0] = b;
}

bool PBTest::boolvop()
{
	return boolvop_;
}

void PBTest::set_boolvop(bool value)
{
	boolvop_ = value;
	byte b = flag[0];
	b |= 0x01 << 5;
	flag[0] = b;
}

PBVector3 PBTest::vectorvop()
{
	return vectorvop_;
}

void PBTest::set_vectorvop(PBVector3 value)
{
	vectorvop_ = value;
	byte b = flag[0];
	b |= 0x01 << 6;
	flag[0] = b;
}

std::vector<int> PBTest::intvre()
{
	return intvre_;
}

void PBTest::set_intvre(std::vector<int> value)
{
	intvre_ = value;
}

std::vector<long long> PBTest::longvre()
{
	return longvre_;
}

void PBTest::set_longvre(std::vector<long long> value)
{
	longvre_ = value;
}

std::vector<float> PBTest::floatvre()
{
	return floatvre_;
}

void PBTest::set_floatvre(std::vector<float> value)
{
	floatvre_ = value;
}

std::vector<std::string> PBTest::stringvre()
{
	return stringvre_;
}

void PBTest::set_stringvre(std::vector<std::string> value)
{
	stringvre_ = value;
}

std::vector<bool> PBTest::boolvre()
{
	return boolvre_;
}

void PBTest::set_boolvre(std::vector<bool> value)
{
	boolvre_ = value;
}

std::vector<PBVector3> PBTest::vectorvre()
{
	return vectorvre_;
}

void PBTest::set_vectorvre(std::vector<PBVector3> value)
{
	vectorvre_ = value;
}

std::vector<ChatMessageType> PBTest::enumvre()
{
	return enumvre_;
}

void PBTest::set_enumvre(std::vector<ChatMessageType> value)
{
	enumvre_ = value;
}

void PBTest::DeSerialize()
{
	DeSerializeInt32();
	DeSerializeOptional(flag, flagSize);
	intv_ = DeSerializeInt32();
	longv_ = DeSerializeInt64();
	floatv_ = DeSerializeFloat();
	stringv_ = DeSerializeString();
	boolv_ = DeSerializeBool();
	{
		int vectorvSize = DeSerializeDataLength();
		int vectorvStartPos = position;
		vectorv_.DeSerialize();
		position = vectorvSize + vectorvStartPos;
	}
	enumv_ = (ChatMessageType)DeSerializeInt32();
	if (HasOptionalFlag(0, flag, flagSize))
	{
		enumvop_ = (ChatMessageType)DeSerializeInt32();
	}

	if (HasOptionalFlag(1, flag, flagSize))
	{
		intvop_ = DeSerializeInt32();
	}

	if (HasOptionalFlag(2, flag, flagSize))
	{
		longvop_ = DeSerializeInt64();
	}

	if (HasOptionalFlag(3, flag, flagSize))
	{
		floatvop_ = DeSerializeFloat();
	}

	if (HasOptionalFlag(4, flag, flagSize))
	{
		stringvop_ = DeSerializeString();
	}

	if (HasOptionalFlag(5, flag, flagSize))
	{
		boolvop_ = DeSerializeBool();
	}

	if (HasOptionalFlag(6, flag, flagSize))
	{
		{
			int vectorvSize = DeSerializeDataLength();
			int vectorvStartPos = position;
			vectorvop_.DeSerialize();
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
				vectorvre_[i].DeSerialize();
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

void PBTest::Serialize()
{
	int fieldCount = 21;
	SerializeInt32(fieldCount);
	SerializeOptional(flag, flagSize);
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
	if (HasOptionalFlag(0, flag, flagSize))
	{
		SerializeInt32(enumvop_);
	}

	if (HasOptionalFlag(1, flag, flagSize))
	{
		SerializeInt32(intvop_);
	}

	if (HasOptionalFlag(2, flag, flagSize))
	{
		SerializeInt64(longvop_);
	}

	if (HasOptionalFlag(3, flag, flagSize))
	{
		SerializeFloat(floatvop_);
	}

	if (HasOptionalFlag(4, flag, flagSize))
	{
		SerializeString(stringvop_);
	}

	if (HasOptionalFlag(5, flag, flagSize))
	{
		SerializeBool(boolvop_);
	}

	if (HasOptionalFlag(6, flag, flagSize))
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

int PBTest::GetSize()
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