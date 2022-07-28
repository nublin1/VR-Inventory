using UnityEditor;
using UnityEngine;

namespace CustomAttributes
{
    public class CustomHeaderAttribute : PropertyAttribute
    {
        public int count;
        public int depth;

        public string label;
        public string tooltip;
        public string toggleBool;

        public System.Type type;

        /// <summary>
        /// Add a header above a field
        /// </summary>
        /// <param name="label">A title for the header label</param>
        /// <param name="count">the number of child elements under this header</param>
        /// <param name="depth">the depth of this header element in the inspector foldout</param>
        public CustomHeaderAttribute(string label, int count = default, int depth = default)
        {
            this.count = count;
            this.depth = depth;
            this.label = label;
        }

        /// <summary>
        /// Add a header above a field with a tooltip
        /// </summary>
        /// <param name="label">A title for the header label</param>
        /// <param name="tooltip">A note or instruction shown when hovering over the header</param>
        /// <param name="count">the number of child elements under this header</param>
        /// <param name="depth">the depth of this header element in the inspector foldout</param>
        public CustomHeaderAttribute(string label, string tooltip, string toggleName, System.Type classType, int count = default, int depth = default)
        {
            this.count = count;
            this.depth = depth;
            this.label = label;
            this.tooltip = tooltip;
        }
    }

    [CustomPropertyDrawer(typeof(CustomHeaderAttribute))]
    public class CustomHeaderDrawer : PropertyDrawer
    {
        const float padding = 2f;
        const float margin = -20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.yMax += position.height;

            var attr = (attribute as CustomHeaderAttribute);

            var newRect = position;
            newRect.position = new Vector2(newRect.x + newRect.width - 18, newRect.y);

            // draw header background and label
            var headerRect = new Rect(position.x + margin, position.y, (position.width - margin) + (padding * 2), position.height);
            EditorGUI.DrawRect(headerRect, Constants.BackgroundColor);

            var labelStyle = Constants.HeaderStyle;
            labelStyle.font = Resources.Load<Font>("Righteous-Big");

            EditorGUI.LabelField(headerRect, new GUIContent(" " + attr.label, attr.tooltip), labelStyle);
            // EditorGUI.LabelField(headerRect, new GUIContent(" " + attr.label, Resources.Load<Texture>("AutoHandLogo"), attr.tooltip), labelStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            position.y += 2f;
        }
    }

    public static class Constants
    {
        public static Color BackgroundColor { get; } = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 0.75f) : new Color(0.7f, 0.7f, 0.7f, 0.75f);
        public static GUIStyle HeaderStyle { get; } = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 26
        };
        public static GUIStyle LabelStyle { get; } = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 15
        };
    }
}