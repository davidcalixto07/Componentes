using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections.Generic;



// Clase para la informacion general del App
public class AppInfo : MonoBehaviour
{
    // info del tomo del APP (001,002,003,004,etc)
    // es el UNICO punto donde se define y se usa en toda la app
    static protected string mVolume = "001";
    // ruta UNICA para las consultas WEB - se ajusta en getAppUrl(...)
    static protected string mUrl = "https://www.3dissect.com/3Dnew/";

    public static AppInfo mInstance = null;

    protected int mCurrUserId = -1;
    protected int mCurrGestUserId = -1;

    private void Awake()
    {
        mInstance = this;
    }

    static public int getCurrUserId()
    {
        return mInstance.mCurrUserId;
    }
    static public void setCurrUserId(int userId)
    {
        mInstance.mCurrUserId = userId;
    }
    static public int getCurrGestUserId()
    {
        return mInstance.mCurrGestUserId;
    }
    static public void setCurrGestUserId(int gestUserId)
    {
        mInstance.mCurrGestUserId = gestUserId;
    }

    // retorna el ID del volumen
    static public string getAppVolume()
    {
        return AppInfo.mVolume;
    }
    static public void setAppVolume(string volId)
    {
        AppInfo.mVolume = volId;
    }

    static public string getAppUrl(string url="")
    {
        if (url.Length > 0)
            return AppInfo.mUrl + "index.php/" + url;
        else
            return AppInfo.mUrl;
    }

    static public void getHttp(string url, WWWForm form, UnityAction<bool, string> funcEnd)
    {
        mInstance.StartCoroutine(mInstance.getHttpInternal(url, form, funcEnd));
    }
    static public void getHttp(string url, UnityAction<bool, string> funcEnd)
    {
        WWWForm form = new WWWForm();
        mInstance.StartCoroutine(mInstance.getHttpInternal(url, form, funcEnd));
    }
    protected IEnumerator getHttpInternal(string url, WWWForm form, UnityAction<bool, string> funcEnd)
    {
#if UNITY_EDITOR
        Debug.Log("GET:" + url);
#endif // UNITY_EDITOR
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

//#if UNITY_EDITOR
//        Debug.Log("GET RESULT:" + www.downloadHandler.text.Length);
//#endif // UNITY_EDITOR
        if (www.result == UnityWebRequest.Result.ProtocolError)
        {
            funcEnd(false, www.error);
        }
        else
        {
            funcEnd(true, www.downloadHandler.text);
        }

        yield return null;
    }

    static public void getBinHttp(string url, WWWForm form, UnityAction<bool, byte[]> funcEnd)
    {
        mInstance.StartCoroutine(mInstance.getBinHttpInternal(url, form, funcEnd));
    }
    protected IEnumerator getBinHttpInternal(string url, WWWForm form, UnityAction<bool, byte[]> funcEnd)
    {
#if UNITY_EDITOR
        Debug.Log("GET BIN:" + url);
#endif // UNITY_EDITOR
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

//#if UNITY_EDITOR
//        Debug.Log("GET RESULT:" + www.downloadHandler.text.Length);
//#endif // UNITY_EDITOR
        if (www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Err:" + www.error);
            funcEnd(false, null);
        }
        else
        {
            funcEnd(true, www.downloadHandler.data);
        }

        yield return null;
    }
    static public void getTextureHttp(string url, UnityAction<bool, Texture> funcEnd)
    {
        mInstance.StartCoroutine(mInstance.getTextureHttpInternal(url, funcEnd));
    }
    protected IEnumerator getTextureHttpInternal(string url, UnityAction<bool, Texture> funcEnd)
    {
#if UNITY_EDITOR
        Debug.Log("GET TEXTURE:" + url);
#endif // UNITY_EDITOR
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            funcEnd(false, null);
        }
        else
        {
            funcEnd(true, ((DownloadHandlerTexture)www.downloadHandler).texture);
        }

