using UnityEngine;

//Uses JSONObject http://u3d.as/1Rh

namespace ListView
{
    public class JSONList : ListViewController<WebListItemData, JSONItem>
    {
        public string dataFile;
        public string defaultTemplate;

        protected override void Setup()
        {
            base.Setup();
            TextAsset text = Resources.Load<TextAsset>(dataFile);
            if (text)
            {
                JSONObject obj = new JSONObject(text.text);
                data = new WebListItemData[obj.Count];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new WebListItemData();
                    data[i].FromJSON(obj[i]);
                    data[i].template = defaultTemplate;
                }
            } else data = new WebListItemData[0];
        }
    }
}