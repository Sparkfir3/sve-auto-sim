using System;
using System.Collections.Generic;

namespace Sparkfire.Utility
{
    public static class ListExtensions
    {
        public static void KeepWhere<T>(this List<T> list, Predicate<T> predicate)
        {
            list.RemoveAll(x => !predicate(x));
        }
    }
}
