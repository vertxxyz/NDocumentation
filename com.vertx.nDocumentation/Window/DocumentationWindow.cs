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
		protected abstract string StateEditorPrefsKey { get; }

		private DocumentationContent content;
		
		private void OnEnable()
		{
			content = new DocumentationContent(rootVisualElement, this, StateEditorPrefsKey);
			content.InitialiseContent();
		}

		public virtual void DrawConstantHeader(VisualElement root) { }
		
		public Button AddHeaderButton(string text, Color textColor, Action action, VisualElement root = null)
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
			return headerButton;
		}

		public Label AddPlainText(string text, VisualElement root = null)
		{
			Label plainText = new Label
			{
				text = text
			};
			plainText.AddToClassList("plain-text");
			content.AddToRoot(plainText, root);
			return plainText;
		}

		public Label AddHeader(string text, int fontSizeOverride = 0, FontStyle fontStyleOverride = FontStyle.Bold, VisualElement root = null)
		{
			Label header = AddPlainText(text, root);
			header.AddToClassList("header");
			if(fontStyleOverride != FontStyle.Bold)
				header.style.unityFontStyleAndWeight = fontStyleOverride;
			if (fontSizeOverride > 0)
				header.style.fontSize = fontSizeOverride;
			AddHalfSplitter(false, root);
			return header;
		}

		public VisualElement AddHalfSplitter(bool inverseColor = false, VisualElement root = null)
		{
			VisualElement halfSplitter = new VisualElement();
			halfSplitter.AddToClassList("splitter");
			halfSplitter.AddToClassList("half");
			if(inverseColor)
				halfSplitter.AddToClassList("inverse");
			content.AddToRoot(halfSplitter, root);
			return halfSplitter;
		}
	}
}