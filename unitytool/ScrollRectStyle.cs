using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using XLua;
using UnityEngine.EventSystems;

/// <summary>
///  scrollview 中兼容俩个不规则物体
/// </summary>
public class ScrollRectStyle: MonoBehaviour { 
    public GameObject mTitleGo;   //物体1
    public GameObject mItemGo;    //物体2
    public GameObject mParent;     //显示的总容器

    public int mColumnCount = 5;   //每行的数量 
    public int mLineSpace1 = 10;      //行间距
    public int mColumnSpace = 5;     //列间距
    public int mLineSpace2 = 10;      //行间距
    public int top = 0;     //顶部距离


    public ScrollRect mScrollView;
    List<Vector2> mScrollVirtualView = new List<Vector2>();  //容器中元素的位置信息列表
	Rect mScrollViewRect;  //世界视口

    int mTitleGoWidth;
    int mTitleGoHeight;
    int mItemGoWidth;
    int mItemGoHeight;
    
    Queue<GameObject> mTitleGoPool = new Queue<GameObject>();    //对象池
    Queue<GameObject> mItemGoPool = new Queue<GameObject>();
    
    Vector3 mParentVec;
    PointerEventData mPointerEventData = null;
    bool mIsDraging = false;

    List<int> dataList = new List<int>();
    public List<int> DataList{
        set{
            dataList = value;
        }
        get{
            return dataList;
        }
    }

    object mTrig = null;
    public object Trig {
        get{
            return mTrig;
        }
        set{
            mTrig = value;
        }
    }

    List<object> titleData;      //第一种物体
    public List<object> TitleData{
        set{
            titleData = value;
            //AppRoot.LOG(titleDataList[0]);
            //Act(titleDataList[0]);
        }
        get{
            return titleData;
        }
    }

    Dictionary<int, List<object>> itemData;     //第二种物体
    public Dictionary<int, List<object>> ItemData{
        set{      
            itemData = value;
        }
        get{
            return itemData;
        }
    }

    Action<GameObject, object, object> tilteAct;
    public Action<GameObject, object, object> FillTitleAct{
        set{
            tilteAct = value;
        }
        get{
            return tilteAct;
        }
    }

    Action<GameObject, object, object> itemAct;
    public Action<GameObject, object, object> FillItemAct{
        set{
            itemAct = value;
        }
        get{
            return itemAct;
        }
    }

    void Start(){
        mParentVec = mParent.transform.position;
        /* 
        dataList.Add(1);
        dataList.Add(2);
        dataList.Add(3);
        dataList.Add(4);
        dataList.Add(5);
        dataList.Add(6);
        dataList.Add(7);        
        dataList.Add(50);
        dataList.Add(60);

        
        FillSelf();
        */
    }

    void InitVirtualScrollView()
	{
		ScrollRect scroll = mScrollView; 
		if (!scroll) return;
        if (TitleData == null || ItemData == null) return;

        LayoutElement titleLayElement = null;
		RectTransform titleRectTrans = null;
        LayoutElement itemLayElement = null;
		RectTransform itemRectTrans = null;
 
        if (mTitleGo) {
			titleLayElement = mTitleGo.GetComponent<LayoutElement> ();
			titleRectTrans = mTitleGo.GetComponent<RectTransform> ();
		} else
			return;

        if (mItemGo) {
			itemLayElement = mItemGo.GetComponent<LayoutElement> ();
			itemRectTrans = mItemGo.GetComponent<RectTransform> ();
		} else
			return;

        mTitleGoWidth = Mathf.RoundToInt(titleLayElement.preferredWidth);
		mTitleGoHeight = Mathf.RoundToInt(titleLayElement.preferredHeight);
        mItemGoWidth = Mathf.RoundToInt(itemLayElement.preferredWidth);
		mItemGoHeight = Mathf.RoundToInt(itemLayElement.preferredHeight);

		mScrollVirtualView.Clear ();

		int xoff = 0;
		int yoff = 0;

        for(int i = 0; i < TitleData.Count; i++){
            mScrollVirtualView.Add (new Vector2 (xoff + titleLayElement.preferredWidth * titleRectTrans.pivot.x, yoff - titleLayElement.preferredHeight * titleRectTrans.pivot.y));
		    yoff -= mTitleGoHeight;
            yoff -= mLineSpace2;

            for(int j = 0; j < ItemData[i + 1].Count; j++){
                xoff = mColumnSpace;
                mScrollVirtualView.Add (new Vector2 (xoff + itemLayElement.preferredWidth * itemRectTrans.pivot.x, yoff - itemLayElement.preferredHeight * itemRectTrans.pivot.y));
                xoff += mItemGoWidth;
                xoff += mColumnSpace;

                for(int z = 1; z<mColumnCount; z++){
                    if (++j < ItemData[i + 1].Count) {
				        mScrollVirtualView.Add (new Vector2 (xoff + itemLayElement.preferredWidth * itemRectTrans.pivot.x, yoff - itemLayElement.preferredHeight * itemRectTrans.pivot.y));
                        xoff += mItemGoWidth;
                        xoff += mColumnSpace;
                    }
                }
                yoff -= mItemGoHeight;
                yoff -= mLineSpace1;
            }
            xoff = 0;
        }
	
		RectTransform rectTrans = mParent.GetComponent<RectTransform> ();
		rectTrans.sizeDelta = new Vector2 (rectTrans.sizeDelta.x, -yoff + top);

		rectTrans = scroll.GetComponent<RectTransform> ();
		mScrollViewRect = GetWorldRect (rectTrans);
		mPointScale = mScrollViewRect.width / rectTrans.sizeDelta.x;
    }

