using System.Linq;
using Tonari.Unity;
using UnityEditor;

namespace Tonari.UnityEditor
{
    [CustomEditor(typeof(GameModeSettingsObject), false)]
    public class GameModeSettingsObjectInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = serializedObject.targetObject as GameModeSettingsObject;

            EditorGUILayout.LabelField("GameInstance", settings.GameInstance.GetType().Name);
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("GameMode");
            using (new EditorGUI.IndentLevelScope(1))
            {
                foreach (var x in settings.Settings.OrderBy(x => x.SceneName))
                {
                    EditorGUILayout.LabelField(x.SceneName, x.GameMode.GetType().Name);
                }
            }
        }
    }
}
