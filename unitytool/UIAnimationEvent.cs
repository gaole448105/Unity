using System;
using UnityEngine;
using System.Collections.Generic;

public class UIAnimationEvent : MonoBehaviour
{
    private UIBasePage mBasePage;

    public UIBasePage BasePage
    {
        get { return mBasePage; }
        set { mBasePage = value; }
    }

    public void ActiveEffect(string effectName)
    {
		if (mBasePage == null) {
			Transform effectTrans = transform.Find(effectName);
			if (effectTrans != null) {
				effectTrans.gameObject.SetActive (true);
				Canvas canvas = effectTrans.GetComponentInParent<Canvas> ();
				if (canvas != null)
					EffectUtility.Instance.SetEffectSortingOrder (effectTrans, canvas.sortingOrder + 1);
			}
		}
        else if(!BasePage.ActiveAnimEffect(effectName))
        {
            Transform effectTrans = transform.Find(effectName);
			if (effectTrans != null) {
				effectTrans.gameObject.SetActive (true);
				EffectUtility.Instance.SetEffectSortingOrder (effectTrans, mBasePage.SortingOrder + 1);
			}
        }
    }

    public void DisActiveEffect(string effectName)
    {
		if (mBasePage == null) {
			Transform effectTrans = transform.Find(effectName);
			if (effectTrans != null)
				effectTrans.gameObject.SetActive(false);
		}
        else if (!BasePage.DisActiveAnimEffect(effectName))
        {
            Transform effectTrans = transform.Find(effectName);
            if (effectTrans != null)
                effectTrans.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 播放UI声音
    /// </summary>
    /// <param name="soundName"></param>
    public void PlayUISound(string soundName)
    {
        if (MusicMgr.Instance == null) return;

        MusicMgr.Instance.PlayUISound(soundName, false);
    }

    /// <summary>
    /// 空事件不进行任何处理
    /// </summary>
    public void EmptyEvent()
    {

    }

    /// <summary>
    /// 隐藏自己事件
    /// </summary>
    public void HideSelf()
    {
        gameObject.SetActive(false);
    }

    public void OnAnimCompleted(string pageName)
    {
        EventMgr.Instance.DispatchEvent(new CoreEvent(GameEventIDs.EID_ANIMATION_COMPLETED, pageName));
    }
}