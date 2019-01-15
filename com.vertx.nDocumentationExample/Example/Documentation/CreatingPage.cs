using UnityEngine;
using static Vertx.Example.ExtendingPages;
using static Vertx.Example.PageAddition;
using static Vertx.Example.WindowPage;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
	public sealed class CreatingPage : DocumentationPage<ExampleWindow>
	{
		public override ButtonInjection[] InjectedButtonLinks => new[] {new ButtonInjection(typeof(LandingPage), 0)};
		public override Color Color => CreateColor;
		public override string Title => "Creating Pages";

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddHeader(Title, 18, FontStyle.Normal);
			window.AddRichText($"Both Root and Sub-pages can be created with a {DocumentationPageString}.");
			window.AddRichText(@"<code>public class BarPage : DocumentationPage<FooWindow>
{
    public override ButtonInjection[] InjectedButtonLinks => new []{new ButtonInjection(typeof(FooPage), 0)};
    public override Color Color => new Color(1,1,1);
    public override string Title => ""Bar Page"";
    public override void DrawDocumentation(FooWindow window)
    {
        ...
    }
}</code>");
			window.AddRichText($"To add a {DocumentationPageSimpleString} as the Root of a {DocumentationWindowString}, provide a ButtonInjection with a {DocumentationWindowButton} Type in the first index. <b>Eg.</b>\n" +
			                   "<code>public override ButtonInjection[] InjectedButtonLinks => new []{new ButtonInjection(typeof(FooWindow), 0)};</code>");
			window.AddRichText($"You can optionally extend a {DocumentationPageString} with additional content by using a {DocumentationPageAdditionString}. (see {ExtendingPagesButton})");
		}

		public override void DrawDocumentationAfterAdditions(ExampleWindow window) => LandingPage.AddNextButton(window, typeof(ExtendingPages));

		public static readonly string DocumentationPageSimpleString = GetColouredString("DocumentationPage", CreateColor);
		public static readonly string DocumentationPageString = GetColouredString(nameof(DocumentationPage<DocumentationWindow>), CreateColor);
	}
}