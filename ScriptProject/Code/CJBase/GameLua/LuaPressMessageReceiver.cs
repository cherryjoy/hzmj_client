using UnityEngine;

public class LuaPressMessageReceiver : MonoBehaviour
{
    public LuaBehaviour receiverBehaviour = null;
    public string eventFunction = "";
    void Awake()
    {
        if (receiverBehaviour == null)
        {
            receiverBehaviour = gameObject.GetComponent<LuaBehaviour>();
        }
        if(receiverBehaviour == null)
        {
            enabled = false;
        }
    }

    void OnPress (bool isPressed)
    {
        receiverBehaviour.CallFunction(gameObject,
            (string.IsNullOrEmpty(eventFunction) ? "_OnPress" : "_" + eventFunction), isPressed);
    }
}