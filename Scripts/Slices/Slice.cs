using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Control slice x,y,z
/// Is controled from PanelControlSlice.cs that have a reference to Slice for x,y,z
/// </summary>
public class Slice : MonoBehaviour
{
    protected string mResourcePath = "";
    protected float mDelta = 0.01f;
    protected int mIndex = 1000;
    protected int mDeltaIndex = 1;
    protected int mMinIndex = 1000;
    protected int mMaxIndex = 1099;
    protected float mValue = 0.5f;
    // la posicion inicial se define para el corte
    // NO se calcula xq si se usan indices de imagenes diferentes queda mal
    // 1000~2027 deltaIndex=1, diferente 1000~2020 deltaIndex=7
    protected float mAxisInitialPosition;

    public enum SLICE_AXIS
    {
        AXIS_X, AXIS_Y, AXIS_Z,
    }
    [SerializeField] protected SLICE_AXIS mAxis;

    // Start is called before the first frame update
    void Start()
    {

    }


    public void init(SLICE_AXIS axis, float initPos, float delta, int deltaIndex, int minIndex, int maxIndex)
    {
        string volId = AppInfo.getAppVolume();
        // imagenes AXIS_X son las sagitales S-y/Slice_y_
        // imagenes AXIS_Y son las coronales C-x/slice_x_
        switch (axis)
        {
            case SLICE_AXIS.AXIS_Y:
                mResourcePath = volId + "-resources/" + volId + "-slices/C-x/slice_x_";
                break;
            case SLICE_AXIS.AXIS_X:
                mResourcePath = volId + "-resources/" + volId + "-slices/S-y/slice_y_";
                break;
            case SLICE_AXIS.AXIS_Z:
                mResourcePath = volId + "-resources/" + volId + "-slices/T-z/slice_z_";
                break;
        }
        mAxis = axis;
        mAxisInitialPosition = initPos;
        mDelta = delta;
        mDeltaIndex = deltaIndex;
        mMinIndex = minIndex;
        mMaxIndex = maxIndex;
        OnSliderChange(0.5f);

#if UNITY_EDITOR
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Debug.Log("Init slice " + axis + " path:" + mResourcePath + " i:" + initPos + " d:" + delta + " di:" + deltaIndex + " min:" + mMinIndex + " max:" + mMaxIndex + " s:" + meshRenderer.bounds.size);
#endif // UNITY_EDITOR
    }


    public void OnSliderChange(float val)
    {
        mValue = val;

        //index = maxIndex - deltaIndex * Mathf.FloorToInt(value * ((maxIndex - minIndex) / deltaIndex));
        mIndex = mMaxIndex - mDeltaIndex * Mathf.RoundToInt(mValue * ((mMaxIndex - mMinIndex) / mDeltaIndex + 1));
        ////index = Mathf.RoundToInt(value);

        int nImages = (mMaxIndex - mMinIndex) / mDeltaIndex + 1; // en 003 son 98 este calculo da 98
        int index2Val = mDeltaIndex * Mathf.FloorToInt(mValue * nImages); // .3*98 da 29.4 -> 6*29=174
        int index2image = mMinIndex + index2Val; // 2032-174=1858 existe
        if (index2image < mMinIndex) index2image = mMinIndex;
        if (index2image > mMaxIndex) index2image = mMaxIndex;
        mIndex = index2image;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("No meshRenderer in axis:" + mAxis);
            return;
        }


        Vector3 pos = transform.localPosition;
        switch (mAxis)
        {
            case SLICE_AXIS.AXIS_X:
                pos.x = mAxisInitialPosition + (mDelta * (mIndex - mMinIndex));
                break;
            case SLICE_AXIS.AXIS_Y:
                pos.y = mAxisInitialPosition + (mDelta * (mIndex - mMinIndex));
                break;
            case SLICE_AXIS.AXIS_Z:
                pos.z = mAxisInitialPosition + (mDelta * (mIndex - mMinIndex));
                break;
        }
        transform.localPosition = pos;


        //Shader.SetGlobalFloat(altSlicePrefix + "Pos", axisPosition);
        string imagePath = mResourcePath + mIndex;



        // Debug.Log("Image:" + imagePath);
        // 2022-08-19 aospino para tener un solo metodo de asignacion de imagenes
        Texture2D text = Resources.Load(imagePath, typeof(Texture2D)) as Texture2D;
#if UNITY_EDITOR
        if (text==null)
        {
            Debug.LogError("NO TEXTURE: " + imagePath);
        }
#endif
        meshRenderer.material.mainTexture = text;
    }

    public void setVisible(bool b)
    {
        // el corte NO se debe ocultar cuando activo - siempre visible en activo
        // SE USABA PARA OCULTAR LA IMAGEN Y DEJAR EL CORTE ACTIVO
        // NO SE USA PORQUE SE VEN HUECOS EN LOS CORTES DE LOS 3D
    }
    public void setActive(bool b)
    {
        gameObject.SetActive(b);

        // activar/desactivar el shader de corte
    }
    public void setFilpped(bool b)
    {
        // flip con shader
    }
    public void load4XmlNode(System.Xml.XmlNode sliceNode)
    {
        bool isVisible = false;
        if (sliceNode.Attributes["VISIBLE"] != null)
        {
            isVisible = bool.Parse(sliceNode.Attributes["VISIBLE"].Value);
        }
        setVisible(isVisible);
        bool isFlipped = false;
        if (sliceNode.Attributes["FLIPPED"] != null)
        {
            isFlipped = bool.Parse(sliceNode.Attributes["FLIPPED"].Value);
        }
        setFilpped(isFlipped);
        float value = 0.5f;
        if (sliceNode.Attributes["VALUE"] != null)
        {
            value = AppInfo.parseF(sliceNode.Attributes["VALUE"].Value);
        }
        OnSliderChange(value);
        bool isActive = false;
        if (sliceNode.Attributes["ACTIVE"] != null)
        {
            isActive = bool.Parse(sliceNode.Attributes["ACTIVE"].Value);
        }
        setActive(isActive);
    }
    /// <summary>
    /// Para restaurar el corte a estado inicial: inactivo, pos=.5,flip=false
    /// </summary>
    public void reset2Init()
    {
        setActive(false);
        setFilpped(false);
        OnSliderChange(0.5f);
    }
}
