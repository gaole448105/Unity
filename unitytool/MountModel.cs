using UnityEngine;

public class MountModel : MonoBehaviour
{
    /// <summary>
    /// 模型名字
    /// </summary>
    public string modelName;

    void Start()
    {
        Mount();
    }


    void Mount()
    {
        if (string.IsNullOrEmpty(modelName))
            return;

#if UNITY_EDITOR
        string model = "Assets/GameAssets/monster_new/Kaichang/" + modelName + ".prefab";
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(model);
        if (prefab != null)
        {
            GameObject go = GameObject.Instantiate<GameObject>(prefab);
            OnLoadModelCompleted(go);
        }
#else
        string packageName = "monster/kaichang";
        CacheResPoolMgr.Instance.LoadModel(packageName, modelName, OnLoadModelCompleted);
#endif

    }

    void OnLoadModelCompleted(GameObject go)
    {
        if (go == null) return;

        go.transform.SetParent(this.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.Euler(Vector3.zero);
        go.name = modelName;
    }
}