        yield return null;
    }

    static public bool isSpanish()
    {
        return true;
    }



    // https://adeshoras.wordpress.com/2008/06/07/eliminando-acentos-de-un-string-con-asp-net-2-formas-distintas-de-eliminar-los-acentos-de-manera-eficiente/#:~:text=Para%20poder%20eliminar%20los%20acentos,medio%20de%20esta%20expresi�n%20regular.
    public static string removeAccents(string inputString)
    {
        Regex replace_a_Accents = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
        Regex replace_e_Accents = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
        Regex replace_i_Accents = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
        Regex replace_o_Accents = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
        Regex replace_u_Accents = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
        inputString = replace_a_Accents.Replace(inputString, "a");
        inputString = replace_e_Accents.Replace(inputString, "e");
        inputString = replace_i_Accents.Replace(inputString, "i");
        inputString = replace_o_Accents.Replace(inputString, "o");
        inputString = replace_u_Accents.Replace(inputString, "u");
        return inputString;
    }

    static public IEnumerator funcTakeScreenshot(Texture2D texture, int w = 0, int h = 0) // android & IOS
    {
        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        if (w > 0 && h > 0) ss.Reinitialize(w, h);
    }

    public static Color parseColor3(string color, char separator=';')
    {
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";

        Vector3 col = AppInfo.parseV3(color, separator);
        Color col2 = new Color(col.x, col.y, col.z);
        return col2;
    }
    public static Color parseColor4(string color, char separator=';')
    {
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";

        Vector4 col = AppInfo.parseV4(color, separator);
        Color col2 = new Color(col.x, col.y, col.z, col.w);
        return col2;
    }
    public static float parseF(string str)
    {
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";

        return float.Parse(str, culture);
    }
    public static Vector3 parseV4(string str, char separator=';')
    {
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";
        
        Vector4 value = Vector4.zero;
        string[] pieces = str.Split(separator);
        if (pieces.Length == 4)
        {
            value.x = float.Parse(pieces[0], culture);
            value.y = float.Parse(pieces[1], culture);
            value.z = float.Parse(pieces[2], culture);
            value.w = float.Parse(pieces[3], culture);
        }
        return value;
    }
    public static Vector3 parseV3(string str, char separator=';')
    {
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";
        
        Vector3 value = Vector3.zero;
        string[] pieces = str.Split(separator);
        if (pieces.Length == 3)
        {
            value.x = float.Parse(pieces[0], culture);
            value.y = float.Parse(pieces[1], culture);
            value.z = float.Parse(pieces[2], culture);
        }
        return value;
    }
    public static Vector2 parseV2(string str, char separator=';')
    {
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";
        
        Vector2 value = Vector2.zero;
        string[] pieces = str.Split(separator);
        if (pieces.Length == 2)
        {
            value.x = float.Parse(pieces[0], culture);
            value.y = float.Parse(pieces[1], culture);
        }
        return value;
    }
    public static Quaternion parseQ(string str, char separator=';')
    {
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";

        var coordinates = str.Split(separator);
        Quaternion quaternion = Quaternion.identity;
        if (coordinates.Length==4)
        {
            quaternion.x = float.Parse(coordinates[0], culture);
            quaternion.y = float.Parse(coordinates[1], culture);
            quaternion.z = float.Parse(coordinates[2], culture);
            quaternion.w = float.Parse(coordinates[3], culture);
        }
        return quaternion;
    }

    static public string joinList(List<string> names)
    {
        string rta = "";
        for (int i = 0; i < names.Count; i++) rta += names[i] + ",";
        return rta;
    }

    //// ==============================================================
    //// ================== DIALOG FUNCTIONS ==========================
    //// ==============================================================
    //// ==============================================================
    //// ==============================================================
    //// ==============================================================
    static public void showDlgErr(string msg)
    {
        string title = AppInfo.isSpanish() ? "Error" : "Error";
        // mostrar DLG OK
    }
    static public void showDlgOk(string msg)
    {
        string title = AppInfo.isSpanish() ? "Informaci�n" : "Information";
        // mostrar DLG OK-CANCEL
    }
    static public void showDlgOkCancel(string msg, UnityEngine.Events.UnityAction actionOk)
    {
        string title = AppInfo.isSpanish() ? "Confirmar" : "Confirm";
        string buttonOkTxt = AppInfo.isSpanish() ? "Aceptar" : "Ok";
        // mostrar DLG OK-CANCEL con funcion OK
    }
    //// ==============================================================
    //// ==============================================================
}
