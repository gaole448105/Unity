package main

import (
 "log"
 "fmt"
 "github.com/golang/protobuf/proto"
 "tianyan"
)

func main() {
 Test := &tianyan.Chat {
  Mid: proto.Int64(9),
  Channel: proto.Int32(17),
  Frome: &tianyan.ChatUser {
	PlayerId: proto.Int64(9),  // 游戏角色ID 必要
	UserId: proto.Int64(9),  // 用户ID, 必要
	Nickname: proto.String("hello"), // 角色昵称 必要
	Level: proto.Int32(9), // 角色等级，必要
	VipLevel: proto.Int32(9), // 角色VIP等级
	Power: proto.Int64(9), // 角色战力值 非必要
  },
  To: &tianyan.ChatUser {
        PlayerId: proto.Int64(9),  // 游戏角色ID 必要
        UserId: proto.Int64(9),  // 用户ID, 必要
        Nickname: proto.String("hello"), // 角色昵称 必要
        Level: proto.Int32(9), // 角色等级，必要
        VipLevel: proto.Int32(9), // 角色VIP等级
        Power: proto.Int64(9), // 角色战力值 非必要
  },
  Content: proto.String("hello"),
  ZoneId: proto.Int32(17),
  ZoneName: proto.String("hello"),
  Ip: proto.String("hello"),
  Banned: proto.Int32(17),
  Extra: proto.String("hello"),
  }
  data, err := proto.Marshal(Test)
  if err != nil {
   log.Fatal("marshaling error: ", err)
  }
  fmt.Println(data)
   
  newTest := &tianyan.Chat{}
  err = proto.Unmarshal(data, newTest)
  if err != nil {
   log.Fatal("unmarshaling error: ", err)
  }
  fmt.Println(newTest)
 // Now test and newTest contain the same data.
 //if test.GetLabel() != newTest.GetLabel() {
 // fmt.Println("data mismatch %q != %q", test.GetLabel(), newTest.GetLabel())
 //}
fmt.Println(Test.GetMid())
fmt.Println("call CustomPkgFunc")
 //test.GetOptionalgroup().GetRequiredField()
 //etc
}

