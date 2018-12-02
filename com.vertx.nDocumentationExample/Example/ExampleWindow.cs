using UnityEditor;

namespace Vertx
{
	public class ExampleWindow : DocumentationWindow
	{
		[MenuItem("Window/Example Window")]
		static void Open()
		{
			ExampleWindow exampleWindow = GetWindow<ExampleWindow>();
			exampleWindow.Show();
		}

		protected override string StateEditorPrefsKey() => "Example_Prefs_Key";
	}
}