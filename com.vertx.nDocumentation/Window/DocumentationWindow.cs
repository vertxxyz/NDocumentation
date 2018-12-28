//#define IGNORE_PICKING

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.RichTextParser;

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

		/// <summary>
		/// Constant Header is an always-drawn header section for all pages.
		/// </summary>
		/// <param name="root">Root Element to append to.</param>
		public virtual void DrawConstantHeader(VisualElement root) { }

		#region Initialise Content

		public bool RegisterButton(string key, Action action) => content.RegisterButton(key, action);

		#region Create Strings

		public static string CreateButtonString(string text, string linkThrough) => $"<button={linkThrough}>{text}</>";
		public static string CreateButtonString(string text, Color colour, string linkThrough) => CreateColouredString($"<button={linkThrough}>{text}</>", colour);
		public static string CreateColouredString(string text, Color colour) => $"<color={colour}>{text}</>";

		#endregion

		#endregion

		#region Adding Content

		#region Header Button

		public Button AddFullWidthButton(string text, Color textColor, Action action, VisualElement root = null)
		{
			Button headerButton = new Button(action)
			{
				text = text,
				style =
				{
					color = textColor
				}
			};
			headerButton.ClearClassList();
			headerButton.AddToClassList("injected-button");
			content.AddToRoot(headerButton, root);
			return headerButton;
		}

		public Button AddFullWidthButton(string text, Color textColor, string registeredAction, VisualElement root = null)
		{
			if (!content.GetRegisteredButtonAction(registeredAction, out var action))
				return null;
			return AddFullWidthButton(text, textColor, action, root);
		}

		public Button AddFullWidthButton(Type pageType, VisualElement root = null)
		{
			string registeredAction = pageType.FullName;
			if (!content.GetRegisteredButtonAction(registeredAction, out var action))
				return null;
			return AddFullWidthButton(content.GetTitleFromPage(pageType), content.GetColorFromPage(pageType), action, root);
		}

		#endregion

		#region Text

		public Label AddPlainText(string text, VisualElement root = null)
		{
			Label plainText = new Label
			{
				text = text, //plain text automatically supports paragraphs.
				#if IGNORE_PICKING
				pickingMode = PickingMode.Ignore
				#endif
			};
			plainText.AddToClassList("plain-text");
			content.AddToRoot(plainText, root);
			return plainText;
		}

		public List<VisualElement> AddRichText(string text, VisualElement root = null) => RichTextUtility.AddRichText(text, content, content.GetRoot(root));

		public Label AddHeader(string text, int fontSizeOverride = 0, FontStyle fontStyleOverride = FontStyle.Bold, VisualElement root = null)
		{
			Label header = AddPlainText(text, root);
			header.AddToClassList("header");
			if (fontStyleOverride != FontStyle.Bold)
				header.style.unityFontStyleAndWeight = fontStyleOverride;
			if (fontSizeOverride > 0)
				header.style.fontSize = fontSizeOverride;
			AddSplitter(true, false, root);
			return header;
		}

		#endregion

		#region Splitters

		public VisualElement AddSplitter(bool halfSize = false, bool inverseColor = false, VisualElement root = null)
		{
			VisualElement halfSplitter = new VisualElement();
			halfSplitter.AddToClassList("splitter");
			if (halfSize)
				halfSplitter.AddToClassList("half");
			if (inverseColor)
				halfSplitter.AddToClassList("inverse");
			content.AddToRoot(halfSplitter, root);
			return halfSplitter;
		}

		public VisualElement AddVerticalSpace(float height, VisualElement root = null)
		{
			VisualElement verticalSpace = new VisualElement();
			verticalSpace.AddToClassList("vertical-space");
			verticalSpace.style.height = height;
			content.AddToRoot(verticalSpace, root);
			return verticalSpace;
		}

		#endregion

		#endregion

		#region Helpers

		public void SetDefaultRoot(VisualElement root) => content.SetCurrentDefaultRoot(root);
		public VisualElement GetDefaultRoot() => content.GetRoot(null);

		public sealed class DefaultRootScope : IDisposable
		{
			private readonly VisualElement lastRoot;
			private readonly DocumentationWindow window;

			public DefaultRootScope(DocumentationWindow window, VisualElement rootOverride)
			{
				this.window = window;
				lastRoot = window.content.GetRoot(null);
				window.content.SetCurrentDefaultRoot(rootOverride);
			}

			public void Dispose() => window.content.SetCurrentDefaultRoot(lastRoot);
		}

		#endregion

		#region Navigation

		public void Home() => content.Home();

		public void GoToPage(Type pageType) => content.GoToPage(pageType.FullName);

		public void GoToPage(string pageType) => content.GoToPage(pageType);

		#endregion
	}
}