using UnityEngine;

namespace Vertx.Example
{
	public class ButtonRegistryPage : DocumentationPage<ExampleWindow>
	{
		public override ButtonInjection[] InjectedButtonLinks => new[] {new ButtonInjection(typeof(LandingPage), 5)};
		public override Color Color => ExtendingPages.InjectColor;
		public override string Title => "Buttons";

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddHeader(Title, 18, FontStyle.Normal);
			window.AddRichText("You can register buttons in Initialise:\n" +
			                   @"<code>public override void Initialise(FooWindow window) => window.RegisterButton(Key, Action);</code>" +
			                   $"\nAfterwards in DrawDocumentation you can use button rich text as specified in {StylingPage.StylingPageButton}, or by using the utility functions to generate a rich text button string:\n" +
			                   @"<code>string richTextButton = RichTextUtility.GetButtonString(Key, ""Label"");</code>" +
			                   "\nWhich can be wrapped in or contain colour tags or other rich text content.\n" +
			                   $"Buttons that link to {CreatingPage.DocumentationPageSimpleString}s do not need to be registered, and <b><i>GetButtonString</i></b> is overloaded to take a type parameter to make this usage consistent.");

			window.AddRichText($"Full Width Buttons function the same way, but do not currently have a rich text representation. Instead they are added manually as specified in {StylingPage.StylingPageButton}.");
			window.AddVerticalSpace(10);
			window.AddSplitter();
			window.AddVerticalSpace(5);
			window.AddRichText($"Details about {LayoutPage.InjectedButtonLinksString} can be found on {CreatingPage.CreatingPageButton}.");
			window.AddVerticalSpace(10);
		}
	}
}