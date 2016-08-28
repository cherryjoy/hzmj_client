using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UIViewCtr:MonoBehaviour
{
    public static UIViewCtr self_;

    void Awake() {
        if (self_ == null)
            self_ = this;
    }

    void OnDestroy() {
        self_ = null;
    }

}

