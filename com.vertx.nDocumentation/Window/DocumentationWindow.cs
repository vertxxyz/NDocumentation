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

		#endregion

		#region Text

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
		
		public Label AddInlineText(string text, VisualElement root = null)
		{
			Label inlineText = new Label
			{
				text = text
			};
			inlineText.AddToClassList("inline-text");
			content.AddToRoot(inlineText, root);
			return inlineText;
		}

		public List<VisualElement> AddRichText(string text, VisualElement root = null)
		{
			List<VisualElement> results = new List<VisualElement>();
			IEnumerable<RichText> richTexts = ParseRichText(text);
			//Parse rich texts to create paragraphs.
			List<List<RichText>> paragraphs = new List<List<RichText>> {new List<RichText>()};
			foreach (RichText richText in richTexts)
			{
				string[] strings = richText.associatedText.Split('\n');
				for (int i = 0; i < strings.Length; i++)
				{
					if (i != 0)
						paragraphs.Add(new List<RichText>());
					//Split paragraph content (already split by tag) into individual words 
					string[] wordSplit = Regex.Split(strings[i], @"(?<=[ -])"); //Split but keep delimiters attached.
					foreach (var word in wordSplit)
					{
						if(!string.IsNullOrEmpty(word))
							paragraphs[paragraphs.Count - 1].Add(new RichText(richText.richTextTag, word));
					}
				}
			}
			
			foreach (List<RichText> paragraph in paragraphs)
			{
				//Add all the paragraphs
				content.SetCurrentDefaultRoot(AddParagraphContainer(root));
				for (int i = 0; i < paragraph.Count; i++)
				{
					RichText word = paragraph[i];

					if (i < paragraph.Count - 1)
					{
						//If there are more words 
						RichText nextWord = paragraph[i + 1];
						string nextText = nextWord.associatedText;
						if (Regex.IsMatch(nextText, "^[^a-zA-Z] ?"))
						{
							VisualElement lastRoot = content.GetRoot(null);
							VisualElement inlineGroup = new VisualElement();
							content.AddToRoot(inlineGroup, lastRoot);
							inlineGroup.AddToClassList("inline-text-group");
							content.SetCurrentDefaultRoot(inlineGroup);
							AddRichText(word);
							AddRichText(nextWord);
							content.SetCurrentDefaultRoot(lastRoot);
							++i;
							continue;
						}
					}
					AddRichText(word);

					//Add all the words and style them.
					//TODO ----------------------------------------------------------------------------------


					void AddRichText(RichText richText)
					{
						Label inlineText = AddInlineText(richText.associatedText);
						RichTextTag tag = richText.richTextTag;
						inlineText.style.unityFontStyleAndWeight = tag.fontStyle;
						if(tag.color != Color.clear)
							inlineText.style.color = tag.color;
						results.Add(inlineText);
					}
					//TODO ----------------------------------------------------------------------------------
				}
			}

			return results;
		}
		
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

		#region Buttons

		public Button AddInlineButton(string key, VisualElement root)
		{
			if (!content.GetRegisteredButtonAction(key, out Action action))
				return null;
			Button inlineButton = new Button(action);
			inlineButton.AddToClassList("inline-button");
			content.AddToRoot(inlineButton);
			return inlineButton;
		}

		#endregion

		#region Splitters

		public VisualElement AddSplitter(bool halfSize = false, bool inverseColor = false, VisualElement root = null)
		{
			VisualElement halfSplitter = new VisualElement();
			halfSplitter.AddToClassList("splitter");
			if(halfSize)
				halfSplitter.AddToClassList("half");
			if (inverseColor)
				halfSplitter.AddToClassList("inverse");
			content.AddToRoot(halfSplitter, root);
			return halfSplitter;
		}

		#endregion

		#region Containers

		private VisualElement AddParagraphContainer(VisualElement root = null)
		{
			VisualElement paragraphContainer = new VisualElement();
			paragraphContainer.AddToClassList("paragraph-container");
			content.AddToRoot(paragraphContainer, root);
			return paragraphContainer;
		}

		#endregion

		#endregion
	}
}