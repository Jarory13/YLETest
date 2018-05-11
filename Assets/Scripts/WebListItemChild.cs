using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ListView;


/*
 * A class modled after the advanced list item. This should be attached 
 * to child object to serve as a details tab when the main descriptor is clicked. 
 * */
public class WebListItemChild : WebListItem
{

    public TextMesh id;
    public TextMesh collection;
    public TextMesh type;
    public TextMesh duration;
    public TextMesh title;

    public override void Setup(WebListItemData data)
    {
        base.Setup(data);
        id.text = data.id;
        title.text = data.title;
        collection.text = data.collection;
        type.text = data.type;
        duration.text = data.duration;


    }

}
