using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.RichTextParser;

namespace Vertx
{
	public static class RichTextUtility
	{
		public static string GetButtonString(Type type, string content) => $"<button={type.FullName}>{content}</button>";
		public static string GetButtonString(string key, string content) => $"<button={key}>{content}</button>";
		public static string GetColouredString(string content, Color colour) => $"<color=#{ColorUtility.ToHtmlStringRGBA(colour)}>{content}</color>";
		public static string GetColoredString(string content, Color color) => GetColouredString(content, color);
		public static string GetBoldItalicsString(string content) => $"<i><b>{content}</b></i>";

		public static List<VisualElement> AddRichText(string text, IButtonRegistry buttonRegistry, VisualElement root)
		{
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
				VisualElement rootTemp = root;
				root = AddParagraphContainer(root);
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
							VisualElement inlineGroup = new VisualElement
							{
								#if IGNORE_PICKING
								pickingMode = PickingMode.Ignore
								#endif
							};
							root.Add(inlineGroup);
							inlineGroup.AddToClassList("inline-text-group");
							AddRichTextInternal(word, inlineGroup);
							AddRichTextInternal(nextWord, inlineGroup);
							++i;
							continue;
						}
					}

					AddRichTextInternal(word, root);

					//Add all the words and style them.
					void AddRichTextInternal(RichText richText, VisualElement rootToAddTo)
					{
						RichTextTag tag = richText.richTextTag;
						TextElement inlineText = null;
						switch (tag.tag)
						{
							case RichTextTag.Tag.none:
								inlineText = AddInlineText(richText.associatedText, rootToAddTo);
								break;
							case RichTextTag.Tag.button:
								if (!buttonRegistry.GetRegisteredButtonAction(tag.stringVariables, out Action action))
									return;
								inlineText = AddInlineButton(action, richText.associatedText, rootToAddTo);
								break;
							case RichTextTag.Tag.code:
								//Button
								Button codeCopyButtonButtonContainer = new Button(() =>
								{
									EditorGUIUtility.systemCopyBuffer = richText.associatedText;
									Debug.Log("Copied Code to Clipboard");
								});
								codeCopyButtonButtonContainer.ClearClassList();
								codeCopyButtonButtonContainer.AddToClassList("code-button-container");
								root.Add(codeCopyButtonButtonContainer);
								//Scroll
								ScrollView codeScroll = new ScrollView(ScrollViewMode.Horizontal)
								{
									#if IGNORE_PICKING
									pickingMode = PickingMode.Ignore
									#endif
								};
								VisualElement contentContainer = codeScroll.contentContainer;
								#if IGNORE_PICKING
								codeScroll.contentViewport.pickingMode = PickingMode.Ignore;
								contentContainer.pickingMode = PickingMode.Ignore;
								#endif
								codeScroll.AddToClassList("code-scroll");
								codeCopyButtonButtonContainer.Add(codeScroll);

								VisualElement codeContainer = new VisualElement
								{
									#if IGNORE_PICKING
									pickingMode = PickingMode.Ignore
									#endif
								};
								codeContainer.ClearClassList();
								codeContainer.AddToClassList("code-container");
								contentContainer.Add(codeContainer);

								//Once closing the code tag we should 
								CsharpHighlighter highlighter = new CsharpHighlighter
								{
									AddStyleDefinition = false
								};
								string highlit = highlighter.Highlight(richText.associatedText);
//								Debug.Log(highlit);
								//Code
								AddRichText(highlit, buttonRegistry, codeContainer);
								//Finalise content container
								foreach (VisualElement child in codeContainer.Children())
								{
									if (child.ClassListContains(paragraphContainerClass))
										child.AddToClassList("code");
								}

								contentContainer.Query<Label>().Build().ForEach(l => l.AddToClassList("code"));

								break;
							case RichTextTag.Tag.span:
								Label spanLabel = new Label
								{
									text = richText.associatedText,
									#if IGNORE_PICKING
									pickingMode = PickingMode.Ignore
									#endif
								};
								spanLabel.AddToClassList(tag.stringVariables);
								rootToAddTo.Add(spanLabel);
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

				root = rootTemp;
			}

			return results;
		}
		
		

		#region Inline
		
		public static Label AddInlineText(string text, VisualElement root)
		{
			Label inlineText = new Label
			{
				text = text,
				#if IGNORE_PICKING
				pickingMode = PickingMode.Ignore
				#endif
			};
			inlineText.AddToClassList("inline-text");
			root.Add(inlineText);
			return inlineText;
		}
		
		public static Button AddInlineButton(Action action, string text, VisualElement root)
		{
			Button inlineButton = new Button(action)
			{
				text = text
			};
			inlineButton.ClearClassList();
			inlineButton.AddToClassList("inline-button");
			root.Add(inlineButton);
			return inlineButton;
		}

		#endregion

		#region Containers

		private const string paragraphContainerClass = "paragraph-container";

		private static VisualElement AddParagraphContainer(VisualElement root)
		{
			VisualElement paragraphContainer = new VisualElement
			{
				#if IGNORE_PICKING
				pickingMode = PickingMode.Ignore
				#endif
			};
			paragraphContainer.AddToClassList(paragraphContainerClass);
			root.Add(paragraphContainer);
			return paragraphContainer;
		}

		#endregion
	}
}