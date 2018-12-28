using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Example
{
	public class RichTextStylesPage : DocumentationPageAddition
	{
		public override Type PageToAddToType => typeof(StylingPage);
		public override float Order => 0;

		public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
		{
			window.AddVerticalSpace(15);
			window.AddHeader("Rich Text", 15);
			
			//Bold
			window.AddRichText("You can style text with <b>bold</b>:\n<code><b>bold</b></code>");
			
			//Italics
			window.AddRichText("You can style text with <i>italics</i>:\n<code><i>italics</i></code>");
			
			//Bold Italics
			window.AddRichText("You can style text with <b><i>italics</i></b>:\n<code><b><i>bold-italics</i></b></code>");
			
			//Colour
			window.AddRichText($"You can style text with {RichTextUtility.GetColouredString("colour", Color.cyan)}:\n<code><color=#00FFFF>color</color></code>");
			
			//Inline Buttons
			window.AddRichText($"You can add inline {RichTextUtility.GetButtonString(buttonKey, RichTextUtility.GetColouredString("buttons", Color.green))}\n<code><button=key><color=#00FF00>\"label\"</color></button></code>");
		}
		
		public const string buttonKey = "button_key_example";
		public override void Initialise(DocumentationWindow window) => window.RegisterButton(buttonKey, ()=> Debug.Log("You can register these buttons in Initialise!"));
	}
}