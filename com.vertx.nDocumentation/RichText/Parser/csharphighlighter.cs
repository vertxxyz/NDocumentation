using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Vertx
{
	public class CsharpHighlighter
	{
		private string _commentCssClass;
		private string _keywordCssClass;
		private string _quotesCssClass;
		private string _typeCssClass;
		private bool _addStyleDefinition;
		private readonly HashSet<string> _keywords;
		private bool _addPreTags;

		/// <summary>
		/// Gets the list of reserved words/keywords.
		/// </summary>
		public HashSet<string> Keywords => _keywords;

		/// <summary>
		/// Gets or sets the CSS class used for comments. The default is 'comment'.
		/// </summary>
		public string CommentCssClass
		{
			get => _commentCssClass;
			set => _commentCssClass = value;
		}

		/// <summary>
		/// Gets or sets the CSS class used for keywords. The default is 'keyword'.
		/// </summary>
		public string KeywordCssClass
		{
			get => _keywordCssClass;
			set => _keywordCssClass = value;
		}

		/// <summary>
		/// Gets or sets the CSS class used for string quotes. The default is 'quotes'.
		/// </summary>
		public string QuotesCssClass
		{
			get => _quotesCssClass;
			set => _quotesCssClass = value;
		}

		/// <summary>
		/// Gets or sets the CSS class used for types. The default is 'type'.
		/// </summary>
		public string TypeCssClass
		{
			get => _typeCssClass;
			set => _typeCssClass = value;
		}

		/// <summary>
		/// Whether to add the CSS style definition to the top of the highlighted code.
		/// </summary>
		public bool AddStyleDefinition
		{
			get => _addStyleDefinition;
			set => _addStyleDefinition = value;
		}

		/// <summary>
		/// Whether to insert opening and closing pre tags around the highlighted code.
		/// </summary>
		public bool AddPreTags
		{
			get => _addPreTags;
			set => _addPreTags = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsharpHighlighter"/> class.
		/// </summary>
		public CsharpHighlighter()
		{
			_addStyleDefinition = true;
			_commentCssClass = "comment";
			_keywordCssClass = "keyword";
			_quotesCssClass = "quotes";
			_typeCssClass = "type";
			_keywords = new HashSet<string>()
			{
				"static", "using", "true", "false", "new",
				"namespace", "void", "private", "public", "protected", "override", "virtual",
				"bool", "string", "return", "class", "internal",
				"const", "readonly", "int", "double", "lock",
				"float", "if", "else", "foreach", "for", "var",
				"get", "set", "byte\\[\\]", "char\\[\\]", "int\\[\\]", "string\\[\\]" // dumb array matching. Escaped as [] is regex syntax
			};
		}

		/// <summary>
		/// Highlights the specified source code and returns it as stylised HTML.
		/// </summary>
		/// <param name="source">The source code.</param>
		/// <returns></returns>
		public string Highlight(string source)
		{
			StringBuilder builder = new StringBuilder();
			if (AddStyleDefinition)
			{
				builder.Append("<style>");
				builder.AppendFormat(".{0}  {{ color: #569CD6  }} ", KeywordCssClass);
				builder.AppendFormat(".{0}  {{ color: #4EC9B0  }} ", TypeCssClass);
				builder.AppendFormat(".{0}  {{ color: #57A64A  }} ", CommentCssClass);
				builder.AppendFormat(".{0}  {{ color: #D69D85  }} ", QuotesCssClass);
				builder.Append("</style>");
			}

			if (AddPreTags)
				builder.Append("<pre>");

			builder.Append(HighlightSource(source));

			if (AddPreTags)
				builder.Append("</pre>");

			return builder.ToString();
		}

		/// <summary>
		/// Occurs when the source code is highlighted, after any style (CSS) definitions are added.
		/// </summary>
		/// <param name="content">The source code content.</param>
		/// <returns>The highlighted source code.</returns>
		protected virtual string HighlightSource(string content)
		{
			if (string.IsNullOrEmpty(CommentCssClass))
				throw new InvalidOperationException("The CommentCssClass should not be null or empty");
			if (string.IsNullOrEmpty(KeywordCssClass))
				throw new InvalidOperationException("The KeywordCssClass should not be null or empty");
			if (string.IsNullOrEmpty(QuotesCssClass))
				throw new InvalidOperationException("The CommentCssClass should not be null or empty");
			if (string.IsNullOrEmpty(TypeCssClass))
				throw new InvalidOperationException("The TypeCssClass should not be null or empty");


			string startBracket = Regex.Escape("(");
			string endBracket = Regex.Escape(")");

			// Some fairly secure token placeholders
			const string COMMENTS_TOKEN = "`````";
			const string MULTILINECOMMENTS_TOKEN = "~~~~~";
			const string QUOTES_TOKEN = "Â¬Â¬Â¬Â¬Â¬";

			// Remove /* */ quotes, taken from ostermiller.org
			Regex regex = new Regex(@"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/", RegexOptions.Singleline);
			List<string> multiLineComments = new List<string>();
			if (regex.IsMatch(content))
			{
				foreach (Match item in regex.Matches(content))
				{
					if (!multiLineComments.Contains(item.Value))
						multiLineComments.Add(item.Value);
				}
			}

			for (int i = 0; i < multiLineComments.Count; i++)
				content = content.ReplaceToken(multiLineComments[i], MULTILINECOMMENTS_TOKEN, i);

			// Remove the quotes first, so they don't get highlighted
			List<string> quotes = new List<string>();
			bool onEscape = false;
			bool onComment1 = false;
			bool onComment2 = false;
			bool inQuotes = false;
			int start = -1;
			for (int i = 0; i < content.Length; i++)
			{
				switch (content[i]) {
					case '/' when !inQuotes && !onComment1:
						onComment1 = true;
						break;
					case '/' when !inQuotes && onComment1:
						onComment2 = true;
						break;
					case '"' when !onEscape && !onComment2: {
						inQuotes = true; // stops cases of: var s = "// I'm a comment";
						if (start > -1)
						{
							string quote = content.Substring(start, i - start + 1);
							if (!quotes.Contains(quote))
								quotes.Add(quote);
							start = -1;
							inQuotes = false;
						}
						else
						{
							start = i;
						}

						break;
					}
					case '\\':
					case '\'':
						onEscape = true;
						break;
					case '\n':
						onComment1 = false;
						onComment2 = false;
						break;
					default:
						onEscape = false;
						break;
				}
			}

			for (int i = 0; i < quotes.Count; i++)
				content = content.ReplaceToken(quotes[i], QUOTES_TOKEN, i);

			// Remove the comments next, so they don't get highlighted
			regex = new Regex("(/{2,3}.+)\n", RegexOptions.Multiline);
			List<string> comments = new List<string>();
			if (regex.IsMatch(content))
			{
				foreach (Match item in regex.Matches(content))
				{
					if (!comments.Contains(item.Value + "\n"))
						comments.Add(item.Value);
				}
			}

			for (int i = 0; i < comments.Count; i++)
				content = content.ReplaceToken(comments[i], COMMENTS_TOKEN, i);

			// Highlight single quotes
			content = Regex.Replace(content, "('.{1,2}')", "<span class=\"quote\">$1</span>", RegexOptions.Singleline);


			List<string> highlightedClasses = new List<string>();
			//Highlight types based on the logic <ClassName>
			regex = new Regex(@"(?<=<)([A-Z]\w+)(?=>)");
			content = regex.ReplaceWithCSS(content, TypeCssClass);

			// Highlight class names based on the logic: "{space OR start of line OR >}{1 capital){alphanumeric} " - must not be followed by an =
			// \w is a "word character" (ie. alphanumeric). "+" means one or more of preceding
			regex = new Regex($@"(?<={startBracket}|\s|^)([A-Z]\w+)(?=\s[^=])", RegexOptions.Singleline);
			content = regex.ReplaceWithCSS(content, TypeCssClass);

			// Pass 2. Doing it in N passes due to my inferior regex knowledge of back/forwardtracking.
			// This does {[}{1 capital){alphanumeric}{]}
			regex = new Regex(@"(?<=\[)([A-Z]\w+)(?=\]|\()", RegexOptions.Singleline);
			content = regex.ReplaceWithCSS(content, TypeCssClass);

			// Pass 3. Generics
			regex = new Regex(@"(?:\s|\[|\()([A-Z]\w+(?:<|&lt;))", RegexOptions.Singleline);
			highlightedClasses = new List<string>();
			if (regex.IsMatch(content))
			{
				foreach (Match item in regex.Matches(content))
				{
					string val = item.Groups[1].Value;
					if (!highlightedClasses.Contains(val))
						highlightedClasses.Add(val);
				}
			}

			foreach (string highlightedClass in highlightedClasses)
			{
				string val = highlightedClass;
				val = val.Replace("<", "").Replace("&lt;", "");
				content = content.ReplaceWithCss(highlightedClass, val, "&lt;", TypeCssClass);
			}

			// Pass 4. new keyword with a type
			regex = new Regex(@"(?<=new\s+)([A-Z]\w+)(?=\()", RegexOptions.Singleline);
			content = regex.ReplaceWithCSS(content, TypeCssClass);
			
			// Pass 5. Array declaration.
			regex = new Regex(@"(?<=\s)([A-Z]\w+)(?=\[\])", RegexOptions.Singleline);
			content = regex.ReplaceWithCSS(content, TypeCssClass);

			// Highlight types surrounded by typeof()
			regex = new Regex($"(?<=typeof{startBracket})([a-zA-Z0-9 ]+)(?={endBracket})");
			content = regex.ReplaceWithCSS(content, TypeCssClass);

			// Highlight keywords
			foreach (string keyword in _keywords)
			{
				Regex regexKeyword = new Regex("(" + keyword + @")(>|&gt;|\s|\n|;|<)", RegexOptions.Singleline);
				content = regexKeyword.Replace(content, "<span class=\"keyword\">$1</span>$2");
			}

			// Highlight typeof
			content = content.Replace("typeof(", "<span class=\"keyword\">typeof</span>(");

			// Shove the multiline comments back in
			for (int i = 0; i < multiLineComments.Count; i++)
				content = content.ReplaceTokenWithCss(multiLineComments[i], MULTILINECOMMENTS_TOKEN, i, CommentCssClass);

			// Shove the quotes back in
			for (int i = 0; i < quotes.Count; i++)
				content = content.ReplaceTokenWithCss(quotes[i], QUOTES_TOKEN, i, QuotesCssClass);

			// Shove the single line comments back in
			for (int i = 0; i < comments.Count; i++)
			{
				string comment = comments[i];
				// Add quotes back in
				for (int q = 0; q < quotes.Count; q++)
					comment = comment.Replace(string.Format("{0}{1}{0}", QUOTES_TOKEN, q), quotes[q]);

				content = content.ReplaceTokenWithCss(comment, COMMENTS_TOKEN, i, CommentCssClass);
			}
			
			//Shove the angle brackets back in
			content = content.Replace("&lt;", "<");

			return content;
		}
	}

	static class MoreExtensions
	{
		public static string ReplaceWithCSS(this Regex regex, string source, string cssClass) => regex.Replace(source, $"<span class=\"{cssClass}\">$1</span>");
		
		public static string ReplaceWithCss(this string content, string source, string cssClass) => content.Replace(source, $"<span class=\"{cssClass}\">{source}</span>");

		public static string ReplaceWithCss(this string content, string source, string replacement, string cssClass) => content.Replace(source, $"<span class=\"{cssClass}\">{replacement}</span>");

		public static string ReplaceWithCss(this string content, string source, string replacement, string suffix, string cssClass) => content.Replace(source, $"<span class=\"{cssClass}\">{replacement}</span>{suffix}");

		public static string ReplaceTokenWithCss(this string content, string source, string token, int counter, string cssClass)
		{
			string formattedToken = string.Format("{0}{1}{0}", token, counter);
			return content.Replace(formattedToken, $"<span class=\"{cssClass}\">{source}</span>");
		}

		public static string ReplaceToken(this string content, string source, string token, int counter)
		{
			string formattedToken = string.Format("{0}{1}{0}", token, counter);
			return content.Replace(source, formattedToken);
		}
	}
}