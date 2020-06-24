using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Serialization;
using UnityEngine;

using UnityEngine.UI;

namespace Titan.UI
{
    /// <summary>
    /// Image is a textured element in the UI hierarchy.
    /// </summary>

    public class ImageT : Image
    {
        #region Public Attributes

        #endregion

        #region Private Attributes

        #endregion

        #region Override Methods

        #endregion

        #region Private Methods
        /// You can set this property by UiColor.Gray and UiColor.Clear
        [SerializeField]
        private bool m_isGray = false;

        private static string grayShader = "UI/DefaultGray";
        private static Dictionary<Texture2D, Material> s_UIGrayMaterials = new Dictionary<Texture2D, Material>();
        #endregion



        #region Public Methods

#if UNITY_ANDROID 
        private static string alphaShader = "UI/DefaultAlpha";

        /***************************** Begin: ETC1 + Alpha Support *****************************/
        private static Dictionary<Texture2D, Material> s_UIMaterials = new Dictionary<Texture2D, Material>();

        void AssignAlphaMaterial(Sprite sprite)
        {
            if (sprite != null)
            {
                if (sprite.associatedAlphaSplitTexture != null) 
                {
                    if (m_isGray)
                    {
                        fillMaterial(s_UIGrayMaterials, grayShader);
                    }
                    else
                    {
                        fillMaterial(s_UIMaterials, alphaShader);
                    }
                    //LogManager.Log("Upd " + sprite.name + " Material:" + m_Material.name);
                }
                else
                {
                    if (m_isGray)
                    {
                        fillMaterial(s_UIGrayMaterials, grayShader);
                    }
                    else
                    {
                        m_Material = s_DefaultUI;
                    }
                }
            }
        }

        void fillMaterial(Dictionary<Texture2D, Material> dict,string shaderName)
        {
            if (dict.ContainsKey(sprite.texture))
            {
                m_Material = dict[sprite.texture];
            }
            else
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    Material newMaterial = new Material(shader);
                    newMaterial.name = newMaterial.name + "_" + sprite.texture.name;
                    if (Application.isPlaying)
                    {
                        newMaterial.SetTexture("_AlphaTex", sprite.associatedAlphaSplitTexture);
                    }
                    dict[sprite.texture] = newMaterial;

                    m_Material = newMaterial;
                }
                else
                    Debug.LogError("Missing Shader:"+shaderName);
            }
        }

        protected override void UpdateMaterial()
        {
            AssignAlphaMaterial(this.sprite);
            base.UpdateMaterial();
        }

        protected override void Start()
        {
            base.Start();
            AssignAlphaMaterial(this.sprite);
        }
        /***************************** End: ETC1 + Alpha Support *****************************/
          
        public void updateMaterial(bool isGray) 
        {
            m_isGray = isGray;
            UpdateMaterial();
        }
        
        public static void clearMaterialDict()
        {
            s_UIMaterials.Clear();
            s_UIGrayMaterials.Clear();
        }
#elif UNITY_IOS || UNITY_STANDALONE
        protected override void UpdateMaterial()
        {
            updateMaterial(m_isGray);
            base.UpdateMaterial();
        }

        protected override void Start()
        {
            base.Start();
            updateMaterial(m_isGray);
        }

        public void updateMaterial(bool isGray)
        {
            m_isGray = isGray;
            if (sprite != null)
            {
                if (m_isGray)
                {
                    if (s_UIGrayMaterials.ContainsKey(sprite.texture))
                    {
                        m_Material = s_UIGrayMaterials[sprite.texture];
                    }
                    else
                    {
                        Shader shader = Shader.Find(grayShader);
                        if (shader != null)
                        {
                            Material newMaterial = new Material(shader);
                            newMaterial.name = newMaterial.name + "_" + sprite.texture.name;
                            s_UIGrayMaterials[sprite.texture] = newMaterial;

                            m_Material = newMaterial;
                        }
                        else
                            Debug.LogError("Missing Shader:" + grayShader);
                    }
                }
                else
                {
                    m_Material = s_DefaultUI;
                }
            }
        }

        public static void clearMaterialDict()
        {
            s_UIGrayMaterials.Clear();
        }

        protected override void OnDestroy()
        {
            clearMaterialDict();
        }
#endif
        #endregion

        #region Inner

        #endregion
    }
}
