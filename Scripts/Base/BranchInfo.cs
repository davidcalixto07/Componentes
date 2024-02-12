using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BranchInfo
{
    public enum BRANCH_TYPE
    {
        BRANCH_TYPE_FILE = 1,
        BRANCH_TYPE_ACTOR = 2,
        BRANCH_TYPE_SYSTEM = 3,
    };

    public BRANCH_TYPE mType;
    public string mObjectName = "";
    public string mNameSp = "";
    public string mDescriptionSp = "";
    public string mNameEn = "";
    public string mDescriptionEn = "";
    public List<BranchInfo> mChilds = new List<BranchInfo>();
    protected List<BranchInfo> mParents = new List<BranchInfo>();

    protected bool mIsGroup = false;    // actor+hijos son 1 solo objeto

    protected GameObject mGameObject;

    protected bool mIsVisible = false;
    protected bool mIsCut = false;
    protected bool mIsXray = false;

    // leidos del xml -> guardados para mantener settings en nuevo xml-arbol
    protected string mReadTrans = "";
    protected string mReadShader = "";
    protected string mReadBumpMap = "";
    protected string mReadSpecColor = "";
    protected string mReadShininess = "";
    protected string mReadParallax = "";
    protected string mReadParallaxMap = "";
    protected string mReadIsCut = "";
    protected string mReadState = ""; // estado AAA,BBB,MMM

    protected Color mInitColor = new Color(0, 0, 0);
    protected Color mColor = new Color(0, 0, 0); // 2023-08-03 pruebas de lectura de color de FILE

    protected const float K_TRANSPARENT_VALUE = 0.8f;

    public BranchInfo()
    {
    }
    public bool init(string objName, BRANCH_TYPE type, XmlNode node)
    {
        mType = type;
        mObjectName = objName;
        mNameEn = mNameSp = objName;

        if (node.Attributes.GetNamedItem("group") != null)
            mIsGroup = true;

        // Debug.Log("Type:"+mType+" Branch:" + mObjectName);

        if (mType == BranchInfo.BRANCH_TYPE.BRANCH_TYPE_FILE)
        {
            // buscar el objeto en la jerarquia
            mGameObject = TreeInfo.get3DObject4Name(mObjectName);
            if (mGameObject == null)
            {
                Debug.LogError("3D NO encontrado:" + mObjectName);
            }
        }

        loadChilds(node);
        // Debug.Log(mObjectName + " childs:" + mChilds.Count);

        return true;
    }
    public void loadChilds(XmlNode node)
    {
        XmlNodeList childs = node.ChildNodes;

        foreach (XmlNode child in childs)
        {
            if (child.Name == "PROPS")
            {
                initProps(child);
            }
            if (child.Name == "CHILD" || child.Name == "ACTOR")
            {
                string childName = child.Attributes.GetNamedItem("name").Value.ToUpper();
                BranchInfo ch = TreeInfo.getBranch(childName);
                if (ch != null)
                {
                    ch.mParents.Add(this);
                    // adiciono el actor (rama) previamente cargado a esta nueva rama
                    mChilds.Add(ch);
                }
            }
        }
    }
    private void initProps(XmlNode props)
    {
        var crAttr = props.Attributes.GetNamedItem("cr");
        var cbAttr = props.Attributes.GetNamedItem("cb");
        var cgAttr = props.Attributes.GetNamedItem("cg");
        var transAttr = props.Attributes.GetNamedItem("trans");
        var shaderAttr = props.Attributes.GetNamedItem("shader");
        var bumpMapAttr = props.Attributes.GetNamedItem("BumpMap");
        var specColorAttr = props.Attributes.GetNamedItem("SpecColor");
        var shininessAttr = props.Attributes.GetNamedItem("Shininess");
        var parallaxAttr = props.Attributes.GetNamedItem("Parallax");
        var parallaxMapAttr = props.Attributes.GetNamedItem("ParallaxMap");
        var isCutAttr = props.Attributes.GetNamedItem("cut");

        if (crAttr!=null && cgAttr!=null && cbAttr!=null)
        {
            mInitColor.r = float.Parse(crAttr.Value);
            mInitColor.g = float.Parse(cgAttr.Value);
            mInitColor.b = float.Parse(cbAttr.Value);
            setColor(mInitColor);
        }
        if (transAttr != null)
        {
            if (transAttr.Value.Length>0) mReadTrans = transAttr.Value;
            ///////if (mReadTrans.Length > 0) Debug.Log(mObjectName + " trans:" + mReadTrans);
        }
        if (shaderAttr!=null)
        {
            mReadShader = shaderAttr.Value;
        }
        if (bumpMapAttr!=null)
        {
            mReadBumpMap = bumpMapAttr.Value;
        }
        if (specColorAttr!=null)
        {
            mReadSpecColor = specColorAttr.Value;
        }
        if (shininessAttr!=null)
        {
            mReadShininess = shininessAttr.Value;
        }
        if (parallaxAttr!=null)
        {
            mReadParallax = parallaxAttr.Value;
        }
        if (parallaxMapAttr!=null)
        {
            mReadParallaxMap = parallaxMapAttr.Value;
        }
        if (isCutAttr!=null)
        {
            mReadIsCut = isCutAttr.Value;
        }
    }

    // ----------------------------------------------------------
    // para mostrar los padres de un objeto
    static public void dumpParentInfo(BranchInfo node, string strParent = "")
    {
        string str = strParent + " | " + node.mObjectName;
        if (node.mParents.Count > 0)
        {
            for (int i = 0; i < node.mParents.Count; i++)
            {
                BranchInfo.dumpParentInfo(node.mParents[i], str);
            }
        }
        else
        {
            Debug.Log("Parents:" + str);
        }
    }
    /// <summary>
    /// retorna la lista de padres principal (la primera)
    /// desde el node hacia arriba hasta el root en ese orden
    /// SIN incluir node - solo padres
    /// </summary>
    static public List<BranchInfo> parentInfo(BranchInfo node)
    {
        List<BranchInfo> parents = new List<BranchInfo>();
        while (true)
        {
            if (node == null) break;
            if (node.mParents.Count <= 0) break;
            BranchInfo parent = node.mParents[0];
            if (parent == null) break;
            parents.Add(parent);
            node = parent;
        }
        return parents;
    }
    // retorna los padres en la jerarquia
    List<BranchInfo> getParents()
    {
        return mParents;
    }
    // retorna la lista de hijos en la gerarquia
    List<BranchInfo> getChilds()
    {
        return mChilds;
    }
#if UNITY_EDITOR
    // para mostrar el arbol leido en consola
    public void dumpInfo(int indent = 0)
    {
        string str = new string(' ', indent);
        Debug.Log(str + mNameSp);
        for (int i = 0; i < mChilds.Count; i++)
        {
            mChilds[i].dumpInfo(indent + 1);
        }
    }
#endif // UNITY_EDITOR
    public GameObject getGameObject()
    {
        return mGameObject;
    }
    public Bounds getVisibleBounds()
    {
        List<GameObject> list = new List<GameObject>();
        funcFind3D4Node(this, ref list);

        Bounds bs = new Bounds();
        for (int i = 0; i < list.Count; i++)
        {
            GameObject obj = list[i];
            Collider coll = obj.GetComponent<Collider>();
            if (!coll) continue;

            if (!obj.activeSelf) continue;

            if (i == 0)
                bs = coll.bounds;
            else
                bs.Encapsulate(coll.bounds);
        }
        return bs;
    }

    
    static private void funcFind3D4Node(BranchInfo node, ref List<GameObject>list)
    {
        for (int i=0; i<node.mChilds.Count; i++)
        {
            funcFind3D4Node(node.mChilds[i], ref list);
        }

        if (node.mGameObject)
        {
            if (!list.Contains(node.mGameObject))
            {
                list.Add(node.mGameObject);
            }
        }
    }
    
    // retorna cuando el branch es un grupo (es un tipo ACTOR)
    public bool isGroup()
    {
        return mIsGroup;
    }
    // muestra/oculta el objeto
    public void setVisible(bool b)
    {
        mIsVisible = b;
        // si es un objeto activa/inactiva el objeto
        if (mGameObject)
        {
            mGameObject.SetActive(b);
        }
        // llama a los hijos (si los tiene) para que hagan lo mismo
        for (int i = 0; i < mChilds.Count; i++)
        {
            mChilds[i].setVisible(b);
        }
    }
    public bool isVisible()
    {
        return mIsVisible;
    }
    public void setCut(bool b)
    {
        mIsCut = b;
        // activar/des-activar el corte del mesh
        if (mGameObject != null)
        {

        }

        // para los hijos hacer lo mismo
        for (int i = 0; i < mChilds.Count; i++)
        {
            mChilds[i].setCut(b);
        }
    }
    public bool isCut()
    {
        return mIsCut;
    }
    public void setXRay(bool b)
    {
        mIsXray = b;
        //Debug.Log(mGameObject + " xray:" + b);
        // activa-des-activar los rayos x del mesh
        if (mGameObject != null)
        {
            //Renderer ren = mGameObject.GetComponent<Renderer>();
            //if (ren)
            //{
            //    ren.material.shader = b ? TreeInfo.getAlphaShader() : TreeInfo.getNormalShader();
            //    // 2023-03-10 aospino define el mismo color para transparencia - solicitud jcastago L244
            //    ren.material.color = getColor(b);
            //}
        }

        // para los hijos hacer lo mismo
        for (int i = 0; i < mChilds.Count; i++)
        {
            mChilds[i].setXRay(b);
        }
    }
    public bool isXRay()
    {
        return mIsXray;
    }

    public void setPosition(Vector3 position)
    {

    }
    public void setRotation(Quaternion rotation)
    {

    }

    /// Funciones de variables leidas del proyecto
    public string getReadTrans()
    {
        return mReadTrans; // transparencia
    }
    public string getReadShader()
    {
        return mReadShader; // shader
    }
    public string getReadBumpMap()
    {
        return mReadBumpMap; // bump map (normales)
    }
    public string getReadSpecColor()
    {
        return mReadSpecColor; // color especular (no usado)
    }
    public string getReadShininess()
    {
        return mReadShininess; // (no usado)
    }
    public string getReadParallax()
    {
        return mReadParallax; // no usado
    }
    public string getReadParallaxMap()
    {
        return mReadParallaxMap; // no usado
    }
    public string getReadIsCut()
    {
        return mReadIsCut; // no usado - nada esta cortado inicialmente
    }
    public string getReadState()
    {
        return mReadState; // no usado
    }
    public void setReadProps(string trans, string shader, string bumpMap,
        string SpecColor, string Shininess, string parallax, string parallaxMap,
        string isCut, Color initColor, bool isGroup)
    {
        mReadTrans = trans;
        mReadShader = shader;
        mReadBumpMap = bumpMap;
        mReadSpecColor = SpecColor;
        mReadShininess = Shininess;
        mReadParallax = parallax;
        mReadParallaxMap = parallaxMap;
        mReadIsCut = isCut;

        mInitColor = initColor; // color inicial SI SE USA
        mIsGroup = isGroup;     // es un grupo - une hijos como 1 solo objeto
    }




    public Color getInitColor()
    {
        return mInitColor;
    }
    public Color getColor(bool alpha = false)
    {
        if (alpha)
            return new Color(mColor.r, mColor.g, mColor.b, K_TRANSPARENT_VALUE);
        return new Color(mColor.r, mColor.g, mColor.b);
    }
    /// <summary>
    /// Color r,g,b
    /// </summary>
    /// <param name="color"></param>
    public void setColor(Vector3 color)
    {
        Color col = new Color(color.x, color.y, color.z);
        setColor(col);
    }
    /// <summary>
    /// Color con alpha
    /// </summary>
    /// <param name="color"></param>
    public void setColor(Vector4 color)
    {
        Color col = new Color(color.x, color.y, color.z, color.w);
        setColor(col);
    }
    public void setColor(Color color)
    {
        mColor = color;
        if (mGameObject)
        {
            Renderer ren = mGameObject.GetComponent<Renderer>();
            if (ren)
            {
                // tener en cuenta transparencia ????????
                ren.material.color = color;
            }
        }
        for (int i = 0; i < mChilds.Count; i++)
        {
            mChilds[i].setColor(color);
        }
    }








    // lee las propiedades desde el xml nuevo -1 solo archivo-
    public bool funReadProps4NewXml(XmlNode node)
    {
        if (node == null) return false;
        if (node.Attributes == null) return false;
        
        if (!haveAttribute(node, "xid")) return false;
        if (!haveAttribute(node, "a_español")) return false;
        if (!haveAttribute(node, "a_ingles")) return false;
        if (!haveAttribute(node, "type")) return false;

        mObjectName = node.Attributes.GetNamedItem("xid").Value;
        mNameSp = node.Attributes.GetNamedItem("a_español").Value;
        mNameEn = node.Attributes.GetNamedItem("a_ingles").Value;
        if (mObjectName.Length==0 || mNameSp.Length==0 || mNameEn.Length==0)
        {
            Debug.LogError("ERR names:" + node.Name);
            if (mObjectName.Length == 0) mObjectName = node.Name;
            if (mNameSp.Length == 0) mNameSp = node.Name;
            if (mNameEn.Length == 0) mNameEn = node.Name;
            return false;
        }
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.Name == "description_en") mDescriptionEn = child.InnerText.Trim();
            if (child.Name == "description_sp") mDescriptionSp = child.InnerText.Trim();
        }
        string type = node.Attributes.GetNamedItem("type").Value;
        if (type == "FILE")
        {
            mType = BRANCH_TYPE.BRANCH_TYPE_FILE;
            mGameObject = TreeInfo.get3DObject4Name(mObjectName);
        }
        if (type == "ACTOR") mType = BRANCH_TYPE.BRANCH_TYPE_ACTOR;
        if (type == "SYSTEM") mType = BRANCH_TYPE.BRANCH_TYPE_SYSTEM;
        // color es opcional
        string colorStr = node.Attributes.GetNamedItem("xcolor")!=null? node.Attributes.GetNamedItem("xcolor").Value:"";
        if (colorStr.Length > 0)
        {
            if (colorStr.Split(',').Length==3)
                mInitColor = AppInfo.parseColor3(colorStr,',');
            setColor(mInitColor);
            ///////Debug.Log(mObjectName + "xcolor:" + colorStr + " parsed:" + mInitColor);
        }
        //////Debug.Log(mObjectName + " en:" + mNameEn + " sp:" + mNameSp);

        if (haveAttribute(node, "trans"))
            mReadTrans = node.Attributes.GetNamedItem("trans").Value;
        if (haveAttribute(node, "shader"))
            mReadShader = node.Attributes.GetNamedItem("shader").Value;
        if (haveAttribute(node, "BumpMap"))
            mReadBumpMap = node.Attributes.GetNamedItem("BumpMap").Value;
        if (haveAttribute(node, "SpecColor"))
            mReadSpecColor = node.Attributes.GetNamedItem("SpecColor").Value;
        if (haveAttribute(node, "Shiniess"))
            mReadShininess = node.Attributes.GetNamedItem("Shiniess").Value;
        if (haveAttribute(node, "Parallax"))
            mReadParallax = node.Attributes.GetNamedItem("Parallax").Value;
        if (haveAttribute(node, "ParallaxMap"))
            mReadParallaxMap = node.Attributes.GetNamedItem("ParallaxMap").Value;
        if (haveAttribute(node, "cut"))
            mReadIsCut = node.Attributes.GetNamedItem("cut").Value;
        if (haveAttribute(node, "estado"))
            mReadState = node.Attributes.GetNamedItem("estado").Value;

        if (haveAttribute(node, "group"))
            mIsGroup = node.Attributes.GetNamedItem("group").Value=="1";

        return true;
    }
    private bool haveAttribute(XmlNode node, string attr)
    {
        if (node == null) return false;
        if (node.Attributes == null) return false;

        return node.Attributes.GetNamedItem(attr) != null;
    }
    public XmlNode save2Xml(XmlDocument xmlDoc)
    {
        string type = "SYSTEMS";
        switch (mType)
        {
            case BRANCH_TYPE.BRANCH_TYPE_FILE:
                type = "FILE";
                break;
            case BRANCH_TYPE.BRANCH_TYPE_ACTOR:
                type = "ACTOR";
                break;
            case BRANCH_TYPE.BRANCH_TYPE_SYSTEM:
                type = "SYSTEM";
                break;
        }
        string objName = mObjectName;
        if (objName.Contains(' '))
        {
            Debug.Log("Espacio:" + objName);
            objName = objName.Replace(' ', '_');
        }
        if (mObjectName.Length==0)
        {
            Debug.LogError(type+" VACIO en:" + mNameEn + " sp:" + mNameSp+"  type"+mType);
            return null;
        }
        XmlNode xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, objName, null);


        xmlSetAttribute(ref xmlNode, xmlDoc, "a_español", mNameSp);
        xmlSetAttribute(ref xmlNode, xmlDoc, "a_ingles", mNameEn);
        xmlSetAttribute(ref xmlNode, xmlDoc, "type", type);
        xmlSetAttribute(ref xmlNode, xmlDoc, "xid", objName);
        if (mIsGroup)
            xmlSetAttribute(ref xmlNode, xmlDoc, "group", "1");

        XmlNode xmlDescSp = xmlDoc.CreateNode(XmlNodeType.Element, "description_sp", null);
        xmlDescSp.InnerText = mDescriptionSp;
        xmlNode.AppendChild(xmlDescSp);

        XmlNode xmlDescEn = xmlDoc.CreateNode(XmlNodeType.Element, "description_en", null);
        xmlDescEn.InnerText = mDescriptionEn;
        xmlNode.AppendChild(xmlDescEn);

        XmlNode xmlObs = xmlDoc.CreateNode(XmlNodeType.Element, "observaciones", null);
        xmlObs.InnerText = "obs ...";
        xmlNode.AppendChild(xmlObs);

        // atributo de descripciones
        if (mType == BRANCH_TYPE.BRANCH_TYPE_FILE)
        {
            xmlSetAttribute(ref xmlNode, xmlDoc, "estado", ""); // puede ser AAA, MMM, BBB
        }


        // establecer el color para lo que tenga color
        if (mInitColor.r!=0 || mInitColor.g!=0 || mInitColor.b!=0)
        {
            //////Debug.Log(mObjectName + " color:" + mInitColor);
            xmlSetAttribute(ref xmlNode, xmlDoc, "xcolor",
                Mathf.FloorToInt(mInitColor.r) + "," + Mathf.FloorToInt(mInitColor.g)
                + "," + Mathf.FloorToInt(mInitColor.b));
        }

        if (mReadTrans.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "trans", mReadTrans);
        if (mReadShader.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "shader", mReadShader);
        if (mReadBumpMap.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "BumpMap", mReadBumpMap);
        if (mReadSpecColor.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "SpecColor", mReadSpecColor);
        if (mReadShininess.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "Shininess", mReadShininess);
        if (mReadParallax.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "Parallax", mReadParallax);
        if (mReadParallaxMap.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "ParallaxMap", mReadParallaxMap);
        if (mReadIsCut.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "cut", mReadIsCut);
        if (mReadState.Length > 0) xmlSetAttribute(ref xmlNode, xmlDoc, "estado", mReadState);


        if (mGameObject)
        {
        }

        for (int i = 0; i < mChilds.Count; i++)
        {
            XmlNode xmlNode_i = mChilds[i].save2Xml(xmlDoc);
            if (xmlNode_i!=null)
            {
                xmlNode.AppendChild(xmlNode_i);
            }
            else
            {
                Debug.LogError("xmlNode NULL en:" + mChilds[i].mObjectName + " i:" + i + "/"+mChilds.Count+" p:" + mObjectName);
            }
        }

        return xmlNode;
    }
    private void xmlSetAttribute(ref XmlNode xmlNode, XmlDocument xmlDoc, string nName, string value)
    {
        XmlAttribute attrName = xmlDoc.CreateAttribute(nName);
        attrName.Value = value;
        xmlNode.Attributes.Append(attrName);
    }
}
