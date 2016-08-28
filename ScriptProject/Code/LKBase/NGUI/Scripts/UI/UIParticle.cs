using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/UIParticle")]
[RequireComponent(typeof(ParticleSystem))]
public class UIParticle : UISprite
{
	public class ParticleComparer : IComparer<ParticleSystem.Particle>
	{
		int IComparer<ParticleSystem.Particle>.Compare(ParticleSystem.Particle left, ParticleSystem.Particle right)
		{
			var leftDepth = left.position.z;
			var rightDepth = right.position.z;
			if (leftDepth < rightDepth) return -1;
			if (leftDepth > rightDepth) return 1;
			return 0;
		}
	}

	private static readonly int MaxParticleSize = 128;
	private ParticleSystem mParticleSystem = null;
	private ParticleSystem.Particle[] particleList = new ParticleSystem.Particle[MaxParticleSize];
    public string[] Animations = new string[0];
    public int Loop = 1;
    public bool mUseParticleSize = false;

	//private ParticleComparer mComparer = new ParticleComparer();

	private bool mIsLocalTransform = true;
	private Transform mSelfTransform = null;

	private Vector2[] uv0;
	private Vector2[] uv1;
	private Vector2[] uv2;
	private Vector2[] uv3;
    private Vector2[] size;

	public bool mUseFixedSeed = false;
	public int mFixedSeed = 0;
	public bool mFollowDirection = false;
	public Vector2 mStartSize = new Vector2(1.0f, 1.0f);
	public Vector2 mEndSize = new Vector2(1.0f, 1.0f);
	public AnimationCurve mSizeCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
	public Gradient mColorGradient = new Gradient();

	bool _HasInitUVs = false;
	public bool mHasInitUVs
	{
		get { return _HasInitUVs; }
		set { _HasInitUVs = value; }
	}

	bool _ShouldPlay = true;
    new void OnEnable()
    {
        base.OnEnable();
        Initialize();
        //Stop();
        Play();
        _ShouldPlay = true;
    }

	new void OnDisable()
	{
		base.OnDisable();
		if (mParticleSystem != null)
		{
			mParticleSystem.Stop();
			mParticleSystem.Clear();
			mParticleSystem = null;
		}
	}

	public void Play()
	{
		if (mParticleSystem != null)
		{
			mParticleSystem.Play(false);
		}
	}

	public void Stop()
	{
		if (mParticleSystem != null)
		{
			mParticleSystem.Stop();
			mParticleSystem.Clear();
		}
	}

	void Initialize()
	{
		if (mParticleSystem == null)
			mParticleSystem = this.GetComponent<ParticleSystem>();

		uint seed; 
		if (mUseFixedSeed)
			seed = (uint)mFixedSeed;
		else
			seed = 0;

		if (seed != mParticleSystem.randomSeed)
			mParticleSystem.randomSeed = seed;

		mIsLocalTransform = (mParticleSystem.simulationSpace == ParticleSystemSimulationSpace.Local);
		mSelfTransform = gameObject.transform;
	}

	void InitUVs()
	{
        if (Animations.Length > 1)
        {
            uv0 = new Vector2[Animations.Length];
            uv1 = new Vector2[Animations.Length];
            uv2 = new Vector2[Animations.Length];
            uv3 = new Vector2[Animations.Length];
            size = new Vector2[Animations.Length];

            for (int i = 0; i < Animations.Length; i++)
            {
                UIAtlas.Sprite sprite = atlas.GetSprite(Animations[i]);
                
                if (sprite == null)
                {
                    uv0[i] = Vector2.zero;
                    uv1[i] = Vector2.zero;
                    uv2[i] = Vector2.zero;
                    uv3[i] = Vector2.zero;
                }
                else
                {
                    Rect outuv = sprite.outer;
                    size[i].x = outuv.width;
                    size[i].y = outuv.height;

                    outuv = NGUIMath.ConvertToTexCoords(outuv, atlas.texture.width, atlas.texture.height);
                    uv0[i] = new Vector2(outuv.xMin, outuv.yMin);
                    uv1[i] = new Vector2(outuv.xMax, outuv.yMax);
                    uv2[i] = new Vector2(uv1[i].x, uv0[i].y);
                    uv3[i] = new Vector2(uv0[i].x, uv1[i].y);
                }
            }
        }
        else
        {
            uv0 = new Vector2[1];
            uv1 = new Vector2[1];
            uv2 = new Vector2[1];
            uv3 = new Vector2[1];
            size = new Vector2[1];

            uv0[0] = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
            uv1[0] = new Vector2(mOuterUV.xMax, mOuterUV.yMax);
            uv2[0] = new Vector2(uv1[0].x, uv0[0].y);
            uv3[0] = new Vector2(uv0[0].x, uv1[0].y);
            size[0].x = sprite.outer.width;
            size[0].y = sprite.outer.height;
        }
	}

	override public bool OnUpdate ()
	{
		base.OnUpdate();
		return true;
	}

