using UnityEngine;

namespace ListView
{
    //!! Change ListViewItem<JSONItemData> to use custom WebListItemDataType
    public class JSONItem : ListViewItem<WebListItemData>
    {
        public TextMesh label;

        public override void Setup(WebListItemData data)
        {
            base.Setup(data);
            label.text = data.text;
        }
    }

    public class JSONItemData : CubeItemData
    {
        public virtual void FromJSON(JSONObject obj)
        {
            obj.GetField(ref text, "text");
        }
    }
}