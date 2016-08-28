using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/UI/Texture Animation")]
public class UITextureAnimation : MonoBehaviour {
	UITexture m_UITexture;
	public Texture2D[] m_Textures;
	public bool mLoop = false;
	public int mFPS;

	//UISprite mSprite;
	float mDelta = 0f;
	int mIndex = 0;
	
	void Awake()
	{
		m_UITexture = GetComponent<UITexture>();
	}
	
	void Update()
	{
		if(m_Textures.Length > 1)
		{
			mDelta += Time.deltaTime;
			float rate = mFPS > 0f ? 1f / mFPS : 0f;

			if (rate < mDelta)
			{
				mDelta = (rate > 0f) ? mDelta - rate : 0f;
				if (++mIndex >= m_Textures.Length) 
				{
					mIndex = 0;
					
					if(mLoop == false)
						gameObject.SetActive(false);
				}
				m_UITexture.material.mainTexture =  m_Textures[mIndex];
			}
		}
	}
}
