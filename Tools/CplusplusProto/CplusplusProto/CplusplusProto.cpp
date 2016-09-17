// CplusplusProtobuf.cpp : 定义控制台应用程序的入口点。
//

#include "np_basetype.h"


int main(int argc, char* argv[])
{
	PBVector3 vec3;
	vec3.set_x(0.9f);
	vec3.set_y(1.1f);
	vec3.set_z(-1);
	//vec3.set_count(10000000000);

	vec3.Serialize();
	
	PBVector3 vecB;
	vecB.DeSerialize(vec3.buffer, vec3.position);
	printf("%f, %f, %f, %lld\n", vecB.x(), vecB.y(), vecB.z(), vecB.count());

	PBTest test;
	test.set_boolv(true);
	test.set_enumvop(WORLD);
	std::vector<float> floatvec;
	floatvec.push_back(12.3);
	floatvec.push_back(1.11111);
	test.set_floatvre(floatvec);
	test.set_intv(123456);
	test.set_longvop(88888888888);
	test.set_vectorvop(vec3);
	test.set_stringvop("hello world!");
	test.Serialize();

	PBTest testB;
	testB.DeSerialize(test.buffer, test.position);


	return 0;
}

