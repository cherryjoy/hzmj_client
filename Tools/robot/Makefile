CFLAGS = -g -02 -Wall
PROTO_INC = ./proto
INC := -I./ -I$(PROTO_INC)

CC = g++

all : robot

robot : robot.cpp basecore.cpp socketbase.cpp ./proto/SimpleProtobuf.cpp ./proto/np_basetype.cpp
	$(CC) -o $@ $^ $(INC)

clean :
	rm robot
