using UnityEngine;

public class LuaHoverMessageReceiver : MonoBehaviour
{
    LuaBehaviour lua_behaviour_ = null;
    void Awake()
    {
        lua_behaviour_ = gameObject.GetComponent<LuaBehaviour>();
        if (lua_behaviour_ == null)
            enabled = false;
    }

    void OnHover(bool isHover)
    {
        lua_behaviour_.CallFunction(null, "OnHover", isHover);
    }

}

