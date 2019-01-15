using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.DocumentationWindow;
using static Vertx.Example.WindowPage;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
	public sealed class LayoutPage : DocumentationPage<ExampleWindow>
	{
		public override ButtonInjection[] InjectedButtonLinks => null;
		public override Color Color => LayoutColor;
		public override string Title => "Page Layout";

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddHeader(Title, 18, FontStyle.Normal);
			window.AddRichText($"All Pages are laid out in the same fashion.\nThere is a <i>Content</i> section followed by <i>Button Injection</i>s. The content section first contains original content provided by the {CreatingPage.DocumentationPageSimpleString}, then content added by {PageAddition.DocumentationPageAdditionsSpacedString}.");

			// Window
			VisualElement windowContainer = new Button(()=>window.GoToPage(typeof(WindowPage)));
			ModifyStyle(windowContainer, CreateColor, 2);
			window.GetDefaultRoot().Add(windowContainer);
			using (new DefaultRootScope(window, windowContainer))
			{
				window.AddRichText(DocumentationWindowString);

				{ // Content
					VisualElement contentContainer = new Button(()=>window.GoToPage(typeof(CreatingPage)));
					ModifyStyle(contentContainer, CreateColor, 2);
					windowContainer.Add(contentContainer);
					using (new DefaultRootScope(window, contentContainer))
						window.AddRichText(GetColouredString("Documentation Content", CreateColor));
				}

				{ // Additions
					VisualElement additionsContainer = new Button(()=>window.GoToPage(typeof(ExtendingPages)));
					ModifyStyle(additionsContainer, ExtendingPages.ExtendColor, 2);
					windowContainer.Add(additionsContainer);
					using (new DefaultRootScope(window, additionsContainer))
						window.AddRichText(PageAddition.DocumentationPageAdditionsSpacedString);
				}
				
				{ // After Additions
					VisualElement contentContainer = new Button(()=>window.GoToPage(typeof(CreatingPage)));
					ModifyStyle(contentContainer, CreateColor, 2);
					windowContainer.Add(contentContainer);
					using (new DefaultRootScope(window, contentContainer))
						window.AddRichText(GetColouredString("Documentation Content After Additions", CreateColor));
				}

				{ // Below
					VisualElement injectedButtonsContainer = new VisualElement();
					ModifyStyle(injectedButtonsContainer, ExtendingPages.InjectColor, 6);
					windowContainer.Add(injectedButtonsContainer);
					using (new DefaultRootScope(window, injectedButtonsContainer))
						window.AddRichText(InjectedButtonLinksString);
				}
			}
			
			void ModifyStyle(VisualElement element, Color borderColor, float borderRadius = 0)
			{
				element.ClearClassList();
				IStyle s = element.style;
				s.marginTop = 5;
				s.marginBottom = 5;
				s.marginLeft = 16;
				s.marginRight = 5;
				s.borderColor = borderColor;
				s.borderTopWidth = 1;
				s.borderLeftWidth = 1;
				s.borderRightWidth = 1;
				s.borderBottomWidth = 1;
				s.alignSelf = Align.Stretch;
				s.borderTopLeftRadius = borderRadius;
				s.borderTopRightRadius = borderRadius;
				s.borderBottomLeftRadius = borderRadius;
				s.borderBottomRightRadius = borderRadius;
			}
		}

		public override void DrawDocumentationAfterAdditions(ExampleWindow window) => LandingPage.AddNextButton(window, typeof(WindowPage));

		public static readonly string InjectedButtonLinksString = GetColouredString("Injected Button Links", ExtendingPages.InjectColor);
		public static readonly Color LayoutColor = new Color(0f, 0.7f, 1f);
	}
}