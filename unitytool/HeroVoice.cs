using UnityEngine;
using System.Collections;

public class HeroVoice : MonoBehaviour
{
    public string[] voices;

    public float delayTime = 0.5f;

    public bool disableRemove = true;

    string mCurrentVoice = null;
    void OnEnable()
    {
        StartCoroutine(DelayPlaySound());
    }

    void OnDisable()
    {
        if(disableRemove)
            StopSound();
    }

    public void PlaySound()
    {
        if (voices == null || voices.Length == 0) return;

        StopSound();

        int idx = Random.Range(0, voices.Length);
        mCurrentVoice = voices[idx];
        MusicMgr.Instance.PlaySound("res/herovoice", mCurrentVoice);
    }

    void StopSound()
    {
        if (string.IsNullOrEmpty(mCurrentVoice)) return;

        MusicMgr.Instance.RemoveSound(mCurrentVoice);
    }

	public void PlayAnimSound(string str)
	{
		string[] strList = StringUtil.split (str, ';');
		if (strList.Length == 0)
			return;

		string audioName = "";
		string pkgName = "res/fight-sound";
		if (strList.Length == 1) {
			audioName = strList [0];
		}
		else if (strList.Length > 1) {
			pkgName = strList [0];
			audioName = strList [1];
		}

		MusicMgr.Instance.PlaySound (pkgName, audioName);
	}

    IEnumerator DelayPlaySound()
    {
        yield return new WaitForSeconds(delayTime);

        PlaySound();
    }
}
