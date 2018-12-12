using System;
using UnityEngine;

namespace Vertx {
	public static class RichTextUtility
	{
		public static string GetButtonString(Type type, string content) => $"<button={type.FullName}>{content}</button>";
		public static string GetButtonString(string key, string content) => $"<button={key}>{content}</button>";
		public static string GetColouredString(string content, Color colour) => $"<color=#{ColorUtility.ToHtmlStringRGBA(colour)}>{content}</color>";
		public static string GetColoredString(string content, Color color) => GetColouredString(content, color);
	}
}