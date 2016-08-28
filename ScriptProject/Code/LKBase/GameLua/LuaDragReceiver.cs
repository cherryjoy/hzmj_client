using UnityEngine;
using System.Collections;
using UniLua;

public class LuaDragReceiver : MonoBehaviour
{
    public LuaBehaviour lua_behaviour_;
    public string function;
    void Awake()
    {
        if (lua_behaviour_==null)
        {
            lua_behaviour_ = gameObject.GetComponent<LuaBehaviour>();
        }
    }
    void OnDrag(Vector2 delta)
    {
        if (lua_behaviour_==null)
        {
            Debug.LogError("lua_behaviour_ is null");
        }else
        {
            lua_behaviour_.CallFunction(delta, "OnDrag"+function);
        }
    }
}
