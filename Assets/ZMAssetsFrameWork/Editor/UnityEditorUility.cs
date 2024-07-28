using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEditorUility
{
    public static GUIStyle GetGUIStyle(string styleName)
    {
        GUIStyle guiStyle = null;
        foreach (GUIStyle item in GUI.skin.customStyles)
        {
            if(string.Equals(item.name.ToLower(), styleName.ToLower()))
            {
                guiStyle = item;
                break;
            }
        }
        return guiStyle;
    }
}
