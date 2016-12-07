using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UniLua;

public class UIHyperLabel : MonoBehaviour
{
    class HyperString 
    {
        public string content;
        public int line;
        public float length;
        public string inputInfo;//clickable will use inputInfo as id,sprite and texture will use as length
        public HyperType type;

        public HyperString(HyperType ty)
        {
            content = string.Empty;
            line = 1;
            inputInfo = string.Empty;
            type = (HyperType)ty;
            length = 0;
        }

        public HyperString(HyperType ty, string con)
        {
            content = con;
            line = 1;
            inputInfo = string.Empty;
            type = (HyperType)ty;
            length = 0;
        }
        public HyperString(HyperType ty, string con, int li)
        {
            content = con;
            line = li;
            inputInfo = string.Empty;
            type = (HyperType)ty;
            length = 0;
        }

    }

    public enum HyperType {
        PureString = 0,
        Sprite = 1,
        Clickable = 2,
        Voice = 3,
        Texture = 4,
    }
    public enum AlignType
    {
        Top,
        Bottom,
    }
    public string sourceTxt = string.Empty;
    List<HyperString> HyperStrs = new List<HyperString>();
    Dictionary<GameObject, HyperString> mClickableDic = new Dictionary<GameObject, HyperString>();
    List<UILabel> mLabelPool = new List<UILabel>();
    List<UISprite> mSpritePool = new List<UISprite>();
    List<UIVoiceChat> mVoicePool = new List<UIVoiceChat>();
    List<MonoBehaviour> mUsingWidgetPool = new List<MonoBehaviour>();
    

    public LuaBehaviour Contoller;
    public string ClickLuaFuncName = "OnHyperStringClick";
    public UILabel LblPrefab;
    public UISprite SprPrefab;
    public UIVoiceChat VoiPrefab;
    public AlignType AlignWay = AlignType.Top;//only support top,bottom now
    public float FirstLineOffset = 0;
    public float LenLimit = 0;
    public float HeightSpace = 24;
    public float ElementSpace = 0;
    public int LineLimit = 0;//the txt at line bigger then lineLimit will be cut off
    [HideInInspector]
    public int MaxLine = -1;
  
    int click_func_ref = LuaAPI.LUA_REFNIL;
    void Awake() {
        LuaState lua = LuaInstance.instance.Get();
        lua.GetGlobal(Contoller.ScriptName);
        lua.GetField(-1, ClickLuaFuncName);
        click_func_ref = lua.L_Ref(LuaAPI.LUA_REGISTRYINDEX);
        lua.Pop(1);
    }
    public void Start() {

    }

    void OnDestory() {
        LuaState lua = LuaInstance.instance.Get();
        lua.L_Unref(LuaAPI.LUA_REGISTRYINDEX,ref click_func_ref);
    }

    //DO NOT invoke this method after SetText() is invoked
    public int CalMaxLineNum(string txt)
    {
        if (LenLimit <= 0)
        {
            return 1;
        }
        else {
            if (MaxLine == -1)
            {
                DecodeText(txt);
                CalcuTextsLengthAndLine();
            }
            return MaxLine+1;
        }
    }

    public int CalMaxLineNumWithLen(string txt,float lenLim)
    {
        float oriLenLimit = LenLimit;
        LenLimit = lenLim;
        if (LenLimit <= 0)
        {
            LenLimit = oriLenLimit;
            return 1;
        }
        else
        {
            DecodeText(txt);
            CalcuTextsLengthAndLine();
            LenLimit = oriLenLimit;
            HyperStrs.Clear();
            return MaxLine + 1;
        }
    }


