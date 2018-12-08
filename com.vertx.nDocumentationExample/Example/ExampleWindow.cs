using UnityEditor;
using UnityEngine;

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
	}
}