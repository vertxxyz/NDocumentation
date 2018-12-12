using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Vertx
{
	public static class DocumentationUtility
	{
		#region Search

		public enum SearchFilter
		{
			All,
			Assets,
			Packages
		}

		static string[] GetSearchDirs(SearchFilter searchAssets)
		{
			string[] searchDirs;
			switch (searchAssets)
			{
				case SearchFilter.All:
					searchDirs = new[] {"Assets", "Packages"};
					break;
				case SearchFilter.Assets:
					searchDirs = new[] {"Assets"};
					break;
				case SearchFilter.Packages:
					searchDirs = new[] {"Packages"};
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(searchAssets), searchAssets, null);
			}

			return searchDirs;
		}

		/// <summary>
		/// Load an asset of type (used for loading single-instance ScriptableObjects like default assets)
		/// </summary>
		/// <param name="contains">a string that must be in the file name (can be null)</param>
		/// <param name="searchAssets">What directories to search. Assets, Packages, or Both.</param>
		/// <param name="error">error to fire if the result isn't found or is null (can be null)</param>
		/// <param name="success">action to fire if the asset is found (can be null)</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T LoadAssetOfType<T>(string contains = null, SearchFilter searchAssets = SearchFilter.All, Action error = null, Action success = null) where T : Object
		{
			bool allowScriptAssets = typeof(T) == typeof(MonoScript);

			T t = null;
			string[] assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(T).Name}", GetSearchDirs(searchAssets));
			foreach (var assetGUID in assetGUIDs)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
				if (string.IsNullOrEmpty(assetPath) || !allowScriptAssets && assetPath.EndsWith(".cs") || contains != null && !Path.GetFileName(assetPath).Contains(contains)) continue;
				t = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				break;
			}

			if (t == null)
				error?.Invoke();
			else
				success?.Invoke();

			return t;
		}

		public static T[] LoadAssetsOfType<T>(string contains = null, SearchFilter searchAssets = SearchFilter.All, Action error = null, Action success = null) where T : Object
		{
			bool allowScriptAssets = typeof(T) == typeof(MonoScript);

			List<T> tToReturn = new List<T>();
			string[] assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(T).Name}", GetSearchDirs(searchAssets));
			foreach (var assetGUID in assetGUIDs)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
				if (string.IsNullOrEmpty(assetPath) || !allowScriptAssets && assetPath.EndsWith(".cs") || (contains != null && !Path.GetFileName(assetPath).Contains(contains))) continue;
				tToReturn.Add(AssetDatabase.LoadAssetAtPath<T>(assetPath));
			}

			if (tToReturn.Count == 0)
				error?.Invoke();
			else
				success?.Invoke();

			return tToReturn.ToArray();
		}

		#endregion

		private static Type _editorAssembliesType;
		private static Type editorAssembliesType => _editorAssembliesType ?? (_editorAssembliesType = Type.GetType("UnityEditor.EditorAssemblies,UnityEditor"));

		/// <summary>
		/// Returns an array of Types inherited from Type T in the Loaded (user & Unity) Assemblies.
		/// This function also operates with interfaces.
		/// </summary>
		private static IEnumerable<Type> GetTypesOfExtensions<T>() => (IEnumerable<Type>) SubclassesOf_Method.Invoke(null, new object[] {typeof(T)});

		private static MethodInfo _SubclassesOf_Method;
		private static MethodInfo SubclassesOf_Method => _SubclassesOf_Method ?? (_SubclassesOf_Method = editorAssembliesType.GetMethod("SubclassesOf", BindingFlags.Static | BindingFlags.NonPublic));

		public static IEnumerable<T> GetExtensionsOfTypeIE<T>()
		{
			IEnumerable<Type> typesOfEditorExtensions = GetTypesOfExtensions<T>();
			List<T> extensions = new List<T>();
			foreach (Type t in typesOfEditorExtensions)
			{
				if (t.IsAbstract)
					continue;
				extensions.Add((T) Activator.CreateInstance(t));
			}

			return extensions;
		}
	}
}