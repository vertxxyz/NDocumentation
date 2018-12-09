//#define VERBOSE_DEBUGGING
using System;
using System.Collections.Generic;
using UnityEngine;
using static Vertx.RichTextParser.RichTextTag;

namespace Vertx
{
	public static class RichTextParser
	{
		private static readonly string[] acceptedTags = {"b", "i", "color", "colour", "size", "button", "code", "span", "img"};
		private const char openingTagDelimiter = '<';
		private const char closingTagDelimiter = '>';
		private const char escapeCharacter = '\\';

		enum Discovery
		{
			Invalid,
			Valid,
			End
		}
		
		public static IEnumerable<RichText> ParseRichText(string richText)
		{
			List<RichText> resultantRichText = new List<RichText>();
			
			RichTextTag currentRichTextTag = new RichTextTag();
			Stack<RichTextTag> previousRichTextTags = new Stack<RichTextTag>();
			int currentIndex = 0;
			int length = richText.Length;
			int lastNewTag = 0;
			
			//Loop through the whole string
			while (currentIndex < length)
			{
				int currentSearchingIndex = currentIndex;
				//Looking to discover a valid starting delimiter
				int indexOfOpening = currentSearchingIndex;
				bool discoveredValidStartingDelimiter = false;
				while (!discoveredValidStartingDelimiter)
				{
					switch (DiscoverValidStartingDelimiter(out indexOfOpening))
					{
						case Discovery.Invalid:
							continue;
						case Discovery.Valid:
							break;
						case Discovery.End:
							Exit();
							return resultantRichText;
						default:
							throw new ArgumentOutOfRangeException();
					}
					currentSearchingIndex = indexOfOpening + 1;
				}
				
				//Now we have the index of a possible starting delimiter look for the ending delimiter.
				int indexOfClosing = richText.IndexOf(closingTagDelimiter, currentSearchingIndex);
				if (indexOfClosing < 0)
				{
					Exit();
					return resultantRichText;
				}
				//Make sure that that closing delimiter doesn't have an opening delimiter behind it.
				while (true)
				{
					switch (DiscoverValidStartingDelimiter(out int indexOfNextOpening, indexOfClosing - currentSearchingIndex))
					{
						case Discovery.Invalid:
							continue;
						case Discovery.Valid:
							Debug.LogError($"Text parsed by {nameof(RichTextParser)} has two un-escaped opening delimiters: \"{openingTagDelimiter}\" at {currentSearchingIndex} & {indexOfNextOpening}. {NotParsedError()}");
							Exit();
							return resultantRichText;
						case Discovery.End:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				}

				string resultantTag = richText.Substring(indexOfOpening, indexOfClosing-indexOfOpening);
				
				#if VERBOSE_DEBUGGING
				Debug.Log($"<color=green>{GetRichTextCapableText($"<{resultantTag}>")}</color>");
				#endif

				if (currentRichTextTag.tag == Tag.code)
				{
					//When inside the a code tag we ignore all tags except the closing of a code tag.
					if (resultantTag.Equals("/code"))
					{
						//Once closing the code tag we should 
						CsharpHighlighter highlighter = new CsharpHighlighter();
						string highlit = highlighter.Highlight(richText.Substring(lastNewTag, indexOfOpening-lastNewTag));
						Debug.Log($"Highlit: \"{highlit}\"");
						//TODO finish code implementation
					}
					else
					{
						//Continue parsing, looking for that closing code tag.
						currentIndex += 1;
						continue;
					}
				}
				else
				{
					//Switch through tags that are entire strings
					switch (resultantTag)
					{
						case "/":
							AddLastTextWithRichTextTag(currentRichTextTag);
							RemoveTag(true);
							break;
						case "b":
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = currentRichTextTag.GetWithAddedBold();
							AddTag();
							break;
						case "/b":
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = currentRichTextTag.GetWithRemovedBold();
							RemoveTag();
							break;
						case "i":
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = currentRichTextTag.GetWithAddedItalics();
							AddTag();
							break;
						case "/i":
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = currentRichTextTag.GetWithRemovedItalics();
							RemoveTag();
							break;
						case "code":
							if (!currentRichTextTag.isDefault)
							{
								Debug.LogError($"Rich Text entered a Code tag without closing prior tags. This is not allowed. {NotParsedError()}");
								RichTextDebug();
								Exit();
								return resultantRichText;
							}
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = new RichTextTag(Tag.code, FontStyle.Normal, Color.clear, 0, null);
							ClearTags();
							break;
						case "/code":
							if (currentRichTextTag.tag != Tag.code)
							{
								Debug.LogError($"Code tag was exited without being in a code block. {NotParsedError()}");
								RichTextDebugHighlit(indexOfOpening, indexOfClosing);
								Exit();
								return resultantRichText;
							}
							break;
					}

					//SIZE
					if (resultantTag.StartsWith("size"))
					{
						if (!GetStringVariables("size", out string stringVariables))
							continue;
						if (!int.TryParse(stringVariables, out int size))
						{
							Debug.Log($"Size tag \"{resultantTag}\" does not contain a parseable integer.");
							RichTextDebugHighlit(indexOfOpening, indexOfClosing);
							continue;
						}
						AddLastTextWithRichTextTag(currentRichTextTag);
						currentRichTextTag = currentRichTextTag.GetWithNewSize(size);
						AddTag();
					}

					bool GetStringVariables(string tag, out string stringVariables)
					{
						stringVariables = null;
						int indexOfEquals = resultantTag.IndexOf('=', tag.Length);
						if (indexOfEquals < 0)
						{
							Debug.Log($"{tag} tag \"{resultantTag}\" does not contain an = and variables.");
							RichTextDebugHighlit(indexOfOpening, indexOfClosing);
							return false;
						}

						stringVariables = resultantTag.Substring(indexOfEquals).Replace(" ", string.Empty);
						return true;
					}
				}

				void RemoveTag (bool assignTag = false)
				{
					if(assignTag)
						currentRichTextTag = previousRichTextTags.Pop();
					else
						previousRichTextTags.Pop();
				}
				void AddTag () => previousRichTextTags.Push(currentRichTextTag);
				void ClearTags() => previousRichTextTags.Clear();

				void AddLastTextWithRichTextTag (RichTextTag tag)
				{
					//Don't add if no text content to add.
					if (lastNewTag == indexOfOpening - 1)
						return;
					string text = richText.Substring(lastNewTag, (indexOfOpening  - 1) - lastNewTag);

					#if VERBOSE_DEBUGGING
					Debug.Log($"Added Text: \"{text}\".");
					#endif
					
					resultantRichText.Add(new RichText(tag, text));
					previousRichTextTags.Push(tag);
				}
				
				currentIndex = indexOfClosing + 1;
				lastNewTag = currentIndex;


				Discovery DiscoverValidStartingDelimiter(out int searchIndex, int count = -1)
				{
					searchIndex = currentSearchingIndex;
					int indexOfOpeningDelimiter = count < 0 ? richText.IndexOf(openingTagDelimiter, currentSearchingIndex) : richText.IndexOf(openingTagDelimiter, currentSearchingIndex, count);
					if (indexOfOpeningDelimiter < 0)
						return Discovery.End;

					if (indexOfOpeningDelimiter - 1 > 0)
					{
						//if the text behind the opening tag is not the escape character then the tag must be valid.
						if (!richText[indexOfOpeningDelimiter - 1].Equals(escapeCharacter))
							discoveredValidStartingDelimiter = true;
					}
					else
					{
						//if there is no characters behind the opening tag it must be valid.
						discoveredValidStartingDelimiter = true;
					}

					searchIndex = indexOfOpeningDelimiter + 1;
					return discoveredValidStartingDelimiter ? Discovery.Valid : Discovery.Invalid;
				}

				string NotParsedError() => "This text has not been parsed beyond this point.";
				void RichTextDebug () => Debug.Log(richText);
				void RichTextDebugHighlit(int highlightStart, int highlightEnd) => Debug.LogWarning($"{GetRichTextCapableText(richText.Substring(0, highlightStart))}<color=red>{GetRichTextCapableText(richText.Substring(highlightStart, highlightEnd - highlightStart))}</color>{GetRichTextCapableText(richText.Substring(highlightEnd))}");
				string GetRichTextCapableText(string text) => text.Replace("<", "<<b></b>");
			}

			
			Exit();
			return resultantRichText;
			
			void Exit() => resultantRichText.Add(new RichText(currentRichTextTag, richText.Substring(currentIndex)));
		}
		
		public struct RichText
		{
			public readonly RichTextTag richTextTag;
			public readonly string associatedText;
			public RichText(RichTextTag richTextTag, string associatedText)
			{
				this.richTextTag = richTextTag;
				this.associatedText = associatedText;
			}
		}
		
		public struct RichTextTag : IEqualityComparer<RichTextTag>
		{
			public enum Tag
			{
				none,
				button,
				code,
				span,
				image
			}
			public readonly Tag tag;
			
			/// <summary>
			/// Font Style (Normal, Bold, Bold-Italic, etc)
			/// </summary>
			public readonly FontStyle fontStyle;
			
			/// <summary>
			/// The colour of the text
			/// </summary>
			public readonly Color color;
			
			/// <summary>
			/// Font Size.
			/// </summary>
			public readonly int size;
			
			/// <summary>
			/// Variables provided with things like buttons.
			/// </summary>
			public readonly string stringVariables;

			private readonly bool _isDefault;
			public bool isDefault => _isDefault;

			public RichTextTag(Tag tag, FontStyle fontStyle, Color color, int size, string stringVariables)
			{
				this.tag = tag;
				this.fontStyle = fontStyle;
				this.color = color;
				this.size = size;
				this.stringVariables = stringVariables;
				_isDefault = false;
			}

			public RichTextTag GetWithAddedBold() => new RichTextTag(tag, fontStyle == FontStyle.Normal ? FontStyle.Bold : FontStyle.BoldAndItalic, color, size, stringVariables);
			public RichTextTag GetWithRemovedBold() => new RichTextTag(tag, fontStyle == FontStyle.Bold ? FontStyle.Normal : FontStyle.Italic, color, size, stringVariables);
			public RichTextTag GetWithAddedItalics() => new RichTextTag(tag, fontStyle == FontStyle.Normal ? FontStyle.Italic : FontStyle.BoldAndItalic, color, size, stringVariables);
			public RichTextTag GetWithRemovedItalics() => new RichTextTag(tag, fontStyle == FontStyle.Bold ? FontStyle.Normal : FontStyle.Bold, color, size, stringVariables);
			public RichTextTag GetWithNewSize (int size) => new RichTextTag(tag, fontStyle, color, size, stringVariables);

			public bool Equals(RichTextTag a, RichTextTag b) =>
				a.color == b.color &&
				a.fontStyle == b.fontStyle &&
				a.size == b.size;

			public int GetHashCode(RichTextTag obj) => obj.GetHashCode();
		}
	}
}