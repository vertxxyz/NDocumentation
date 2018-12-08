using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using static Vertx.DocumentationPage;

namespace Vertx
{
	internal class DocumentationContent
	{
		//The editor prefs key for the state of this DocumentationContent.
		private readonly string stateEditorPrefsKey;
		private string searchString = string.Empty;

		//The default root is set as pages are added as to provide an easy way for content to be added without providing the root to functions constantly.
		private static VisualElement currentDefaultRoot;
		public void SetCurrentDefaultRoot(VisualElement root) => currentDefaultRoot = root;
		public VisualElement GetRoot(VisualElement root) => root ?? currentDefaultRoot;

		private DocumentationPageRoot pageRoot;
		private Dictionary<string, IDocumentationPage> pages;
		private Dictionary<IDocumentationPage, List<DocumentationPageAddition>> additions;
		private Dictionary<IDocumentationPage, List<IDocumentationPage>> aboveButtonLinks;
		private Dictionary<IDocumentationPage, List<IDocumentationPage>> belowButtonLinks;

		public DocumentationContent(VisualElement root, Type windowRootType, string stateEditorPrefsKey = null)
		{
			this.stateEditorPrefsKey = stateEditorPrefsKey;
			InitialisePages();
			IMGUIContainer browserBar = new IMGUIContainer(BrowserBar)
			{
				style =
				{
					height = 18
				}
			};
			root.Add(browserBar);

			void InitialisePages()
			{
				//Find all the documentation and additions to it
				IEnumerable<IDocumentation> documentation = GetExtensionsOfTypeIE<IDocumentation>();
				List<DocumentationPage> documentationPages = new List<DocumentationPage>();
				List<DocumentationPageAddition> documentationAdditions = new List<DocumentationPageAddition>();
				
				pages = new Dictionary<string, IDocumentationPage>();
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
							throw new NotImplementedException();
					}
				}

				if (pageRoot == null)
				{
					Debug.LogError("No root page defined.");
					return;
				}

				//fill the additions dictionary
				additions = new Dictionary<IDocumentationPage, List<DocumentationPageAddition>>();
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
					additionList.Sort((a,b)=>a.Order.CompareTo(b.Order));


				//Discover the injected buttons
				var _aboveButtonLinksTemp = new Dictionary<IDocumentationPage, List<ButtonInjection>>();
				var _belowButtonLinksTemp = new Dictionary<IDocumentationPage, List<ButtonInjection>>();
				foreach (DocumentationPage iDoc in documentationPages)
				{
					ButtonInjection[] buttonLinkAbove = iDoc.InjectButtonLinkAbove;
					if (buttonLinkAbove != null)
						AddButtonInjections(buttonLinkAbove, "above", _aboveButtonLinksTemp);
					
					ButtonInjection[] buttonLinkBelow = iDoc.InjectButtonLinkBelow;
					if (buttonLinkBelow != null)
						AddButtonInjections(buttonLinkBelow, "below", _belowButtonLinksTemp);

					void AddButtonInjections(ButtonInjection[] injections, string location, Dictionary<IDocumentationPage, List<ButtonInjection>> dictionary)
					{
						foreach (ButtonInjection buttonInjection in injections)
						{
							GetDocumentationPage<ButtonInjection>(buttonInjection.pageType,
								"does not provide a page to add to.",
								$"{iDoc.GetType().FullName}'s intended location for button injection ({location}) {buttonInjection.pageType.FullName}",
								dictionary, buttonInjection);
						}
					}
				}
				//Sort the injected buttons
				foreach (List<ButtonInjection> buttonInjections in _aboveButtonLinksTemp.Values)
					buttonInjections.Sort((a, b) => a.order.CompareTo(b.order));
				foreach (List<ButtonInjection> buttonInjections in _belowButtonLinksTemp.Values)
					buttonInjections.Sort((a, b) => a.order.CompareTo(b.order));
				//Finalise the injected buttons
				aboveButtonLinks = new Dictionary<IDocumentationPage, List<IDocumentationPage>>();
				belowButtonLinks = new Dictionary<IDocumentationPage, List<IDocumentationPage>>();
				FinaliseInjectedButtons(_aboveButtonLinksTemp, aboveButtonLinks);
				FinaliseInjectedButtons(_belowButtonLinksTemp, belowButtonLinks);

				void FinaliseInjectedButtons(Dictionary<IDocumentationPage, List<ButtonInjection>> _buttonLinksTemp, Dictionary<IDocumentationPage, List<IDocumentationPage>> buttonLinks)
				{
					foreach (var keyValuePair in _buttonLinksTemp)
					{
						List<IDocumentationPage> _pages = new List<IDocumentationPage>();
						foreach (ButtonInjection buttonInjection in keyValuePair.Value)
							_pages.Add(pages[buttonInjection.pageType.FullName]);
						buttonLinks.Add(keyValuePair.Key, _pages);
					}
				}

				void GetDocumentationPage<T>(Type query, string queryNullError, string queryNotIDocError, Dictionary<IDocumentationPage, List<T>> dictionary, T add)
				{
					if (query == null)
					{
						Debug.LogError($"{query.GetType().FullName} {queryNullError}");
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

		#region Editor Extensions 

		private static Type _editorAssembliesType;
		private static Type editorAssembliesType => _editorAssembliesType ?? (_editorAssembliesType = Type.GetType("UnityEditor.EditorAssemblies,UnityEditor"));

		/// <summary>
		/// Returns an array of Types inherited from Type T in the Loaded (user & Unity) Assemblies.
		/// This function also operates with interfaces.
		/// </summary>
		private static IEnumerable<Type> GetTypesOfExtensions<T>() => (IEnumerable<Type>) SubclassesOf_Method.Invoke(null, new object[] {typeof(T)});

		private static MethodInfo _SubclassesOf_Method;
		private static MethodInfo SubclassesOf_Method => _SubclassesOf_Method ?? (_SubclassesOf_Method = editorAssembliesType.GetMethod("SubclassesOf", BindingFlags.Static | BindingFlags.NonPublic));

		private static IEnumerable<T> GetExtensionsOfTypeIE<T>()
		{
			IEnumerable<Type> typesOfEditorExtensions = GetTypesOfExtensions<T>();
			List<T> extensions = new List<T>();
			foreach (Type t in typesOfEditorExtensions)
			{
				if(t.IsAbstract)
					continue;
				extensions.Add((T) Activator.CreateInstance(t));
			}

			return extensions;
		}

		#endregion

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
		private string currentPageStateName;
		private readonly Stack<string> forwardHistory = new Stack<string>();

		void Back()
		{
			if (history.Count == 0)
				return;
			string backState = history.Pop();
			forwardHistory.Push(currentPageStateName);
			GoToPage(backState, true);
		}

		void Forward()
		{
			if (forwardHistory.Count == 0)
				return;
			string forwardState = forwardHistory.Pop();
			history.Push(currentPageStateName);
			GoToPage(forwardState, true);
		}

		#endregion

		#region Navigation

		void Home() => GoToPage(pageRoot.GetType().FullName);


		/// <summary>
		/// Displays the provided DocumentationPage.
		/// </summary>
		/// <param name="pageName">Page key</param>
		/// <param name="dontAddToHistory">Whether to append a history item to the stack</param>
		public void GoToPage(string pageName, bool dontAddToHistory = false)
		{
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
	}
}