using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UIEditor.Scripts.Editor.Helper.Utils
{
    public class PreviewTextureUtil
    {
        
        public Texture2D LoadTextureInLocal(string file_path)
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

        public Texture GetUIPrefabPreviewTexture(GameObject prefab)
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
        
        public Texture GetUIPrefabListTexture(GameObject prefab)
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

        private Camera InitCaptureCamera(GameObject prefab, out GameObject canvasObj, out GameObject cameraObj, out Bounds bounds)
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
     
        private Texture2D RTImage(Camera camera)
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
     
        private Bounds GetBounds(GameObject obj)
        {
            var min = new Vector3(99999, 99999, 99999);
            var max = new Vector3(-99999, -99999, -99999);
            var renders = obj.GetComponentsInChildren<MeshRenderer>();
            if (renders.Length > 0)
            {
                for (var i = 0; i < renders.Length; i++)
                {
                    if (renders[i].bounds.min.x < min.x) min.x = renders[i].bounds.min.x;
                    if (renders[i].bounds.min.y < min.y) min.y = renders[i].bounds.min.y;
                    if (renders[i].bounds.min.z < min.z) min.z = renders[i].bounds.min.z;
     
                    if (renders[i].bounds.max.x > max.x) max.x = renders[i].bounds.max.x;
                    if (renders[i].bounds.max.y > max.y) max.y = renders[i].bounds.max.y;
                    if (renders[i].bounds.max.z > max.z) max.z = renders[i].bounds.max.z;
                }
            }
            else
            {
                var rectTrans = obj.GetComponentsInChildren<RectTransform>();
                var corner = new Vector3[4];
                for (var i = 0; i < rectTrans.Length; i++)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    rectTrans[i].GetWorldCorners(corner);
                    
                    if (corner[0].x < min.x) min.x = corner[0].x;
                    if (corner[0].y < min.y) min.y = corner[0].y;
                    if (corner[0].z < min.z) min.z = corner[0].z;
     
                    if (corner[2].x > max.x) max.x = corner[2].x;
                    if (corner[2].y > max.y) max.y = corner[2].y;
                    if (corner[2].z > max.z) max.z = corner[2].z;
                }
            }
     
            var center = (min + max) / 2;
            var size = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
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