using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.DocumentationUtility;

namespace Vertx
{
	internal class DocumentationContent<T> : DocumentationContentBase where T : DocumentationWindow
	{
		//The editor prefs key for the state of this DocumentationContent.
		private readonly string stateEditorPrefsKey;
		private readonly string searchEditorPrefsKey;
		private string searchString = string.Empty;

		//The default root is set as pages are added as to provide an easy way for content to be added without providing the root to functions constantly.
		private VisualElement _currentDefaultRoot;
		public override void SetCurrentDefaultRoot(VisualElement root) => _currentDefaultRoot = root;
		/// <summary>
		/// Returns the root if provided, otherwise returns the default root.
		/// </summary>
		/// <param name="root">Optional root to provide.,</param>
		/// <returns></returns>
		public override VisualElement GetRoot(VisualElement root = null) => root ?? _currentDefaultRoot;

		/// <summary>
		/// Adds a VisualElement to a root (default root if null is provided)
		/// </summary>
		/// <param name="element">Element to add to root</param>
		/// <param name="root">Root to add to</param>
		public override void AddToRoot(VisualElement element, VisualElement root = null) => GetRoot(root).Add(element);

		private DocumentationPageRoot<T> pageRoot;
		private VisualElement windowRoot;
		private readonly VisualElement contentRoot;
		private readonly VisualElement searchRoot;

		private readonly T window;

		//Full Name to page
		private readonly Dictionary<string, IDocumentationPage<T>> pages = new Dictionary<string, IDocumentationPage<T>>();

		//Page to documentation additions associated with it
		private readonly Dictionary<IDocumentationPage<T>, List<DocumentationPageAddition<T>>> additions = new Dictionary<IDocumentationPage<T>, List<DocumentationPageAddition<T>>>();

		//Page to buttons injected into it.
		private readonly Dictionary<IDocumentationPage<T>, List<DocumentationPage<T>.ButtonInjection>> aboveButtonLinks = new Dictionary<IDocumentationPage<T>, List<DocumentationPage<T>.ButtonInjection>>();
		private readonly Dictionary<IDocumentationPage<T>, List<DocumentationPage<T>.ButtonInjection>> belowButtonLinks = new Dictionary<IDocumentationPage<T>, List<DocumentationPage<T>.ButtonInjection>>();


		private readonly string searchFieldName;
		
		public DocumentationContent(VisualElement root, T window, string stateEditorPrefsKey = null)
		{
			this.window = window;
			this.stateEditorPrefsKey = stateEditorPrefsKey;
			searchEditorPrefsKey = $"{stateEditorPrefsKey}_Search";
			searchFieldName = $"SearchField_{window.GetType().FullName}";

			IMGUIContainer browserBar = new IMGUIContainer(BrowserBar)
			{
				style =
				{
					height = 18
				}
			};
			root.Add(browserBar);
			StyleSheet docsStyleSheet = LoadAssetOfType<StyleSheet>("nDocumentationStyles", SearchFilter.Packages);
			root.styleSheets.Add(docsStyleSheet);

			StyleSheet codeStyleSheet = LoadAssetOfType<StyleSheet>("CsharpHighlightingStyles", SearchFilter.Packages);
			root.styleSheets.Add(codeStyleSheet);

			ScrollView scrollView = new ScrollView
			{
				name = "Scroll View",
				showHorizontal = false,
				showVertical = true
			};
			scrollView.AddToClassList("scroll-view");
			scrollView.contentContainer.AddToClassList("scroll-view-content");
			contentRoot = scrollView.contentContainer;
			root.Add(scrollView);
			
			searchRoot = new VisualElement();
			searchRoot.ClearClassList();
			searchRoot.AddToClassList("search-container");
			root.Add(searchRoot);
			searchRoot.StretchToParentSize();
			searchRoot.visible = false;
			
			SetCurrentDefaultRoot(contentRoot);
		}

