
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace VRSL{

    public enum DataType{
        Bool,
        Int,
        Float,
        String
    }

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class VRSL_ReadBackFunction_UITextOut : UdonSharpBehaviour
{
    public DataType type;
    public VRSL_ReadBackFunction readBackFunction;
    public UnityEngine.UI.Text text;
    public int dataIndex;

    void Start()
    {
        switch(type)
        {
            case DataType.Bool:
                SendCustomEventDelayedSeconds("_GetBool",1.0f);
                //_GetBool();
                break;
            case DataType.Int:
                SendCustomEventDelayedSeconds("_GetInt",1.0f);
                //_GetInt();
                break;
            case DataType.Float:
                SendCustomEventDelayedSeconds("_GetFloat",1.0f);
                //_GetFloat();
                break;
            case DataType.String:
                SendCustomEventDelayedSeconds("_GetString",1.0f);
               // _GetString();
                break;
        }
    }

    public void _GetBool()
    {
        bool b = readBackFunction._GetBoolData(dataIndex);
        text.text = b.ToString();
        text.color = b ? Color.green : Color.red;
    }
    public void _GetInt()
    {
       // Debug.Log("Updated Int at Index: " + dataIndex);
        text.text = readBackFunction._GetIntData(dataIndex).ToString();
        text.color = Color.magenta;
    }
    public void _GetFloat()
    {
        text.text = readBackFunction._GetFloatData(dataIndex).ToString();
        text.color = Color.cyan;
    }
    public void _GetString()
    {
        //Debug.Log("Updated String at Index: " + dataIndex);
        text.text = readBackFunction._GetStringData(dataIndex);
        text.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
    }
}
}
