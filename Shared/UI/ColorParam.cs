using System;
using UnityEngine;

namespace Graphics.UI
{
    /// <summary>
    /// A colour the picker edits: read and written through the owner's own getter and setter, so the
    /// picker never holds a value of its own.
    /// </summary>
    internal sealed class ColorParam
    {
        private readonly Func<Color> _get;
        private readonly Action<Color> _set;

        public ColorParam(Func<Color> get, Action<Color> set, bool alpha = false)
        {
            _get = get;
            _set = set;
            Alpha = alpha;
            Default = get();
        }

        public bool Alpha { get; }
        public Color Default { get; }

        public Color Value
        {
            get { return _get(); }
            set { _set(value); }
        }

        public void Reset()
        {
            Value = Default;
        }
    }
}
