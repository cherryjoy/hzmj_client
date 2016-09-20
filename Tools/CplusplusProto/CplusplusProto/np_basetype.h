#ifndef _NP_BASETYPE_H_
#define _NP_BASETYPE_H_

#include "SimpleProtobuf.h"

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

// PBVector3
class PBVector3 : public SimpleProtobuf
{
// 成员变量
private:
	byte	flag[1];
	int		flagSize;

	float	x_;
	float	y_;
	float	z_;
	long long count_; //option

public:
// 构造、析构函数
	PBVector3();
	~PBVector3();

// 成员函数
	float x();
	void set_x(const float& value);
	float y();
	void set_y(const float& value);
	float z();
	void set_z(const float& value);
	long long count();
	void set_count(const long long& value);

// 序列化、反序列化
	void DeSerialize();
	void Serialize();
// 计算大小
	int GetSize();
};

// PBTest
class PBTest : public SimpleProtobuf
{
private:
	byte	flag[1];
	int		flagSize;

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
public:
	PBTest();
	~PBTest();

	// 成员函数
	int intv();
	void set_intv(int value);
	long long longv();
	void set_longv(long long value);
	float floatv();
	void set_floatv(float value);
	std::string stringv();
	void set_stringv(std::string value);
	bool boolv();
	void set_boolv(bool value);
	PBVector3 vectorv();
	void set_vectorv(PBVector3 value);
	ChatMessageType enumv();
	void set_enumv(ChatMessageType value);
	ChatMessageType enumvop();
	void set_enumvop(ChatMessageType value);
	int intvop();
	void set_intvop(int value);
	long long longvop();
	void set_longvop(long long value);
	float floatvop();
	void set_floatvop(float value);
	std::string stringvop();
	void set_stringvop(std::string value);
	bool boolvop();
	void set_boolvop(bool value);
	PBVector3 vectorvop();
	void set_vectorvop(PBVector3 value);
	std::vector<int> intvre();
	void set_intvre(std::vector<int> value);
	std::vector<long long> longvre();
	void set_longvre(std::vector<long long> value);
	std::vector<float> floatvre();
	void set_floatvre(std::vector<float> value);
	std::vector<std::string> stringvre();
	void set_stringvre(std::vector<std::string> value);
	std::vector<bool> boolvre();
	void set_boolvre(std::vector<bool> value);
	std::vector<PBVector3> vectorvre();
	void set_vectorvre(std::vector<PBVector3> value);
	std::vector<ChatMessageType> enumvre();
	void set_enumvre(std::vector<ChatMessageType> value);

	void DeSerialize();
	void Serialize();
	int GetSize();
};

#endif