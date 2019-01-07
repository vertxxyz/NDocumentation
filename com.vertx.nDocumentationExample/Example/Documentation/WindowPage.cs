using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
	public sealed class WindowPage : DocumentationPage<ExampleWindow>
	{
		public override ButtonInjection[] InjectButtonLinkAbove => null;
		public override ButtonInjection[] InjectButtonLinkBelow => new[] {new ButtonInjection(typeof(LandingPage), -1)};
		public override Color Color => CreateColor;
		public override string Title => "Creating a Window";

		public override void DrawDocumentation(ExampleWindow window, VisualElement root)
		{
			window.AddHeader(Title, 18, FontStyle.Normal);
			window.AddRichText($"A {DocumentationWindowString} is the base Editor Window that displays documentation content.");
			window.AddRichText(@"<code>public class FooWindow : DocumentationWindow
{
	[MenuItem(""Window/Foo Window"")]
	static void Open()
	{
		FooWindow fooWindow = GetWindow<ExampleWindow>();
		fooWindow.Show();
	}

	protected override string StateEditorPrefsKey => ""FooWindow_Prefs_Key"";
	private void OnEnable() => InitialiseDocumentationOnRoot(this, rootVisualElement);

}</code>");

			window.AddRichText($"Once the {DocumentationWindowString} is created it requires a {DocumentationPageRootString}. This page binds specifically to the target window and is the root for navigating to other documentation.\n{GetBoldItalicsString("Home()")} will navigate directly to this page.");

			window.AddRichText(@"<code>public class FooWindowPageRoot : DocumentationPageRoot<FooWindow>
{
	public override void DrawDocumentation(FooWindow window, VisualElement root)
	{
		...
	}
}</code>");
			window.AddRichText($"You can optionally extend a {DocumentationPageRootString} with additional content by using a {PageAddition.DocumentationPageAdditionString}. (see {ExtendingPages.ExtendingPagesButton})");
		}

		public override void DrawDocumentationAfterAdditions(ExampleWindow window, VisualElement root) => LandingPage.AddNextButton(window, typeof(CreatingPage));

		public static readonly Color CreateColor = new Color(1f, 0.11f, 0.33f);
		public static readonly string DocumentationWindowString = GetColouredString(nameof(DocumentationWindow), CreateColor);
		public static readonly string DocumentationPageRootString = GetColouredString(nameof(DocumentationPageRoot<DocumentationWindow>), CreateColor);
	}
}