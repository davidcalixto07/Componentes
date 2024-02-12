using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hold 3D Slice X,Y,Z
/// </summary>
public class SlicesManager : MonoBehaviour
{
    static public SlicesManager mInstance;

    public Slice mSliceX;
    public Slice mSliceY;
    public Slice mSliceZ;

    private void Awake()
    {
        mInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    static public void readConfig()
    {
        string volId = AppInfo.getAppVolume();

        //string txt = LoadAsset.loadText("config");
        TextAsset asset = Resources.Load(volId + "-resources/" + volId + "-slices/config") as TextAsset;
        string txt = asset!=null? asset.text:null;

        if (txt!=null)
        {
            Debug.Log("Init slice config ...");
            string[] lines = txt.Split('\n');
            // deben ser 19 (3 bloques de 6 + 1 id tomo 00X)
            //Axis X
            //delta: 0.0033
            //delta index:10
            //min index:1000
            //max index:1990
            //init pos:0.3269
            if (lines.Length == 19)
            {
                int count = 0;
                if (lines[count++].Trim() != volId)
                {
                    Debug.LogError("IMAGE ID:" + volId + " d'nt match:" + lines[0]);
                }
                else
                {
                    //Debug.Log("Read image config:");
                    var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    // axis X
                    {
                        count++;
                        float delta = float.Parse(lines[count++].Split(':')[1], culture);
                        int deltaIndex = int.Parse(lines[count++].Split(':')[1]);
                        int minIndex = int.Parse(lines[count++].Split(':')[1]);
                        int maxIndex = int.Parse(lines[count++].Split(':')[1]);
                        float initPos = float.Parse(lines[count++].Split(':')[1], culture);
                        mInstance.mSliceX.init(Slice.SLICE_AXIS.AXIS_X, initPos, delta, deltaIndex, minIndex, maxIndex);
                    }
                    // axis Y
                    {
                        count++;
                        float delta = float.Parse(lines[count++].Split(':')[1], culture);
                        int deltaIndex = int.Parse(lines[count++].Split(':')[1]);
                        int minIndex = int.Parse(lines[count++].Split(':')[1]);
                        int maxIndex = int.Parse(lines[count++].Split(':')[1]);
                        float initPos = float.Parse(lines[count++].Split(':')[1], culture);
                        mInstance.mSliceY.init(Slice.SLICE_AXIS.AXIS_Y, initPos, delta, deltaIndex, minIndex, maxIndex);
                    }
                    // axis Z
                    {
                        count++;
                        float delta = float.Parse(lines[count++].Split(':')[1], culture);
                        int deltaIndex = int.Parse(lines[count++].Split(':')[1]);
                        int minIndex = int.Parse(lines[count++].Split(':')[1]);
                        int maxIndex = int.Parse(lines[count++].Split(':')[1]);
                        float initPos = float.Parse(lines[count++].Split(':')[1], culture);
                        mInstance.mSliceZ.init(Slice.SLICE_AXIS.AXIS_Z, initPos, delta, deltaIndex, minIndex, maxIndex);
                    }
                } // end ID Ok

            } // end fields Ok
        }
        else
        {
            Debug.LogError("No slices config found");
        } // end asset Ok
    }
}
