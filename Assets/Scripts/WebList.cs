using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace ListView
{

    /**
     * A script inherited from the list view package 
     * Aside from the GetBatch Function, most of this script has been changed
     * from it's original implementation. 
     * */
    public class WebList : ListViewController<WebListItemData, WebListItem>
    {

        //If we search again, we need to create a new query
        public UnityEvent OnNewSearch;

        //Use this bool to determine if details need to be cleared. 
        private bool detailsPopulated;

        //A reference to the inputField where the use types
        public InputField inputField;

        //A reference to the gameobject we'll use to place the detail's view
        public GameObject detailsHolder;



        //The name of the field that corresponds to the subject of a program
        [SerializeField]
        private string subjectJsonSearchField = "subject";
        //The name of the field that corresponds to the title of a program
        [SerializeField]
        private string titleJsonSearch = "title";

        //The name of the field that corresponds to the proper language
        [SerializeField]
        private string languageSearchField = "fi";

        //the root url we're going to be using
        [SerializeField]
        private string baseUrl = Constants.baseUrl;

        //append this string to add the 10 limit count on queries
        private string limitString = "&&limit=";
        private string queryStart = "&&q=";
        private int offsetCount = 0;

        public string URLFormatString;
        public string defaultTemplate = "WebItem";
        public string offsetQuery = "&&offset=";

        public int batchSize = 10;

        //A reference to the attached script in the details pane
        private WebListItemChild details;

        //Use this as a temp variable for who is currenlty expanded
        private WebListItemData previousDetailsHolder;

        delegate void WebResult(List<WebListItemData> data);

        //inherited items from listview
        readonly Dictionary<string, Vector3> m_TemplateSizes = new Dictionary<string, Vector3>();
        float m_ItemHeight;
        float m_ScrollReturn = float.MaxValue;

        int m_BatchOffset;
        bool m_WebLock;
        bool m_Loading;

        List<WebListItemData> m_Cleanup;

        private void Awake()
        {
            GameObject inputFieldGameObject = GameObject.FindGameObjectWithTag(Constants.inputTag);
            if (inputFieldGameObject)
            {
                inputField = inputFieldGameObject.GetComponent<InputField>();
            }

            //set up the on New Search event so we can query again if needed
            if (OnNewSearch == null)
            {
                OnNewSearch = new UnityEvent();
            }

            OnNewSearch.AddListener(delegate { NewSearchHappened(); });

            details = GetComponentInChildren<WebListItemChild>();

            //Set this to true so that we can clear out the dummy data in details;
            detailsPopulated = true;
            ClearDetails();

        }

        protected override void Setup()
        {
            base.Setup();

            foreach (var kvp in m_Templates)
            {
                if (!m_TemplateSizes.ContainsKey(kvp.Key))
                {
                    m_TemplateSizes[kvp.Key] = GetObjectSize(kvp.Value.prefab);
                }
            }

            QueryStringSetup();

            StartCoroutine(GetBatch(offsetCount * batchSize, batchSize, data =>
            { this.DataList = data; }));
        }

        IEnumerator GetBatch(int offset, int range, WebResult result)
        {
            if (m_WebLock)
                yield break;
            m_WebLock = true;
            List<WebListItemData> items = new List<WebListItemData>();

            string newOffset = string.Concat(offsetQuery, offset);
            string offsetSubSTring = URLFormatString.Substring(URLFormatString.IndexOf(offsetQuery));
            URLFormatString = URLFormatString.Replace(offsetSubSTring, newOffset);
            WWW www = new WWW(string.Format(URLFormatString, offset, range));

            while (!www.isDone)
            {
                yield return null;
            }

            JSONObject response = new JSONObject(www.text);

            // Get the last piece of the Json response. This corresponds to "Data
            //Which is where everything we need lies. 
            response = response.list[response.list.Count - 1];
            //Debug.Log($"Response is {response.list[0]}");
            for (int i = 0; i < response.list.Count; i++)
            {
                JSONObject temp;

                items.Add(new WebListItemData { template = defaultTemplate });
                //Get most of what we need here
                items[i].FromJSON(response.list[i], this, true);

                //Dive into the "subject" field in the JSON
                temp = response[i].GetField(subjectJsonSearchField);

                //Grab the first entry in the list, which contains the fi title if it exists
                //In some cases this field is null, in which case we can just leave it blank
                temp = temp.list.Count > 0 ? temp.list[0].GetField(titleJsonSearch) : null;
                if (temp)
                {
                    temp.GetField(ref items[i].text, languageSearchField);
                }
            }
            result(items);
            m_WebLock = false;
            m_Loading = false;
            offsetCount++;
        }

        protected override void ComputeConditions()
        {
            if (templates.Length > 0)
            {
                //Use first template to get item size
                m_ItemSize = GetObjectSize(templates[0]);
            }
            //Resize range to nearest multiple of item width
            m_NumItems = Mathf.RoundToInt(range / m_ItemSize.y); //Number of cards that will fit
            range = m_NumItems * m_ItemSize.y;

            //Get initial conditions. This procedure is done every frame in case the collider bounds change at runtime
            m_LeftSide = transform.position + Vector3.up * range * 0.5f + Vector3.left * m_ItemSize.x * 0.5f;

            m_DataOffset = (int)(scrollOffset / itemSize.y);
            if (scrollOffset < 0)
                m_DataOffset--;

            int currBatch = -m_DataOffset / batchSize;

            //Basically this will check our scrolling to see if we need to ask for more data. 
            //TODO: smooth out this process
            if (-m_DataOffset > offsetCount * batchSize)
            {

                //Check how many batches we jumped
                if (currBatch == m_BatchOffset + 1)
                { //Just one batch, fetch only the next one
                    StartCoroutine(GetBatch((offsetCount * batchSize) * batchSize, batchSize, words =>
                    {
                        foreach (WebListItemData item in words)
                        {
                            DataList.Add(item);
                        }
                        m_BatchOffset++;
                    }));
                }
                else if (currBatch != m_BatchOffset)
                { //Jumped multiple batches. Get a whole new dataset
                    if (!m_Loading)
                    {
                        m_Loading = true;
                        m_Cleanup = DataList;
                        StartCoroutine(GetBatch(offsetCount * batchSize, batchSize * 3, words =>
                        {
                            foreach (WebListItemData item in words)
                            {
                                DataList.Add(item);
                            }
                            m_BatchOffset = currBatch - 1;
                        }));

                    }
                }
            }
            else if (m_BatchOffset > 0 && -m_DataOffset < (m_BatchOffset + 1) * batchSize)
            {
                if (currBatch == m_BatchOffset)
                { //Just one batch, fetch only the next one
                    StartCoroutine(GetBatch(offsetCount * batchSize, batchSize, words =>
                    {
                        foreach (WebListItemData item in words)
                        {
                            DataList.Add(item);
                        }
                        m_BatchOffset--;
                    }));
                }
                else if (currBatch != m_BatchOffset)
                { //Jumped multiple batches. Get a whole new dataset
                    if (!m_Loading)
                        m_Cleanup = DataList;
                    m_Loading = true;
                    if (currBatch < 1)
                        currBatch = 1;
                    StartCoroutine(GetBatch(offsetCount * batchSize, batchSize * 3, words =>
                    {
                        foreach (WebListItemData item in words)
                        {
                            DataList.Add(item);
                        }
                        m_BatchOffset = currBatch - 1;
                    }));
                }

            }
            if (m_Cleanup != null)
            {
                //Clean up all existing gameobjects
                foreach (var item in m_Cleanup)
                {
                    if (item == null)
                        continue;
                    if (item.item != null)
                    {
                        RecycleItem(item.template, item.item);
                        item.item = null;
                    }
                }
                m_Cleanup = null;
            }
        }

        protected override void UpdateItems()
        {

            if (DataList == null || DataList.Count == 0 || m_Loading)
            {
                return;
            }

            float totalOffset = 0;
            UpdateRecursively(DataList, ref totalOffset);
            totalOffset -= m_ItemHeight;
            if (totalOffset < -scrollOffset)
            {
                m_ScrollReturn = -totalOffset;
            }
        }

        //Recursively update the position of a given item including its children 
        void UpdateRecursively(List<WebListItemData> listData, ref float totalOffset)
        {

            for (int i = 0; i < listData.Count; i++)
            {
                if (listData[i] == null)
                    continue;

                m_ItemHeight = m_TemplateSizes[listData[i].template].y;

                //This should almost also default to the final else
                if (i + m_DataOffset + m_BatchOffset * batchSize < -1)
                { //Checking against -1 lets the first element overflow
                    ExtremeLeft(listData[i]);
                }
                else if (i + m_DataOffset + m_BatchOffset * batchSize > m_NumItems)
                {
                    ExtremeRight(listData[i]);
                }
                else
                {
                    float nextOffset = i + m_BatchOffset * batchSize;

                    ListMiddle(listData[i], nextOffset);
                }

                totalOffset += m_ItemHeight;

                //If we have children we need to place them in the world
                if (listData[i].childrenList != null)
                {
                    if (listData[i].expanded)
                    {

                        DetailsExanpded(listData[i], listData[i].childrenList[0]);

                    }
                }
            }
        }

        //call setup on the details pane
        private void AssignDetails(WebListItemData data)
        {
            if (!detailsPopulated)
            {
                details.Setup(data);
            }
            detailsPopulated = true;
        }

        //clear the details pane so we can fill it with new data. 
        private void ClearDetails()
        {
            if (detailsPopulated)
            {
                details.id.text = "";
                details.collection.text = "";
                details.type.text = "";
                details.duration.text = "";
                details.title.text = "";
            }
            detailsPopulated = false;

            if (previousDetailsHolder != null)
            {
                previousDetailsHolder.expanded = false;
            }
        }

        //Called when we notice that the details pane is expaneded
        private void DetailsExanpded(WebListItemData parent, WebListItemData data)
        {
           
            ClearDetails();
            AssignDetails(data);
            previousDetailsHolder = parent;
        }

        //Inherited method. Get's the item for the webitemdata object and sets its position
        void ListMiddle(WebListItemData data, float offset)
        {
            if (data.item == null)
            {
                data.item = GetItem(data);
            }

            Positioning(data.item.transform, offset);

        }

        public void OnStopScrolling()
        {

            if (scrollOffset > 0)
            {
                scrollOffset = 0;

            }
            if (m_ScrollReturn < float.MaxValue)
            {
                scrollOffset = m_ScrollReturn;
                m_ScrollReturn = float.MaxValue;
            }
        }

        void RecycleChildren(WebListItemData data)
        {
            foreach (var child in data.childrenList)
            {
                RecycleItem(child.template, child.item);
                child.item = null;
                if (child.childrenList != null)
                    RecycleChildren(child);
            }
        }

        //Set the position of a new webitem or web item child
        protected virtual void Positioning(Transform t, float offset)
        {
            t.position = m_LeftSide + (offset * m_ItemSize.y + scrollOffset) * Vector3.down;
        }



        //We need to reset some things when we press search after we've already searched
        private void NewSearchHappened()
        {
            //Set loading to true. Set batch will set this back to false when it's done
            //this keeps us from updating and reactivating the items we're trying to remove
            m_Loading = true;
            offsetCount = 0;
            scrollOffset = 0;

           

            if (DataList != null)
            {
                //Clean up all existing gameobjects
                foreach (var item in DataList)
                {
                    
                    if (item == null)
                        continue;
                    if (item.item != null && item.template != null)
                    {
                        Debug.Log($"{item.title} to be be recycled");
                        RecycleItem(item.template, item.item);
                        item.item = null;
                    }
                    if (item.item)
                    {
                        Debug.Log($"{item.title} to be set inactive");
                        item.item.gameObject.SetActive(false);
                        item.item = null;
                    }
                    else
                    {
                        Debug.Log($"Can't recycle item with title {item.title}");
                    }

                }
               
            }
            //Build a new query string to fetch using the new search
            QueryStringSetup();
            //If for some reason we were in the middle of fetching a batch let's stop that
            StopAllCoroutines();
            StartCoroutine(GetBatch(0, batchSize, data => { this.DataList = data; }));
        }


        //Grab the data from the input field and prepare the string to be passed to the www object
        private void QueryStringSetup()
        {
             
        
            //Create the limit parameter to be appended the to format string
            string newLimitString = string.Concat(limitString, batchSize);

            //Gather our query by replacing in spaces with %
            string query = inputField.text.Replace(" ", "%");
            query = string.Concat(queryStart, query);

            string offsetString = string.Concat(offsetQuery, offsetCount);

            //Combine everything into our final search string
            URLFormatString = String.Concat(baseUrl, Constants.APIKey, newLimitString, query, offsetString);
            Debug.Log(URLFormatString);
        }
    }


}