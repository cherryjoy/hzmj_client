using UnityEngine;
using System.Collections;

public struct LuaMessage
{
    public GameObject sender;
    public string functionName;
    public System.Object parameter;

    public LuaMessage(GameObject go ,System.Object para,string function = "")
    {
        sender = go;
        functionName = function;
        parameter = para;
    }
}


public class LuaMessageReceiver : MonoBehaviour {

	// Use this for initialization
    LuaBehaviour lua_behaviour_ = null;
	void Awake () {
        lua_behaviour_ = gameObject.GetComponent<LuaBehaviour>();
        if (lua_behaviour_ == null)
            enabled = false;
	}
	
	void OnClick(GameObject obj)
    {
        if(obj == gameObject){
            lua_behaviour_.CallFunctionWithoutSenderName(obj, "OnClickSelf");
        }else{
            lua_behaviour_.CallFunction(obj, "_OnClick");
        }
    }

    void OnEventWithMessage(LuaMessage message)
    {
        lua_behaviour_.CallFunction(message.sender,
            (string.IsNullOrEmpty(message.functionName) ? "_Event" : "_"+message.functionName), message.parameter);
    }
}