    void OnScrollMove(Vector2 data) 
    {
        FillSelf();
    }

    void FillSelf()
    {
        int vIndex = 0;
        for(int i = 0; i< TitleData.Count; i++)
        {
            if (mTitleGo == null) return; 
            FillTitleGo(vIndex, i);
            vIndex++;
            for(int j = 0; j< ItemData[i + 1].Count; j++)
            {
                FillItemGo(vIndex, i , j);
                vIndex++;
            }
        }
    }

    void FillTitleGo1(int vIndex, int i){
        Rect itemRect;

        string goName = "title"+ i;
        GameObject titleGo = FindGo(mParent, goName);
           
        if(titleGo == null)
        {
            titleGo = PopTitleGo();
            titleGo.transform.SetParent(mParent.transform);
            titleGo.name =  "title"+ i;
            titleGo.transform.localScale = mTitleGo.transform.localScale;
            RectTransform rtTrans = titleGo.GetComponent<RectTransform> ();
		    rtTrans.anchoredPosition = mScrollVirtualView [vIndex];
            itemRect = GetWorldRect(rtTrans); 
        }
        else
        {
            if (true)
            {
                RectTransform rtTrans = titleGo.GetComponent<RectTransform>();
                rtTrans.anchoredPosition = mScrollVirtualView[vIndex];
                itemRect = GetWorldRect(rtTrans); 
            }
        }
        titleGo.SetActive(true);
        if(!mScrollViewRect.Overlaps(itemRect))
        {
            if(titleGo)
                PushTitleGo(titleGo);
        }
        else{
            FillTitleAct(titleGo, TitleData[i], mTrig);  
        }
    }

    void FillTitleGo(int vIndex, int i)
    {
        Vector2 v2 = mScrollVirtualView[vIndex];
        //Rect itemRect = WorldRect2(mScrollView.transform.GetComponent<RectTransform>(), new Vector3(v2.x, v2.y, 0));
        float oy = mParent.transform.GetComponent<RectTransform>().anchoredPosition.y;
        //Vector2 p;
        //UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(mScrollView.transform.GetComponent<RectTransform>(), new Vector2(v2.x, v2.y - mTitleGoHeight + oy), Engine.Lib.KTool.FindCameraForLayer(UnityEngine.LayerMask.NameToLayer("UI")), out p);

        Rect itemRect = GetWorldRect(mScrollView.transform, (int)v2.x, (int)((v2.y - mTitleGoHeight + oy + mScrollView.transform.GetComponent<RectTransform>().rect.height * 0.5)), mTitleGoWidth, mTitleGoHeight);
        string goName = "title" + i;
        GameObject titleGo = FindGo(mParent, goName);
        if (mScrollViewRect.Overlaps(itemRect))
        {
            if (titleGo == null)
            {
                titleGo = PopTitleGo();
                titleGo.transform.SetParent(mParent.transform);
                titleGo.name = "title" + i;
                titleGo.transform.localScale = mTitleGo.transform.localScale;
                RectTransform rtTrans = titleGo.GetComponent<RectTransform>();
                rtTrans.anchoredPosition = mScrollVirtualView[vIndex];
                FillTitleAct(titleGo, TitleData[i], mTrig);
            }
            else
            {
                //RectTransform rtTrans = titleGo.GetComponent<RectTransform>();
                //rtTrans.anchoredPosition = mScrollVirtualView[vIndex];
            }
            titleGo.SetActive(true);
        }
        else
        {
            if (titleGo)
                PushTitleGo(titleGo);
        }
    }