    //text format: purestring{2ItemName,100}purestring{1SpriteName,20}purestring
    //{1ItemName,100} represent: {(hyperType)(textWillShow),(ID)}
    public void SetText(string txt) {
        RecycleWidgets();
        sourceTxt = txt;
        HyperStrs = new List<HyperString>();
        mClickableDic = new Dictionary<GameObject, HyperString>();

        DecodeText(txt);
        if (LenLimit > 0)
        {
            CalcuTextsLengthAndLine();
        }
        else
        {
            for (int i = 0; i < HyperStrs.Count; i++)
            {
                Vector2 conLen = NGUIText.CalculatePrintedSize(HyperStrs[i].content, LblPrefab);
                conLen = new Vector2(conLen.x + ElementSpace, conLen.y);
                HyperStrs[i].length = conLen.x;
            }
        }
        DisplayText();
    }

    void RecycleWidgets() {
        if (mUsingWidgetPool.Count >0)
        {
            foreach (var widget in mUsingWidgetPool)
            {
                if (widget.GetType() == typeof(UILabel))
                {
                    mLabelPool.Add((UILabel)widget);
                }
                else if (widget.GetType() == typeof(UISprite))
                {
                    mSpritePool.Add((UISprite)widget);
                }
                widget.gameObject.SetActive(false);
            }
            mUsingWidgetPool.Clear();
        }
    }

    void DecodeText(string txt) {
        StringBuilder strB = new StringBuilder();
        for (int i = 0, imax = txt.Length; i < imax; ++i)
        {
            char c = txt[i];
            if (c == '\n')
            {
                if (strB.Length > 0)
                {
                    string content = strB.ToString();
                    HyperStrs.Add(new HyperString(HyperType.PureString, content));
                    strB.Length = 0;
                }
                HyperStrs.Add(new HyperString(HyperType.PureString, "\n"));
                continue;
            }

            // Skip invalid characters
            if (c < ' ') { continue; }

            //hit hyper text
            if (c == '{')
            {
                //save previous pure str
                if (strB.Length>0)
                {
                    string content = strB.ToString();
                    HyperStrs.Add(new HyperString(HyperType.PureString,content));
                    strB.Length = 0;
                }

                //decode one hyper text
                if (i + 1 < imax)
                {
                    //first char is the type
                    int type = int.Parse(txt[i + 1].ToString());
                    HyperString hyperS = new HyperString((HyperType)type);
                    i += 2;
                    //get the content before we hit first ','
                    StringBuilder contentS = new StringBuilder();
                    for (; i < imax; i++)
                    {
                        if (txt[i] == ',')
                        {
                            //get Id before we hit first '}'
                            StringBuilder idS = new StringBuilder();
                            i++;
                            for (; i < imax; i++)
                            {
                                if (txt[i] == '}')
                                {
                                    hyperS.inputInfo = idS.ToString();
                                    break;
                                }

                                idS.Append(txt[i]);
                            }
                        }

                        if (txt[i] == '}')
                        {
                            string content = contentS.ToString();
                            hyperS.content = content;
                            HyperStrs.Add(hyperS);
                            break;
                        }
                        contentS.Append(txt[i]);
                       
                    }
                }
            }
            else
            {
                strB.Append(c);
            }
        }

        if (strB.Length>0)
        {
            string content = strB.ToString();
            HyperStrs.Add(new HyperString(HyperType.PureString, content));
        }
    }

