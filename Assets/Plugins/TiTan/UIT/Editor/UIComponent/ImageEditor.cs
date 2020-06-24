using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

namespace Titan.UI
{
    /// <summary>
    /// Editor class used to edit UI Sprites.
    /// </summary>

    [CustomEditor(typeof(ImageT), true)]
    [CanEditMultipleObjects]
    public class ImageTEditor : ImageEditor
    {
        /// <summary>
        /// Draw the Image preview.
        /// </summary>

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            ImageT image = target as ImageT;
            if (image == null) return;

            Sprite sf = image.sprite;
            if (sf == null) return;

            SpriteDrawUtility.DrawSprite(sf, rect, image.canvasRenderer.GetColor());
        }

        /// <summary>
        /// Info String drawn at the bottom of the Preview
        /// </summary>

        public override string GetInfoString()
        {
            ImageT image = target as ImageT;
            Sprite sprite = image.sprite;

            int x = (sprite != null) ? Mathf.RoundToInt(sprite.rect.width) : 0;
            int y = (sprite != null) ? Mathf.RoundToInt(sprite.rect.height) : 0;

            return string.Format("Image Size: {0}x{1}", x, y);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ImageT img = target as ImageT;

            EditorGUI.BeginChangeCheck();
            var gray = serializedObject.FindProperty("m_isGray");
            EditorGUILayout.PropertyField(gray, new GUILayoutOption[0]);

            if (EditorGUI.EndChangeCheck())
            {
                img.updateMaterial(gray.boolValue);
            }
            serializedObject.ApplyModifiedProperties();
        }

    }
}
