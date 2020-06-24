using UnityEngine;
using System.Collections;
using Titan.UI;

namespace UnityEngine.UI
{
    public static class UiColor
    { 
        #region Public Attributes

        #endregion

        #region Private Attributes
        static private Material m_GrayMaterial;
        #endregion

        #region Public Methods
        /// <summary>设置灰色</summary>
        /// <param name="tf">对象</param>
        /// <param name="flag">灰 还是还原</param>
        static public void Gray(Transform tf)
        {
            changeColor(tf, true);
        }

        /// <summary>清除颜色效果</summary>
        /// <param name="tf">对象</param>
        static public void Clear(Transform tf)
        {
            changeColor(tf, false);
        }
        #endregion

        #region Override Methods

        #endregion

        #region Private Methods
        static private void SetMaterial(Transform tf, Material material)
        {
            ImageT img = tf.GetComponent<ImageT>();
            TextT txt = tf.GetComponent<TextT>();
            if (img != null)
                img.material = material;
            if (txt != null)
            {
                txt.material = material;
                Outline outLine = txt.GetComponent<Outline>();
                if (outLine != null)
                    outLine.enabled = material == null;
            }
            for (int i = 0; i < tf.childCount; i++)
            {
                SetMaterial(tf.GetChild(i), material);
            }
        }

        static private void changeColor(Transform tf, bool isGray)
        {
            ImageT img = tf.GetComponent<ImageT>();
            if (img != null)
            {
                img.updateMaterial(isGray);
            }

            for (int i = 0; i < tf.childCount; i++)
            {
                changeColor(tf.GetChild(i), isGray);
            }
        }
        #endregion

        #region Inner

        #endregion
    }
}