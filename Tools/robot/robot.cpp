// CplusplusProtobuf.cpp : 定义控制台应用程序的入口点。
//
#include <unistd.h>
#include <stdio.h>
#include <string.h>
#include "basecore.h"
#include "socketbase.h"
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

	char buff[1024] = {0};
	//PBTest testB;
	//SimpleProtobuf::DeSerialize(&testB, test.buffer, test.position);
	int sz = test.position + 2 + 4 + 4;
	unsigned char s1 = BIT32_EXTRACT(sz, 8, 8);
	unsigned char s2 = BIT32_EXTRACT(sz, 0, 8);
	memset(buff, s1, sizeof(unsigned char));
	memset(buff + 1, s2, sizeof(unsigned char));
	int id = 2;
	unsigned char o1 = BIT32_EXTRACT(id, 24, 8);
	unsigned char o2 = BIT32_EXTRACT(id, 16, 8);
	unsigned char o3 = BIT32_EXTRACT(id, 8, 8);
	unsigned char o4 = BIT32_EXTRACT(id, 0, 8);
	memset(buff + 2, o1, sizeof(unsigned char));
	memset(buff + 3, o2, sizeof(unsigned char));
	memset(buff + 4, o3, sizeof(unsigned char));
	memset(buff + 5, o4, sizeof(unsigned char));

	memset(buff + 6, 0, sizeof(unsigned char));
	memset(buff + 7, 0, sizeof(unsigned char));
	memset(buff + 8, 0, sizeof(unsigned char));
	memset(buff + 9, 0, sizeof(unsigned char));

	memcpy(buff + 10, test.buffer, test.position);

	SocketBase socketbase;
	socketbase.Initialize("127.0.0.1", 11400);
	socketbase.Connect();
	socketbase.Send(buff, sz);
	
	while (true) 
	{
		sleep(1);
	}

	return 0;
}
