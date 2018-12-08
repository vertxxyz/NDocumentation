using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
			content = new DocumentationContent(rootVisualElement, this, StateEditorPrefsKey());
			content.InitialiseContent();
		}

		public void CreateHeaderButton(string text, Color textColor, Action action, VisualElement root = null)
		{
			Button headerButton = new Button(action)
			{
				text = text,
				style =
				{
					color = textColor
				}
			};
			headerButton.AddToClassList("injected-button");
			content.AddToRoot(headerButton, root);
		}
	}
}