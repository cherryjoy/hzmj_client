CFLAGS = -g -02 -Wall
INC := -I./CplusplusProto/

CC = g++

all : CppProto

CppProto : ./CplusplusProto/CplusplusProto.cpp ./CplusplusProto/SimpleProtobuf.cpp ./CplusplusProto/np_basetype.cpp
	$(CC) -o $@ $^ $(INC)

clean :
	rm CppProto
