//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UIParticle.
/// </summary>

[CustomEditor(typeof(UIParticle))]
public class UIParticleInspector : UISpriteInspector
{
    bool mShowAniamtions = false;
    int mIndex = 0;
	override protected bool OnDrawProperties()
	{
		UIParticle particle = mWidget as UIParticle;
		
		if (!base.OnDrawProperties())
			return false;

		EditorGUILayout.BeginHorizontal();
		{
			particle.mUseFixedSeed = EditorGUILayout.Toggle("UseSeed:", particle.mUseFixedSeed, GUILayout.MinWidth(50f));
			particle.mFixedSeed = EditorGUILayout.IntField("FixedSeed:", particle.mFixedSeed, GUILayout.MinWidth(150f));
		}
		EditorGUILayout.EndHorizontal();


		particle.mFollowDirection = EditorGUILayout.Toggle("Follow Direction:", particle.mFollowDirection);

		Vector2 start = EditorGUILayout.Vector2Field("Start Size:", particle.mStartSize);
		if (start != particle.mStartSize)
		{
			NGUIEditorTools.RegisterUndo("Undo ParticleSize", particle);
			particle.mStartSize = start;
		}
		Vector2 end = EditorGUILayout.Vector2Field("End Size:", particle.mEndSize);
		if (end != particle.mEndSize)
		{
			NGUIEditorTools.RegisterUndo("Undo ParticleSize", particle);
			particle.mEndSize = end;
		}

		particle.mSizeCurve = EditorGUILayout.CurveField("Size Curve", particle.mSizeCurve, Color.green, new Rect(0.0f, 0.0f, 1.0f, 1.0f));

		SerializedObject serializedObj = new SerializedObject(particle);
		SerializedProperty serializedProp = serializedObj.FindProperty("mColorGradient");
		EditorGUILayout.PropertyField(serializedProp, new GUIContent("Color"));

        serializedObj.ApplyModifiedProperties();

        mShowAniamtions = EditorGUILayout.Foldout(mShowAniamtions, "Aniamtions");
        if (mShowAniamtions == true)
        { 
            int size = EditorGUILayout.IntField("size", particle.Animations.Length);
            if (size != particle.Animations.Length)
            {
                particle.mHasInitUVs = false;
                int newsize = Mathf.Min(size, particle.Animations.Length);
                string[] buf = new string[size];
                for (int i = 0; i < newsize; i++)
                    buf[i] = particle.Animations[i];
                particle.Animations = buf;
            }
            for (int i = 0; i < particle.Animations.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Sprite") == true)
                {
                    mIndex = i;
                    SpriteSelector.Show(AnimationIdxCallBack);
                }
                //particle.Animations[i] = EditorGUILayout.TextField(particle.Animations[i]);
                string name = EditorGUILayout.TextField(particle.Animations[i]);
                if (name != particle.Animations[i])
                {
                    particle.Animations[i] = name;
                    particle.mHasInitUVs = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        particle.Loop = EditorGUILayout.IntField("Loop", particle.Loop);
        particle.mUseParticleSize = EditorGUILayout.Toggle("Use Size", particle.mUseParticleSize);
		return true;
	}

    void AnimationIdxCallBack(string sprite)
    {
        UIParticle particle = mWidget as UIParticle;
        particle.Animations[mIndex] = sprite;
        particle.mHasInitUVs = false;
    }
}
