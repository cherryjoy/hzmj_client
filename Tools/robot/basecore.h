#ifndef _BASE_CORE_H
#define _BASE_CORE_H
#include <iostream>
#include <fstream>
#include <string>
#include <stdlib.h>

#define LONGBITS		32
#define ALLONES         (~(((~(unsigned int)0) << (LONGBITS - 1)) << 1))
#define MASK(n)         (~((ALLONES << 1) << ((n) - 1)))
#define BIT32_EXTRACT(value, pos, size) \
	((value >> pos) & MASK(size))

#define HEADSIZE		8			// 8 byte package head.
#define PKGSIZE(buff)	(((unsigned char)buff[0] << 8) | (unsigned char)buff[1])	// package size
#define PVERSION(buff)	(((unsigned char)buff[2] << 8) | (unsigned char)buff[3])	// protocol version
#define POPCODE(buff)	((unsigned char)buff[4] << 24) | ((unsigned char)buff[5] << 16) | ((unsigned char)buff[6] << 8) | (unsigned char)buff[7]	// msg opcode


unsigned int bit32_extract(unsigned int value, int pos, int size);

struct ConnInfo
{
	int m_type;
	int m_error;
};

#endif