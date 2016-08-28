using UnityEngine;
using System.Collections;

public class CameraAdjustment : MonoBehaviour 
{
    public Transform target_;
    Camera main_camera_;
    // distance between man and camera
    float distance_ = 20.0f;
    float angle_ = 20.0f;
    float weight_ = 1.0f;

	// Use this for initialization
	void Start () 
    {
        main_camera_ = GameObject.Find("SceneCamera").GetComponent<Camera>();
        if (main_camera_ == null)
        {
            Debug.LogError("Cant find camera");
            Destroy(this);
            return;
        }
        RotateCamera(main_camera_, distance_, angle_, weight_, Vector3.zero);
	}
	
    float GUILayoutFloat(float value)
    {
        string v = GUILayout.TextField(value.ToString());
        return float.Parse(v);
    }
    #if !DISONGUI
    void OnGUI()
    {
        Vector3 target_pos_ = target_ == null ? Vector3.zero : target_.position;
        GUILayout.BeginArea(new Rect(0, 20, 200, 100));
        GUILayout.BeginHorizontal();
        float f = GUILayout.HorizontalSlider(distance_, 2, 20, GUILayout.Width(100));
        f = GUILayoutFloat(f);
        if (f != distance_)
        {
            distance_ = f;
            RotateCamera(main_camera_, distance_, angle_, weight_, target_pos_);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        float a = GUILayout.HorizontalSlider(angle_, 5, 90, GUILayout.Width(100));
        a = GUILayoutFloat(a);
        if (a != angle_)
        {
            angle_ = a;
            RotateCamera(main_camera_, distance_, angle_, weight_, target_pos_);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        float w = GUILayout.HorizontalSlider(weight_, 0, 3, GUILayout.Width(100));
        w = GUILayoutFloat(w);
        if (w != weight_)
        {
            weight_ = w;
            RotateCamera(main_camera_, distance_, angle_, weight_, target_pos_);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
#endif

    public static void RotateCamera(Camera camera, float distance, float angle, float weight, Vector3 target)
    {
        Vector3 direct = new Vector3(0, Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad));
        camera.transform.localPosition = direct * distance + new Vector3(target.x, target.y + weight, target.z);

        Transform trans = camera.transform;
        trans.LookAt(new Vector3(target.x, target.y + weight, target.z));
    }

    void CalcCameraInitSetting()
    {
        angle_ = 20.6673f;
        Vector3 pos_joy = new Vector3(0, 0.5956f, 4.4478f);
        Vector3 pos_cam = new Vector3(0, 9.26349f, -15.703f);
        Vector3 offset = pos_cam - pos_joy;
        float cosA = Mathf.Cos(angle_ / Mathf.PI);
        float sinA = Mathf.Sin(angle_ / Mathf.PI);
        distance_ = -offset.z / cosA;
        weight_ = offset.y - distance_ * sinA;
    }
}