    void CalcuTextsLengthAndLine() {
        int lineNumNow = 0;
        float oneLenLeft = LenLimit - FirstLineOffset;
        int hitLineLimitIndex = -1;
        for (int i = 0, iMax = HyperStrs.Count; i < iMax; i++)
        {
            if (LineLimit>0&&lineNumNow>LineLimit)
            {
                hitLineLimitIndex = i;
                break;
            }
       
            string content = HyperStrs[i].content;
            if (content =="\n")
            {
                oneLenLeft = LenLimit;
                lineNumNow++;
                continue;
            }

            Vector2 conLen = GetHyperStrLength(HyperStrs[i]);
           
            if (conLen.x <= oneLenLeft || conLen.x - ElementSpace <= oneLenLeft)
            {
                //set this line now,can short the length left in this line
                oneLenLeft -= conLen.x;
                HyperStrs[i].line = lineNumNow;
            }
            else { 
                //this content is cross tow line
                if (HyperStrs[i].type == HyperType.PureString)
                {
                    //pure string will cut self to twzo new string,old str remain this line,new str insert to list,then check next routine
                    HyperString purStr = HyperStrs[i];
                    int changeLineIndex = NGUIText.GetCharIndexFitLength(content, oneLenLeft, LblPrefab);

                    string remainStr = content.Substring(0, changeLineIndex);
                    string newStr = content.Substring(changeLineIndex, content.Length - changeLineIndex);
                    RepairStringColor(ref remainStr,ref newStr);
                    HyperStrs[i].line = lineNumNow;
                    HyperStrs[i].content = remainStr;
                    HyperStrs.Insert(i+1,new HyperString(HyperType.PureString, newStr));
                    iMax = HyperStrs.Count;
                    oneLenLeft = LenLimit;
                }
                else
                {
                    //otherwise,move to next line
                    HyperStrs[i].line = lineNumNow+1;
                    oneLenLeft = LenLimit;
                    oneLenLeft -= conLen.x;
                }
                lineNumNow++;
            }
        }
        if (hitLineLimitIndex > 0)
        {
            HyperStrs.RemoveRange(hitLineLimitIndex, HyperStrs.Count - hitLineLimitIndex-1);
        }
        MaxLine = lineNumNow;
    }

    Vector2 GetHyperStrLength(HyperString hyperStr) {
        Vector2 conLen = Vector2.zero;
        if (hyperStr.type == HyperType.Sprite)
        {
           
            if (hyperStr.inputInfo != string.Empty)
            {
                conLen.x = int.Parse(hyperStr.inputInfo);
            }
            else {
                Vector3 sprSize = SprPrefab.Dimensions;
                conLen.x = sprSize.x;
            }
            hyperStr.length = conLen.x;
        }
        else if (hyperStr.type == HyperType.PureString || hyperStr.type == HyperType.Clickable)
        {
            conLen = NGUIText.CalculatePrintedSize(hyperStr.content, LblPrefab);
            conLen = new Vector2(conLen.x + ElementSpace, conLen.y);
            hyperStr.length = conLen.x;
        }
        else if (hyperStr.type == HyperType.Voice)
        {
            if (VoiPrefab == null)
            {
                Debug.Log(gameObject.name);
            }
            Vector3 sprSize = VoiPrefab.BackgroundSprite.Dimensions;
            conLen.x = sprSize.x;
            hyperStr.length = conLen.x;
        }

        return conLen;
    }

    void DisplayText() {
        float lineLenNow = FirstLineOffset;
        int lastStrLine = 0;
        
        for (int i = 0; i < HyperStrs.Count; i++)
        {
            if (lastStrLine != HyperStrs[i].line)
            {
                lastStrLine = HyperStrs[i].line;
                lineLenNow = 0;
            }
            HyperString hyperStr = HyperStrs[i];
            float heightSpace = AlignWay == AlignType.Bottom ? HeightSpace * (MaxLine - hyperStr.line) : -HeightSpace * hyperStr.line;
            if (hyperStr.type == HyperType.PureString || hyperStr.type == HyperType.Clickable)
            {
                UILabel lbl = GetLabel();
                lbl.text = hyperStr.content;
                lbl.transform.localPosition = new Vector3(lineLenNow, heightSpace , lbl.transform.localPosition.z);
                lineLenNow += hyperStr.length;
                 if (hyperStr.type == HyperType.Clickable){
                     SetClickWidget(lbl, hyperStr.length);
                     mClickableDic.Add(lbl.gameObject, hyperStr);
                 }

            }
            else if (hyperStr.type == HyperType.Sprite)
            {
                UISprite spr = GetSprite();
                spr.spriteName = hyperStr.content;
                if (hyperStr.inputInfo != string.Empty)
                    spr.Dimensions = new Vector2(hyperStr.length, spr.Dimensions.y);
                else
                    spr.Dimensions = SprPrefab.Dimensions;
                Vector3 sprSize = spr.Dimensions;
                spr.transform.localPosition = new Vector3(lineLenNow, heightSpace , spr.transform.localPosition.z);
                lineLenNow += hyperStr.length;
            }
            else if (hyperStr.type == HyperType.Voice)
            {
                UIVoiceChat voiChat = GetVoiceChat();
                voiChat.TimeLabel.text = hyperStr.content;
                voiChat.transform.localPosition = new Vector3(lineLenNow, heightSpace, voiChat.transform.localPosition.z);
                lineLenNow += hyperStr.length;
                SetClickWidget(voiChat, hyperStr.length);
                mClickableDic.Add(voiChat.gameObject, hyperStr);
            }
        }


    }

