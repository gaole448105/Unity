using System;
using UnityEngine;
using System.Collections.Generic;

public class UIEffectCfg:MonoBehaviour
{
    [Serializable]
    public struct stUISoundParam
    {
        public string clipName;
        public string clipPackageName;
        public bool repeat;
        public float delayTime;
        public float minInterval;
        public float maxInterval;
        public int countLimit;
        public bool playOnAwake;
        public bool continuePlayAfterDestroy;
    }

    [Serializable]
    public struct stUIEffect
    {
        /// <summary>
        /// 使用的名字
        /// </summary>
        public string aliasName;

        /// <summary>
        /// 特效资源名字
        /// </summary>
        public string assetName;
        /// <summary>
        /// 特效挂载的parent路径
        /// </summary>
        public Transform parent;

        /// <summary>
        /// 特效挂载的位置
        /// </summary>
        public Vector3 pos;

        /// <summary>
        /// 对应着声音配置配置下标
        /// </summary>
        public int soundIdx;

        public bool autoAttach;
        public bool autoActive;
		public bool autoOrder;
		public int autoOrderDiff;
    }

    [NoToLua]
    public stUIEffect[] UsedEffect;

    [NoToLua]
    public stUISoundParam[] SoundParam;

	/// <summary>
	/// 实例化对象
	/// </summary>
	GameObject[] insts = null;

    bool mReady = false;

    public bool Ready
    {
        get { return mReady; }
    } 
    
    void Awake()
    {
        if (UsedEffect == null || UsedEffect.Length == 0) return;

        CacheResPoolMgr.Instance.PreloadUIEffect(UsedEffect);

		insts = new GameObject[UsedEffect.Length];
    }

    void Update()
    {
        if (mReady) return;

        for(int idx =0; idx < UsedEffect.Length;idx++)
        {
            if (!CacheResPoolMgr.Instance.HasUIEffect(UsedEffect[idx].assetName))
                return;
        }

        mReady = true;

        PerformAutoAttach();
    }
    
    void PerformAutoAttach()
    {
        for (int idx = 0; idx < UsedEffect.Length; idx++)
        {
            if (UsedEffect[idx].autoAttach)
            {
                if (UsedEffect[idx].autoActive)
                    ShowEffect(UsedEffect[idx].aliasName, UsedEffect[idx].parent);
                else
                    CreateEffect(UsedEffect[idx].aliasName, UsedEffect[idx].parent, 0);
            }
                
        }
    }

    void OnDestroy()
    {
        if (UsedEffect == null && UsedEffect.Length == 0) return;

        Clean();

        //for (int idx = 0; idx < UsedEffect.Length;idx++ )
        //    CacheResPoolMgr.Instance.UnloadUIEffect(UsedEffect[idx].assetName);

        insts = null;
        UsedEffect = null;
    }

    /// <summary>
    /// 创建一个特效，但是不显示【在界面刚打开的使用进行使用】
    /// </summary>
    /// <param name="name">特效名字</param>
    /// <param name="parent"></param>
    /// <param name="SortingOrder"></param>
    /// <returns></returns>
    public GameObject CreateEffect(string name,Transform parent,int SortingOrder)
    {
        if (UsedEffect == null || UsedEffect.Length == 0) return null;

        for (int idx = 0; idx < UsedEffect.Length; idx++)
        {
            stUIEffect ef = UsedEffect[idx];
            if (ef.aliasName == name && parent == ef.parent)
            {
                if (insts[idx] != null)
                {
                    GameObject go = insts[idx];
					if (SortingOrder > 0)
						EffectUtility.Instance.SetEffectSortingOrder (go, SortingOrder);
					else if (ef.autoOrder)
						SetAutoOrder (go, ef.autoOrderDiff);
                    return go;
                }

                GameObject effectGo = CacheResPoolMgr.Instance.LoadUIEffect(ef.assetName);
                if (effectGo != null)
                {
                    MountSound(effectGo, ef.soundIdx);
                    effectGo.name = name;
                    effectGo.transform.SetParent(ef.parent);
                    effectGo.transform.localPosition = ef.pos;
                    effectGo.transform.localScale = Vector3.one;
                    insts[idx] = effectGo;
                    if (SortingOrder > 0)
                        EffectUtility.Instance.SetEffectSortingOrder(effectGo, SortingOrder);
					if (ef.autoOrder)
						SetAutoOrder (effectGo, ef.autoOrderDiff);
                    effectGo.SetActive(false);
                    return effectGo;
                }

            }
        }
        return null;
    }

