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

			RichTextTag currentRichTextTag = default;
			Stack<RichTextTag> previousRichTextTags = new Stack<RichTextTag>();
			int currentIndex = 0;
			int length = richText.Length;
			int lastNewTag = 0;

			//Loop through the whole string
			while (currentIndex < length)
			{
				int currentSearchingIndex = currentIndex;
				//Looking to discover a valid starting delimiter
				//indexOfOpening is the index of the character after the opening <
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
				
				//Get the location of the starting delimiter if it exists.
				//search index returned is the index after the start delimiter.
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

				string resultantTag = richText.Substring(indexOfOpening, indexOfClosing - indexOfOpening);

				#if VERBOSE_DEBUGGING
				Debug.Log($"<color=green>{GetRichTextCapableText($"<{resultantTag}>")}</color>");
				#endif
				bool successfullyParsedTag = true;

				if (currentRichTextTag.tag == Tag.code)
				{
					//When inside the a code tag we ignore all tags except the closing of a code tag.
					if (resultantTag.Equals("/code"))
					{
						AddLastTextWithRichTextTag(currentRichTextTag, false);
						currentRichTextTag = default;
						ClearTags();
					}
					else
					{
						//Continue parsing, looking for that closing code tag.
						currentIndex = indexOfOpening;
						continue;
					}
				}
				else
				{
					//Switch through tags that are entire strings
					switch (resultantTag)
					{
						case "/": //Exit Tag
							AddLastTextWithRichTextTag(currentRichTextTag, false);
							RemoveTag(true);
							break;
						case "b": //Start Bold
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = currentRichTextTag.GetWithAddedBold();
							break;
						case "/b": //End Bold
							AddLastTextWithRichTextTag(currentRichTextTag, false);
							currentRichTextTag = currentRichTextTag.GetWithRemovedBold();
							RemoveTag();
							break;
						case "i": //Start Italics
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = currentRichTextTag.GetWithAddedItalics();
							break;
						case "/i": //End Italics
							AddLastTextWithRichTextTag(currentRichTextTag, false);
							currentRichTextTag = currentRichTextTag.GetWithRemovedItalics();
							RemoveTag();
							break;
						case "code": //Start Code
							if (!currentRichTextTag.Equals(default(RichTextTag)))
							{
								Debug.LogError($"Rich Text entered a Code tag without closing prior tags. This is not allowed. {NotParsedError()}");
								RichTextDebug();
								Exit();
								return resultantRichText;
							}

							AddLastTextWithRichTextTag(currentRichTextTag, false);
							currentRichTextTag = new RichTextTag(Tag.code, FontStyle.Normal, Color.clear, 0, null);
							ClearTags();
							break;
						case "/code": //End Code but if not already in a code block.
							if (currentRichTextTag.tag != Tag.code)
							{
								Debug.LogError($"Code tag was exited without being in a code block. {NotParsedError()}");
								RichTextDebugHighlit(indexOfOpening, indexOfClosing);
								Exit();
								return resultantRichText;
							}

							break;
						case "/button": //End Button
							AddLastTextWithRichTextTag(currentRichTextTag, false);
							currentRichTextTag = currentRichTextTag.GetWithRemovedButton();
							RemoveTag();
							break;
						case "/color": //End Colour
							AddLastTextWithRichTextTag(currentRichTextTag, false);
							currentRichTextTag = currentRichTextTag.GetWithRemovedColor();
							RemoveTag();
							break;
						case "/span": //End Span
							AddLastTextWithRichTextTag(currentRichTextTag, false);
							currentRichTextTag = currentRichTextTag.GetWithRemovedSpan();
							RemoveTag();
							break;
						default:
							successfullyParsedTag = false;
							break;
					}

					//StartsWith cannot be in the above switch.
					if (!successfullyParsedTag)
					{
						successfullyParsedTag = true;
						if (resultantTag.StartsWith("size")) //START SIZE
						{
							if (!GetStringVariables("size", out string stringVariables))
								continue;
							if (!int.TryParse(stringVariables, out int size))
							{
								Debug.Log($"Size tag \"{resultantTag}\" does not contain a parseable integer. \"{stringVariables}\"");
								RichTextDebugHighlit(indexOfOpening, indexOfClosing);
							}
							else
							{
								AddLastTextWithRichTextTag(currentRichTextTag);
								currentRichTextTag = currentRichTextTag.GetWithNewSize(size);
							}
						}
						else if (resultantTag.StartsWith("button")) //START BUTTON
						{
							if (!GetStringVariables("button", out string stringVariables))
								continue;
							if (string.IsNullOrEmpty(stringVariables))
							{
								Debug.Log($"Button tag \"{resultantTag}\" does not contain a key. \"{stringVariables}\"");
								RichTextDebugHighlit(indexOfOpening, indexOfClosing);
							}
							else
							{
								AddLastTextWithRichTextTag(currentRichTextTag);
								currentRichTextTag = currentRichTextTag.GetWithButton(stringVariables);
							}
						}
						else if (resultantTag.StartsWith("color")) //START COLOR
						{
							if (!GetStringVariables("color", out string stringVariables))
								continue;
							if (!ColorUtility.TryParseHtmlString(stringVariables, out Color colour))
							{
								Debug.Log($"Color tag \"{resultantTag}\" does not contain a HTML colour. \"{stringVariables}\"");
								RichTextDebugHighlit(indexOfOpening, indexOfClosing);
							}
							else
							{
								AddLastTextWithRichTextTag(currentRichTextTag);
								currentRichTextTag = currentRichTextTag.GetWithNewColor(colour);
							}
						}
						else if (ParseForSpan(resultantTag, out string styleClass))
						{
							AddLastTextWithRichTextTag(currentRichTextTag);
							currentRichTextTag = currentRichTextTag.GetWithSpan(styleClass);
						}
						else
						{
							successfullyParsedTag = false;
						}
					}

					//Gets the plain string variables following a tag.
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

						stringVariables = resultantTag.Substring(indexOfEquals + 1).Replace(" ", string.Empty);
						return true;
					}
				}

				void RemoveTag(bool assignTag = false)
				{
					if (previousRichTextTags.Count == 0)
					{
						Debug.LogError($"No Tags to Pop! Last added text: {(resultantRichText.Count > 0 ? $"{resultantRichText[resultantRichText.Count-1].richTextTag.ToString()} | {resultantRichText[resultantRichText.Count-1].associatedText}"  : "none")}");
						return;
					}
					
					if (assignTag)
						currentRichTextTag = previousRichTextTags.Pop();
					else
						previousRichTextTags.Pop();
				}

				void AddTag() => previousRichTextTags.Push(currentRichTextTag);

				void ClearTags() => previousRichTextTags.Clear();

				void AddLastTextWithRichTextTag(RichTextTag tag, bool addTag = true)
				{
					if (addTag) AddTag();

					//Don't add if no text content to add.
					if (lastNewTag == indexOfOpening - 1)
						return;
					
					string text = richText.Substring(lastNewTag, (indexOfOpening - 1) - lastNewTag);
					#if VERBOSE_DEBUGGING
					Debug.Log($"Added Text: \"{text}\".");
					#endif

					resultantRichText.Add(new RichText(tag, text));
				}

				if (successfullyParsedTag)
				{
					currentIndex = indexOfClosing + 1;
					lastNewTag = currentIndex;
				}
				else
				{
					currentIndex = indexOfOpening;
				}

				string NotParsedError() => "This text has not been parsed beyond this point.";
				void RichTextDebug() => Debug.Log(richText);
				void RichTextDebugHighlit(int highlightStart, int highlightEnd) => Debug.LogWarning($"{GetRichTextCapableText(richText.Substring(0, highlightStart))}<color=red>{GetRichTextCapableText(richText.Substring(highlightStart, highlightEnd - highlightStart))}</color>{GetRichTextCapableText(richText.Substring(highlightEnd))}");
				string GetRichTextCapableText(string text) => text.Replace("<", "<<b></b>");
			}


			Exit();
			return resultantRichText;

			void Exit() => resultantRichText.Add(new RichText(currentRichTextTag, richText.Substring(currentIndex)));
		}

		static bool ParseForSpan(string resultantTag, out string styleClass)
		{
			if (resultantTag.StartsWith("span "))
			{
				int indexOfClass = resultantTag.IndexOf("class=", "span ".Length, StringComparison.Ordinal);
				if (indexOfClass >= 0)
					indexOfClass += "class=".Length;
				else
				{
					indexOfClass = resultantTag.IndexOf("class =", "span ".Length, StringComparison.Ordinal);
					if (indexOfClass >= 0)
						indexOfClass += "class =".Length;
				}
						
				if (indexOfClass < 0)
				{
					//no class specified
				}
				else
				{
					string restOfTag = resultantTag.Substring(indexOfClass+1, (resultantTag.Length-(indexOfClass+1)) -1);
					styleClass = restOfTag;
					return true;
				}
			}

			styleClass = null;
			return false;
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

			public RichTextTag(Tag tag, FontStyle fontStyle, Color color, int size, string stringVariables)
			{
				this.tag = tag;
				this.fontStyle = fontStyle;
				this.color = color;
				this.size = size;
				this.stringVariables = stringVariables;
			}

			public RichTextTag GetWithAddedBold() => new RichTextTag(tag, fontStyle == FontStyle.Normal ? FontStyle.Bold : FontStyle.BoldAndItalic, color, size, stringVariables);
			public RichTextTag GetWithRemovedBold() => new RichTextTag(tag, fontStyle == FontStyle.BoldAndItalic ? FontStyle.Italic : FontStyle.Normal, color, size, stringVariables);
			public RichTextTag GetWithAddedItalics() => new RichTextTag(tag, fontStyle == FontStyle.Normal ? FontStyle.Italic : FontStyle.BoldAndItalic, color, size, stringVariables);
			public RichTextTag GetWithRemovedItalics() => new RichTextTag(tag, fontStyle == FontStyle.BoldAndItalic ? FontStyle.Bold : FontStyle.Normal, color, size, stringVariables);
			public RichTextTag GetWithNewSize(int size) => new RichTextTag(tag, fontStyle, color, size, stringVariables);
			public RichTextTag GetWithNewColor(Color color) => new RichTextTag(tag, fontStyle, color, size, stringVariables);
			public RichTextTag GetWithRemovedColor() => new RichTextTag(tag, fontStyle, Color.clear, size, stringVariables);
			public RichTextTag GetWithButton(string stringVariables) => new RichTextTag(Tag.button, fontStyle, color, size, stringVariables);
			public RichTextTag GetWithRemovedButton() => new RichTextTag(Tag.none, fontStyle, color, size, null);
			public RichTextTag GetWithSpan(string stringVariables) => new RichTextTag(Tag.span, fontStyle, color, size, stringVariables);
			public RichTextTag GetWithRemovedSpan() => new RichTextTag(Tag.none, fontStyle, color, size, null);

			public bool Equals(RichTextTag a, RichTextTag b) =>
				a.tag == b.tag &&
				a.color == b.color &&
				a.fontStyle == b.fontStyle &&
				a.size == b.size;

			public int GetHashCode(RichTextTag obj) => obj.GetHashCode();

			public override string ToString() => $"Tag : {tag}; FontStyle : {fontStyle}; Color : {color}; Size : {size}; String Variables : {stringVariables};";
		}
	}
}