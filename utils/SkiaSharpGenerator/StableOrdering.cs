using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkiaSharpGenerator
{
	internal static class StableOrdering
	{
		public static IOrderedEnumerable<T> ByName<T>(
			IEnumerable<T> items,
			Func<T, string> nameSelector) =>
			items.OrderBy(nameSelector, StringComparer.Ordinal);

		public static IOrderedEnumerable<T> ByPathThenName<T>(
			IEnumerable<T> items,
			string root,
			Func<T, string> pathSelector,
			Func<T, string> nameSelector) =>
			items
				.OrderBy(item => NormalizePath(root, pathSelector(item)), StringComparer.Ordinal)
				.ThenBy(nameSelector, StringComparer.Ordinal);

		public static IReadOnlyList<string> EnumerateFiles(
			string root,
			IEnumerable<KeyValuePair<string, string[]>> fileGroups,
			Func<string, string, IEnumerable<string>> enumerateFiles) =>
			fileGroups
				.OrderBy(group => group.Key, StringComparer.Ordinal)
				.SelectMany(group => group.Value
					.OrderBy(filter => filter, StringComparer.Ordinal)
					.SelectMany(filter => enumerateFiles(Path.Combine(root, group.Key), filter)))
				.OrderBy(path => NormalizePath(root, path), StringComparer.Ordinal)
				.ToArray();

		public static IReadOnlyList<string> EnumerateFiles(
			string root,
			IEnumerable<string> filters,
			Func<string, string, IEnumerable<string>> enumerateFiles) =>
			filters
				.OrderBy(filter => filter, StringComparer.Ordinal)
				.SelectMany(filter => enumerateFiles(root, filter))
				.OrderBy(path => NormalizePath(root, path), StringComparer.Ordinal)
				.ToArray();

		public static string NormalizePath(string root, string path)
		{
			var relativePath = Path.GetRelativePath(Path.GetFullPath(root), Path.GetFullPath(path));
			return relativePath.Replace('\\', '/');
		}
	}
}
