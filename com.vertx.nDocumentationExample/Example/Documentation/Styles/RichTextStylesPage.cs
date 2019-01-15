using System;
using UnityEngine;

namespace Vertx.Example
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RichTextStylesPage : DocumentationPageAddition<ExampleWindow>
	{
		public override Type PageToAddToType => typeof(StylingPage);
		public override float Order => 0;

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddVerticalSpace(15);
			window.AddHeader("Rich Text", 15);
			
			//Rich Text
			window.AddRichText("You can add Rich Text using:\n<code>window.AddRichText(text);</code>");
			
			//Bold
			window.AddRichText("You can style text with <b>bold</b>:\n<code>\"<b>bold</b>\"</code>");
			
			//Italics
			window.AddRichText("You can style text with <i>italics</i>:\n<code>\"<i>italics</i>\"</code>");
			
			//Bold Italics
			window.AddRichText("You can style text with <b><i>bold-italics</i></b>:\n<code>\"<b><i>bold-italics</i></b>\"</code>");
			
			//Colour
			window.AddRichText($"You can style text with {RichTextUtility.GetColouredString("colour", Color.cyan)}:\n<code>\"<color=#00FFFF>color</color>\"</code>");
			
			//Inline Buttons
			window.AddRichText($"You can add inline {RichTextUtility.GetButtonString(ButtonKey, RichTextUtility.GetColouredString("buttons", Color.green))}\n<code>\"<button=key><color=#00FF00>label</color></button>\"</code>");
		}
		
		public const string ButtonKey = "button_key_example";
		public override void Initialise(ExampleWindow window) => window.RegisterButton(ButtonKey, ()=> window.GoToPage(typeof(ButtonRegistryPage)));
	}
}