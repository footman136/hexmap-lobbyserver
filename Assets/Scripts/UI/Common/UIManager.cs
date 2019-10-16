using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Transform _rootLobby;
    [SerializeField] private Transform _rootInGame;
    [SerializeField] private Transform _rootLogo;
    
    public Transform RootLogo
    {
        get { return _rootLogo; }
    }

    public Transform RootLobby
    {
        get { return _rootLobby; }
    }

    public Transform RootInGame
    {
        get { return _rootInGame; }
    }
    
    private static UIManager _inst;
    public static UIManager Instance => _inst;

    void Awake()
    {
        _inst = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
#region 杂项    
    public bool IsPointerOverGameObject(Vector2 screenPosition)
    {
        //实例化点击事件
        PointerEventData eventDataCurrentPosition = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        //将点击位置的屏幕坐标赋值给点击事件
        eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);
 
        List<RaycastResult> results = new List<RaycastResult>();
        //向点击处发射射线
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
 
        return results.Count > 0;
    }
//    ————————————————
//    版权声明：本文为CSDN博主「PassionY」的原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接及本声明。
//    原文链接：https://blog.csdn.net/yupu56/article/details/54561553    

    private PanelSystemTips _systemTips;
    public void SystemTips(string msg, PanelSystemTips.MessageType msgType)
    {
        if (_systemTips == null)
        {
            var go = Resources.Load("UI/Common/PanelSystemTips");
            if (go!=null)
            {
                var go2 = Instantiate(go, transform) as GameObject;
                if (go2 != null)
                {
                    _systemTips = go2.GetComponent<PanelSystemTips>();
                }
            }
            else
            {
                Debug.LogError("UI/PanelSystemTips not found!");
            }
        }
        if (_systemTips != null)
        {
            _systemTips.Show(msg, msgType);
        }
    }

    /// <summary>
    /// 创建一个UI
    /// </summary>
    /// <param name="anchor">锚点，放在哪个节点下</param>
    /// <param name="packageName">资源包名</param>
    /// <param name="prefabName">预制件名</param>
    /// <returns>创建出来的UI</returns>
    public static GameObject CreatePanel(Transform anchor, string packageName, string prefabName)
    {
        var go = Resources.Load(prefabName);
        if (go != null)
        {
            var go2 = Instantiate(go, anchor) as GameObject;
            return go2;
        }

        Debug.LogError("UIManager - CreatePanel() Failed - <" + packageName + "> <" + prefabName + ">");
        return null;
    }

    public static void DestroyPanel(ref GameObject go)
    {
        if (go != null)
        {
            Destroy(go);
            go = null;
        }
    }
#endregion

#region 大厅界面

    public void ShowLobbyMenu(bool show)
    {
        
    }
#endregion
}
