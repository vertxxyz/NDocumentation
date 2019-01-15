using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Example
{
	public class ExampleWindow : DocumentationWindow
	{
		[MenuItem("Window/Example Window")]
		static void Open()
		{
			ExampleWindow exampleWindow = GetWindow<ExampleWindow>();
			exampleWindow.titleContent = new GUIContent("Example");
			exampleWindow.Show();
		}

		protected override string StateEditorPrefsKey => "Example_Prefs_Key";

		private void OnEnable()
		{
			InitialiseDocumentationOnRoot(this, rootVisualElement);
			StyleSheet exampleStyleSheet = DocumentationUtility.LoadAssetOfType<StyleSheet>("ExampleStyles", DocumentationUtility.SearchFilter.Packages);
			GetDefaultRoot().styleSheets.Add(exampleStyleSheet);
		}
	}
}