    /// <summary>
    /// 显示特效
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="SortingOrder"></param>
    /// <returns></returns>
    public GameObject ShowEffect(string name,Transform parent,int SortingOrder)
    {
        if (parent == null || UsedEffect == null || UsedEffect.Length == 0) return null;

        GameObject orgGo = FindExistGO(name, parent, SortingOrder);
        if (orgGo != null) return orgGo;

        for (int idx = 0; idx < UsedEffect.Length; idx++)
        {
            stUIEffect ef = UsedEffect[idx];
            if (ef.aliasName == name && parent == ef.parent)
            {
                if (insts[idx] != null)
                {
                    GameObject go = insts[idx];
                    go.SetActive(true);

                    if (SortingOrder > 0)
                        EffectUtility.Instance.SetEffectSortingOrder(go, SortingOrder);
					else if (ef.autoOrder)
						SetAutoOrder (go, ef.autoOrderDiff);
                    return go;
                }
                else
                {
                    GameObject effectGo = CacheResPoolMgr.Instance.LoadUIEffect(ef.assetName);
                    if (effectGo != null)
                    {
                        MountSound(effectGo, ef.soundIdx);
                        effectGo.name = name;
                        effectGo.transform.SetParent(ef.parent);
                        effectGo.transform.localPosition = ef.pos;
                        effectGo.transform.localScale = Vector3.one;
                        insts[idx] = effectGo;
                        if (SortingOrder > 0)
                            EffectUtility.Instance.SetEffectSortingOrder(effectGo, SortingOrder);
						else if (ef.autoOrder)
							SetAutoOrder (effectGo, ef.autoOrderDiff);
                        return effectGo;
                    }
                    else
                    {
                        ShowEffectAsync(name, parent, SortingOrder);
                    }

                }

            }
        }
        return null;
    }

    /// <summary>
    /// 显示特效
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    [NoToLua]
    public GameObject ShowEffect(string name, Transform parent)
    {
        if (parent == null || UsedEffect == null || UsedEffect.Length == 0) return null;

        GameObject orgGo = FindExistGO(name, parent, 0);
        if (orgGo != null) return orgGo;

        for (int idx = 0; idx < UsedEffect.Length; idx++)
        {
            stUIEffect ef = UsedEffect[idx];
            if (ef.aliasName == name && parent == ef.parent)
            {
                if (insts[idx] != null)
                {
                    GameObject go = insts[idx];
                    go.SetActive(true);
					if (ef.autoOrder)
						SetAutoOrder (go, ef.autoOrderDiff);
                    return go;
                }
                else
                {
                    GameObject effectGo = CacheResPoolMgr.Instance.LoadUIEffect(ef.assetName);
                    if (effectGo != null)
                    {
                        MountSound(effectGo, ef.soundIdx);
                        effectGo.name = name;
                        effectGo.transform.SetParent(ef.parent);
                        effectGo.transform.localPosition = ef.pos;
                        effectGo.transform.localScale = Vector3.one;
                        insts[idx] = effectGo;
						if (ef.autoOrder)
							SetAutoOrder (effectGo, ef.autoOrderDiff);
                        return effectGo;
                    }
                    else
                    {
                        ShowEffectAsync(name, parent, 0);
                    }

                }

            }
        }
        return null;
    }

    public void ShowEffectAsync(string name,Transform parent,int SortingOrder)
    {
        if (parent == null || FindExistGO(name, parent, SortingOrder)!=null) return;

        if (UsedEffect == null || UsedEffect.Length == 0) return;

        for(int idx =0; idx < UsedEffect.Length;idx++)
        {
            stUIEffect ef = UsedEffect[idx];
            if(ef.aliasName == name && parent == ef.parent)
            {
                if (insts[idx] != null)
                {
                    GameObject go = insts[idx];
                    go.SetActive(true);
                    if(SortingOrder > 0)
                        EffectUtility.Instance.SetEffectSortingOrder(go, SortingOrder);
					else if (ef.autoOrder)
						SetAutoOrder (go, ef.autoOrderDiff);
                }
                else
                {
                    CacheResPoolMgr.Instance.LoadUIEffectAsync(ef.assetName, (GameObject effectGo) => {
                        if (effectGo != null)
                        {
                            MountSound(effectGo, ef.soundIdx);
                            effectGo.name = name;
                            effectGo.transform.SetParent(ef.parent);
                            effectGo.transform.localPosition = ef.pos;
                            effectGo.transform.localScale = Vector3.one;
                            if (SortingOrder > 0)
                                EffectUtility.Instance.SetEffectSortingOrder(effectGo, SortingOrder);
							else if (ef.autoOrder)
								SetAutoOrder (effectGo, ef.autoOrderDiff);
                        }
                    });
                }
                
            }
        }
    }

    void MountSound(GameObject go, int soundIdx)
    {
        if (SoundParam == null || SoundParam.Length == 0) return;

        if (soundIdx < 0 || soundIdx >= SoundParam.Length) return;

        if (go == null) return;

        stUISoundParam param = SoundParam[soundIdx];
        NewAudioAttach audio = go.AddComponent<NewAudioAttach>();
        audio.clipName = param.clipName;
        audio.clipPackageName = param.clipPackageName;
        audio.repeat = param.repeat;
        audio.delayTime = param.delayTime;
        audio.minInterval = param.minInterval;
        audio.maxInterval = param.maxInterval;
        audio.countLimit = param.countLimit;
        audio.playOnAwake = param.playOnAwake;
        audio.continuePlayAfterDestroy = param.continuePlayAfterDestroy;
    }

