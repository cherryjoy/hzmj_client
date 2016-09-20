// CplusplusProtobuf.cpp : 定义控制台应用程序的入口点。
//

#include "np_basetype.h"


int main(int argc, char* argv[])
{
	PBVector3 vec3;
	vec3.set_x(0.9f);
	vec3.set_y(1.1f);
	vec3.set_z(-1);
	vec3.set_count(1);

	SimpleProtobuf::Serialize(&vec3);
	
	PBVector3 vecB;
	SimpleProtobuf::DeSerialize(&vecB, vec3.buffer, vec3.position);
	printf("%f, %f, %f, %lld\n", vecB.x(), vecB.y(), vecB.z(), vecB.count());

	PBTest test;
	test.set_intv(123456);
	test.set_longv(10);
	test.set_floatv(-0.88);
	test.set_stringv("hello world!");
	test.set_boolv(true);
	test.set_vectorv(vec3);
	test.set_enumv(WORLD);
	test.set_longvop(88888888888);
	std::vector<float> floatvec;
	floatvec.push_back(12.3);
	floatvec.push_back(1.11111);
	test.set_floatvre(floatvec);
	SimpleProtobuf::Serialize(&test);

	PBTest testB;
	SimpleProtobuf::DeSerialize(&testB, test.buffer, test.position);

	return 0;
}

/*
local data = PBVector3.Create()
data.x = 0.9
data.y = 1.1
data.z = -1
data.count = number_to_id(1)
--local buffer, length = LuaProto.serialize(data)
--ClientSendMsg.SendNetMsg(NetOpcodes_C2S.C2S_AUTH,buffer,length)
local test = PBTest.Create()
test.intv = 123456
test.longv = long_get(10)
test.floatv = -0.88
test.stringv = "hello world!"
test.boolv = true
test.vectorv = data
test.enumv = enumChatMessageType.WORLD
test.longvop = long_get(88888888888)
test.floatvre = {}
test.floatvre[1] = 12.3
test.floatvre[2] = 1.11111

local buffer, length = LuaProto.serialize(test)
ClientSendMsg.SendNetMsg(NetOpcodes_C2S.C2S_AUTH,buffer,length)
*/