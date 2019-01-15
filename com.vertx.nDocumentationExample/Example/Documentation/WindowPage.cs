using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
	public sealed class WindowPage : DocumentationPage<ExampleWindow>
	{
		public override ButtonInjection[] InjectedButtonLinks => new[] {new ButtonInjection(typeof(LandingPage), -1)};
		public override Color Color => CreateColor;
		public override string Title => "Creating a Window";

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddHeader(Title, 18, FontStyle.Normal);
			window.AddRichText($"A {DocumentationWindowString} is the base Editor Window that displays documentation content.");
			window.AddRichText(@"<code>public class FooWindow : DocumentationWindow
{
	[MenuItem(""Window/Foo Window"")]
	static void Open()
	{
		FooWindow fooWindow = GetWindow<FooWindow>();
		fooWindow.Show();
	}

	protected override string StateEditorPrefsKey => ""FooWindow_Prefs_Key"";
	private void OnEnable() => InitialiseDocumentationOnRoot(this, rootVisualElement);
}</code>");
		}

		public override void DrawDocumentationAfterAdditions(ExampleWindow window) => LandingPage.AddNextButton(window, typeof(CreatingPage));

		public static readonly Color CreateColor = new Color(1f, 0.11f, 0.33f);
		public static readonly string DocumentationWindowString = GetColouredString(nameof(DocumentationWindow), CreateColor);
		public static readonly string DocumentationWindowButton = GetButtonString(typeof(WindowPage), DocumentationWindowString);
	}
}