    Vector2 v2;
    Rect cellRect;
    GameObject cellGo;
    RectTransform cellRT;

    void FillItemGo(int vIndex, int i, int j)
    {
        v2 = mScrollVirtualView[vIndex];

        //Rect itemRect = GetWorldRect(mScrollView.transform, (int)v2.x, (int)((v2.y - mItemGoHeight + oy + mScrollView.transform.GetComponent<RectTransform>().rect.height * 0.5)), mItemGoWidth, mItemGoHeight);
        Rect cellRect = GetWorldRect(mParent.transform, (int)v2.x, (int)((v2.y)), mItemGoWidth, mItemGoHeight);

        string goName = "item" + i + j;
        GameObject cellGo = FindGo(mParent, goName);
        if (mScrollViewRect.Overlaps(cellRect))
        {
            if (cellGo == null)
            {
                Debug.Log("1");
                cellGo = PopItemGo();
                cellGo.name = "item" + i + j;
                //FillItemAct(cellGo, li[j], mTrig);
                cellRT = cellGo.GetComponent<RectTransform>();
                cellRT.anchoredPosition = mScrollVirtualView[vIndex];
                cellGo.SetActive(true);
            }
        }
        else
        {
            if (cellGo)
                PushItemGo(cellGo);
        }
        /*
        itemGo = FindGo(mParent, goName);

        if (!mScrollViewRect.Overlaps(itemRect))
        {
            if (itemGo)
            {
                PushItemGo(itemGo);
            }
            return;
        }

        if (itemGo == null)
        {
            itemGo = PopItemGo();
            itemGo.transform.SetParent(mParent.transform);
            itemGo.name = "item" + i + j;
            itemGo.transform.localScale = itemGo.transform.localScale;
            RectTransform rtTrans = itemGo.GetComponent<RectTransform>();
            rtTrans.anchoredPosition = mScrollVirtualView[vIndex];
        }
        else
        {
            RectTransform rtTrans = itemGo.GetComponent<RectTransform>();
            rtTrans.anchoredPosition = mScrollVirtualView[vIndex];
        }
        itemGo.SetActive(true);
        if (itemGo) { 
            List<object> li = ItemData[i + 1];
            FillItemAct(itemGo, li[j], mTrig);
        }
        */
    }


    /*
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            CacheDragPointerEventData(eventData);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = true;
            CacheDragPointerEventData(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = false;
            mPointerEventData = null;
        }

        void Update()
        {
            if (mNeedAdjustVec)
            {
                mNeedAdjustVec = false;
                if (mScrollRect.velocity.y * mAdjustedVec.y > 0)
                {
                   mScrollRect.velocity = mAdjustedVec;
                }
            }
            UpdateSnapMove();
            UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
            ClearAllTmpRecycledItem();
            mLastFrameContainerPos = mContainerTrans.anchoredPosition3D;
        }



        void CacheDragPointerEventData(PointerEventData eventData)
        {
            if (mPointerEventData == null)
            {
                mPointerEventData = new PointerEventData(EventSystem.current);
            }
            mPointerEventData.button = eventData.button;
            mPointerEventData.position = eventData.position;
            mPointerEventData.pointerPressRaycast = eventData.pointerPressRaycast;
            mPointerEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
        }
        */

