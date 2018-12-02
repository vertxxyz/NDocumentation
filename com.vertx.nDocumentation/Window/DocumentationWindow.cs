using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;

namespace Vertx
{
	public abstract class DocumentationWindow : EditorWindow
	{
		/// <summary>
		/// A EditorPrefs key for use tracking window state.
		/// </summary>
		/// <returns>The window key.</returns>
		protected abstract string StateEditorPrefsKey ();

		private DocumentationContent content;
		
		private void OnEnable()
		{
			VisualElement root = this.GetRootVisualContainer();
			content = new DocumentationContent(root, GetType(), StateEditorPrefsKey());
		}
		
		
	}
}