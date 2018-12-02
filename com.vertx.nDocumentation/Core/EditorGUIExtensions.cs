using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Vertx
{
    public static class EditorGUIExtensions
    {
        #region Search Field

        private static MethodInfo searchField => _searchField ?? (_searchField = typeof(EditorGUI).GetMethod("SearchField", BindingFlags.NonPublic | BindingFlags.Static));

        private static MethodInfo _searchField;
	
        public static string SearchField(Rect r, string searchString) => (string)searchField.Invoke(null, new object[]{r, searchString});

        private static MethodInfo toolbarSearchField => _toolbarSearchField ?? (_toolbarSearchField = typeof(EditorGUI).GetMethod(
                                                            "ToolbarSearchField",
                                                            BindingFlags.NonPublic | BindingFlags.Static,
                                                            null,
                                                            new[]{typeof(Rect), typeof(string), typeof(bool)},
                                                            null)
                                                        );

        private static MethodInfo _toolbarSearchField;
	
        public static string ToolbarSearchField(Rect r, string searchString) => (string)toolbarSearchField.Invoke(null, new object[]{r, searchString, false});

        #endregion
    }
}