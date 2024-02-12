using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Clase para consultar el server or una escena con un ID
/// La clase se encarga de la consulta WEB -> Descarga scena asociada a la imagen ID
/// Carga la escena desde el XML descargado
/// Usa TreeInfo (q es la clase que guarda la informacion del arbol ara la escena)
/// Usa SliceManagers (q es la clase q controla los cortes y los posiciona)
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Esta es la funcion de consulta de la escena con la imagen ID
    /// </summary>
    /// <param name="imageId"></param>
    /// <param name="fEnd"></param>
    static public void findScene4ImageId(string imageId, UnityAction<bool> fEnd)
    {
        string url = AppInfo.getAppUrl("Aumented/getScene4ImageId/" + imageId);
        WWWForm form = new WWWForm();
        AppInfo.getHttp(url, form, (bOk, xml) =>
        {
            if (bOk) // ok consulta del imageId
            {
                // restaurar la escena inicial (todo visible y en el origen)
                SceneLoader.resetScene();

                bool bOkScene = SceneLoader.readScene4Xml(xml);

                fEnd(bOkScene); // reportar la carga OK de la escena
            }
            else
            {
                fEnd(false); // por si se requiere informar q la consulta de la imagen NO fue exitosa
            }
        });
    }

    /// <summary>
    /// Para restaurar los 3D y TODO a la configuracion original - resposicionar 3D, color, xray cortes etc
    /// AL FINAL SIEMPRE DEJA LA PIEL OCULTA
    /// </summary>
    static public void resetScene()
    {
        BranchInfo root = TreeInfo.getRootBranch();
        // para todos los 3D restaurar la posicion inicial, xray(false), cut(false), color(init)
        root.setRotation(Quaternion.identity);
        root.setPosition(Vector3.zero);
        root.setVisible(true);
        root.setXRay(false);
        root.setCut(false);

        // asegurar oculta la piel al reiniciar escena
        BranchInfo piel = TreeInfo.getBranch("PIEL");
        if (piel != null) piel.setVisible(false);


        //// ocultar y posicionar al centro los cortes
        SlicesManager.mInstance.mSliceX.reset2Init();
        SlicesManager.mInstance.mSliceY.reset2Init();
        SlicesManager.mInstance.mSliceZ.reset2Init();

        // LA POSICION Y ORIENTACION DE LA CAMARA
        Vector3 center = root.getVisibleBounds().center;
        Debug.Log("Center:" + center);
        //Camera cam = Camera.current;
        //cam.transform.position = center;
    }

    static protected bool readScene4Xml(string xml)
    {
        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode cameraNode = xmlDoc.SelectSingleNode("views/camera");
            if (cameraNode != null)
            {
                if (cameraNode.Attributes.GetNamedItem("position") != null)
                {
                    Vector3 position = AppInfo.parseV3(cameraNode.Attributes["position"].Value, ';');
                }
                if (cameraNode.Attributes.GetNamedItem("rotation") != null)
                {
                    Quaternion rotation = AppInfo.parseQ(cameraNode.Attributes["rotation"].Value, ';');
                }
                if (cameraNode.Attributes.GetNamedItem("size") != null)
                {
                    float size = AppInfo.parseF(cameraNode.Attributes["size"].Value);
                }
                if (cameraNode.Attributes.GetNamedItem("center") != null)
                {
                    Vector3 center = AppInfo.parseV3(cameraNode.Attributes["center"].Value, ';');
                }
                if (cameraNode.Attributes.GetNamedItem("IsPortrait") != null)
                {
                    bool isPortrail = bool.Parse(cameraNode.Attributes["IsPortrait"].Value);
                }
            }

            {   // carga de estado de los cortes x(coronal), y(sagital), z(transversal)
                XmlNode slicesNodeX = xmlDoc.SelectSingleNode("views/SLICES/CORONAL");
                if (slicesNodeX != null)
                {
                    SlicesManager.mInstance.mSliceX.load4XmlNode(slicesNodeX);
                }
                XmlNode slicesNodeY = xmlDoc.SelectSingleNode("views/SLICES/SAGITAL");
                if (slicesNodeY != null)
                {
                    SlicesManager.mInstance.mSliceY.load4XmlNode(slicesNodeY);
                }
                XmlNode slicesNodeZ = xmlDoc.SelectSingleNode("views/SLICES/TRANSVERSAL");
                if (slicesNodeZ != null)
                {
                    SlicesManager.mInstance.mSliceZ.load4XmlNode(slicesNodeZ);
                }
            }

            // para los actores/ramas (branch) del arbol
            XmlNode actorNode = xmlDoc.SelectSingleNode("views/nodes");
            if (actorNode != null)
            {
                for (int i = 0; i < actorNode.ChildNodes.Count; i++)
                {
                    XmlNode nodeElement = actorNode.ChildNodes[i];
                    if (nodeElement.Attributes["id"] != null)
                    {
                        // verificar la rama/branch por id
                        string id = nodeElement.Attributes["id"].Value;
                        BranchInfo branch = TreeInfo.getBranch(id);
                        if (branch == null)
                        {
                            Debug.LogError("Branch null:" + id);
                            continue;
                        }

                        bool isCut = false;
                        if (nodeElement.Attributes["isCut"] != null)
                        {
                            isCut = bool.Parse(nodeElement.Attributes["isCut"].Value);
                        }
                        branch.setCut(isCut);
                        bool isVisible = true;
                        if (nodeElement.Attributes["isVisible"] != null)
                        {
                            isVisible = bool.Parse(nodeElement.Attributes["isVisible"].Value);
                        }
                        branch.setVisible(isVisible);
                        bool isXRay = false;
                        if (nodeElement.Attributes["isXray"] != null)
                        {
                            isXRay = bool.Parse(nodeElement.Attributes["isXray"].Value);
                        }
                        branch.setXRay(isXRay);

                        // posicionar, rotar, color del nodo
                        Vector3 position = new Vector3(0, 0, 0);
                        Quaternion rotation = Quaternion.identity;
                        Vector3 color = new Vector3(-1, -1, -1); // color no valido - restaurar color inicial
                        if (nodeElement.HasChildNodes)
                        {
                            XmlNode modelNode = nodeElement.FirstChild;

                            if (modelNode.Attributes.GetNamedItem("position") != null)
                            {
                                position = AppInfo.parseV3(modelNode.Attributes["position"].Value);
                            }
                            if (modelNode.Attributes.GetNamedItem("rotation") != null)
                            {
                                rotation = AppInfo.parseQ(modelNode.Attributes["rotation"].Value);
                            }
                            if (modelNode.Attributes.GetNamedItem("color") != null)
                            {
                                color = AppInfo.parseV3(modelNode.Attributes["color"].Value);
                            }
                        }
#if UNITY_EDITOR
                        Debug.LogError("Inicialmente NO se asigna posicion o rotacion - mientras se define origen etc");
#endif // UNITY_EDITOR
                        //branch.setPosition(position);
                        //branch.setRotation(rotation);
                        branch.setColor(color);
                    }
                }
            }
            XmlNode pinNode = xmlDoc.SelectSingleNode("views/PINS");
            if (pinNode != null)
            {
                //PinController.Instance.PinContainer.Clear();
                for (int i = 0; i < pinNode.ChildNodes.Count; i++)
                {
                    //ReadPinPerUnit(_nodeList[i]);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("XML: " + e.Message);
            Debug.Log(xml);
            return false;
        }

        return true;
    }
}