	public override void MarkAsChanged()
	{
		base.MarkAsChanged();
		mHasInitUVs = false;
	}

	private Vector3 GetLocalPosition(Vector3 position)
	{
		Vector3 outPos;
		if (mIsLocalTransform)
			outPos = position;
		else
			outPos = mSelfTransform.InverseTransformPoint(position);
		return outPos;
	}

    public override Vector2 pivotOffset
    {
        get
        {
            Vector2 v = new Vector2(0.5f, -0.5f);

            if (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom) v.x = 0f;
            else if (pivot == Pivot.TopRight || pivot == Pivot.Right || pivot == Pivot.BottomRight) v.x = -0.5f;

            if (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right) v.y = 0f;
            else if (pivot == Pivot.BottomLeft || pivot == Pivot.Bottom || pivot == Pivot.BottomRight) v.y = 0.5f;
            v.x *= width;
            v.y *= height;
            return v;
        }
    }

	override public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (_ShouldPlay == false)
		{
			return;
		}
		
		if (mParticleSystem == null)
			return;

#if UNITY_EDITOR
		Initialize();
		mHasInitUVs = false;
#endif

		if (!mHasInitUVs)
		{
			InitUVs();

			mHasInitUVs = true;
		}
		
		//mParticleSystem.Simulate(Time.fixedDeltaTime, true, false);
        if (mParticleSystem.particleCount > 0)
		{
			int count = Mathf.Min(mParticleSystem.particleCount, MaxParticleSize);
            this.GetComponent<ParticleSystem>().GetParticles(particleList);
			//System.Array.Sort(particleList, 0, count, mComparer);

			for (int i = 0 ; i < count ; i++)
			{
				ParticleSystem.Particle particle = particleList[i];

				float time = (1.0f - particle.lifetime / particle.startLifetime);
                // animations
                int animation_idx = 0;
                if (Animations.Length > 1)
                {
                    animation_idx = ((int)(time * Animations.Length * Loop)) % Animations.Length;
                }

                float v = mSizeCurve.Evaluate(time);
				Vector2 curSize = (mStartSize + (mEndSize - mStartSize) * v) * this.GetComponent<ParticleSystem>().startSize;

                if (mUseParticleSize == true)
                {
                    curSize.x *= size[animation_idx].x;
                    curSize.y *= size[animation_idx].y;
                }
				Color curColor = mColorGradient.Evaluate(time);
				Vector3 curPos = GetLocalPosition(particle.position);

				float curRot = 0.0f;

				if (!mFollowDirection)
				{
					curRot = particle.rotation;
				}
				else
				{
                    Vector3 normal_vel = particle.velocity.normalized;
					//Vector2 curVec2 = new Vector2(particle.velocity.normalized.x, particle.velocity.normalized.y);
                    Vector2 curVec2 = new Vector2(normal_vel.x, normal_vel.y);

					float angle = Vector2.Angle(Vector2.up, curVec2);
					if (curVec2.x <= 0.0f)
					{
						curRot = angle;
					}
					else
					{
						curRot = 360.0f - angle;
					}
					curRot *= 0.0174532924f;

				}

				Vector2 halfSize = curSize * 0.5f ;
                //halfSize.x = ratio[animation_idx] * halfSize.y;
				float c = Mathf.Cos(curRot) * halfSize.x;
				float s = Mathf.Sin(curRot) * halfSize.x;
				Vector3 vUp = new Vector3(c, s, 0.0f);

				Vector3 vLeft = Vector3.Cross(vUp, new Vector3(0.0f, 0.0f, 1.0f));
				vLeft.Normalize();
				vLeft *= halfSize.y;

				/*
				float invRot = Mathf.PI / 2.0f - curRot;
				float inv_c = -Mathf.Cos(invRot) * halfSize.y;
				float inv_s = Mathf.Sin(invRot) * halfSize.y;
				Vector3 vLeft = new Vector3(inv_c, inv_s, 0.0f);
				*/

				Vector3 vTL = vLeft + vUp;
				Vector3 vBL = vLeft - vUp;

                //curPos = mPivotOffset;
                curPos += new Vector3(pivotOffset.x, pivotOffset.y);

                verts.Add(new Vector3((vBL.x * width + curPos.x), (vBL.y * height + curPos.y)));
                verts.Add(new Vector3((-vTL.x * width + curPos.x), (-vTL.y * height + curPos.y)));
                verts.Add(new Vector3((-vBL.x * width + curPos.x), (-vBL.y * height + curPos.y)));
                verts.Add(new Vector3((vTL.x * width + curPos.x), (vTL.y * height + curPos.y)));

                uvs.Add(uv0[animation_idx]);
                uvs.Add(uv3[animation_idx]);
				uvs.Add(uv1[animation_idx]);
				uvs.Add(uv2[animation_idx]);
		
				cols.Add(curColor);
				cols.Add(curColor);
				cols.Add(curColor);
				cols.Add(curColor);
            }
		}
	}

}
