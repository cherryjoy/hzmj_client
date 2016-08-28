using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour
{
    float time = 0;
    int c = 0;

    //public LKControl ctrl_;

    static public void AddFPS(/*LKControl ctrl*/)
    {
        GameObject fps_go = new GameObject("FPS");
        FPS fps = fps_go.AddComponent<FPS>();
        //fps.ctrl_ = ctrl;
    }

    UILabel fps_label_;

    void Start()
    {
        time = Time.realtimeSinceStartup;

        fps_label_ = new UILabel();
        /*fps_label_.font = UIFont.DefaultFont;
        fps_label_.space_x = 2;
		fps_label_.Scale = new Vector3(20, 20);
        fps_label_.Text = "FPS:60 | PING:30";
        fps_label_.Start();*/

        Vector3 position = UIAnchor.GetAnchorScreenPosition(UIAnchor.Side.TopLeft);
        fps_label_.transform.position = new Vector3(position.x, position.y + 100.0f, -110f);
    }

    void Update()
    {
        c++;
        if (Time.realtimeSinceStartup - time > 1f)
        {
            time = Time.realtimeSinceStartup;
			string text = "FPS:" + c + " | PING:" + PingManager.Instance.netPing
#if OPEN_BROAD_PING
				+ " | BPING:" + PingManager.Instance.mBroadPingTime
#endif
;
            c = 0;
            fps_label_.text = text;
        }
    }
}
