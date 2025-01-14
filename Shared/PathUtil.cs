using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Graphics
{
    internal static class PathUtils
    {
        private static readonly char[] PathSeparators = new char[] { '\\', '/', ':' };
        private static readonly char[] PathSeparatorsWithExtension = new char[] { '\\', '/', ':', '.' };

        public static string GetAbsolutePath(string basePath, string relativePath)
        {
            return NormalizePath(Path.Combine(basePath ?? "", relativePath ?? ""));
        }

        public static string GetRelativePathWithoutExtension(string basePath, string absolutePath)
        {
            string path = GetRelativePath(basePath, absolutePath);
            int index = path.LastIndexOfAny(PathSeparatorsWithExtension);
            return index != -1 && path[index] == '.' ? path.Remove(index) : path;
        }

        public static string GetRelativePath(string basePath, string absolutePath, string emptyPath = "")
        {
            if (string.IsNullOrEmpty(absolutePath))
                return emptyPath;
            string relativeTo = !string.IsNullOrEmpty(basePath) ?
                NormalizePath(basePath) : Directory.GetCurrentDirectory();
            string path = NormalizePath(absolutePath);
            int relativeToLength = relativeTo.Length, pathLength = path.Length;
            if (relativeTo[relativeToLength - 1] != '\\')
                relativeToLength = (relativeTo += '\\').Length;
            if (path[pathLength - 1] != '\\')
                pathLength = (path += '\\').Length;
            int length = Math.Min(relativeToLength, pathLength), i = 0, pathStart = 0;
            while (i < length && PathCharEquals(relativeTo[i], path[i]))
                if (path[i++] == '\\')
                    pathStart = i;
            if ((length = GetRootLength(path)) > pathStart)
                return path.Remove(Math.Max(pathLength - 1, length));
            int count = relativeTo.Skip(i).Where(c => c == '\\').Count();
            if ((length = count * 3 + (pathLength -= pathStart)) == 0)
                return emptyPath;
            StringBuilder sb = new StringBuilder(length);
            sb.Insert(0, "..\\", count).Append(path, pathStart, pathLength).Length--;
            return sb.ToString();
        }

        private static int GetRootLength(string path)
        {
            int length = path.Length, i;
            if (length <= 0)
                return 0;
            char c;
            if ((c = path[0]) != '\\' && c != '/')
                return
                    length >= 2 && path[1] == ':' && PathCharIsAlpha(c) ?
                    length >= 3 && ((c = path[2]) == '\\' || c == '/') ?
                    3 : 2 : 0;
            if (length < 2 || (c = path[1]) != '\\' && c != '/')
                return 1;
            i = length >= 4 && ((c = path[2]) == '.' || c == '?') && ((c = path[3]) == '\\' || c == '/') ?
                length >= 8 && ((c = path[7]) == '\\' || c == '/') && path[4] == 'U' && path[5] == 'N' && path[6] == 'C' ?
                8 : 4 : 2;
            for (int j = i != 4 ? 2 : 1; i < length;)
                if (((c = path[i++]) == '\\' || c == '/') && --j == 0)
                    break;
            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PathCharIsAlpha(char c) =>
            (uint)((c | ('a' - 'A')) - 'a') < 'z' - 'a' + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char PathCharToLower(char c) =>
            (uint)(c - 'A') < 'Z' - 'A' + 1 ? (char)(c + ('a' - 'A')) : c;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PathCharEquals(char lhs, char rhs) =>
            lhs == rhs ||
            (lhs |= (char)('a' - 'A')) == (rhs | (char)('a' - 'A')) &&
            (uint)(lhs - 'a') < 'z' - 'a' + 1;

        public static bool PathEquals(string lhs, string rhs)
        {
            int length = lhs.Length;
            if (length != rhs.Length)
                return false;
            for (int i = length - 1; i >= 0; i--)
            {
                char a, b;
                if (PathCharEquals(a = lhs[i], b = rhs[i]))
                    continue;
                if (a == '\\' ? b == '/' : a == '/' && b == '\\')
                    continue;
                return false;
            }
            return true;
        }

        public class PathComparer : IComparer<string>
        {
            public int Compare(string lhs, string rhs)
            {
                int x = lhs.Length, y = rhs.Length;
                int count = Math.Min(x, y);
                for (int i = 0; i < count; i++)
                {
                    char a, b;
                    if (PathCharEquals(a = lhs[i], b = rhs[i]))
                        continue;
                    if (a == '\\' ? b == '/' : a == '/' && b == '\\')
                        continue;
                    if ((x = lhs.IndexOfAny(PathSeparators, i)) != -1)
                        x = (lhs[x] + 1) & 2;
                    if ((y = rhs.IndexOfAny(PathSeparators, i)) != -1)
                        y = (rhs[y] + 1) & 2;
                    if ((y -= x) != 0)
                        return y;
                    return PathCharToLower(a) - PathCharToLower(b);
                }
                if (x > y)
                    return lhs.IndexOfAny(PathSeparators, y) ^ -2;
                if (x < y)
                    return rhs.IndexOfAny(PathSeparators, x) | 1;
                return 0;
            }
        }

        public static bool IsSubPathOf(this string subPath, string basePath)
        {
            subPath = NormalizePath(Path.Combine(subPath ?? "", ".\\"));
            basePath = NormalizePath(Path.Combine(basePath ?? "", ".\\"));
            return subPath.Length >= basePath.Length && PathEquals(subPath.Remove(basePath.Length), basePath);
        }

        public static string NormalizePath(string path)
        {
            if (path != null)
            {
                path = Path.GetFullPath(path);
                if (path.Length >= 2 && path[0] == '\\' && path[1] == '\\')
                {
                    int i;
                    if (path.Length < 4 || path[3] != '\\')
                    {
                        i = 2;
                    }
                    else
                    {
                        i = 4;
                        if (path[2] != '?')
                        {
                            if (path[2] != '.')
                                return path;
                            if (path.Length >= 8 && path[7] == '\\' && path[4] == 'U' && path[5] == 'N' && path[6] == 'C')
                                i = 8;
                        }
                    }
                    if (path.Length >= i + 10 && path[i + 9] == '\\' && (
                        path[i    ] == '1' ?
                        path[i + 1] == '2' &&
                        path[i + 2] == '7' &&
                        path[i + 3] == '.' &&
                        path[i + 4] == '0' &&
                        path[i + 5] == '.' &&
                        path[i + 6] == '0' &&
                        path[i + 7] == '.' &&
                        path[i + 8] == '1' :
                        (path[i    ] | ('a' - 'A')) == 'l' &&
                        (path[i + 1] | ('a' - 'A')) == 'o' &&
                        (path[i + 2] | ('a' - 'A')) == 'c' &&
                        (path[i + 3] | ('a' - 'A')) == 'a' &&
                        (path[i + 4] | ('a' - 'A')) == 'l' &&
                        (path[i + 5] | ('a' - 'A')) == 'h' &&
                        (path[i + 6] | ('a' - 'A')) == 'o' &&
                        (path[i + 7] | ('a' - 'A')) == 's' &&
                        (path[i + 8] | ('a' - 'A')) == 't'))
                    {
                        i += 10;
                        if (path.Length >= i + 2 && path[i + 1] == '$' && PathCharIsAlpha(path[i]))
                        {
                            var chars = path.ToCharArray();
                            chars[i + 1] = ':';
                            path = new string(chars);
                        }
                    }
                    if (i >= 4)
                        path = Path.GetFullPath(path.Substring(i));
                }
            }
            return path;
        }
    }
}