    /// <summary>
    /// 判断父节点上是否存在特效对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="sortingOrder"></param>
    /// <returns>存在的对象</returns>
    GameObject FindExistGO(string name, Transform parent,int sortingOrder)
    {
        Transform effectTrans = parent.Find(name);
        if (effectTrans != null)
        {
            effectTrans.gameObject.SetActive(true);
            EffectUtility.Instance.SetEffectSortingOrder(effectTrans.gameObject, sortingOrder);
            return effectTrans.gameObject;
        }

        return null;
    }

	void SetAutoOrder(GameObject go, int orderDiff)
	{
		var sync = go.GetComponent<SyncRenderSortingOrderWithCanvas> (true);
		sync.orderDiff = orderDiff;
	}

    [NoToLua]
	public void Clean()
	{
		if (insts != null && insts.Length > 0) 
        {
            for (int idx = 0; idx < insts.Length; idx++)
            {
                GameObject.DestroyObject(insts[idx]);
                insts[idx] = null;
            }
		}
	}

    [NoToLua]
    public void DisactiveAll()
    {
        if (insts != null && insts.Length > 0)
        {
            for (int idx = 0; idx < insts.Length; idx++)
            {
                if (insts[idx] != null)
                    insts[idx].SetActive(false);
            }
        }
    }
}

public class PageEffectMgr
{
	UIEffectCfg mEffectCfg;

	Dictionary<string, GameObject> mEffects;

	public PageEffectMgr(UIBasePage page, string nodeName)
	{
		mEffectCfg = page.FindElement<UIEffectCfg> (nodeName);
		if (mEffectCfg != null)
			mEffects = new Dictionary<string, GameObject> ();
	}

	public void Dispose()
	{
		mEffects.Clear ();
		mEffects = null;
		mEffectCfg = null;
	}

	/// <summary>
	/// Shows the effect.
	/// </summary>
	/// <returns>The effect.</returns>
	/// <param name="efxName">Efx name.</param>
	/// <param name="parent">Parent. 如果为空则使用mEffectCfg的节点</param>
	/// <param name="aliveTime">Alive time. 如果为0，则一直显示直到StopEffect或者HideEffect. aliveTime大于0的特效无法通过stop或者hide关闭</param>
	/// <param name="initOrderDiff">首次显示时，相对父级canvas的sorting order差。如果预设中已经设置SyncRenderSortingOrderWithCanvas， 则该值无效。</param>
	public GameObject ShowEffect(string efxName, Transform parent = null, float aliveTime = 0, int initOrderDiff = 1)
	{
		if (mEffectCfg == null)
			return null;
		
		if (mEffects.ContainsKey (efxName)) {
			var go = mEffects [efxName];
			if (go != null) {
				if (aliveTime > 0) {
					mEffects.Remove (efxName);
					AutoHide autoHide = go.GetComponent<AutoHide> (true);
					autoHide.playTime = aliveTime;
				} 
				go.SetActive (false);
				go.SetActive (true);
				return go;
			} else {
				mEffects.Remove (efxName);
			}
		}

		if (parent == null)
			parent = mEffectCfg.transform;

		GameObject efxGo = mEffectCfg.ShowEffect (efxName, parent);
		if (efxGo == null)
			return null;
		
		InitEfxSortingOrderDiff (efxGo, initOrderDiff);

		if (aliveTime > 0) {
			AutoHide autoHide = efxGo.GetComponent<AutoHide> (true);
			autoHide.playTime = aliveTime;
		} else
			mEffects.Add (efxName, efxGo);
		
		return efxGo;
	}

	/// <summary>
	/// Stops the effect (Destroy gameobject).
	/// </summary>
	/// <returns>The effect.</returns>
	/// <param name="efxName">Efx name.</param>
	public void StopEffect(string efxName)
	{
		if (mEffects.ContainsKey (efxName)) {
			var go = mEffects [efxName];
			if (go != null)
				go.Destroy ();
			mEffects.Remove (efxName);
		}
	}

	/// <summary>
	/// Hides the effect (Do not Destory gameobject).
	/// </summary>
	/// <returns>The effect.</returns>
	/// <param name="efxName">Efx name.</param>
	public void HideEffect(string efxName)
	{
		if (mEffects.ContainsKey (efxName)) {
			var go = mEffects [efxName];
			if (go != null)
				go.SetActive (false);
			else
				mEffects.Remove (efxName);
		}
	}

	void InitEfxSortingOrderDiff(GameObject go, int diffValue)
	{
		var sync = go.GetComponent<SyncRenderSortingOrderWithCanvas> ();
		if (sync == null) {
			sync = go.AddComponent<SyncRenderSortingOrderWithCanvas> ();
			sync.orderDiff = diffValue;
		}
	}
}