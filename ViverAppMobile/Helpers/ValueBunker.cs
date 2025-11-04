using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Helpers
{
    public static class ValueBunker<T>
    {
        public static T? SavedValue { get; set; }
        public static void Clear() => SavedValue = default;

        public static void Build(T tvalue)
        {
            Clear();
            SavedValue = tvalue;
        }
    }

    public static class ValueBunker<T1, T2>
    {
        public static T1? SavedValue1 { get; set; }
        public static T2? SavedValue2 { get; set; }

        public static void Build(T1 t1, T2 t2)
        {
            Clear();

            SavedValue1 = t1;
            SavedValue2 = t2;
        }

        public static void Clear()
        {
            SavedValue1 = default;
            SavedValue2 = default;
        }
    }
}
