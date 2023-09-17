
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using VRC.Udon.Common;
using System.Collections.Immutable;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace VRSL{
    public enum ReadType
    {
        Animation,
        Particles,
        Event,
        ObjectToggle,
        Data
    }

    public enum AnimationParamType
    {
        Bool,
        Int,
        Float,
        Trigger
    }

    public enum ValueOutputMethod
    {
        Value,
        Range
    }
    public enum DataOutputType
    {
        Bool,
        Int,
        Float,
        String
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRSL_ReadBackFunction : UdonSharpBehaviour
    {

        [SerializeField]
        private float universalSmoothingStrength = 0.5f;
        public VRSL_GPUReadBack reader;
        public int size = 10;
        //public bool extendedUniverseMode;

        //public int testChannel = 1;
        [SerializeField]
        private Animator[] animators = new Animator[512];
        [SerializeField]
        private int[] dmxUnviverses = new int[512];
        [SerializeField]
        private int[] dmxChannels = new int[512];
        [SerializeField]
        private int[] dmxRawData = new int[512];
        private int[] previousdmxRawData = new int[512];
        private float[] dmxRawDataVelocity = new float[512];
        private float[] floatDataVelocity = new float[512];
        public UdonBehaviour[] targetFunctions = new UdonBehaviour[512];
        [SerializeField]
        private bool[] eventFireReset = new bool[512];
        [SerializeField]
        private int[] eventFireResetThreshold = new int[512];
        [SerializeField]
        private bool[] eventSpamToggle = new bool[512];
        [SerializeField]
        private float[] eventFireTimer = new float[512];
        [SerializeField]
        private float[] eventSpamDelay = new float[512];
        public string[] eventNames = new string[512];
        [SerializeField]
        private int[] eventCallThreshold = new int[512];
        [SerializeField]
        private ParticleSystem[] particlesSystems = new ParticleSystem[512];
        [SerializeField]
        private int[]particlesSystemsThreshold = new int[512];
        [SerializeField]
        private bool[]particlesSystemsOneShotToggle = new bool[512];
        [SerializeField]
        private bool[]particlesSystemsOneShotReset = new bool[512];
        [SerializeField]
        private int[]particlesSystemsOneShotResetThreshold = new int[512];
        [SerializeField]
        private ReadType[] readTypes = new ReadType[512];
        [SerializeField]
        private AnimationParamType[] parameterTypes = new AnimationParamType[512];
        [SerializeField]
        private bool[] animationBoolParams = new bool[512];
        [SerializeField]
        private bool[] floatSmoothingToggle = new bool[512];
        [SerializeField]
        private ValueOutputMethod[] animationRangeToggle = new ValueOutputMethod[512];
        [SerializeField]
        private int[] animationBoolParamsThreshold = new int[512];
        [SerializeField]
        private float[] animationFloatParams = new float[512];
        [SerializeField]
        private int[] animationFloatParamsThreshold = new int[512];
        [SerializeField]
        private Vector2[] animationFloatParamsRange = new Vector2[512];
        [SerializeField]
        private int[] animationIntParams = new int[512];
        [SerializeField]
        private int[] animationIntParamsThreshold = new int[512];
        [SerializeField]
        private int[] animationIntParamsRangeX = new int[512];
        [SerializeField]
        private int[] animationIntParamsRangeY = new int[512];
        [SerializeField]
        private string[] animationParamNames = new string[512];
        // [SerializeField]
        // private bool[]animationTriggerToggle = new bool[512];
        [SerializeField]
        private bool[]animationTriggerReset = new bool[512];
        [SerializeField]
        private int[]animationTriggerResetThreshold = new int[512];
        [SerializeField]
        private int[] animationTriggerParamsThreshold = new int[512];
    /////////////////////////////////////////////////////////////////////////////////////////////

        [SerializeField]
        private DataOutputType[] dataTypes = new DataOutputType[512];
        [SerializeField]
        private bool[] dataBools = new bool[512];
        [SerializeField]
        private bool[] dataBoolsPrevious = new bool[512];
        [SerializeField]
        private int[]dataBoolsThreshold = new int[512];
        [SerializeField]
        private int[] dataInts = new int[512];
        [SerializeField]
        private int[] dataIntsPrevious = new int[512];
        [SerializeField]
        private int[] dataIntThreshold = new int[512];
        [SerializeField]
        private int[] dataIntRangeX = new int[512];
        [SerializeField]
        private int[] dataIntRangeY = new int[512];
        [SerializeField]
        private float[] dataFloats = new float[512];
        [SerializeField]
        private float[] dataFloatsPrevious = new float[512];
        [SerializeField]
        private int[] dataFloatThreshold = new int[512];
        [SerializeField]
        private int[] dataInterpolationTolerance = new int[512];
        [SerializeField]
        private Vector2[] dataFloatRange = new Vector2[512];
        [SerializeField]
        private bool[] dataUseEvent = new bool[512];
        [SerializeField]
        private GameObject[] toggleGameobjects = new GameObject[512];
        [SerializeField]
        private bool[] invertToggle = new bool[512];
    /////////////////////////////////////////////////////////////////////////////////////////////////////
        public VRSL_ReadBackFunction_StringList[]dataStringList = new VRSL_ReadBackFunction_StringList[512];
        [SerializeField]
        private int[] dataStringRangeX = new int[512]; 
        [SerializeField]
        private int[] dataStringRangeY = new int[512]; 
        
        private int[] dataStringPreviousIndex = new int[512];
        int width, height;
        int offsetUpBy4Pixels, rowSize, rowSizeInv, totalNumOfPixels;

        [SerializeField]
        private bool[] dataFoldout = new bool[512];


        public string[] dataStringActive = new string[512];
        public bool[] dataBoolActive = new bool[512];
        public int[] dataIntActive = new int[512];
        public float[] dataFloatActive = new float[512];
        private float[] previousDataFloatActive = new float[512];
        [SerializeField]
        private float[] smoothingAmount = new float[512];
        [SerializeField]
        private float[] floatSmoothingMultiplier = new float[512];
        [SerializeField]
        private float smoothingSpeed = 0.1f;

        //int hardoffset = 1;


        void Start()
        {
            width = (int)universalSmoothingStrength;
            width = reader.texture.width;
            height = reader.texture.height;

            offsetUpBy4Pixels = width * ( (int) Mathf.Clamp(((reader.dmxPixelSize/2) - 1), 1, 10));
            rowSize = width / reader.dmxPixelSize;
            rowSizeInv = reader.dmxPixelSize * width;
            totalNumOfPixels = width * height;
            

            //hardoffset = reader.dmxPixelSize == 2 ? 0 : 1;

        }
        int IntRemap(int source, int sourceFrom, int sourceTo, int targetFrom, int targetTo)
        {
            return (int) Mathf.Round((float)targetFrom + ((float)source-(float)sourceFrom)*((float)targetTo-(float)targetFrom)/((float)sourceTo-(float)sourceFrom));
        }
        float FloatRemap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
        {
            return targetFrom + (source-sourceFrom)*(targetTo-targetFrom)/(sourceTo-sourceFrom);
        }

        int GetIndex(int startindex, int i, out int index, out int xOffset, out int yOffset)
        {
            xOffset = ((dmxChannels[i]-1) % (rowSize)) * reader.dmxPixelSize;
            yOffset = (((dmxChannels[i]-1) / (rowSize)) * (rowSizeInv));
            index =  Mathf.Clamp(startindex + yOffset + xOffset,0, totalNumOfPixels-1);
            return index;
        }
        public bool _GetBoolData(int index)
        {
            return dataBoolActive[index];
        }
        public int _GetIntData(int index)
        {
            return dataIntActive[index];
        }
        public float _GetFloatData(int index)
        {
            return dataFloatActive[index];
        }
        public string _GetStringData(int index)
        {
            return dataStringActive[index];
        }
        public ParticleSystem _GetParticleSystem(int index)
        {
            return particlesSystems[index];
        }
        void RunFunctions(int i)
        {
            int rawdata = dmxRawData[i];
            if(smoothingAmount[i] > 0.0f){
                rawdata = (int) Mathf.Lerp(dmxRawData[i], (int) Mathf.SmoothDamp(previousdmxRawData[i], dmxRawData[i], ref dmxRawDataVelocity[i], (reader.updateRate * smoothingSpeed),10000.0f, reader.updateRate), smoothingAmount[i]);
            }            
            switch(readTypes[i])
            {
                case ReadType.Animation:
                    if(animators[i] == null || animationParamNames[i] == ""){return;}
                    switch(parameterTypes[i])
                    {
                        case AnimationParamType.Bool:
                            if(rawdata >= animationBoolParamsThreshold[i]){
                                animationBoolParams[i] = true;
                            }
                            else{
                                animationBoolParams[i] = false;
                            }
                            dataBoolActive[i] = animationBoolParams[i];

                            if(dataBoolActive[i] != dataBoolsPrevious[i])
                            {
                                animators[i].SetBool(animationParamNames[i], animationBoolParams[i]);
                                dataBoolsPrevious[i] = dataBoolActive[i];
                                if(dataUseEvent[i] == true){
                                    targetFunctions[i].SendCustomEvent(eventNames[i]);
                                }
                            }

                            break;
                        case AnimationParamType.Int:
                            if(animationRangeToggle[i] == ValueOutputMethod.Value){
                                Debug.Log("Updated Int at Index With Value: " + i);
                                if(rawdata >= animationIntParamsThreshold[i])
                                {
                                    if(animators[i].GetInteger(animationParamNames[i]) != animationIntParams[i])
                                    {
                                        animators[i].SetInteger(animationParamNames[i], animationIntParams[i]);
                                        dataIntActive[i] = animationIntParams[i];
                                        if(dataUseEvent[i] == true){
                                            targetFunctions[i].SendCustomEvent(eventNames[i]);
                                        }
                                    }
                                }
                                else
                                {  
                                    if(animators[i].GetInteger(animationParamNames[i]) != 0)
                                    {
                                        animators[i].SetInteger(animationParamNames[i], 0);
                                        dataIntActive[i] = 0;
                                        if(dataUseEvent[i] == true){
                                            targetFunctions[i].SendCustomEvent(eventNames[i]);
                                        }
                                    }
                                }
                            }
                            else if(animationRangeToggle[i] == ValueOutputMethod.Range){
                                float valf = FloatRemap((float) rawdata,0.0f,255.0f,(float) animationIntParamsRangeX[i],(float) animationIntParamsRangeY[i]);
                                int val = Mathf.RoundToInt(valf);
                                //Debug.Log("Updated Intf With Range: " + rawdata);
                                //Debug.Log("Updated Int at Index With Range: " + valf);
                                if(animators[i].GetInteger(animationParamNames[i]) != val)
                                {
                                    animators[i].SetInteger(animationParamNames[i], val);
                                    dataIntActive[i] = val;
                                    if(dataUseEvent[i] == true){
                                        targetFunctions[i].SendCustomEvent(eventNames[i]);
                                    }
                                }
                            }
                            break;
                        case AnimationParamType.Float:
                            if(animationRangeToggle[i] == ValueOutputMethod.Value){
                                float previousData = dataFloatActive[i];
                                if(rawdata >= animationFloatParamsThreshold[i])
                                {
                                    if(animators[i].GetFloat(animationParamNames[i]) != animationFloatParams[i])
                                    {
                                        animators[i].SetFloat(animationParamNames[i], animationFloatParams[i]);
                                        dataFloatActive[i] = animationFloatParams[i];
                                        if(dataUseEvent[i] == true){
                                            targetFunctions[i].SendCustomEvent(eventNames[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    if(animators[i].GetFloat(animationParamNames[i]) != 0.0f)
                                    {
                                        animators[i].SetFloat(animationParamNames[i], 0.0f);
                                        dataFloatActive[i] = 0.0f;
                                        if(dataUseEvent[i] == true){
                                            targetFunctions[i].SendCustomEvent(eventNames[i]);
                                        }
                                    }
                                }
                                previousDataFloatActive[i] = previousData;
                            }
                            else if(animationRangeToggle[i] == ValueOutputMethod.Range){
                                float previousData = dataFloatActive[i];
                                
                                float val = FloatRemap((float) rawdata, 0.0f, 255.0f, animationFloatParamsRange[i].x, animationFloatParamsRange[i].y);
                                if(animators[i].GetFloat(animationParamNames[i]) != val){
                                    
                                    if(floatSmoothingToggle[i]){
                                        dataFloatActive[i] = Mathf.Clamp(Mathf.SmoothDamp(previousDataFloatActive[i], val, ref floatDataVelocity[i], (reader.updateRate * (smoothingSpeed*floatSmoothingMultiplier[i])),10000.0f, reader.updateRate),animationFloatParamsRange[i].x, animationFloatParamsRange[i].y);
                                        animators[i].SetFloat(animationParamNames[i], dataFloatActive[i]);
                                    }else{
                                        animators[i].SetFloat(animationParamNames[i], val);
                                        dataFloatActive[i] = val;
                                    }
                                    if(dataUseEvent[i] == true){
                                        targetFunctions[i].SendCustomEvent(eventNames[i]);
                                    }
                                }
                                previousDataFloatActive[i] = previousData;
                            }
                            break;
                        case AnimationParamType.Trigger:
                            if(animationTriggerReset[i] == false && rawdata >= animationTriggerParamsThreshold[i])
                            {
                                animators[i].SetTrigger(animationParamNames[i]);
                                animationTriggerReset[i] = true;
                                if(dataUseEvent[i] == true){
                                    targetFunctions[i].SendCustomEvent(eventNames[i]);
                                }
                            }
                            else if (animationTriggerReset[i] == true && rawdata <= animationTriggerResetThreshold[i])
                            {
                                animators[i].ResetTrigger(animationParamNames[i]);
                                animationTriggerReset[i] = false;
                            }
                            else
                            {
                                animators[i].ResetTrigger(animationParamNames[i]);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case ReadType.Particles:
                    if(particlesSystems[i] == null){return;}
                    if(particlesSystemsOneShotToggle[i])
                    {
                        if(particlesSystemsOneShotReset[i] == false && rawdata >= particlesSystemsThreshold[i])
                        {
                            particlesSystems[i].Play();
                            particlesSystemsOneShotReset[i] = true;
                            if(dataUseEvent[i] == true){
                                targetFunctions[i].SendCustomEvent(eventNames[i]);
                            }
                        }
                        else if (particlesSystemsOneShotReset[i] == true && rawdata <= particlesSystemsOneShotResetThreshold[i])
                        {
                            particlesSystemsOneShotReset[i] = false;
                        }
                        else if(particlesSystems[i].isPlaying)
                        {
                            particlesSystems[i].Stop();
                        }
                        dataBoolActive[i] = particlesSystems[i].isPlaying;
                    }
                    else
                    {
                        // if(dataIntActive[i] != rawdata){
                            if(rawdata >= particlesSystemsThreshold[i])
                            {
                                particlesSystems[i].Play();
                            }
                            else if(particlesSystems[i].isPlaying)
                            {
                                particlesSystems[i].Stop();
                            }


                        dataBoolActive[i] = (rawdata >= particlesSystemsThreshold[i]);
                        if(dataBoolActive[i] != dataBoolsPrevious[i]){
                            if(dataUseEvent[i] == true){
                                targetFunctions[i].SendCustomEvent(eventNames[i]);
                            }
                        }
                        dataBoolsPrevious[i] =dataBoolActive[i];
                    }
                    break;
                case ReadType.Event:
                    if(targetFunctions[i] == null || eventNames[i] == ""){return;}
                    if(eventSpamToggle[i] == true)
                    {
                        eventFireReset[i] = false;
                        if(eventSpamDelay[i] > 0.0f)
                        {
                            eventFireTimer[i] += Time.deltaTime;
                            if(eventFireTimer[i] >= eventSpamDelay[i] && rawdata >= eventCallThreshold[i])
                            {
                                eventFireTimer[i] = 0.0f;
                                targetFunctions[i].SendCustomEvent(eventNames[i]);
                            }
                        }
                        else
                        {
                            if(rawdata >= eventCallThreshold[i])
                            {
                                targetFunctions[i].SendCustomEvent(eventNames[i]);
                            }
                        }      
                    }
                    else
                    {
                        if(eventFireReset[i] == false && rawdata >= eventCallThreshold[i])
                        {
                            targetFunctions[i].SendCustomEvent(eventNames[i]);
                            eventFireReset[i] = true;
                        }
                        else if (eventFireReset[i] == true && rawdata <= eventFireResetThreshold[i])
                        {
                            eventFireReset[i] = false;
                        }
                    }
                    break;
                
                    case ReadType.ObjectToggle:
                    if(toggleGameobjects[i] == null){return;}
                        if(invertToggle[i])
                        {
                            if(rawdata >= eventCallThreshold[i] && toggleGameobjects[i].activeSelf == true)
                            {
                                toggleGameobjects[i].SetActive(false);
                            }
                            else if(rawdata < eventCallThreshold[i] && toggleGameobjects[i].activeSelf == false)
                            {
                                toggleGameobjects[i].SetActive(true);
                            }
                        }
                        else
                        {
                            if(rawdata >= eventCallThreshold[i] && toggleGameobjects[i].activeSelf == false)
                            {
                                toggleGameobjects[i].SetActive(true);
                            }
                            else if(rawdata < eventCallThreshold[i] && toggleGameobjects[i].activeSelf == true)
                            {
                                toggleGameobjects[i].SetActive(false);
                            }
                        }
                        dataBoolActive[i] = toggleGameobjects[i].activeSelf;
                        if(dataBoolActive[i] != dataBoolsPrevious[i])
                        {
                            if(dataUseEvent[i] == true){
                                targetFunctions[i].SendCustomEvent(eventNames[i]);
                            }
                        }
                        dataBoolsPrevious[i] = dataBoolActive[i];

                    break;


                case ReadType.Data:
                    if(dataUseEvent[i] == true && targetFunctions[i] == null){return;}
                    switch(dataTypes[i])
                    {
                        case DataOutputType.Bool:
                            if(rawdata >= dataBoolsThreshold[i]){
                                dataBools[i] = true;
                            }
                            else{
                                dataBools[i] = false;
                            }
                            if(dataBools[i] != dataBoolsPrevious[i])
                            {
                                dataBoolsPrevious[i] = dataBools[i];
                                dataBoolActive[i] = dataBools[i];
                                if(dataUseEvent[i] == true){
                                    targetFunctions[i].SendCustomEvent(eventNames[i]);
                                }
                            }
                            break;
                        case DataOutputType.Int:
                            if(animationRangeToggle[i] == ValueOutputMethod.Value)
                            {
                                // if(dataUseEvent[i] == true)
                                // {
                                    if(rawdata >= dataIntThreshold[i])
                                    {     
                                        if(eventFireReset[i] == false && dataUseEvent[i] == true)
                                        {
                                            targetFunctions[i].SendCustomEvent(eventNames[i]);
                                            eventFireReset[i] = true;
                                        }
                                        //Debug.Log("Activating Value!");
                                        dataIntActive[i] = dataInts[i];
                                    }
                                    else if(rawdata <= dataIntThreshold[i])
                                    {
                                        if (eventFireReset[i] == true && dataUseEvent[i] == true)
                                        {
                                            eventFireReset[i] = false;
                                        }
                                        dataIntActive[i] = 0;
                                        //Debug.Log("Deactivating Value!");
                                    }
                                // }
                                
                            }
                            else
                            {
                                int val = IntRemap(rawdata,0,255,dataIntRangeX[i],dataIntRangeY[i]);
                                //Debug.Log("Current Value: " + val.ToString());
                                if(val != dataIntsPrevious[i])
                                {
                                    dataInts[i] = val;
                                    dataIntsPrevious[i] = val;
                                    dataIntActive[i] = val;
                                    //Debug.Log(dataIntActive[i]);
                                    if(dataUseEvent[i] == true)
                                    {
                                        int delta = Mathf.Abs(rawdata - previousdmxRawData[i]);
                                        if(delta > dataInterpolationTolerance[i])
                                        {
                                            targetFunctions[i].SendCustomEvent(eventNames[i]);
                                        }

                                    }
                                }
                            }
                            break;
                        case DataOutputType.Float:
                            if(animationRangeToggle[i] == ValueOutputMethod.Value)
                                {
                                    float previousData = dataFloatActive[i];
                                    
                                    // if(dataUseEvent[i] == true)
                                    // {
                                        if(rawdata >= dataFloatThreshold[i])
                                        {     
                                            if(eventFireReset[i] == false && dataUseEvent[i] == true)
                                            {
                                                targetFunctions[i].SendCustomEvent(eventNames[i]);
                                                eventFireReset[i] = true;
                                            }
                                            dataFloatActive[i] = dataFloats[i];
                                        }
                                        else if(rawdata <= dataFloatThreshold[i])
                                        {
                                            if (eventFireReset[i] == true && dataUseEvent[i] == true)
                                            {
                                                eventFireReset[i] = false;
                                            }
                                            dataFloatActive[i] = 0.0f;
                                        }
                                    // }
                                    previousDataFloatActive[i] = previousData;
                                }
                                else
                                {
                                    float previousData = dataFloatActive[i];
                                    float val = FloatRemap((float)rawdata,0.0f,255.0f,dataFloatRange[i].x,dataFloatRange[i].y);
                                    if(val != dataFloatsPrevious[i])
                                    {
                                        if(floatSmoothingToggle[i])
                                        {
                                            dataFloats[i] = Mathf.Clamp(Mathf.SmoothDamp(previousDataFloatActive[i], val, ref floatDataVelocity[i], (reader.updateRate * (smoothingSpeed*floatSmoothingMultiplier[i])),10000.0f, reader.updateRate),dataFloatRange[i].x, dataFloatRange[i].y);
                                            dataFloatsPrevious[i] = dataFloats[i];
                                            dataFloatActive[i] = dataFloats[i]; 
                                        }
                                        else
                                        {
                                            dataFloats[i] = val;
                                            dataFloatsPrevious[i] = val;
                                            dataFloatActive[i] = val; 
                                        }
                                        if(dataUseEvent[i] == true)
                                        {
                                            int delta = Mathf.Abs(rawdata - previousdmxRawData[i]);
                                            if(delta > dataInterpolationTolerance[i])
                                            {
                                                targetFunctions[i].SendCustomEvent(eventNames[i]);
                                            }
                                        }
                                    }
                                    previousDataFloatActive[i] = previousData;
                                }
                            break;
                        case DataOutputType.String:
                            int valIndex = IntRemap(rawdata,0,255,dataStringRangeX[i],dataStringRangeY[i]);
                            string newString = dataStringList[i].stringList[valIndex];
                            if(dataUseEvent[i] == true && dataStringActive[i] != newString)
                            {
                                dataStringActive[i] = newString;
                                if(dataInterpolationTolerance[i] == 0)
                                {
                                    targetFunctions[i].SendCustomEvent(eventNames[i]);
                                    //Debug.Log("Current String: " + dataStringActive[i]);
                                }
                                else
                                {
                                    int delta = Mathf.Abs(rawdata - previousdmxRawData[i]);
                                    if(delta > dataInterpolationTolerance[i])
                                    {
                                        targetFunctions[i].SendCustomEvent(eventNames[i]);
                                    }
                                }
                            }
                            else
                            {
                            dataStringActive[i] = newString;}
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            previousdmxRawData[i] = rawdata;
            dmxRawData[i] = rawdata;
        }

        public void _GetData()
        {
            Color[] pixelData = reader.output;
            int startindex = 0;
            int index, xOffset, yOffset;

            // Sanity check width and rowSize to see if they are not zero.
            // If they are zero, then the Start event was never fired, likely
            // from the object being disabled upon world load. This causes
            // a DivideByZero error that the Unity editor's UDON exception handler
            // catches, but the in-game one does not, resulting in VRChat hard crashing
            if(width == 0 || rowSize == 0) {
                Debug.LogError("VRSL-GPUReadback Function had width or rowSize of 0. This would cause divde by zero errors! Start() event was not executed.\nWas this object disabled on world load?");
                return;
            }

            if(reader.extendedUniverseMode)
            {       
                for(int i = 0; i < Mathf.Min(Mathf.Max(size, 1), 512); i++)
                {
                    switch(dmxUnviverses[i])
                    {
                        case 1:
                            startindex = ((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels));
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].r) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].r) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 2:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + 32448 + (rowSizeInv);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].r) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].r) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 3:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + (32448*2) + (rowSizeInv*2);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].r) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].r) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 4:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels)));
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].g) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].g) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 5:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + 32448 + (rowSizeInv);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].g) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].g) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 6:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + (32448*2) + (rowSizeInv*2);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].g) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].g) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 7:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels)));
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].b) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].b) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 8:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + 32448 + (rowSizeInv);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].b) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].b) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 9:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + (32448*2) + (rowSizeInv*2);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].b) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].b) * 255.0f);}
                            RunFunctions(i);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                for(int i = 0; i < Mathf.Min(Mathf.Max(size, 1), 512); i++)
                {
                    switch(dmxUnviverses[i])
                    {
                        case 1:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels)));
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 2:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + 32448 + (rowSizeInv);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 3:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + (32448*2) + (rowSizeInv*2);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].r) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 4:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels)));
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 5:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + 32448 + (rowSizeInv);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 6:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + (32448*2) + (rowSizeInv*2);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 7:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels)));
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 8:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + 32448 + (rowSizeInv);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        case 9:
                            startindex = (((reader.dmxPixelSize / 2) + (offsetUpBy4Pixels))) + (32448*2) + (rowSizeInv*2);
                            dmxRawData[i] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i, out index, out xOffset, out yOffset)].grayscale) * 255.0f);
                            // if(reader.isInterpolated) {dmxRawData[i+1] = Mathf.RoundToInt((pixelData[GetIndex(startindex, i+1, out index, out xOffset, out yOffset)].grayscale) * 255.0f);}
                            RunFunctions(i);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        
    }
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(VRSL_ReadBackFunction))]
    [CanEditMultipleObjects]
    public class VRSL_ReadBackFunction_Editor : Editor
    {
        public static Texture logo;
        public static string ver = "VRSL GPUReadback ver:" + " <b><color=#6a15ce> 1.0</color></b>";
        SerializedProperty reader, animators, targetFunctions, particlesSystems, readTypes, eventNames, dmxRawData, smoothingAmount, smoothingSpeed, toggleGameobjects, invertToggle, dataInterpolationTolerance, floatSmoothingMultiplier;
        SerializedProperty dmxUnviverses, dmxChannels, parameterTypes, size, eventCallThreshold, animationFloatParamsRange, animationRangeToggle, animationFloatParamsThreshold, animationIntParamsThreshold;
        SerializedProperty animationFloatParams, animationBoolParams, animationIntParams, animationParamNames, animationBoolParamsThreshold, animationIntParamsRangeX, animationIntParamsRangeY, particlesSystemsThreshold, floatSmoothingToggle;
        SerializedProperty dataTypes, dataBools, dataBoolsThreshold, dataInts, dataIntThreshold, dataIntRangeX, dataIntRangeY, dataFloats, dataFloatThreshold, dataFloatRange, dataStringList, dataStringRangeX, dataStringRangeY, dataUseEvent;
        SerializedProperty  dataFoldout, particlesSystemsOneShotToggle, particlesSystemsOneShotReset, particlesSystemsOneShotResetThreshold, universalSmoothingStrength;
        SerializedProperty dataStringActive, dataBoolActive, dataIntActive, dataFloatActive, eventSpamToggle, eventSpamDelay, eventFireReset, eventFireResetThreshold, animationTriggerReset, animationTriggerResetThreshold, animationTriggerParamsThreshold;
        
        void OnEnable()
        {
            universalSmoothingStrength = serializedObject.FindProperty("universalSmoothingStrength");
            dataInterpolationTolerance = serializedObject.FindProperty("dataInterpolationTolerance");
            invertToggle = serializedObject.FindProperty("invertToggle");  
            toggleGameobjects = serializedObject.FindProperty("toggleGameobjects"); 
            smoothingSpeed = serializedObject.FindProperty("smoothingSpeed");
            smoothingAmount = serializedObject.FindProperty("smoothingAmount");
            reader = serializedObject.FindProperty("reader");
            animators = serializedObject.FindProperty("animators");
            targetFunctions = serializedObject.FindProperty("targetFunctions");
            particlesSystems = serializedObject.FindProperty("particlesSystems");
            readTypes = serializedObject.FindProperty("readTypes");
            eventNames = serializedObject.FindProperty("eventNames");
            dmxUnviverses = serializedObject.FindProperty("dmxUnviverses");
            dmxChannels = serializedObject.FindProperty("dmxChannels");
            animationParamNames = serializedObject.FindProperty("animationParamNames");
            animationBoolParams = serializedObject.FindProperty("animationBoolParams");
            animationIntParams = serializedObject.FindProperty("animationIntParams");
            animationFloatParams = serializedObject.FindProperty("animationFloatParams");
            parameterTypes = serializedObject.FindProperty("parameterTypes");
            size = serializedObject.FindProperty("size");
            eventCallThreshold = serializedObject.FindProperty("eventCallThreshold");
            animationFloatParamsRange = serializedObject.FindProperty("animationFloatParamsRange");
            animationBoolParamsThreshold = serializedObject.FindProperty("animationBoolParamsThreshold");
            animationRangeToggle = serializedObject.FindProperty("animationRangeToggle");
            animationIntParamsRangeX = serializedObject.FindProperty("animationIntParamsRangeX");
            animationIntParamsRangeY = serializedObject.FindProperty("animationIntParamsRangeY");
            animationFloatParamsThreshold = serializedObject.FindProperty("animationFloatParamsThreshold");
            animationIntParamsThreshold = serializedObject.FindProperty("animationIntParamsThreshold");
            particlesSystemsThreshold = serializedObject.FindProperty("particlesSystemsThreshold");
            dmxRawData = serializedObject.FindProperty("dmxRawData");
            //testChannel = serializedObject.FindProperty("testChannel");
            //extendedUniverseMode = serializedObject.FindProperty("extendedUniverseMode");

            animationTriggerReset = serializedObject.FindProperty("animationTriggerReset");
            animationTriggerResetThreshold = serializedObject.FindProperty("animationTriggerResetThreshold");
            animationTriggerParamsThreshold = serializedObject.FindProperty("animationTriggerParamsThreshold");

            particlesSystemsOneShotToggle = serializedObject.FindProperty("particlesSystemsOneShotToggle");
            particlesSystemsOneShotReset = serializedObject.FindProperty("particlesSystemsOneShotReset");
            particlesSystemsOneShotResetThreshold = serializedObject.FindProperty("particlesSystemsOneShotResetThreshold");



            dataStringActive = serializedObject.FindProperty("dataStringActive");
            dataBoolActive = serializedObject.FindProperty("dataBoolActive");
            dataIntActive = serializedObject.FindProperty("dataIntActive");
            dataFloatActive = serializedObject.FindProperty("dataFloatActive");
            eventSpamToggle = serializedObject.FindProperty("eventSpamToggle");
            eventSpamDelay = serializedObject.FindProperty("eventSpamDelay");
            eventFireReset = serializedObject.FindProperty("eventFireReset");
            eventFireResetThreshold = serializedObject.FindProperty("eventFireResetThreshold");

            dataFoldout = serializedObject.FindProperty("dataFoldout");

            dataTypes = serializedObject.FindProperty("dataTypes");
            dataBools = serializedObject.FindProperty("dataBools");
            dataBoolsThreshold = serializedObject.FindProperty("dataBoolsThreshold");
            dataInts = serializedObject.FindProperty("dataInts");
            dataIntThreshold = serializedObject.FindProperty("dataIntThreshold");
            dataIntRangeX = serializedObject.FindProperty("dataIntRangeX");
            dataIntRangeY = serializedObject.FindProperty("dataIntRangeY");
            dataFloats = serializedObject.FindProperty("dataFloats");
            dataFloatThreshold = serializedObject.FindProperty("dataFloatThreshold");
            dataFloatRange = serializedObject.FindProperty("dataFloatRange");
            dataStringList = serializedObject.FindProperty("dataStringList");
            dataStringRangeX = serializedObject.FindProperty("dataStringRangeX");
            dataStringRangeY = serializedObject.FindProperty("dataStringRangeY");
            dataUseEvent = serializedObject.FindProperty("dataUseEvent");
            floatSmoothingToggle = serializedObject.FindProperty("floatSmoothingToggle");
            floatSmoothingMultiplier = serializedObject.FindProperty("floatSmoothingMultiplier");
            logo = Resources.Load("VRStageLighting-Logo") as Texture;
        }

        
        public static void DrawLogo()
        {
            ///GUILayout.BeginArea(new Rect(0,0, Screen.width, Screen.height));
            // GUILayout.FlexibleSpace();
            //GUI.DrawTexture(pos,logo,ScaleMode.ScaleToFit);
            //EditorGUI.DrawPreviewTexture(new Rect(0,0,400,150), logo);
            Vector2 contentOffset = new Vector2(0f, -2f);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fixedHeight = 150;
            //style.fixedWidth = 300;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(300f, 140f, style);
            //GUILayout.Label(logo,style, GUILayout.MaxWidth(500), GUILayout.MaxHeight(200));
            GUI.Box(rect, logo,style);
            //GUILayout.Label(logo);
            // GUILayout.FlexibleSpace();
            //GUILayout.EndArea();
        }
        private static Rect DrawShurikenCenteredTitle(string title, Vector2 contentOffset, int HeaderHeight)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fontSize = 14;
            style.fixedHeight = HeaderHeight;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(16f, HeaderHeight, style);

            GUI.Box(rect, title, style);
            return rect;
        }
        public static void ShurikenHeaderCentered(string title)
        {
            DrawShurikenCenteredTitle(title, new Vector2(0f, -2f), 22);
        }
        GUIStyle RawDataStyle(int value)
        {
            GUIStyle style = new GUIStyle("label");
            if(value <= 0)
            {
                style.normal.textColor = Color.red;
            }
            else if (value > 0 && value <= 127)
            {
                style.normal.textColor = Color.yellow;
            }
            else
            {
                style.normal.textColor = Color.green;
            }

            return style;
        }
        GUIStyle BoolDataStyle(bool value)
        {
            GUIStyle style = new GUIStyle("label");
            if(value == false)
            {
                style.normal.textColor = Color.red;
            }
            else
            {
                style.normal.textColor = Color.green;
            }

            return style;
        }
        GUIStyle IntDataStyle(int value)
        {
            GUIStyle style = new GUIStyle("label");
            if(value <= 0)
            {
                style.normal.textColor = Color.red;
            }
            else
            {
                style.normal.textColor = Color.green;
            }

            return style;
        }
        GUIStyle FloatDataStyle(float value)
        {
            GUIStyle style = new GUIStyle("label");
            if(value <= 0.0f)
            {
                style.normal.textColor = Color.red;
            }
            else
            {
                style.normal.textColor = Color.green;
            }

            return style;
        }
        GUIStyle StringDataStyle()
        {
            GUIStyle style = new GUIStyle("label");

            style.normal.textColor = Color.magenta;

            return style;
        }

        
    public static GUIStyle Title1Foldout()
    {
        GUIStyle g = new GUIStyle(EditorStyles.foldoutHeader);
         g.fontSize = 20;
        // g.fontStyle = FontStyle.Bold;
        // g.normal.textColor = Color.white;
         g.alignment = TextAnchor.MiddleCenter;
         g.fixedHeight = 40;
         g.fontStyle = FontStyle.BoldAndItalic;
        g.stretchWidth = true;
        
        return g;
    }


        GUIContent Label(string label)
        {
            GUIContent content = new GUIContent();
            content.text = label;
            return content;
        }
        GUIStyle Title()
        {
            GUIStyle style = new GUIStyle("label");
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Italic;
            return style;
        }
        void GuiLine( int i_height = 1 )

    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height );
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );
    }

        public void DrawElement(int index, VRSL_ReadBackFunction targetScript)
        {
            //EditorGUILayout.LabelField("Data Point #" + index, Title());
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("box");
            dmxUnviverses.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Universe",dmxUnviverses.GetArrayElementAtIndex(index).intValue, 1, 9);
            dmxChannels.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Channel",dmxChannels.GetArrayElementAtIndex(index).intValue, 1, 512);
            
            EditorGUILayout.Space(5);
            smoothingAmount.GetArrayElementAtIndex(index).floatValue = EditorGUILayout.Slider("Smoothing Strength", smoothingAmount.GetArrayElementAtIndex(index).floatValue, 0.0f, 1.0f);
            EditorGUILayout.LabelField("RAW VALUE:", dmxRawData.GetArrayElementAtIndex(index).intValue.ToString(), RawDataStyle(dmxRawData.GetArrayElementAtIndex(index).intValue));
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(readTypes.GetArrayElementAtIndex(index),Label("Type"));
            EditorGUILayout.BeginVertical("box");
            switch(readTypes.GetArrayElementAtIndex(index).enumValueIndex)
            {
                case ((int)ReadType.Animation):
                    EditorGUILayout.PropertyField(animators.GetArrayElementAtIndex(index),Label("Animator"));
                    EditorGUILayout.PropertyField(animationParamNames.GetArrayElementAtIndex(index),Label("Parameter Name"));
                    EditorGUILayout.PropertyField(parameterTypes.GetArrayElementAtIndex(index),Label("Parameter Type"));
                    EditorGUILayout.BeginVertical("box");
                    switch(parameterTypes.GetArrayElementAtIndex(index).enumValueIndex)
                    {
                        case ((int) AnimationParamType.Bool):
                            EditorGUILayout.PropertyField(animationBoolParams.GetArrayElementAtIndex(index),Label("Bool Value"));
                            animationBoolParamsThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",animationBoolParamsThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                            EditorGUILayout.LabelField("Value:", dataBoolActive.GetArrayElementAtIndex(index).boolValue.ToString(), BoolDataStyle(dataBoolActive.GetArrayElementAtIndex(index).boolValue)); 
                            break;
                        case ((int) AnimationParamType.Int):
                            EditorGUILayout.PropertyField(animationRangeToggle.GetArrayElementAtIndex(index),Label("Value Method"));
                            EditorGUILayout.BeginVertical("box");
                            if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Value)
                            {
                                EditorGUILayout.PropertyField(animationIntParams.GetArrayElementAtIndex(index),Label("Integer Value"));
                                animationIntParamsThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",animationIntParamsThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                            }
                            else
                            {
                                //EditorGUILayout.PropertyField(animationIntParamsRange.GetArrayElementAtIndex(index), Label("Integer Range"));
                                
                                EditorGUILayout.BeginHorizontal();
                                float lw = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = 50.0f;
                                EditorGUILayout.PropertyField(animationIntParamsRangeX.GetArrayElementAtIndex(index), Label("From:"), GUILayout.MaxWidth(100.0f));
                               // animationIntParamsRangeX.GetArrayElementAtIndex(index).intValue = Mathf.Abs(animationIntParamsRangeX.GetArrayElementAtIndex(index).intValue);
                                //GUILayout.FlexibleSpace();
                                GUILayout.Space(100.0f);
                                EditorGUILayout.PropertyField(animationIntParamsRangeY.GetArrayElementAtIndex(index), Label("To:"), GUILayout.MaxWidth(100.0f));
                               // animationIntParamsRangeY.GetArrayElementAtIndex(index).intValue = Mathf.Abs(animationIntParamsRangeY.GetArrayElementAtIndex(index).intValue);
                                EditorGUIUtility.labelWidth = lw;
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.LabelField("Value:", dataIntActive.GetArrayElementAtIndex(index).intValue.ToString(), IntDataStyle(dataIntActive.GetArrayElementAtIndex(index).intValue)); 
                            EditorGUILayout.EndVertical();
                            break;
                        case ((int) AnimationParamType.Float):
                            EditorGUILayout.PropertyField(animationRangeToggle.GetArrayElementAtIndex(index),Label("Value Method"));
                            EditorGUILayout.BeginVertical("box");
                            if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Value)
                            {
                                EditorGUILayout.PropertyField(animationFloatParams.GetArrayElementAtIndex(index),Label("Float Value"));
                                animationFloatParamsThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",animationFloatParamsThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(animationFloatParamsRange.GetArrayElementAtIndex(index), Label("Float Range"));
                                EditorGUILayout.PropertyField(floatSmoothingToggle.GetArrayElementAtIndex(index), Label("Apply Smoothing To Float Value"));
                                if(floatSmoothingToggle.GetArrayElementAtIndex(index).boolValue){
                                    EditorGUI.indentLevel++;
                                    floatSmoothingMultiplier.GetArrayElementAtIndex(index).floatValue = EditorGUILayout.Slider("Smoothing Strength Multiplier", floatSmoothingMultiplier.GetArrayElementAtIndex(index).floatValue, 1.0f, 25.0f);
                                    EditorGUI.indentLevel--;
                                }

                            } 
                            EditorGUILayout.LabelField("Value:", dataFloatActive.GetArrayElementAtIndex(index).floatValue.ToString(), FloatDataStyle(dataFloatActive.GetArrayElementAtIndex(index).floatValue)); 
                            EditorGUILayout.EndVertical();  
                            break;
                        case ((int) AnimationParamType.Trigger):

                            animationTriggerParamsThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",animationTriggerParamsThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                            EditorGUILayout.LabelField("READY TO FIRE: ", (!animationTriggerReset.GetArrayElementAtIndex(index).boolValue).ToString(), BoolDataStyle(!(animationTriggerReset.GetArrayElementAtIndex(index).boolValue)));
                            animationTriggerResetThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Reset Threshold",animationTriggerResetThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                             
                            break;
                    }
                    EditorGUILayout.PropertyField(dataUseEvent.GetArrayElementAtIndex(index),Label("Send Event On Value Change?"));
                    if(dataUseEvent.GetArrayElementAtIndex(index).boolValue){
                        EditorGUILayout.PropertyField(targetFunctions.GetArrayElementAtIndex(index),Label("Target Function"));
                        EditorGUILayout.PropertyField(eventNames.GetArrayElementAtIndex(index),Label("Event Name"));
                        //if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Range){
                            dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Smoothing Tolerance",dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue, 0, 50);
                       // }
                        //EditorGUILayout.LabelField("READY TO FIRE: ", (!eventFireReset.GetArrayElementAtIndex(index).boolValue).ToString(), BoolDataStyle(!(eventFireReset.GetArrayElementAtIndex(index).boolValue)));
                        //EditorGUILayout.PropertyField(eventFireResetThreshold.GetArrayElementAtIndex(index),Label("Reset Event At:"));
                        }
                    EditorGUILayout.EndVertical();
                    break;
                case ((int)ReadType.Event):
                    EditorGUILayout.PropertyField(targetFunctions.GetArrayElementAtIndex(index),Label("Target Function"));
                    EditorGUILayout.PropertyField(eventNames.GetArrayElementAtIndex(index),Label("Event Name"));
                    eventCallThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Event Activate Threshold",eventCallThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                    
                    EditorGUILayout.PropertyField(eventSpamToggle.GetArrayElementAtIndex(index),Label("Spam? (Repeatedly Fire Event When Active)"));
                    EditorGUILayout.BeginVertical("box");
                    if(eventSpamToggle.GetArrayElementAtIndex(index).boolValue == true){
                        EditorGUILayout.PropertyField(eventSpamDelay.GetArrayElementAtIndex(index),Label("# Of Seconds Between Each Fire"));
                    }
                    else{
                        EditorGUILayout.LabelField("READY TO FIRE: ", (!eventFireReset.GetArrayElementAtIndex(index).boolValue).ToString(), BoolDataStyle(!(eventFireReset.GetArrayElementAtIndex(index).boolValue)));
                        eventFireResetThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Event Reset Threshold",eventFireResetThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                    }
                    EditorGUILayout.EndVertical();
                    break;

                case ((int)ReadType.ObjectToggle):
                    EditorGUILayout.PropertyField(toggleGameobjects.GetArrayElementAtIndex(index),Label("Target Object"));
                    eventCallThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Event Activate Threshold",eventCallThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                    EditorGUILayout.PropertyField(invertToggle.GetArrayElementAtIndex(index),Label("Invert?"));
                    try{
                        GameObject targetObject = (GameObject)toggleGameobjects.GetArrayElementAtIndex(index).objectReferenceValue;
                        EditorGUILayout.LabelField("Value:", targetObject.activeSelf.ToString(), BoolDataStyle(targetObject.activeSelf));
                    }catch(System.Exception e){e.GetType();}
                    //EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(dataUseEvent.GetArrayElementAtIndex(index),Label("Send Event On Value Change?"));
                    if(dataUseEvent.GetArrayElementAtIndex(index).boolValue){
                        EditorGUILayout.PropertyField(targetFunctions.GetArrayElementAtIndex(index),Label("Target Function"));
                        EditorGUILayout.PropertyField(eventNames.GetArrayElementAtIndex(index),Label("Event Name"));
                        //if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Range){
                            dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Smoothing Tolerance",dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue, 0, 50);
                       // }
                        //EditorGUILayout.LabelField("READY TO FIRE: ", (!eventFireReset.GetArrayElementAtIndex(index).boolValue).ToString(), BoolDataStyle(!(eventFireReset.GetArrayElementAtIndex(index).boolValue)));
                        //EditorGUILayout.PropertyField(eventFireResetThreshold.GetArrayElementAtIndex(index),Label("Reset Event At:"));
                        }
                    break;


                case ((int)ReadType.Particles):
                    EditorGUILayout.PropertyField(particlesSystems.GetArrayElementAtIndex(index),Label("Particle System"));
                    particlesSystemsThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",particlesSystemsThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                    try{
                    ParticleSystem particles = targetScript._GetParticleSystem(index);
                    EditorGUILayout.LabelField("Is Playing: ", particles.isPlaying.ToString(), BoolDataStyle(particles.isPlaying));
                    }
                    catch(System.Exception e){e.GetType();}
                    EditorGUILayout.PropertyField(particlesSystemsOneShotToggle.GetArrayElementAtIndex(index),Label("One Shot?"));
                    if(particlesSystemsOneShotToggle.GetArrayElementAtIndex(index).boolValue == true){
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("READY TO FIRE: ", (!particlesSystemsOneShotReset.GetArrayElementAtIndex(index).boolValue).ToString(), BoolDataStyle(!(particlesSystemsOneShotReset.GetArrayElementAtIndex(index).boolValue)));
                        particlesSystemsOneShotResetThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Reset Threshold",particlesSystemsOneShotResetThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.PropertyField(dataUseEvent.GetArrayElementAtIndex(index),Label("Send Event On Value Change?"));
                    if(dataUseEvent.GetArrayElementAtIndex(index).boolValue){
                        EditorGUILayout.PropertyField(targetFunctions.GetArrayElementAtIndex(index),Label("Target Function"));
                        EditorGUILayout.PropertyField(eventNames.GetArrayElementAtIndex(index),Label("Event Name"));
                        //if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Range){
                            dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Smoothing Tolerance",dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue, 0, 50);
                       // }
                        //EditorGUILayout.LabelField("READY TO FIRE: ", (!eventFireReset.GetArrayElementAtIndex(index).boolValue).ToString(), BoolDataStyle(!(eventFireReset.GetArrayElementAtIndex(index).boolValue)));
                        //EditorGUILayout.PropertyField(eventFireResetThreshold.GetArrayElementAtIndex(index),Label("Reset Event At:"));
                        }
                    break;
                case ((int)ReadType.Data):
                    EditorGUILayout.PropertyField(dataTypes.GetArrayElementAtIndex(index),Label("Data Type"));
                    EditorGUILayout.BeginVertical("box");
                    switch(dataTypes.GetArrayElementAtIndex(index).enumValueIndex)
                    {
                        case ((int) DataOutputType.Bool):
                            EditorGUILayout.PropertyField(dataBools.GetArrayElementAtIndex(index),Label("Bool Value"));
                            dataBoolsThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",dataBoolsThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                            EditorGUILayout.LabelField("Value:", dataBoolActive.GetArrayElementAtIndex(index).boolValue.ToString(), BoolDataStyle(dataBoolActive.GetArrayElementAtIndex(index).boolValue));
                            break;
                        case ((int) DataOutputType.Int):
                            EditorGUILayout.PropertyField(animationRangeToggle.GetArrayElementAtIndex(index),Label("Value Method"));
                            
                            if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Value)
                            {
                                EditorGUILayout.PropertyField(dataInts.GetArrayElementAtIndex(index),Label("Integer Value"));
                                dataIntThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",dataIntThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal();
                                float lw = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = 50.0f;
                                EditorGUILayout.PropertyField(dataIntRangeX.GetArrayElementAtIndex(index), Label("From:"), GUILayout.MaxWidth(100.0f));
                               // dataIntRangeX.GetArrayElementAtIndex(index).intValue = Mathf.Abs(dataIntRangeX.GetArrayElementAtIndex(index).intValue);
                                GUILayout.Space(100.0f);
                                EditorGUILayout.PropertyField(dataIntRangeY.GetArrayElementAtIndex(index), Label("To:"), GUILayout.MaxWidth(100.0f));
                                //dataIntRangeY.GetArrayElementAtIndex(index).intValue = Mathf.Abs(dataIntRangeY.GetArrayElementAtIndex(index).intValue);
                                EditorGUIUtility.labelWidth = lw;
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.LabelField("Value:", dataIntActive.GetArrayElementAtIndex(index).intValue.ToString(), IntDataStyle(dataIntActive.GetArrayElementAtIndex(index).intValue)); 
                            break;    
                        case ((int) DataOutputType.Float):
                            EditorGUILayout.PropertyField(animationRangeToggle.GetArrayElementAtIndex(index),Label("Value Method"));
                            if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Value)
                            {
                                EditorGUILayout.PropertyField(dataFloats.GetArrayElementAtIndex(index),Label("Float Value"));
                                dataFloatThreshold.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Activation Threshold",dataFloatThreshold.GetArrayElementAtIndex(index).intValue, 0, 255);
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(dataFloatRange.GetArrayElementAtIndex(index), Label("Float Range"));
                                EditorGUILayout.PropertyField(floatSmoothingToggle.GetArrayElementAtIndex(index), Label("Apply Smoothing To Float Value"));
                                if(floatSmoothingToggle.GetArrayElementAtIndex(index).boolValue){
                                    EditorGUI.indentLevel++;
                                    floatSmoothingMultiplier.GetArrayElementAtIndex(index).floatValue = EditorGUILayout.Slider("Smoothing Strength Multiplier", floatSmoothingMultiplier.GetArrayElementAtIndex(index).floatValue, 1.0f, 25.0f);
                                    EditorGUI.indentLevel--;
                                }
                            }
                            EditorGUILayout.LabelField("Value:", dataFloatActive.GetArrayElementAtIndex(index).floatValue.ToString(), FloatDataStyle(dataFloatActive.GetArrayElementAtIndex(index).floatValue));
                            break; 
                        case ((int) DataOutputType.String):
                            
                            targetScript.dataStringList[index] = EditorGUILayout.ObjectField("String List",dataStringList.GetArrayElementAtIndex(index).objectReferenceValue,typeof(VRSL_ReadBackFunction_StringList) ,true) as VRSL_ReadBackFunction_StringList;
                        try
                        {
                                var editor = Editor.CreateEditor(targetScript.dataStringList[index]);
                                var root = editor.DrawDefaultInspector();
                                if(root == true)
                                {
                                    editor.OnInspectorGUI();
                                }
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginHorizontal();
                                float lw = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = 100.0f;
                                EditorGUILayout.PropertyField(dataStringRangeX.GetArrayElementAtIndex(index), Label("Index From:"), GUILayout.MaxWidth(150.0f));
                                dataStringRangeX.GetArrayElementAtIndex(index).intValue = Mathf.Clamp(dataStringRangeX.GetArrayElementAtIndex(index).intValue, 0, targetScript.dataStringList[index].stringList.Length - 1);
                                GUILayout.Space(50.0f);
                                EditorGUILayout.PropertyField(dataStringRangeY.GetArrayElementAtIndex(index), Label("Index To:"), GUILayout.MaxWidth(150.0f));
                                dataStringRangeY.GetArrayElementAtIndex(index).intValue = Mathf.Clamp(dataStringRangeY.GetArrayElementAtIndex(index).intValue, 0, targetScript.dataStringList[index].stringList.Length - 1);
                                EditorGUIUtility.labelWidth = lw;
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space();
                                EditorGUILayout.LabelField("Value:", dataStringActive.GetArrayElementAtIndex(index).stringValue.ToString(), StringDataStyle());
                        }
                        catch(System.Exception e){e.GetType();}
                            break;
                    }
                    EditorGUILayout.PropertyField(dataUseEvent.GetArrayElementAtIndex(index),Label("Send Event On Value Change?"));
                    if(dataUseEvent.GetArrayElementAtIndex(index).boolValue){
                        EditorGUILayout.PropertyField(targetFunctions.GetArrayElementAtIndex(index),Label("Target Function"));
                        EditorGUILayout.PropertyField(eventNames.GetArrayElementAtIndex(index),Label("Event Name"));
                        //if(animationRangeToggle.GetArrayElementAtIndex(index).enumValueIndex == (int)ValueOutputMethod.Range){
                            dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue = EditorGUILayout.IntSlider("Smoothing Tolerance",dataInterpolationTolerance.GetArrayElementAtIndex(index).intValue, 0, 50);
                       // }
                        //EditorGUILayout.LabelField("READY TO FIRE: ", (!eventFireReset.GetArrayElementAtIndex(index).boolValue).ToString(), BoolDataStyle(!(eventFireReset.GetArrayElementAtIndex(index).boolValue)));
                        //EditorGUILayout.PropertyField(eventFireResetThreshold.GetArrayElementAtIndex(index),Label("Reset Event At:"));
                        }
                    EditorGUILayout.EndVertical();
                    break;
                default:
                    break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            DrawLogo();
            ShurikenHeaderCentered(ver);
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            if(GUILayout.Button("Find VRSL GPUReadback Reader"))
            {
                reader.objectReferenceValue = Object.FindObjectOfType<VRSL_GPUReadBack>();
            }
            EditorGUILayout.PropertyField(reader);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Size", GUILayout.MaxWidth(50.0f));
            
            EditorGUI.BeginDisabledGroup(size.intValue <= 1);
            if(GUILayout.Button("-")){Mathf.Min(Mathf.Max(size.intValue--, 1), 512);}
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(false);
            size.intValue = Mathf.Min(Mathf.Max(EditorGUILayout.IntField(Mathf.Min(Mathf.Max(size.intValue, 1), 512), GUILayout.MaxWidth(100.0f)),1), 512);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(size.intValue >= 512);
            if(GUILayout.Button("+")){Mathf.Min(Mathf.Max(size.intValue++, 1), 512);}
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            //testChannel.intValue = EditorGUILayout.IntSlider("Test Channel",testChannel.intValue, 1, 512);
           // EditorGUILayout.PropertyField(extendedUniverseMode, Label("Extended Universe Mode"));
            EditorGUILayout.BeginHorizontal();
            universalSmoothingStrength.floatValue = EditorGUILayout.Slider("Global Smoothing Strength",universalSmoothingStrength.floatValue, 0.0f, 1.0f);
            if(GUILayout.Button("Apply To All"))
            {
                for(int i = 0; i < Mathf.Min(Mathf.Max(size.intValue, 1), 512); i++)
                {
                    smoothingAmount.GetArrayElementAtIndex(i).floatValue = universalSmoothingStrength.floatValue;
                }
            }
            EditorGUILayout.EndHorizontal();
            smoothingSpeed.floatValue = EditorGUILayout.Slider("Smoothing Speed (Lower = Faster)",smoothingSpeed.floatValue, 0.001f, 1.0f);
            EditorGUILayout.Space(15);
            GuiLine(1);
            //EditorGUILayout.Space(10);
            var readbackfunctiotarget = (target as VRSL_ReadBackFunction);
            for(int i = 0; i < Mathf.Min(Mathf.Max(size.intValue, 1), 512); i++)
            {
                dataFoldout.GetArrayElementAtIndex(i).boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(dataFoldout.GetArrayElementAtIndex(i).boolValue, "Data Point #" + i, Title1Foldout());
                if(dataFoldout.GetArrayElementAtIndex(i).boolValue){
                    DrawElement(i, readbackfunctiotarget);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                GuiLine(1);
                //EditorGUILayout.Space(10);
            }
            Mathf.Min(Mathf.Max(size.intValue, 1), 512);
            if (EditorGUI.EndChangeCheck())
            {
                //Debug.Log("Found changes");
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
            //DrawDefaultInspector();
        }
    }
    #endif
}