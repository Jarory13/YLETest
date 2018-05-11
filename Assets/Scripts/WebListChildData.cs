using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ListView;


/*
 * An override for the advanced list item that will allow us to grab all the details
 * we need to fill out the details pane when an item is clicked. This entire
 * class is written from scratch
 * **/
public class WebListChildData : ListViewItemNestedData<WebListChildData>
{

    public string type, duration, collection, title, description, model, text;
    public WebList list;


    public void FromJSON(JSONObject obj, WebList list)
    {
        Debug.Log($"We have a json details list as { obj}");

        JSONObject temp;
        this.list = list;
        temp = obj.GetField("title");
        temp.GetField(ref title, "und");
        obj.GetField(ref description, "description");
        obj.GetField(ref model, "model");
        obj.GetField(ref template, "template");
        obj.GetField(ref template, "duraction");
        obj.GetField(ref type, "duraction");
        obj.GetField(ref type, "collection");
        obj.GetField("children", delegate (JSONObject _children)
        {
            children = new WebListChildData[_children.Count];
            for (int i = 0; i < _children.Count; i++)
            {
                children[i] = new WebListChildData();
                children[i].FromJSON(_children[i], list);
            }
        });
    }
}
