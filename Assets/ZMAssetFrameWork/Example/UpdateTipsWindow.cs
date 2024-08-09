using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTipsWindow : MonoBehaviour
{
    /// <summary>
    /// 更新回调
    /// </summary>
    private Action OnUpdateCallback;

    /// <summary>
    /// 退出回调
    /// </summary>
    private Action OnQuitCallback;

    /// <summary>
    /// 内容文本
    /// </summary>
    public Text contentText;

    public void InitView(string content, Action onUpdateCallback, Action onQuitCallback)
    {
        this.OnUpdateCallback = onUpdateCallback;
        this.OnQuitCallback = onQuitCallback;
        contentText.text = content;
    }
    
    public void OnUpdateButtonClick()
    {
        this.OnUpdateCallback?.Invoke();
        Destroy(gameObject);
    }

    public void OnQuitButtonClick()
    {
        this.OnQuitCallback?.Invoke();
        Destroy(gameObject); 
    }
}
