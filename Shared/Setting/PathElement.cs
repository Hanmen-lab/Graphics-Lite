using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class PathElement
    {
        public string PathName { get; set; }
        public int PathIndex { get; set; }

        public PathElement PathParent { get; set; }

        public PathElement()
        {

        }
        public PathElement(string name, int index, PathElement parent)
        {
            PathName = name;
            PathIndex = index;
            PathParent = parent;
        }

        public bool Matches(Transform t)
        {
            if (PathParent == null && t.parent != null)
                return false;
            else if (PathParent != null && t.parent == null)
                return false;
            else
                return t.name == PathName && t.GetSiblingIndex() == PathIndex && ((PathParent == null && t.parent == null) || PathParent.Matches(t.parent));
        }

        public override string ToString()
        {
            return PathParent?.ToString() + $"/{PathName}({PathIndex})";
        }

        public static PathElement Build(Transform t)
        {
            return new PathElement(t.name, t.GetSiblingIndex(), t.parent == null ? null : Build(t.parent));
        }
    }
}
