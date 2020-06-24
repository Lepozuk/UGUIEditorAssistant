using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Titan.UI
{
    public static class UICreate
    {

        #region Public Attributes

        #endregion

        #region Private Attributes

        private const int ScreenWidth = 1280;
        private const int ScreenHight = 720;

        private const string kUILayerName = "UI";
        private static EventSystem eSys = null;
        private static Camera uiCamera = null;

        #endregion

        #region Public Methods

        // Helper methods

        static public GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ScreenWidth, ScreenHight);
            scaler.matchWidthOrHeight = 1;
            root.AddComponent<GraphicRaycaster>();

            if (uiCamera == null)
            {
                //Add Camera
                GameObject camObj = new GameObject("UICamera");
                uiCamera = camObj.AddComponent<Camera>();
                camObj.transform.SetParent(root.transform, false);
                camObj.transform.localPosition = new Vector3(0, 0, -469);
                uiCamera.clearFlags = CameraClearFlags.Depth;
                uiCamera.cullingMask = 1 << LayerMask.NameToLayer(kUILayerName);
                uiCamera.orthographic = true;
                uiCamera.orthographicSize = 1;
                uiCamera.useOcclusionCulling = false;
            }
            canvas.worldCamera = uiCamera;

            return root;
        }
        static public GameObject CreateEventSystem()
        {
            GameObject res = null;
            if (eSys == null)
            {
                res = new GameObject("EventSystem");
                eSys = res.AddComponent<EventSystem>();
                res.AddComponent<StandaloneInputModule>();
                res.AddComponent<TouchInputModule>();
            }
            else
            {
                res = eSys.gameObject;
            }
            return res;
        }
        #endregion

        #region Override Methods

        #endregion

        #region Private Methods

        #endregion

        #region Inner

        #endregion
    }
}
