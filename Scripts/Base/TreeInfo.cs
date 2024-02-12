using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;

public class TreeInfo : MonoBehaviour
{
    // lista de objetos del arbol - para consultas
    public List<BranchInfo> mBranchList = new List<BranchInfo>();

    public BranchInfo mSystems = new BranchInfo();

    static protected TreeInfo mInstance = null;

    protected Shader mNormalShader;
    protected Shader mAlphaShader;

    static public BranchInfo getRootBranch()
    {
        return mInstance.mSystems;
    }
    /// <summary>
    /// para buscar un actor/file por ID
    /// </summary>
    /// <param name="objName"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    // para buscar una rama del arbol por nombre
    // va a ser la funcion mas usada para encontrar objetos del arbol
    static public bool getBranch(string objName, out BranchInfo branch)
    {
        if (mInstance == null) { Debug.LogError("NO treeInfo instance"); branch = null; return false; }
        if (mInstance.mBranchList == null) { Debug.LogError("NO branch list"); branch = null; return false; }
        for (int i = 0; i < TreeInfo.mInstance.mBranchList.Count; i++)
        {
            if (TreeInfo.mInstance.mBranchList[i].mObjectName == objName)
            {
                branch = TreeInfo.mInstance.mBranchList[i];
                return true;
            }
        }
        branch = null;
        return false;
    }
    /// <summary>
    /// retorna el system/actor/file para el ID eje: SISTEMA_OSEO, LIGAMENTOS_INTERTRANSVERSOS, PIEL
    /// </summary>
    /// <param name="objName"></param>
    /// <returns></returns>
    static public BranchInfo getBranch(string objName)
    {
        if (mInstance == null) { Debug.LogError("NO treeInfo instance"); return null; }
        if (mInstance.mBranchList == null) { Debug.LogError("NO branch list"); return null; }

        BranchInfo branch = null;
        for (int i = 0; i < TreeInfo.mInstance.mBranchList.Count; i++)
        {
            if (TreeInfo.mInstance.mBranchList[i].mObjectName == objName)
            {
                branch = TreeInfo.mInstance.mBranchList[i];
                break;
            }
        }
        return branch;
    }
    /// <summary>
    /// retorna el indice system/actor/file para el ID eje: SISTEMA_OSEO, LIGAMENTOS_INTERTRANSVERSOS, PIEL
    /// dentro de la lista mBranchList
    /// </summary>
    /// <param name="objName"></param>
    /// <returns></returns>
    // retorna el indice del objeto 3D
    static public int getBranchIndex(string objName)
    {
        if (mInstance == null) { Debug.LogError("NO treeInfo instance"); return -1; }
        if (mInstance.mBranchList == null) { Debug.LogError("NO branch list"); return -1; }

        for (int i = 0; i < TreeInfo.mInstance.mBranchList.Count; i++)
        {
            if (TreeInfo.mInstance.mBranchList[i].mObjectName == objName)
            {
                return i;
            }
        }
        return -1;
    }
    /// <summary>
    /// retorna lista de branch q contienen texto a buscar
    /// para buscar un system/actor/file por nombre de mostrado - removiendo acentos
    /// púlmón -> busca pulmon y retorna referencia al objeto PULMON_DERECHO y PULMON_IZQUIERDO
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    static public List<BranchInfo> findBranch(string text)
    {
        text = AppInfo.removeAccents(text).ToUpper();

        List<BranchInfo> find2 = new List<BranchInfo>();
        mInstance.findInItem(mInstance.mSystems, ref find2, text, 0);

        return find2;
    }
    private void findInItem(BranchInfo branch, ref List<BranchInfo> find, string text, int level)
    {
        if (AppInfo.isSpanish())
        {
            if (branch.mNameSp.ToUpper().Contains(text))
            {
                find.Add(branch);
            }
        }
        else
        {
            if (branch.mNameEn.ToUpper().Contains(text))
            {
                find.Add(branch);
            }
        }
        // buscar en los hijos  de forma recursiva y ordenada
        for (int i = 0; i < branch.mChilds.Count; i++)
        {
            findInItem(branch.mChilds[i], ref find, text, level + 1);
        }
    }
    /// <summary>
    /// retorna el GameObject 3D correspondiente al nombre del ID objName
    /// </summary>
    /// <param name="objName"></param>
    /// <returns></returns>
    static public GameObject get3DObject4Name(string objName)
    {
        return GameObject.Find(objName);
    }
    static public Shader getNormalShader()
    {
        return mInstance.mNormalShader;
    }
    static public Shader getAlphaShader()
    {
        return mInstance.mAlphaShader;
    }
    static public List<BranchInfo> getBranchList()
    {
        return mInstance.mBranchList;
    }
    // ==============================================================
    // ==============================================================


