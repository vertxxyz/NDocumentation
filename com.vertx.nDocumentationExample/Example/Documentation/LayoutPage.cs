using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.DocumentationWindow;
using static Vertx.Example.WindowPage;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
	public sealed class LayoutPage : DocumentationPage
	{
		public override ButtonInjection[] InjectButtonLinkAbove => null;
		public override ButtonInjection[] InjectButtonLinkBelow => null;
		public override Color Color => LayoutColor;
		public override string Title => "Page Layout";

		public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
		{
			window.AddHeader(Title, 18, FontStyle.Normal);
			window.AddRichText($"All Pages are laid out in the same fashion.\nThere are above and below <i>Button Injection</i> locations, and in between is the <i>Content</i> section. The content section first contains original content provided by the page, then content added by {PageAddition.DocumentationPageAdditionsSpacedString}.");

			VisualElement windowContainer = new Button(()=>window.GoToPage(typeof(WindowPage)));
			ModifyStyle(windowContainer, CreateColor, 2);
			root.Add(windowContainer);
			using (new DefaultRootScope(window, windowContainer))
			{
				window.AddRichText(DocumentationWindowString);
				{ // Above
					VisualElement buttonInjectionAboveContainer = new VisualElement();
					ModifyStyle(buttonInjectionAboveContainer, ExtendingPages.InjectColor, 5);
					windowContainer.Add(buttonInjectionAboveContainer);
					using (new DefaultRootScope(window, buttonInjectionAboveContainer))
						window.AddRichText(GetColouredString("Button Links Injected Above", ExtendingPages.InjectColor));
				}

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

				{ // Below
					VisualElement buttonInjectionBelowContainer = new VisualElement();
					ModifyStyle(buttonInjectionBelowContainer, ExtendingPages.InjectColor, 5);
					windowContainer.Add(buttonInjectionBelowContainer);
					using (new DefaultRootScope(window, buttonInjectionBelowContainer))
						window.AddRichText(GetColouredString("Button Links Injected Below", ExtendingPages.InjectColor));
				}
			}
		}

		private static void ModifyStyle(VisualElement element, Color borderColor, float borderRadius = 0)
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

		public static readonly Color LayoutColor = new Color(0f, 0.7f, 1f);
	}
}