using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.DocumentationPage;
using static Vertx.NDocumentationUtility;

namespace Vertx
{
	internal class DocumentationContent
	{
		//The editor prefs key for the state of this DocumentationContent.
		private readonly string stateEditorPrefsKey;
		private string searchString = string.Empty;

		//The default root is set as pages are added as to provide an easy way for content to be added without providing the root to functions constantly.
		private static VisualElement _currentDefaultRoot;
		public void SetCurrentDefaultRoot(VisualElement root) => _currentDefaultRoot = root;
		public VisualElement GetRoot(VisualElement root) => root ?? _currentDefaultRoot;

		/// <summary>
		/// Adds a VisualElement to a root (default root if null is provided)
		/// </summary>
		/// <param name="element">Element to add to root</param>
		/// <param name="root">Root to add to</param>
		public void AddToRoot(VisualElement element, VisualElement root = null) => GetRoot(root).Add(element);

		private DocumentationPageRoot pageRoot;
		private VisualElement windowRoot;
		private VisualElement contentRoot;

		private readonly DocumentationWindow window;

		//Full Name to page
		private readonly Dictionary<string, IDocumentationPage> pages = new Dictionary<string, IDocumentationPage>();

		//Page to documentation additions associated with it
		private readonly Dictionary<IDocumentationPage, List<DocumentationPageAddition>> additions = new Dictionary<IDocumentationPage, List<DocumentationPageAddition>>();

		//Page to buttons injected into it.
		private readonly Dictionary<IDocumentationPage, List<ButtonInjection>> aboveButtonLinks = new Dictionary<IDocumentationPage, List<ButtonInjection>>();
		private readonly Dictionary<IDocumentationPage, List<ButtonInjection>> belowButtonLinks = new Dictionary<IDocumentationPage, List<ButtonInjection>>();


		public DocumentationContent(VisualElement root, DocumentationWindow window, string stateEditorPrefsKey = null)
		{
			this.window = window;
			this.stateEditorPrefsKey = stateEditorPrefsKey;

			IMGUIContainer browserBar = new IMGUIContainer(BrowserBar)
			{
				style =
				{
					height = 18
				}
			};
			root.Add(browserBar);
			StyleSheet styleSheet = LoadAssetOfType<StyleSheet>("nDocumentationStyles", SearchFilter.Packages);
			root.styleSheets.Add(styleSheet);

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
			SetCurrentDefaultRoot(contentRoot);
		}