    void SetClickWidget(Component comp,float compLength) {
        BoxCollider boxColli = comp.GetComponent<BoxCollider>();
        if (boxColli == null)
        {
            boxColli = comp.gameObject.AddComponent<BoxCollider>();
        }
        boxColli.enabled = true;
        boxColli.size = new Vector3(compLength, HeightSpace, boxColli.size.z);
        boxColli.center = new Vector3(compLength / 2, 0, 0);
        UIButtonMessage btnMsg = comp.GetComponent<UIButtonMessage>();
        if (btnMsg == null)
        {
            btnMsg = comp.gameObject.AddComponent<UIButtonMessage>();
        }
        btnMsg.target = gameObject; ;
        btnMsg.functionName = "OnHyperTextClick";
    }

    UILabel GetLabel()
    {
        return GetWidget<UILabel>(mLabelPool,LblPrefab.gameObject);
    }

    UISprite GetSprite()
    {
        return GetWidget<UISprite>(mSpritePool,SprPrefab.gameObject);
    }

    UIVoiceChat GetVoiceChat()
    {
        return GetWidget<UIVoiceChat>(mVoicePool,VoiPrefab.gameObject);
    }

    T GetWidget<T>(List<T> pool,GameObject prefab)where T:MonoBehaviour
    {
        int poolsize = pool.Count;
        if (poolsize > 0)
        {
            T widget = pool[poolsize - 1];
            pool.RemoveAt(poolsize - 1);
            widget.gameObject.SetActive(true);
            mUsingWidgetPool.Add(widget);
            return widget;
        }
        else
        {
            GameObject obj = NGUITools.AddChildNotLoseAnything(gameObject, prefab.gameObject);
            T widget = obj.GetComponent<T>();
            mUsingWidgetPool.Add(widget);
            return widget;
        }
    }

    void OnHyperTextClick(GameObject obj) {
        if (mClickableDic.ContainsKey(obj))
        {
            HyperString hypStr;
            mClickableDic.TryGetValue(obj, out hypStr);

            LuaState lua = LuaInstance.instance.Get();
            lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, click_func_ref);
            lua.RawGetI(LuaAPI.LUA_REGISTRYINDEX, Contoller.Object_ref);
            lua.PushString(hypStr.inputInfo);
            LuaInstance.instance.Get().PCall(2, 0, 0);   
        }
    }


    private void RepairStringColor(ref string text, ref string newStr)
    {
        Stack<string> colors = new Stack<string>();
        int i = 0;
        for (int imax = text.Length; i < imax; ++i)
        {
            char c = text[i];

            // Skip invalid characters
            if (c < ' ') continue;

            // When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
            if (c == '[')
            {
                if (i + 2 < imax)
                {
                    if (text[i + 1] == '-' && text[i + 2] == ']')
                    {
                        colors.Pop();
                        i += 2;
                        continue;
                    }
                    else if (i + 7 < imax && text[i + 7] == ']')
                    {
                        string temp = text.Substring(i, 8);
                        colors.Push(temp);
                        i += 7;
                        continue;
                    }
                }
            }
        }

        while (colors.Count > 0)
        {
            newStr = colors.Pop() + newStr;
        }
    }
}