		public override bool InitialiseContent()
		{
			if (!InitialisePages())
				return false;
			if (EditorPrefs.HasKey(stateEditorPrefsKey))
			{
				string page = EditorPrefs.GetString(stateEditorPrefsKey);
				if (!LoadPage(page))
					Home();
			}
			else
			{
				Home();
			}

			if (EditorPrefs.HasKey(searchEditorPrefsKey))
				searchString = EditorPrefs.GetString(searchEditorPrefsKey);

			return true;

			bool InitialisePages()
			{
				//Find all the documentation and additions to it
				Type iDocGenericType = typeof(IDocumentation<>).MakeGenericType(window.GetType());
				IEnumerable<IDocumentation<T>> documentation = GetExtensionsOfTypeIE<IDocumentation<T>>(iDocGenericType);
				List<DocumentationPage<T>> documentationPages = new List<DocumentationPage<T>>();
				List<DocumentationPageAddition<T>> documentationAdditions = new List<DocumentationPageAddition<T>>();

				foreach (IDocumentation<T> iDoc in documentation)
				{
					switch (iDoc)
					{
						case DocumentationPageAddition<T> pageAddition:
							documentationAdditions.Add(pageAddition);
							break;
						case DocumentationPage<T> page:
							documentationPages.Add(page);
							pages.Add(page.GetType().FullName, page);
							break;
						case DocumentationPageRoot<T> pageRoot:
							pages.Add(pageRoot.GetType().FullName, pageRoot);
							if (this.pageRoot != null)
							{
								Debug.LogError($"Multiple pages are assigned to be the root for window. {this.pageRoot} & {pageRoot}.");
								continue;
							}

							this.pageRoot = pageRoot;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					iDoc.Initialise(window);
				}

				if (pageRoot == null)
				{
					Debug.LogError("No root page defined.");
					return false;
				}

				//fill the additions dictionary
				foreach (DocumentationPageAddition<T> docAddition in documentationAdditions)
				{
					Type pageToAddToType = docAddition.PageToAddToType;
					GetDocumentationPage<DocumentationPageAddition<T>>(pageToAddToType,
						"does not provide a page to add to.",
						$"{docAddition.GetType().FullName}'s provided PageToAddToType",
						additions,
						docAddition);
				}

				foreach (var additionList in additions.Values)
					additionList.Sort((a, b) => a.Order.CompareTo(b.Order));


				//Discover the injected buttons
				foreach (DocumentationPage<T> iDoc in documentationPages)
				{
					DocumentationPage<T>.ButtonInjection[] buttonLinkAbove = iDoc.InjectButtonLinkAbove;
					if (buttonLinkAbove != null)
						AddButtonInjections(buttonLinkAbove, "above", aboveButtonLinks);

					DocumentationPage<T>.ButtonInjection[] buttonLinkBelow = iDoc.InjectButtonLinkBelow;
					if (buttonLinkBelow != null)
						AddButtonInjections(buttonLinkBelow, "below", belowButtonLinks);

					void AddButtonInjections(DocumentationPage<T>.ButtonInjection[] injections, string location, Dictionary<IDocumentationPage<T>, List<DocumentationPage<T>.ButtonInjection>> dictionary)
					{
						//For all the buttons in we're injecting
						foreach (DocumentationPage<T>.ButtonInjection buttonInjection in injections)
						{
							buttonInjection.pageOfOrigin = iDoc;
							//We find the relevant page and add our injected button to its dictionary entry
							GetDocumentationPage<DocumentationPage<T>.ButtonInjection>(buttonInjection.pageType,
								"does not provide a page to add to.",
								$"{iDoc.GetType().FullName}'s intended location for button injection ({location}) {buttonInjection.pageType.FullName}",
								dictionary, buttonInjection);
						}
					}
				}

				//Sort the injected buttons
				foreach (List<DocumentationPage<T>.ButtonInjection> buttonInjections in aboveButtonLinks.Values)
					buttonInjections.Sort((a, b) => a.order.CompareTo(b.order));
				foreach (List<DocumentationPage<T>.ButtonInjection> buttonInjections in belowButtonLinks.Values)
					buttonInjections.Sort((a, b) => a.order.CompareTo(b.order));

				void GetDocumentationPage<TType>(Type query, string queryNullError, string queryNotIDocError, Dictionary<IDocumentationPage<T>, List<TType>> dictionary, TType add)
				{
					if (query == null)
					{
						Debug.LogError($"{query.FullName} {queryNullError}");
						return;
					}

					string key = query.FullName;
					if (!pages.TryGetValue(key, out IDocumentationPage<T> pageToAddTo))
					{
						if (!query.IsSubclassOf(typeof(IDocumentation<T>)))
							Debug.LogError($"{queryNotIDocError} ({key}) is not a Documentation Page {GetType().FullName}.");
						return;
					}

					if (!dictionary.TryGetValue(pageToAddTo, out List<TType> addList))
					{
						addList = new List<TType>();
						dictionary.Add(pageToAddTo, addList);
					}

					addList.Add(add);
				}

				return true;
			}
		}

		private bool LoadPage(string pageFullName)
		{
			if (!pages.TryGetValue(pageFullName, out IDocumentationPage<T> page))
			{
				Debug.LogError($"Window does not contain a reference to {pageFullName}.");
				return false;
			}
			LoadPage(page);
			return true;
		}

		private void LoadPage (IDocumentationPage<T> page, VisualElement rootOverride = null) {
			VisualElement root = rootOverride ?? contentRoot;
			SetCurrentDefaultRoot(root);
			root.Clear();

			//Constant header
			window.DrawConstantHeader(root);

			//Above buttons
			if (aboveButtonLinks.TryGetValue(page, out var buttonsAbove))
				AddInjectedButtons(buttonsAbove);

			//Documentation
			SetCurrentDefaultRoot(root);
			page.DrawDocumentation(window, root);

			//Additions
			if (additions.TryGetValue(page, out var additionsList))
			{
				foreach (var addition in additionsList)
					addition.DrawDocumentation(window, root);
			}
			
			page.DrawDocumentationAfterAdditions(window, root);

			//Below buttons
			if (belowButtonLinks.TryGetValue(page, out var buttonsBelow))
				AddInjectedButtons(buttonsBelow);

			currentPageStateName = page.GetType().FullName;

			void AddInjectedButtons(List<DocumentationPage<T>.ButtonInjection> buttons)
			{
				VisualElement injectedButtonContainer = new VisualElement();
				injectedButtonContainer.AddToClassList("injected-button-container");
				AddToRoot(injectedButtonContainer);
				SetCurrentDefaultRoot(injectedButtonContainer);
				
				foreach (DocumentationPage<T>.ButtonInjection button in buttons)
				{
					DocumentationPage<T> pageOfOrigin = button.pageOfOrigin;
					window.AddFullWidthButton(pageOfOrigin.Title, pageOfOrigin.Color, () => GoToPage(pageOfOrigin.GetType().FullName));
				}
			}
		}

		void BrowserBar()
		{
			float w2 = Screen.width / 2f - 5;

			GUI.Box(new Rect(0, 0, Screen.width, 20), GUIContent.none, EditorStyles.toolbar);

			GUI.enabled = history.Count > 0;
			if (GUI.Button(new Rect(5, 0, 20, 20), new GUIContent(EditorGUIUtility.isProSkin ? GetTexture("Back") : GetTexture("Back_Alt"), "Back"), EditorStyles.toolbarButton))
				Back();

			GUI.enabled = forwardHistory.Count > 0;
			if (GUI.Button(new Rect(25, 0, 20, 20), new GUIContent(EditorGUIUtility.isProSkin ? GetTexture("Forward") : GetTexture("Forward_Alt"), "Forward"), EditorStyles.toolbarButton))
				Forward();

			GUI.enabled = currentPageStateName != null && !currentPageStateName.Equals(pageRoot.GetType().FullName);
			if (GUI.Button(new Rect(48, 0, 20, 20), new GUIContent(EditorGUIUtility.isProSkin ? GetTexture("Home") : GetTexture("Home_Alt"), "Home"), EditorStyles.toolbarButton))
			{
				searchString = string.Empty;
				GUI.FocusControl(null);
				Home();
			}

			GUI.enabled = true;
			Rect searchRect = new Rect(w2, 2, w2, 16);
			using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
			{
				GUI.SetNextControlName(searchFieldName);
				searchString = EditorGUIExtensions.ToolbarSearchField(searchRect, searchString);

				if (!searchRoot.visible && !string.IsNullOrEmpty(searchString) && GUI.GetNameOfFocusedControl().Equals(searchFieldName))
				{
					searchRoot.visible = true;
					if(searchStringsCache.Count == 0)
						DoSearch();
				}

				if (changeCheckScope.changed)
					DoSearch();
			}
		}

		#region Search

		private readonly Dictionary<IDocumentationPage<T>, List<string>> searchStringsCache = new Dictionary<IDocumentationPage<T>,List<string>>();
		private const int maxMatchCount = 6;
		
		void DoSearch()
		{
			EditorPrefs.SetString(searchEditorPrefsKey, searchString);
			
			if (string.IsNullOrEmpty(searchString))
			{
				searchRoot.Clear();
				searchRoot.visible = false;
				return;
			}

			string searchStringLower = searchString.ToLower();
						
			//Cache current page
			string currentPageStateNameCached = currentPageStateName;
			
			//Clear the previous search
			searchRoot.Clear();
			searchRoot.visible = true;

			//searchRootTemp is an un-parented root to append to so we can search content.
			VisualElement searchRootTemp = new VisualElement();
			Dictionary<IDocumentationPage<T>, List<string>> searchResults = new Dictionary<IDocumentationPage<T>, List<string>>();

			StringBuilder stringBuilder = new StringBuilder();
			foreach (IDocumentationPage<T> page in pages.Values)
			{
				if (!searchStringsCache.TryGetValue(page, out var searchStrings))
				{
					searchStrings = new List<string>();
					searchStringsCache.Add(page, searchStrings);
					
					//Load the page under searchRootTemp
					LoadPage(page, searchRootTemp);
					
					//Get all the paragraph element's text (combined together)
					searchRootTemp.Query(null, "paragraph-container").ForEach(paragraph =>
					{
						stringBuilder.Clear();
						paragraph.Query<TextElement>().Build().ForEach(tE => stringBuilder.Append(tE.text));
						searchStrings.Add(stringBuilder.ToString());
						paragraph.Clear();
					});
					//Get all the text from the Text elements under searchRootTemp
					searchRootTemp.Query<TextElement>().ForEach(element => searchStrings.Add(element.text));
				}

				foreach (string searchStringLocal in searchStrings)
				{
					string searchStringLocalLower = searchStringLocal.ToLower();
					if (searchStringLocalLower.Contains(searchStringLower))
					{
						if(!searchResults.TryGetValue(page, out var resultStrings))
							searchResults.Add(page, resultStrings = new List<string>());
						resultStrings.Add(searchStringLocal);
					}
				}
			}
			
			List<string> matches = new List<string>(maxMatchCount);

			foreach (var result in searchResults)
			{
				//Result Container
				VisualElement resultContainer = new VisualElement();
				resultContainer.ClearClassList();
				resultContainer.AddToClassList("result-container");
				resultContainer.userData = result.Key;
				searchRoot.Add(resultContainer);

				//Result Button
				Button searchResultButton = window.AddFullWidthButton(result.Key.GetType(), resultContainer);
				searchResultButton.AddToClassList("result");
				Label resultLabel = RichTextUtility.AddInlineText(searchResultButton.text, searchResultButton);
				resultLabel.style.color = searchResultButton.style.color;
				searchResultButton.text = null;
				Label resultCountLabel = RichTextUtility.AddInlineText($" ({result.Value.Count} Results)", searchResultButton);
				resultCountLabel.style.color = new Color(1, 1, 1, 0.35f);
				searchResultButton.RegisterCallback<MouseUpEvent>(evt => searchRoot.visible = false);
				
				//Results (the string matches)
				VisualElement resultMatchesContainer = new VisualElement();
				resultMatchesContainer.ClearClassList();
				resultMatchesContainer.AddToClassList("result-matches-container");
				resultContainer.Add(resultMatchesContainer);
								
				//Create a hashset of all the matched words.
				matches.Clear();
				foreach (string m in result.Value)
				{
					string match = Regex.Match(m, $@"\w*{searchStringLower}\w*", RegexOptions.IgnoreCase).Value;
					if (matches.Contains(match))
						continue;
					matches.Add(match);
					if (matches.Count > maxMatchCount)
						break;
				}
				//Add a max of maxMatchCount inline text to the resultMatchesContainer
				int l = Mathf.Min(maxMatchCount, matches.Count);
				for (int i = 0; i < l; i++)
				{
					//Create group for matched coloured text.
					VisualElement inlineTextGroup = new VisualElement();
					inlineTextGroup.ClearClassList();
					inlineTextGroup.AddToClassList("inline-text-group");
					inlineTextGroup.AddToClassList("result");
					resultMatchesContainer.Add(inlineTextGroup);

					int indexOf = matches[i].IndexOf(searchStringLower, StringComparison.OrdinalIgnoreCase);

					Label matchContent;
					if (matches[i].Length == searchStringLower.Length)
					{
						matchContent = RichTextUtility.AddInlineText(matches[i], inlineTextGroup);
					}
					else if (indexOf == 0)
					{
						matchContent = RichTextUtility.AddInlineText(matches[i].Substring(indexOf, searchStringLower.Length), inlineTextGroup);
						RichTextUtility.AddInlineText(matches[i].Substring(searchStringLower.Length), inlineTextGroup);
					}else if (indexOf == matches[i].Length - searchStringLower.Length)
					{
						RichTextUtility.AddInlineText(matches[i].Substring(0, indexOf), inlineTextGroup);
						matchContent = RichTextUtility.AddInlineText(matches[i].Substring(indexOf), inlineTextGroup);
					}
					else
					{
						RichTextUtility.AddInlineText(matches[i].Substring(0, indexOf), inlineTextGroup);
						matchContent = RichTextUtility.AddInlineText(matches[i].Substring(indexOf, searchStringLower.Length), inlineTextGroup);
						RichTextUtility.AddInlineText(matches[i].Substring(indexOf + searchStringLower.Length), inlineTextGroup);
					}
					matchContent.style.color = new Color(1, 0.5f, 0);
				}

				if (l != matches.Count)
					RichTextUtility.AddInlineText("...", resultMatchesContainer);
			}
			
			searchRoot.Sort((a,b)=>searchResults[(IDocumentationPage<T>) b.userData].Count.CompareTo(searchResults[(IDocumentationPage<T>) a.userData].Count));
			
			//Reset page
			currentPageStateName = currentPageStateNameCached;
		}

		#endregion

		#region History

		public override bool HasHistory() => history.Count > 0 || forwardHistory.Count > 0;

		public override void ClearHistory()
		{
			history.Clear();
			forwardHistory.Clear();
		}
		
		private readonly Stack<string> history = new Stack<string>();
		private string _currentPageStateName;

		private string currentPageStateName
		{
			get => _currentPageStateName;
			set
			{
				_currentPageStateName = value;
				EditorPrefs.SetString(stateEditorPrefsKey, _currentPageStateName);
			}
		}

		private readonly Stack<string> forwardHistory = new Stack<string>();

		void Back()
		{
			if (history.Count == 0)
				return;
			string backState = history.Pop();
			forwardHistory.Push(currentPageStateName);
			GoToPage(backState, false);
		}

		void Forward()
		{
			if (forwardHistory.Count == 0)
				return;
			string forwardState = forwardHistory.Pop();
			history.Push(currentPageStateName);
			GoToPage(forwardState, false);
		}

		#endregion

		#region Navigation

		public override void Home() => GoToPage(pageRoot.GetType().FullName);


		/// <summary>
		/// Displays the provided DocumentationPage.
		/// </summary>
		/// <param name="pageName">Page key</param>
		/// <param name="addToHistory">Whether to append a history item to the stack</param>
		public override void GoToPage(string pageName, bool addToHistory = true)
		{
			if (addToHistory)
			{
				if(currentPageStateName != pageName)
					history.Push(currentPageStateName);
			}

			LoadPage(pageName);
		}

		#endregion
		
		#region Helpers 

		public override string GetTitleFromPage(Type pageType) => pages[pageType.FullName].Title;
		public override Color GetColorFromPage(Type pageType) => pages[pageType.FullName].Color;

		#endregion


		#region Textures

		private static readonly Dictionary<string, Texture> helpTextures = new Dictionary<string, Texture>();

		/*
		/// <summary>
		/// Adds a UI Image with the provided texture
		/// </summary>
		/// <param name="root">root to append Image to</param>
		/// <param name="textureName">name of the texture in Resources</param>
		/// <param name="alignment">Align Self</param>
		public Image AddImageTexture(string textureName, Align alignment = Align.FlexStart, VisualElement root = null)
		{
		    root = GetRoot(root);
		    return AddImageTexture(textureName,  new RectOffset(4,4,2,10), alignment, root);
		}

		/// <summary>
		/// Adds a UI Image with the provided texture
		/// </summary>
		/// <param name="root">root to append Image to</param>
		/// <param name="textureName">name of the texture in Resources</param>
		/// <param name="padding">padding between other content</param>
		/// <param name="alignment">Align Self</param>
		public Image AddImageTexture(string textureName, RectOffset padding, Align alignment = Align.FlexStart, VisualElement root = null)
		{
		    root = GetRoot(root);
		    Image image = GetImageUI(GetTexture(textureName), padding, alignment);
		    root.Add(image);
		    return image;
		}
*/

		private static Texture GetTexture(string textureName)
		{
			if (helpTextures.TryGetValue(textureName, out Texture t)) return t;
			t = LoadAssetOfType<Texture2D>(textureName);
			helpTextures.Add(textureName, t);
			return t;
		}

		#endregion

		#region Button Registry

		private readonly Dictionary<string, Action> _buttonRegistry = new Dictionary<string, Action>();

		public override bool RegisterButton(string key, Action action)
		{
			if (pages.ContainsKey(key))
			{
				Debug.LogWarning($"\"{key}\" already exists as a page, pages are automatically added to the button registry.");
				return false;
			}

			if (_buttonRegistry.ContainsKey(key))
			{
				Debug.LogWarning($"\"{key}\" already exists in button registry, please find another key.");
				return false;
			}

			if (action == null)
			{
				Debug.LogWarning($"Action provided with \"{key}\" cannot be null.");
				return false;
			}
			_buttonRegistry.Add(key, action);
			return true;
		}

		public override bool GetRegisteredButtonAction(string key, out Action action)
		{
			if (pages.TryGetValue(key, out var page))
			{
				action = () => GoToPage(page.GetType().FullName);
				return true;
			}

			if (_buttonRegistry.TryGetValue(key, out action))
				return true;
			Debug.LogError($"\"{key}\" does not exist in button registry. This likely means you have not registered the action from the Initialise function in your documentation.");
			return false;
		}

		private bool InvokeRegisteredButton(string key)
		{
			if (!GetRegisteredButtonAction(key, out var action))
				return false;
			action.Invoke();
			return true;
		}

		#endregion
	}
}