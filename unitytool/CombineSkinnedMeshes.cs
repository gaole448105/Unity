using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public  class CombineSkinnedMeshes : MonoBehaviour
{
    public string[] ExposedBoneName;

	public Transform[] DontCombineRender;

    public string hideNotCombineRender;

    /// <summary>
    /// 是否启用合并网格
    /// </summary>
    public bool EnableCombined = false;

    /// <summary>
    /// 是否启用离屏更新
    /// </summary>
    public bool EnableUpdateWhenOffscreen = false;

	/// <summary>
	/// 此模型需要Hide武器
	/// </summary>
	public bool NeedHideWeapon = false;

    public bool DontUnloadCommbinedMeshes = false;

    const int mFaceGroupNum = 4;

    Material mMat = null;

    Vector3 mOrgPos = Vector3.zero;

    Quaternion mOrgRot = Quaternion.identity;

    SkinnedMeshRenderer mSMR = null;

    /// <summary>
    /// 脸部UV数据
    /// </summary>
    Vector2[][] mFaceUVs = new Vector2[mFaceGroupNum][];

    /// <summary>
    /// 合并网格后脸部UV起始位置
    /// </summary>
    int faceUVStartPos = 0;

    //脸所在的UV个数
    int faceUVCount = 0;

	int currentFaceIndx = 0;

    bool bInitFaceData = false;

	Mesh mNormalMesh = null;

	Mesh mHidedWeaponMesh = null;

    List<GameObject> mAttachedEfxList = null;

    public SkinQuality skinQuality = SkinQuality.Auto;

	public Vector3 boundOffset = new Vector3(0, 1, 0);

	public Vector3 boundSize = new Vector3(1, 2, 1);

    void Awake()
    {
        mOrgPos = transform.localPosition;
        mOrgRot = transform.localRotation;
        mSMR = GetComponent<SkinnedMeshRenderer>();
        //if (EnableCombined)
        //    CombineToMesh();
    }

	void Start()
	{
        if (mSMR != null && !EnableCombined)
            SetMat();
    }

    private bool mbIsCombined = false;
    private void LateUpdate()
    {
        if (EnableCombined && !mbIsCombined)
        {
            CombineToMesh();
            mbIsCombined = true;

            if (mSMR != null)
                SetMat();
        }
    }

    void OnDestroy()
	{
		if (mAttachedEfxList != null) {
			mAttachedEfxList.Clear ();
			mAttachedEfxList = null;
		}

		mFaceUVs = null;
		ExposedBoneName = null;

		DontCombineRender = null;

		if (mNormalMesh) {
			mNormalMesh.Destroy ();
			mNormalMesh = null;
		}

		if (mHidedWeaponMesh) {
			mHidedWeaponMesh.Destroy ();
			mHidedWeaponMesh = null;
		}
	}


    public SkinnedMeshRenderer GetRenderer()
    { return mSMR; }

    /// <summary>
    /// 角色换脸
    /// </summary>
    /// <param name="lookIndex">换脸的下标</param>
    public void ChangeFace(int lookIndex)
    {
        if (!bInitFaceData) return;

		lookIndex = lookIndex % mFaceGroupNum;

		if (currentFaceIndx == lookIndex)
			return;
		
        Mesh mesh = mSMR.sharedMesh;
        if (mesh == null)
            return;

        Vector2[] uvs = mesh.uv;
        for (int idx = 0; idx < faceUVCount; idx++)
        {
            uvs[idx + faceUVStartPos] = mFaceUVs[lookIndex][idx];
        }
        mesh.uv = uvs;
		currentFaceIndx = lookIndex;
    }


	/// <summary>
	/// 设置武器可见性
	/// </summary>
	/// <param name="bVisbile">If set to <c>true</c> b visbile.</param>
	public void SetWeaponVisible(bool bVisbile)
	{
		if (mHidedWeaponMesh == null)
			return;

		mSMR.sharedMesh = bVisbile ? mNormalMesh : mHidedWeaponMesh;
	}

    /// <summary>
    /// 模型处理函数
    /// </summary>
    void CombineToMesh()
    {
		List<CombineInstance> combineInstances = new List<CombineInstance>();
		List<Transform> bones = new List<Transform>();
		SkinnedMeshRenderer[] smrList = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (DontCombineRender != null && DontCombineRender.Length > 0) {
			List<SkinnedMeshRenderer> tempList = new List<SkinnedMeshRenderer> (smrList.Length);
			tempList.AddRange (smrList);
			for (int idx = 0; idx < DontCombineRender.Length; idx++) {
				for (int jdx = 0; jdx < tempList.Count; jdx++) {
					if (tempList [jdx] == null || tempList [jdx].transform == DontCombineRender [idx]) {
						tempList.RemoveAt (jdx);
						break;
					}
				}
                if(!string.IsNullOrEmpty(hideNotCombineRender))
                {
                    if (hideNotCombineRender == DontCombineRender[idx].name)
                        DontCombineRender[idx].gameObject.SetActive(false);
                }
			}
			smrList = tempList.ToArray ();

		}

		if (NeedHideWeapon) {
			List<CombineInstance> hideCombineInst  =new List<CombineInstance>();
			for (int idx = 0; idx < smrList.Length; idx++) {
				SkinnedMeshRenderer smr = smrList[idx];
				Mesh mesh = smr.sharedMesh;
				if (mesh == null)
					continue;

				if (mesh.name.ToLower ().Contains ("weapon"))
					continue;

				CombineInstance ci = new CombineInstance();
				ci.mesh = mesh;
				ci.subMeshIndex = 0;
				hideCombineInst.Add(ci);
			}
			mHidedWeaponMesh = new Mesh ();
			mHidedWeaponMesh.CombineMeshes (hideCombineInst.ToArray(), true, false);
			mHidedWeaponMesh.RecalculateBounds ();
		}

       
		int uvOffset = 0;
        for (int idx = 0; idx < smrList.Length; idx++)
        {
            SkinnedMeshRenderer smr = smrList[idx];
            if (mMat == null)
                mMat = smr.material;

            Mesh mesh = smr.sharedMesh;
			if (mesh == null)
				continue;
            int uvCount = mesh.uv.Length;

            if(smr.sharedMesh.name.Contains("face"))
            {
                faceUVStartPos = uvOffset;
                faceUVCount = uvCount;
            }

            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            ci.subMeshIndex = 0;
            combineInstances.Add(ci);
            uvOffset += uvCount;
           
            bones.AddRange(smr.bones);
            Object.Destroy(smr.gameObject);
        }

        if(mSMR == null)
            mSMR = gameObject.AddComponent<SkinnedMeshRenderer>();
		mNormalMesh = new Mesh ();
		mNormalMesh.CombineMeshes(combineInstances.ToArray(), true, false);
		mNormalMesh.RecalculateBounds ();

        mSMR.quality = skinQuality;
        mSMR.sharedMesh = mNormalMesh;
        mSMR.bones = bones.ToArray();
        if (EnableUpdateWhenOffscreen)
            mSMR.updateWhenOffscreen = EnableUpdateWhenOffscreen;

//        Bounds bounds = mSMR.localBounds;
//		bounds.center = boundOffset;
//		bounds.extents = boundSize;
		mSMR.localBounds = new Bounds (boundOffset, boundSize);


		if (!DontUnloadCommbinedMeshes) {
			for (int idx = 0; idx < smrList.Length; idx++) {
				SkinnedMeshRenderer mr = smrList [idx];
				Resources.UnloadAsset (mr.sharedMesh);
				mr.sharedMesh = null;
			}
		}

        if(mbNeedInitFaceUV)
        {
            InitFaceUVs();
            mbNeedInitFaceUV = false;
        }

        
    }

    /// <summary>
    /// 初始化脸部数据的UV数据
    /// </summary>
    /// <param name="smr">脸部的skinned mesh</param>
    private bool mbNeedInitFaceUV = false;
    public void InitFaceUVs()
    {
        if (!EnableCombined) return;

        if (mSMR == null)
        {
            mbNeedInitFaceUV = true;
            return;
        }

        Mesh m = mSMR.sharedMesh;
        if (m == null || m.uv == null)
            return;

        Vector2[] normalUV = m.uv;
        for(int idx =0; idx < mFaceGroupNum;idx++)
        {
            mFaceUVs[idx] = new Vector2[faceUVCount];
        }

        for (int idx = 0; idx < faceUVCount; idx++)
        {
            mFaceUVs[0][idx] = normalUV[idx + faceUVStartPos];
            mFaceUVs[1][idx] = normalUV[idx + faceUVStartPos] + Vector2.down * 0.25f;
            mFaceUVs[2][idx] = normalUV[idx + faceUVStartPos] + Vector2.down * 0.5f;
            mFaceUVs[3][idx] = normalUV[idx + faceUVStartPos] + Vector2.down * 0.75f;
        }

        bInitFaceData = true;
    }

    void SetMat()
    {
        if (mMat != null && mMat.shader.name == "Shader Forge/hero_hq")
        {
            if (QualitySettings.GetQualityLevel() < 4)
            {
                Shader s = Shader.Find("GOD/DoubleRim");
                Material newMat = new Material(s);
                newMat.hideFlags = HideFlags.DontSave;
                newMat.SetTexture("_MainTex", mMat.GetTexture("_diffuse"));
				newMat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f));
                newMat.SetColor("_RimColor1", new Color(0.23f, 0.16f, 0.07f));
                mMat = newMat;
            } 
        }
        mSMR.material = mMat;
    }

//	void Update()
//	{
//		if (mSMR != null && mSMR.sharedMesh != null) {
//			mSMR.sharedMesh.RecalculateBounds ();
//			var b = mSMR.sharedMesh.bounds;
//			mSMR.localBounds = b;
//		}
//	}
}