    private void Awake()
    {
        mInstance = this;

        //mNormalShader = Shader.Find("Legacy Shaders/Bumped Diffuse");
        //mAlphaShader = Shader.Find("Legacy Shaders/Transparent/Bumped Diffuse");
        ////mNormalShader = Shader.Find("BioTK/Bumped Diffuse");
        ////mAlphaShader = Shader.Find("BioTK/Transparent/Bumped Diffuse");
        //if (!mNormalShader)
        //{
        //    Debug.Log("Err normal shader");
        //}
        //if (!mAlphaShader)
        //{
        //    Debug.Log("Err alpha shader");
        //}
    }

    private void Start()
    {
        funcRead4Resource1Xml();
        SlicesManager.readConfig();
        SceneLoader.resetScene();
    }


    /// <summary>
    /// para usar al leer xml externo
    /// </summary>
    public bool funcRead4File1Xml()
    {
        string dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/3Dissect";
        System.IO.Directory.CreateDirectory(dir);

        string fileName = dir + "/proyect_" + AppInfo.getAppVolume() + ".xml";
        string buffer = System.IO.File.ReadAllText(fileName);

        if (buffer.Length == 0)
        {
            Debug.LogError("No leido:" + fileName);
            return false;
        }

        return read1XmlInternal(buffer);
    }
    /// <summary>
    /// Para usar al leer en el proyecto
    /// </summary>
    public bool funcRead4Resource1Xml()
    {
        string volId = AppInfo.getAppVolume();
        TextAsset asset = Resources.Load(volId + "-resources/proyect_" + volId) as TextAsset;

        if (asset==null)
        {
            Debug.LogError("Proyect xml no leido");
            return false;
        }
        if (asset.text==null || asset.text.Length<=0)
        {
            Debug.LogError("Proyect xml no leido 2");
            return false;
        }

        return read1XmlInternal(asset.text);
    }
    private bool read1XmlInternal(string buffer)
    {
        try
        {
            if (buffer.Length==0)
            {
                Debug.LogError("Proyect buffer empty");
                return false;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(buffer);

            // iniciar mSystem y mBranchList leidos desde 1 xml
            mBranchList = new List<BranchInfo>();
            mSystems = read1XmlNode4BranchList(xmlDoc.FirstChild);

        }
        catch (System.Exception e)
        {
            Debug.LogError("Read XML:" + e.Message);
        }
        Debug.Log("Read items:" + mBranchList.Count);

        return mBranchList.Count > 0;
    }
    private BranchInfo read1XmlNode4BranchList(XmlNode node)
    {
        /////Debug.Log("Read:" + node.Name);
        BranchInfo branch = new BranchInfo();

        if (branch.funReadProps4NewXml(node))
        {
            mBranchList.Add(branch);

            for (int i=0; i<node.ChildNodes.Count; i++)
            {
                XmlNode child = node.ChildNodes.Item(i);

                // los hijos de descripcion y observaciones - NO tenerlos en cuenta (se lee en: branch.funReadProps4NewXml)
                if (child.Name == "description_sp" || child.Name == "description_en" || child.Name == "observaciones") continue;

                BranchInfo bChild = read1XmlNode4BranchList(child);
                if (bChild!=null) branch.mChilds.Add(bChild);
                else
                {
                    Debug.LogError("Child NULL in :" + branch.mObjectName + " i:"+i+"/"+node.ChildNodes.Count);
                }
            }
            //// verificar que FILE NO tenga hijos -> reportar al log
            //if (branch.mType==BranchInfo.BRANCH_TYPE.BRANCH_TYPE_FILE && branch.mChilds.Count>0)
            //{
            //    Debug.LogError(" x|x|x|x "+branch.mObjectName + " TIENE HIJOS:" + branch.mChilds.Count);
            //}
            return branch;
        }
        return null;
    }
    // desde lo leido en TreeInfo.mSystem -> escribe 1 SOLO archivo proyect_xxx.xml NUEVO arbol
    public void funcWrite1Xml()
    {
        XmlDocument xmlDoc = new XmlDocument();

        XmlNode xmlNode = mSystems.save2Xml(xmlDoc);
        if (xmlNode!=null) xmlDoc.AppendChild(xmlNode);

        string dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/3Dissect";
        System.IO.Directory.CreateDirectory(dir);

        string fileName = dir + "/proyect_" + AppInfo.getAppVolume() + "_xx.xml";

        xmlDoc.Save(fileName);
        Debug.Log("Save:" + fileName);
    }

}