		public void InitialiseContent()
		{
			Type windowRootType = window.GetType();
			InitialisePages();
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

			void InitialisePages()
			{
				//Find all the documentation and additions to it
				IEnumerable<IDocumentation> documentation = GetExtensionsOfTypeIE<IDocumentation>();
				List<DocumentationPage> documentationPages = new List<DocumentationPage>();
				List<DocumentationPageAddition> documentationAdditions = new List<DocumentationPageAddition>();

				foreach (IDocumentation iDoc in documentation)
				{
					switch (iDoc)
					{
						case DocumentationPageAddition pageAddition:
							documentationAdditions.Add(pageAddition);
							break;
						case DocumentationPage page:
							documentationPages.Add(page);
							pages.Add(page.GetType().FullName, page);
							break;
						case DocumentationPageRoot pageRoot:
							pages.Add(pageRoot.GetType().FullName, pageRoot);
							if (pageRoot.ParentDocumentationWindowType != windowRootType) continue;
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
					return;
				}

				//fill the additions dictionary
				foreach (DocumentationPageAddition docAddition in documentationAdditions)
				{
					Type pageToAddToType = docAddition.PageToAddToType;
					GetDocumentationPage<DocumentationPageAddition>(pageToAddToType,
						"does not provide a page to add to.",
						$"{docAddition.GetType().FullName}'s provided PageToAddToType",
						additions,
						docAddition);
				}

				foreach (var additionList in additions.Values)
					additionList.Sort((a, b) => a.Order.CompareTo(b.Order));


				//Discover the injected buttons
				foreach (DocumentationPage iDoc in documentationPages)
				{
					ButtonInjection[] buttonLinkAbove = iDoc.InjectButtonLinkAbove;
					if (buttonLinkAbove != null)
						AddButtonInjections(buttonLinkAbove, "above", aboveButtonLinks);

					ButtonInjection[] buttonLinkBelow = iDoc.InjectButtonLinkBelow;
					if (buttonLinkBelow != null)
						AddButtonInjections(buttonLinkBelow, "below", belowButtonLinks);

					void AddButtonInjections(ButtonInjection[] injections, string location, Dictionary<IDocumentationPage, List<ButtonInjection>> dictionary)
					{
						//For all the buttons in we're injecting
						foreach (ButtonInjection buttonInjection in injections)
						{
							buttonInjection.pageOfOrigin = iDoc;
							//We find the relevant page and add our injected button to its dictionary entry
							GetDocumentationPage<ButtonInjection>(buttonInjection.pageType,
								"does not provide a page to add to.",
								$"{iDoc.GetType().FullName}'s intended location for button injection ({location}) {buttonInjection.pageType.FullName}",
								dictionary, buttonInjection);
						}
					}
				}

				//Sort the injected buttons
				foreach (List<ButtonInjection> buttonInjections in aboveButtonLinks.Values)
					buttonInjections.Sort((a, b) => a.order.CompareTo(b.order));
				foreach (List<ButtonInjection> buttonInjections in belowButtonLinks.Values)
					buttonInjections.Sort((a, b) => a.order.CompareTo(b.order));

				void GetDocumentationPage<T>(Type query, string queryNullError, string queryNotIDocError, Dictionary<IDocumentationPage, List<T>> dictionary, T add)
				{
					if (query == null)
					{
						Debug.LogError($"{query.FullName} {queryNullError}");
						return;
					}

					string key = query.FullName;
					if (!pages.TryGetValue(key, out IDocumentationPage pageToAddTo))
					{
						if (!query.IsSubclassOf(typeof(IDocumentation)))
							Debug.LogError($"{queryNotIDocError} ({key}) is not a Documentation Page {GetType().FullName}.");
						return;
					}

					if (!dictionary.TryGetValue(pageToAddTo, out List<T> addList))
					{
						addList = new List<T>();
						dictionary.Add(pageToAddTo, addList);
					}

					addList.Add(add);
				}
			}
		}

		private bool LoadPage(string pageFullName)
		{
			if (!pages.TryGetValue(pageFullName, out IDocumentationPage page))
			{
				Debug.LogError($"Window does not contain a reference to {pageFullName}.");
				return false;
			}


			VisualElement root = contentRoot;
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

			//Below buttons
			if (belowButtonLinks.TryGetValue(page, out var buttonsBelow))
				AddInjectedButtons(buttonsBelow);

			currentPageStateName = pageFullName;

			void AddInjectedButtons(List<ButtonInjection> buttons)
			{
				VisualElement injectedButtonContainer = new VisualElement();
				injectedButtonContainer.AddToClassList("injected-button-container");
				AddToRoot(injectedButtonContainer);
				SetCurrentDefaultRoot(injectedButtonContainer);
				
				foreach (ButtonInjection button in buttons)
				{
					DocumentationPage pageOfOrigin = button.pageOfOrigin;
					window.AddFullWidthButton(pageOfOrigin.Title, pageOfOrigin.Color, () => GoToPage(pageOfOrigin.GetType().FullName));
				}
			}

			return true;
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
				searchString = EditorGUIExtensions.ToolbarSearchField(searchRect, searchString);
				if (changeCheckScope.changed)
				{
					if (string.IsNullOrEmpty(searchString))
						GoToPage(currentPageStateName, true);
					else
					{
						//TODO
//						DoSearch();
					}
				}
			}
		}

		#region History

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

		public void Home() => GoToPage(pageRoot.GetType().FullName);


		/// <summary>
		/// Displays the provided DocumentationPage.
		/// </summary>
		/// <param name="pageName">Page key</param>
		/// <param name="addToHistory">Whether to append a history item to the stack</param>
		public void GoToPage(string pageName, bool addToHistory = true)
		{
			if (addToHistory)
				history.Push(currentPageStateName);
			LoadPage(pageName);
		}

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
			t = Resources.Load<Texture2D>(textureName);
			helpTextures.Add(textureName, t);
			return t;
		}

		#endregion

		#region Button Registry

		private readonly Dictionary<string, Action> _buttonRegistry = new Dictionary<string, Action>();

		public bool RegisterButton(string key, Action action)
		{
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

		public bool GetRegisteredButtonAction(string key, out Action action)
		{
			if (!_buttonRegistry.TryGetValue(key, out action))
			{
				Debug.LogError($"\"{key}\" does not exist in button registry. This likely means you have not registered the action from the Initialise function in your documentation.");
				return false;
			}
			return true;
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