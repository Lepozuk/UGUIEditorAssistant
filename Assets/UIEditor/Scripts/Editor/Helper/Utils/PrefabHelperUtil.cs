
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.UIEditor
{
    public static class PrefabHelperUtil
    {
        
        private static Texture2D mBackdropTex, mBorderTex;
        public static Texture2D BackdropTexture => mBackdropTex ?? (mBackdropTex = CreateCheckerTex(new Color(0.33f, 0.33f, 0.33f, 1f), 1,1 ));
        public static Texture2D BorderTexture => mBorderTex ?? (mBorderTex = CreateCheckerTex(new Color(0f, 0f, 0f, 1f), 1,1 ));
        
        
        private static Texture2D CreateCheckerTex(Color col, int w, int h)
        {
            var tex = new Texture2D(w, h)
            {
                hideFlags = HideFlags.DontSave
            };

            for (var x = 0; x < tex.width ; x++)
                for (var y = 0; y < tex.height ; y++)
                    tex.SetPixel(x, y, col);
            
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }
        
        private static Object GUIDToObject(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            int id = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)).GetInstanceID();
            if (id != 0) return EditorUtility.InstanceIDToObject(id);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }

        public static T GUIDToObject<T>(string guid) where T : UnityEngine.Object
        {
            UnityEngine.Object obj = GUIDToObject(guid);
            if (obj == null) return null;

            System.Type objType = obj.GetType();
            if (objType == typeof(T) || objType.IsSubclassOf(typeof(T))) return obj as T;

            if (objType == typeof(GameObject) && typeof(T).IsSubclassOf(typeof(Component)))
            {
                GameObject go = obj as GameObject;
                return go.GetComponent(typeof(T)) as T;
            }
            return null;
        }
        
        
        public static Texture2D LoadTextureInLocal(string file_path)
        {
            //创建文件读取流
            var fileStream = new FileStream(file_path, FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            
            //创建文件长度缓冲区
            var bytes = new byte[fileStream.Length];
            
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            
            
            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            
            return texture;
        }

        public static Texture GetUIPrefabPreviewTexture(GameObject prefab)
        {
            var renderCamera = InitCaptureCamera(prefab, out var canvasObj, out var cameraObj, out var bounds);
      
            var trans = prefab.GetComponent<RectTransform>();
            var width = Convert.ToInt32(trans.rect.width);
            var height = Convert.ToInt32(trans.rect.height);
            
            var texture = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
            
            var Min = bounds.min;
            var Max = bounds.max;
     
            var bw = Max.x - Min.x;
            var bh = Max.y - Min.y;
            
            var minCamSize = bw < bh ? bw : bh;
            renderCamera.orthographicSize = minCamSize / 2; //预览图要尽量少点空白
            
            renderCamera.targetTexture = texture;
            var tex = RTImage(renderCamera);
     
            Object.DestroyImmediate(canvasObj);
            Object.DestroyImmediate(cameraObj);
     
            return tex;
        }
        
        public static Texture GetUIPrefabListTexture(GameObject prefab)
        {
            var renderCamera = InitCaptureCamera(prefab, out var canvasObj, out var cameraObj, out var bounds);
      
            var Min = bounds.min;
            var Max = bounds.max;
     
            var width = Max.x - Min.x;
            var height = Max.y - Min.y;
            var minCamSize = width > height ? width : height;
            renderCamera.orthographicSize = minCamSize / 2; //预览图要尽量少点空白
            
            var texture = new RenderTexture(128, 128, 0, RenderTextureFormat.Default);
            renderCamera.targetTexture = texture;
     
            var tex = RTImage(renderCamera);
     
            Object.DestroyImmediate(canvasObj);
            Object.DestroyImmediate(cameraObj);
     
            return tex;
        }

        static Camera InitCaptureCamera(GameObject prefab, out GameObject canvasObj, out GameObject cameraObj, out Bounds bounds)
        {
            var clone = GameObject.Instantiate(prefab);
            var cloneTransform = clone.transform;
     
            cameraObj = new GameObject("render camera");
            var renderCamera = cameraObj.AddComponent<Camera>();
            
            renderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            renderCamera.clearFlags = CameraClearFlags.Color;
            renderCamera.cameraType = CameraType.SceneView;
            renderCamera.cullingMask = 1 << 21;
            renderCamera.nearClipPlane = -100;
            renderCamera.farClipPlane = 100;
            
            //如果是UGUI节点的话就要把它们放在Canvas下了
            canvasObj = new GameObject("render canvas", typeof(Canvas));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            cloneTransform.SetParent(canvasObj.transform);
            cloneTransform.localPosition = Vector3.zero;
            
            canvasObj.layer = 21;//放在21层，摄像机也只渲染此层的，避免混入了奇怪的东西
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = renderCamera;
                
            Transform[] all = clone.GetComponentsInChildren<Transform>();
            foreach (Transform trans in all)
            {
                trans.gameObject.layer = 21;
            }
            
            bounds = GetBounds(clone);
            
            cameraObj.transform.position = new Vector3(0, 0, -10);
            cameraObj.transform.LookAt(Vector3.zero);
 
            renderCamera.orthographic = true;
            
            return renderCamera;
        }
     
        static Texture2D RTImage(Camera camera)
        {
            // The Render Texture in RenderTexture.active is the one
            // that will be read by ReadPixels.
            var currentRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;
     
            // Render the camera's view.
            //camera.Render();
     
            camera.Render();
            // Make a new texture and read the active Render Texture into it.
            Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            image.Apply();
     
            // Replace the original active Render Texture.
            RenderTexture.active = currentRT;
            return image;
        }
     
        public static Bounds GetBounds(GameObject obj)
        {
            Vector3 Min = new Vector3(99999, 99999, 99999);
            Vector3 Max = new Vector3(-99999, -99999, -99999);
            MeshRenderer[] renders = obj.GetComponentsInChildren<MeshRenderer>();
            if (renders.Length > 0)
            {
                for (int i = 0; i < renders.Length; i++)
                {
                    if (renders[i].bounds.min.x < Min.x)
                        Min.x = renders[i].bounds.min.x;
                    if (renders[i].bounds.min.y < Min.y)
                        Min.y = renders[i].bounds.min.y;
                    if (renders[i].bounds.min.z < Min.z)
                        Min.z = renders[i].bounds.min.z;
     
                    if (renders[i].bounds.max.x > Max.x)
                        Max.x = renders[i].bounds.max.x;
                    if (renders[i].bounds.max.y > Max.y)
                        Max.y = renders[i].bounds.max.y;
                    if (renders[i].bounds.max.z > Max.z)
                        Max.z = renders[i].bounds.max.z;
                }
            }
            else
            {
                RectTransform[] rectTrans = obj.GetComponentsInChildren<RectTransform>();
                Vector3[] corner = new Vector3[4];
                for (int i = 0; i < rectTrans.Length; i++)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    rectTrans[i].GetWorldCorners(corner);
                    if (corner[0].x < Min.x)
                        Min.x = corner[0].x;
                    if (corner[0].y < Min.y)
                        Min.y = corner[0].y;
                    if (corner[0].z < Min.z)
                        Min.z = corner[0].z;
     
                    if (corner[2].x > Max.x)
                        Max.x = corner[2].x;
                    if (corner[2].y > Max.y)
                        Max.y = corner[2].y;
                    if (corner[2].z > Max.z)
                        Max.z = corner[2].z;
                }
            }
     
            Vector3 center = (Min + Max) / 2;
            Vector3 size = new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
            return new Bounds(center, size);
        }
     
        
        
        public static bool SaveTextureToPNG(Texture inputTex, string save_file_name)
        {
            RenderTexture temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(inputTex, temp);
            bool ret = SaveRenderTextureToPNG(temp, save_file_name);
            RenderTexture.ReleaseTemporary(temp);
            return ret;

        }

        //将RenderTexture保存成一张png图片  
        public static bool SaveRenderTextureToPNG(RenderTexture rt, string save_file_name)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            byte[] bytes = png.EncodeToPNG();
            string directory = Path.GetDirectoryName(save_file_name);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            FileStream file = File.Open(save_file_name, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();
            file.Dispose();
            
            Texture2D.DestroyImmediate(png);
            
            png = null;
            RenderTexture.active = prev;
            return true;

        }
    }
}