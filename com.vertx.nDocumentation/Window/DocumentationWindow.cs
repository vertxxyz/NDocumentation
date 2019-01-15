using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx
{
	/// <summary>
	/// An EditorWindow to be used with:
	/// DocumentationPageRoot, DocumentationPage, and DocumentationPageAddition.
	/// This window provides functions for adding rich text to these pages, whilst also being a platform for consistent layout.
	/// </summary>
	public abstract class DocumentationWindow : EditorWindow, IHasCustomMenu
	{
		/// <summary>
		/// A EditorPrefs key for use tracking window state.
		/// </summary>
		/// <returns>The window key.</returns>
		protected abstract string StateEditorPrefsKey { get; }

		private DocumentationContentBase content;

		protected void InitialiseDocumentationOnRoot<T>(T window, VisualElement root) where T : DocumentationWindow
		{
			content = new DocumentationContent<T>(root, window, StateEditorPrefsKey);
			content.InitialiseContent();
		}

		/// <summary>
		/// Constant Header is an always-drawn header section for all pages.
		/// </summary>
		public virtual void DrawConstantHeader() { }

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

		/// <summary>
		/// Adds a plain text paragraph
		/// </summary>
		/// <param name="text">The text to plainly display as a paragraph.</param>
		/// <param name="root">Root, current default if left null</param>
		/// <returns>The Label representing the paragraph of text</returns>
		public Label AddPlainText(string text, VisualElement root = null)
		{
			Label plainText = new Label
			{
				text = text //plain text automatically supports paragraphs.
			};
			plainText.AddToClassList("plain-text");
			content.AddToRoot(plainText, root);
			return plainText;
		}

		/// <summary>
		/// Adds VisualElements corresponding to the provided rich text to a root.
		/// </summary>
		/// <param name="text">The rich text to parse</param>
		/// <param name="root">Visual Element to append the rich text UI to, current default if left null</param>
		/// <returns>A list of all immediate children added to the root.</returns>
		public List<VisualElement> AddRichText(string text, VisualElement root = null) => RichTextUtility.AddRichText(text, content, content.GetRoot(root));

		/// <summary>
		/// Adds a header (including a half splitter for separation)
		/// </summary>
		/// <param name="text">text to display on the header Label (supports Rich Text)</param>
		/// <param name="fontSizeOverride">Font size override.</param>
		/// <param name="fontStyleOverride">Font style override.</param>
		/// <param name="root">Visual Element to append the header to, current default if left null</param>
		/// <returns>The VisualElement that contains the rich text paragraph and splitter. .</returns>
		public VisualElement AddHeader(string text, int fontSizeOverride = 0, FontStyle fontStyleOverride = FontStyle.Bold, VisualElement root = null)
		{
			VisualElement headerContainer = new VisualElement();
			GetDefaultRoot().Add(headerContainer);
			using (new DefaultRootScope(this, headerContainer))
			{
				List<VisualElement> header = AddRichText(text, root);
				foreach (VisualElement h in header)
				{
					if (!(h is Label l)) continue;
					l.AddToClassList("header");
					if (fontStyleOverride != FontStyle.Bold)
						l.style.unityFontStyleAndWeight = fontStyleOverride;
					if (fontSizeOverride > 0)
						l.style.fontSize = fontSizeOverride;
				}

				header.Add(AddSplitter());
			}

			return headerContainer;
		}

		#endregion

		#region Splitters

		/// <summary>
		/// Adds a splitter (a horizontal break line)
		/// </summary>
		/// <param name="inverseColor">Whether to inverse the colour scheme</param>
		/// <param name="root">Root, current default if left null</param>
		/// <returns>The splitter element</returns>
		public VisualElement AddSplitter(bool inverseColor = false, VisualElement root = null)
		{
			VisualElement halfSplitter = new VisualElement();
			halfSplitter.AddToClassList("splitter");
			if (inverseColor)
				halfSplitter.AddToClassList("inverse");
			content.AddToRoot(halfSplitter, root);
			return halfSplitter;
		}

		/// <summary>
		/// Adds blank vertical space.
		/// </summary>
		/// <param name="height">Height of the vertical space</param>
		/// <param name="root">Root, current default if left null</param>
		/// <returns>The vertical space element</returns>
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

		/// <summary>
		/// Sets the root used by Documentation content. DefaultRootScope is the safe way to set and reset content.
		/// </summary>
		/// <param name="root">Visual Element to append Documentation content to by default.</param>
		public void SetDefaultRoot(VisualElement root) => content.SetCurrentDefaultRoot(root);

		/// <summary>
		/// You can set the DefaultRoot using a DefaultRootScope or by calling SetDefaultRoot manually.
		/// </summary>
		/// <returns>The root VisualElement currently used by the Documentation content</returns>
		public VisualElement GetDefaultRoot() => content.GetRoot();

		public struct DefaultRootScope : IDisposable
		{
			private readonly VisualElement lastRoot;
			private readonly DocumentationWindow window;

			public DefaultRootScope(DocumentationWindow window, VisualElement rootOverride)
			{
				this.window = window;
				lastRoot = window.content.GetRoot();
				window.content.SetCurrentDefaultRoot(rootOverride);
			}

			public void Dispose() => window.content.SetCurrentDefaultRoot(lastRoot);
		}

		#endregion

		#region Navigation

		//Clear History
		public void AddItemsToMenu(GenericMenu menu)
		{
			if (content.HasHistory())
				menu.AddItem(new GUIContent("Clear History"), false, content.ClearHistory);
			else
				menu.AddDisabledItem(new GUIContent("Clear History"));
		}

		public void Home() => content.Home();

		public void GoToPage(Type pageType) => content.GoToPage(pageType.FullName);

		public void GoToPage(string pageType) => content.GoToPage(pageType);

		#endregion
	}
}