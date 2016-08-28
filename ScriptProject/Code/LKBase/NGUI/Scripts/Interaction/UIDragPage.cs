using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/UIDragPage")]
public class UIDragPage : IgnoreTimeScale
{
    public Transform m_Target;

    private DragDirection m_DragDirection;
    private string m_PressCall = "OnPressCell";
    private string m_UnPressCall = "OnUnPressCell";
    private Vector3 m_Scale = new Vector3(0, 1, 0);
    private Plane m_Plane;
    private Vector3 m_LastPos;
    private float m_LastLocalAxisPos;
    private float m_LastPressTime;

    public DragDirection DragDirection
    {
        set
        {
            m_DragDirection = value;
            if (m_DragDirection == DragDirection.Horizontal)
                m_Scale = new Vector3(1, 0, 0);
            else
                m_Scale = new Vector3(0, 1, 0);
        }
        get
        {
            return m_DragDirection;
        }
    }

    void OnEnable()
    {
        DragDirection = DragDirection.Horizontal;
    }

    void OnPress(bool pressed)
    {
        if (m_Target == null)
        {
            return;
        }
        if (pressed)
        {
            SpringPosition sp = m_Target.GetComponent<SpringPosition>();
            if (sp != null)
            {
                sp.enabled = false;
            }
            m_LastPos = UICamera.lastHit.point;
            m_LastPressTime = Time.timeSinceLevelLoad;

            if (DragDirection == DragDirection.Vertical)
                m_LastLocalAxisPos = m_Target.localPosition.y;
            else
                m_LastLocalAxisPos = m_Target.localPosition.x;

            m_Plane = new Plane(Vector3.back, m_LastPos);
            m_Target.SendMessage(m_PressCall, SendMessageOptions.DontRequireReceiver);
        }
        else
        {

            float timeOffset = Time.timeSinceLevelLoad - m_LastPressTime;

            float curLocalAxisPos, offset, speed;
            if (DragDirection == DragDirection.Vertical)
            {
                curLocalAxisPos = m_Target.localPosition.y;
                offset = curLocalAxisPos - m_LastLocalAxisPos;
            }
            else
            {
                curLocalAxisPos = m_Target.localPosition.x;
                offset = m_LastLocalAxisPos - curLocalAxisPos;
            }
            speed = offset / timeOffset;
            m_Target.SendMessage(m_UnPressCall, speed, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrag(Vector2 delta)
    {
        if (m_Target == null)
        {
            return;
        }
        Ray ray = UICamera.lastCamera.ScreenPointToRay(UICamera.lastTouchPosition);
        float dist;
        if (m_Plane.Raycast(ray, out dist))
        {
            Vector3 currentPos = ray.GetPoint(dist);
            Vector3 offset = currentPos - m_LastPos;
            m_LastPos = currentPos;
            if ((DragDirection == DragDirection.Vertical && offset.y != 0f)
                || (DragDirection == DragDirection.Horizontal && offset.x != 0f))
            {
                offset = m_Target.InverseTransformDirection(offset);
                offset.Scale(m_Scale);
                offset = m_Target.TransformDirection(offset);
                m_Target.position += offset;
            }
        }
    }
}

public enum DragDirection
{
    Horizontal,
    Vertical
}

