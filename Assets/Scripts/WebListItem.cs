using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ListView;


/**
 * Override the JSONItem class and brdige it to the advanced list item. 
 * */
public class WebListItem : JSONItem
{


    public override void Setup(WebListItemData data)
    {
        base.Setup(data);
        //Store a reference to the web list, we'll need this for our count of disabled objects;
    }

}

/**
 * The main data structure we're working with. 
 * This will parse most of the JSON and assign it to variables
 * for later consumption.
 * */
public class WebListItemData : ListViewItemNestedData<WebListItemData>
{
    public string type, duration, collection, title, id, model, text;

    private string childTemplate;


    //This only exists to shut the example scene up
    //TODO remove this
    public void FromJSON(JSONObject obj)
    {

    }


    //Get everything we need from the JSON
    public void FromJSON(JSONObject obj, WebList list, bool makeChild)
    {
        childTemplate = list.templates[list.templates.Length - 1].name;

        //Use this temp object to dig as deep as we need to. 
        JSONObject temp;

        //since the title may not have an FI language we have to parse this differently
        temp = obj.GetField("itemTitle");
        if (temp.list.Count > 0)
        {
            title = temp.list[0].ToString();
        }
        else
        {
            obj.GetField(ref title, "title");
        }

        obj.GetField(ref id, "id");

        obj.GetField(ref duration, "duration");
        obj.GetField(ref collection, "collection");
        obj.GetField(ref type, "type");

        if (makeChild)
        {
            childrenList = new List<WebListItemData>();
            childrenList.Add(new WebListItemData());
            for (int i = 0; i < childrenList.Count; i++)
            {
                
                childrenList[i].template = childTemplate;
                childrenList[i].FromJSON(obj, list, false);
            }
        }




    }


}