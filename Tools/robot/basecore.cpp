#include "basecore.h"

unsigned int bit32_extract(unsigned int value, int pos, int size)
{
	//return (((value >> pos) & MASK(size)));
	unsigned char shift = sizeof(int)* 8 - pos - size;
	return size << shift >> pos + shift;
}
