
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Rendering;
using VRC.Udon.Common.Interfaces;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VRSL{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRSL_GPUReadBack : UdonSharpBehaviour
    {
        //public bool isVertical;
        public VRSL_ReadBackFunction[] functionReaders;
        public bool extendedUniverseMode;
        public int dmxPixelSize = 8;
        public float updateRate = 0.05f;
        public float startDelay = 1.0f;
        public Texture texture;
        public TextureFormat textureReadFormat = TextureFormat.RGBAFloat;
        bool initialize;

        [HideInInspector]
        public Color[] output;
        float currentTime;
        // public Color ch1, ch2, ch3;

        void Start()
        {
            initialize = false;
            currentTime = 0.0f;
            output = new Color[texture.width * texture.height];
            SendCustomEventDelayedSeconds("_StartGPUReadback",startDelay);

            // Sanity check all connected Readback Functions to see if they are enabled
            // so if we try to update them they will have proper variable states
            VRSL_ReadBackFunction rb;
            for(int i = 0; i < functionReaders.Length; i++)
            {
                rb = functionReaders[i];
                if(rb.gameObject.activeSelf == false) {
                    Debug.LogWarning($"Found a GPU-Readback function that was not enabled! (Index {i})\nTemporarily enabling it for initialization...");
                    rb.gameObject.SetActive(true); // Set to true so Start event fires
                    rb.gameObject.SetActive(false); // Revert to disabled state
                }
            }
        }

        void Update()
        {
            if(initialize)
            {
                if(currentTime >= updateRate)
                {
                    _MakeReadBackRequest();
                    currentTime = 0.0f;
                }
                else
                {
                    currentTime += Time.deltaTime;
                }
            }
        }

        public void _MakeReadBackRequest()
        {
            VRCAsyncGPUReadback.Request(texture, 0,textureReadFormat, (IUdonEventReceiver)this);
        }

        public void _StartGPUReadback()
        {
            initialize = true;
        }

        public void OnAsyncGpuReadbackComplete(VRCAsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error!");
                return;
            }
            else
            {
                bool wasSuccessful = request.TryGetData(output);
                if(wasSuccessful && functionReaders.Length > 0)
                {
                    foreach(VRSL_ReadBackFunction rb in functionReaders)
                    {
                        rb.SendCustomEvent("_GetData");
                    }
                }
            }
        }
    }
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(VRSL_GPUReadBack))]
    [CanEditMultipleObjects]
    public class VRSL_GPUReadBack_Editor : Editor
    {
        public static Texture logo;
        public static string ver = "VRSL GPUReadback ver:" + " <b><color=#6a15ce> 1.0</color></b>";
         SerializedProperty functionReaders;
        void OnEnable()
        {
            functionReaders = serializedObject.FindProperty("functionReaders");
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
        public override void OnInspectorGUI()
        {   
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            DrawLogo();
            ShurikenHeaderCentered(ver);
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            //var reader = (target as VRSL_GPUReadBack);
            if(GUILayout.Button("Link All ReadBack Function Objects"))
            {
                try{
                    VRSL_ReadBackFunction[] x = Object.FindObjectsOfType<VRSL_ReadBackFunction>();
                    if(x[0] != null)
                    {
                        functionReaders.arraySize = x.Length;
                        for(int i = 0; i < functionReaders.arraySize; i++)
                        {
                            functionReaders.GetArrayElementAtIndex(i).objectReferenceValue = x[i];
                        }
                    }
                }
                catch{}
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
            DrawDefaultInspector();

        }

    }
    #endif
}