    void FillItemGo1(int vIndex, int i, int j)
    {
        Rect itemRect;
        string goName =  "item"+ i + j;
        GameObject itemGo = FindGo(mParent, goName);

        if(itemGo == null)
        {
            itemGo = PopItemGo();
            itemGo.transform.SetParent(mParent.transform);
            itemGo.name = "item"+ i + j;
            itemGo.transform.localScale = itemGo.transform.localScale;
            RectTransform rtTrans = itemGo.GetComponent<RectTransform>();
            rtTrans.anchoredPosition = mScrollVirtualView [vIndex];
            itemRect = GetWorldRect(rtTrans); 
        }
        else
        {
            if (true)
            {
                RectTransform rtTrans = itemGo.GetComponent<RectTransform>();
                rtTrans.anchoredPosition = mScrollVirtualView[vIndex];
                itemRect = GetWorldRect(rtTrans); 
            }
        }       
        itemGo.SetActive(true);
        if(!mScrollViewRect.Overlaps(itemRect))
        {
            if(itemGo)
            PushItemGo(itemGo);
        }    
        else{
            List<object> li = ItemData[i + 1];
            FillItemAct(itemGo, li[j], mTrig);
        }
    }


	GameObject PopTitleGo()
	{
		if (mTitleGoPool.Count > 0)
			return mTitleGoPool.Dequeue ();

		GameObject item = GameObject.Instantiate(mTitleGo, mParent.transform);
        return item;
	}
	void PushTitleGo(GameObject go)
	{
        //go.transform.SetParent (null);
        //go.SetActive (false);
        VisibleGO(go, false);
        mTitleGoPool.Enqueue (go);
	}

    GameObject PopItemGo()
    {
        if (mItemGoPool.Count > 0)
            return mItemGoPool.Dequeue();

        GameObject item = GameObject.Instantiate(mItemGo, mParent.transform);
        return item;
    }
    void PushItemGo(GameObject go)
    {
        //go.transform.SetParent(null);
        //go.SetActive(false);
        VisibleGO(go, false);
        mItemGoPool.Enqueue(go);
    }

	readonly Vector3[] mWorldCorners = new Vector3[4];
	float mPointScale = 1.0f;
	Rect GetWorldRect(RectTransform rtTrans)
	{
		rtTrans.GetWorldCorners (mWorldCorners);

		return new Rect (mWorldCorners [0].x, mWorldCorners [0].y, mWorldCorners [2].x - mWorldCorners[0].x, mWorldCorners [2].y - mWorldCorners[0].y);
	}

	Rect GetWorldRect(Transform parentTrans, int cx, int cy, int w, int h)
	{
		Vector3 wPos = parentTrans.localToWorldMatrix.MultiplyPoint (new Vector3 (cx, cy, 0));
		Vector2 center = new Vector2 (wPos.x, wPos.y);
		Vector2 size = new Vector2 (w * mPointScale, h * mPointScale);
		return new Rect (center - size / 2, size);
	}

    void VisibleGO(GameObject go, bool visiable)
    {
        if(visiable) go.SetActive(true);
        else go.SetActive(false);
    }

    GameObject FindGo(GameObject go, string name)
    {
        Transform trf = go.transform.Find(name);
        if(trf)
            return trf.gameObject;
        else
            return null;
    }

    public void RefeshChangePos(){
        mParent.transform.position = mParentVec;
        mScrollView.onValueChanged.RemoveAllListeners();
        InitVirtualScrollView();
        DisableAllGo();
        FillSelf();
        mScrollView.onValueChanged.AddListener(OnScrollMove);
    }

    public void Refesh(){
        mScrollView.onValueChanged.RemoveAllListeners();
        InitVirtualScrollView();
        DisableAllGo();
        FillSelf();
        mScrollView.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<Vector2>(OnScrollMove));
    }

    public void FillBegin()
    {
        InitVirtualScrollView();
        FillSelf();
        mScrollView.onValueChanged.AddListener(OnScrollMove);
    }

    void DisableAllGo()
    {
        for(int i = 0; i< mParent.transform.childCount; i++)
        {
            mParent.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    void OnDestroy(){
        while(mTitleGoPool.Count > 0){
            Destroy(mTitleGoPool.Dequeue());
        }
        while(mItemGoPool.Count > 0){
            Destroy(mItemGoPool.Dequeue());
        }
    }
}
