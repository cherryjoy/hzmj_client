using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class UITrail : MonoBehaviour
{
    public class TrailSection
    {
        public Vector3 startPoint;
        public Vector3 upDirection;
        public float spawnTime;
        public float lifeTime;

        public bool IsAlive(float currentTime)
        {
            if (currentTime - spawnTime > lifeTime)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public float trailLength = 1.0f;
    public float trailLifetime = 1.0f;
    public float minDistance = 0.1f;
    public float timeTransitionSpeed = 1.0f;
    public float desiredTrailLifetime = 1.0f;
    public Color startColor = Color.white;
    public Color endColor = Color.white;
    public Material trailMaterial;

    private TrailSection mCurrentTrailSection;
    private Matrix4x4 mWorldToLocalMatrix;
    private Mesh mMesh;
    private Vector3[] mVertexBuffer;
    private Color[] mColorBuffer;
    private Vector2[] mUVColor;
    private MeshRenderer mMeshRenderer;
    private List<TrailSection> mTrailSectionList = new List<TrailSection>();
    private float mDeltaTime = 0.003f;
    private Vector3 mLastPosition;

    void Start()
    {
        MeshFilter meshFilter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        mMesh = new Mesh();
        meshFilter.mesh = mMesh;

        mMeshRenderer = gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        if (mMeshRenderer == null)
        {
            mMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        mMeshRenderer.material = trailMaterial;
        mMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        mMeshRenderer.receiveShadows = false;

        mLastPosition = transform.position;
    }

    void Update()
    {
        Iterate(Time.time);
        UpdateTrail(Time.time);
    }

    public void Iterate(float iterateTime)
    {
        Vector3 currentPosition = transform.position;
        Vector3 upDirection = Cross(Vector3.forward, (currentPosition - mLastPosition));
        upDirection = Vector3.Normalize(upDirection);

        if (mTrailSectionList.Count > 0 && (mTrailSectionList[0].startPoint - currentPosition).sqrMagnitude > (minDistance * minDistance))
        {
            TrailSection trailSection = new TrailSection();
            trailSection.startPoint = currentPosition;
            trailSection.upDirection = upDirection;
            trailSection.spawnTime = iterateTime;
            trailSection.lifeTime = trailLifetime;

            mTrailSectionList.Insert(0, trailSection);

            mLastPosition = currentPosition;
        }
        else if (mTrailSectionList.Count == 0)
        {
            if ((currentPosition - mLastPosition).sqrMagnitude > (minDistance * minDistance))
            {
                TrailSection trailSection = new TrailSection();
                trailSection.startPoint = currentPosition;
                trailSection.upDirection = upDirection;
                trailSection.spawnTime = iterateTime;
                trailSection.lifeTime = trailLifetime;

                mTrailSectionList.Insert(0, trailSection);

                mLastPosition = currentPosition;
            }
            else
            {
                mMesh.Clear();
            }
        }
    }

    public void UpdateTrail(float currentTime)
    {
        while (mTrailSectionList.Count > 0 && !mTrailSectionList[mTrailSectionList.Count - 1].IsAlive(currentTime))
        {
            mTrailSectionList.RemoveAt(mTrailSectionList.Count - 1);
        }

        if (mTrailSectionList.Count < 2)
        {
            mMesh.Clear();
            return;
        }

        mVertexBuffer = new Vector3[mTrailSectionList.Count * 2];
        mColorBuffer = new Color[mTrailSectionList.Count * 2];
        mUVColor = new Vector2[mTrailSectionList.Count * 2];
        mCurrentTrailSection = mTrailSectionList[0];
        mWorldToLocalMatrix = transform.worldToLocalMatrix;

        for (var i = 0; i < mTrailSectionList.Count; i++)
        {
            mCurrentTrailSection = mTrailSectionList[i];
            float u = 0.0f;
            if (i != 0)
            {
                u = Mathf.Clamp01((currentTime - mCurrentTrailSection.spawnTime) / trailLifetime);
            }

            Vector3 upDirection = mCurrentTrailSection.upDirection;

            mVertexBuffer[i * 2 + 0] = mWorldToLocalMatrix.MultiplyPoint(mCurrentTrailSection.startPoint - upDirection * trailLength * 0.5f);
            mVertexBuffer[i * 2 + 1] = mWorldToLocalMatrix.MultiplyPoint(mCurrentTrailSection.startPoint + upDirection * trailLength * 0.5f);

            mUVColor[i * 2 + 0] = new Vector2(u, 0);
            mUVColor[i * 2 + 1] = new Vector2(u, 1);

            Color interpolatedColor = Color.Lerp(startColor, endColor, u);
            mColorBuffer[i * 2 + 0] = interpolatedColor;
            mColorBuffer[i * 2 + 1] = interpolatedColor;
        }

        int[] triangles = new int[(mTrailSectionList.Count - 1) * 6];
        for (int i = 0; i < triangles.Length / 6; i++)
        {
            triangles[i * 6 + 0] = i * 2;
            triangles[i * 6 + 1] = i * 2 + 1;
            triangles[i * 6 + 2] = i * 2 + 2;

            triangles[i * 6 + 3] = i * 2 + 2;
            triangles[i * 6 + 4] = i * 2 + 1;
            triangles[i * 6 + 5] = i * 2 + 3;
        }

        mMesh.Clear();
        mMesh.vertices = mVertexBuffer;
        mMesh.colors = mColorBuffer;
        mMesh.uv = mUVColor;
        mMesh.triangles = triangles;

        if (trailLifetime > desiredTrailLifetime)
        {
            trailLifetime -= mDeltaTime * timeTransitionSpeed;
            if (trailLifetime <= desiredTrailLifetime)
            {
                trailLifetime = desiredTrailLifetime;
            }
        }
        else if (trailLifetime < desiredTrailLifetime)
        {
            trailLifetime += mDeltaTime * timeTransitionSpeed;
            if (trailLifetime >= desiredTrailLifetime)
            {
                trailLifetime = desiredTrailLifetime;
            }
        }
    }

    public void ClearTrail()
    {
        desiredTrailLifetime = 0;
        trailLifetime = 0;
        if (mMesh != null)
        {
            mMesh.Clear();
            mTrailSectionList.Clear();
        }
    }

    private Vector3 Cross(Vector3 a, Vector3 b)
    {
        Vector3 normalVector = new Vector3(a.y * b.z - b.y * a.z, a.z * b.x - b.z * a.x, a.x * b.y - b.y * a.x);

        return normalVector;
    }
}
