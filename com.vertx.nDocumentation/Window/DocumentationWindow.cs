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
				pickingMode = PickingMode.Ignore
			};
			plainText.AddToClassList("plain-text");
			content.AddToRoot(plainText, root);
			return plainText;
		}

		public Label AddInlineText(string text, VisualElement root = null)
		{
			Label inlineText = new Label
			{
				text = text,
				pickingMode = PickingMode.Ignore
			};
			inlineText.AddToClassList("inline-text");
			content.AddToRoot(inlineText, root);
			return inlineText;
		}

		public List<VisualElement> AddRichText(string text, VisualElement root = null)
		{
			VisualElement rootTemp = content.GetRoot(null);
			List<VisualElement> results = new List<VisualElement>();
			IEnumerable<RichText> richTexts = ParseRichText(text);
			//Parse rich texts to create paragraphs.
			List<List<RichText>> paragraphs = new List<List<RichText>> {new List<RichText>()};
			foreach (RichText richText in richTexts)
			{
				if (richText.richTextTag.tag == RichTextTag.Tag.button || richText.richTextTag.tag == RichTextTag.Tag.code)
				{
					paragraphs[paragraphs.Count - 1].Add(richText);
					continue;
				}

				string[] strings = richText.associatedText.Split('\n');
				for (int i = 0; i < strings.Length; i++)
				{
					if (i != 0)
						paragraphs.Add(new List<RichText>());
					//Split paragraph content (already split by tag) into individual words 
					string[] wordSplit = Regex.Split(strings[i], @"(?<=[ -])"); //Split but keep delimiters attached.
					foreach (var word in wordSplit)
					{
						if (!string.IsNullOrEmpty(word))
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
							VisualElement inlineGroup = new VisualElement
							{
								pickingMode = PickingMode.Ignore
							};
							content.AddToRoot(inlineGroup, lastRoot);
							inlineGroup.AddToClassList("inline-text-group");
							content.SetCurrentDefaultRoot(inlineGroup);
							AddRichTextInternal(word);
							AddRichTextInternal(nextWord);
							content.SetCurrentDefaultRoot(lastRoot);
							++i;
							continue;
						}
					}

					AddRichTextInternal(word);

					//Add all the words and style them.
					void AddRichTextInternal(RichText richText)
					{
						RichTextTag tag = richText.richTextTag;
						TextElement inlineText = null;
						switch (tag.tag)
						{
							case RichTextTag.Tag.none:
								inlineText = AddInlineText(richText.associatedText, root);
								break;
							case RichTextTag.Tag.button:
								inlineText = AddInlineButton(tag.stringVariables, richText.associatedText, root);
								break;
							case RichTextTag.Tag.code:
								VisualElement lastRoot = content.GetRoot(null);
								//Button
								Button codeCopyButtonButtonContainer = new Button(() =>
								{
									EditorGUIUtility.systemCopyBuffer = richText.associatedText;
									Debug.Log("Copied Code to Clipboard");
								});
								codeCopyButtonButtonContainer.ClearClassList();
								codeCopyButtonButtonContainer.AddToClassList("code-button-container");
								content.AddToRoot(codeCopyButtonButtonContainer);
								content.SetCurrentDefaultRoot(codeCopyButtonButtonContainer);
								//Scroll
								ScrollView codeScroll = new ScrollView(ScrollViewMode.Horizontal)
								{
									pickingMode = PickingMode.Ignore
								};
								codeScroll.contentViewport.pickingMode = PickingMode.Ignore;
								VisualElement contentContainer = codeScroll.contentContainer;
								contentContainer.pickingMode = PickingMode.Ignore;
								codeScroll.AddToClassList("code-scroll");
								content.AddToRoot(codeScroll);
								content.SetCurrentDefaultRoot(contentContainer);
								
								VisualElement codeContainer = new VisualElement
								{
									pickingMode = PickingMode.Ignore
								};
								codeContainer.ClearClassList();
								codeContainer.AddToClassList("code-container");
								content.AddToRoot(codeContainer);
								content.SetCurrentDefaultRoot(codeContainer);

								//Once closing the code tag we should 
								CsharpHighlighter highlighter = new CsharpHighlighter
								{
									AddStyleDefinition = false
								};
								string highlit = highlighter.Highlight(richText.associatedText);
//								Debug.Log(highlit);
								//Code
								AddRichText(highlit, root);
								//Finalise content container
								foreach (VisualElement child in codeContainer.Children())
								{
									if (child.ClassListContains(paragraphContainerClass))
										child.AddToClassList("code");
								}

								contentContainer.Query<Label>().Build().ForEach(l => l.AddToClassList("code"));

								//Reset
								content.SetCurrentDefaultRoot(lastRoot);
								break;
							case RichTextTag.Tag.span:
								Label spanLabel = new Label
								{
									text = richText.associatedText,
									pickingMode = PickingMode.Ignore
								};
								spanLabel.AddToClassList(tag.stringVariables);
								content.AddToRoot(spanLabel, root);
								break;
							case RichTextTag.Tag.image:
								throw new NotImplementedException();
							default:
								throw new ArgumentOutOfRangeException();
						}

						if (inlineText != null)
						{
							inlineText.style.unityFontStyleAndWeight = tag.fontStyle;
							if (tag.size > 0)
								inlineText.style.fontSize = tag.size;
							if (tag.color != Color.clear)
								inlineText.style.color = tag.color;
							results.Add(inlineText);
						}
					}
				}

				content.SetCurrentDefaultRoot(rootTemp);
			}

			content.SetCurrentDefaultRoot(rootTemp);
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

		public Button AddInlineButton(string key, string text, VisualElement root = null)
		{
			if (!content.GetRegisteredButtonAction(key, out Action action))
				return null;
			Button inlineButton = new Button(action)
			{
				text = text
			};
			inlineButton.ClearClassList();
			inlineButton.AddToClassList("inline-button");
			content.AddToRoot(inlineButton, root);
			return inlineButton;
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

		#region Containers

		private const string paragraphContainerClass = "paragraph-container";

		private VisualElement AddParagraphContainer(VisualElement root = null)
		{
			VisualElement paragraphContainer = new VisualElement
			{
				pickingMode = PickingMode.Ignore
			};
			paragraphContainer.AddToClassList(paragraphContainerClass);
			content.AddToRoot(paragraphContainer, root);
			return paragraphContainer;
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