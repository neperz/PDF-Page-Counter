using System;

namespace PDF_Page_Counter.Cripto
{

    internal static class ArrayExtensions
    {
        public static T[] Append<T>(this T[] data, T[] arrayToAppend)
        {
            int length = (int)data.Length;
            Array.Resize<T>(ref data, (int)data.Length + (int)arrayToAppend.Length);
            Buffer.BlockCopy(arrayToAppend, 0, data, length, (int)arrayToAppend.Length);
            return data;
        }

        public static T[] CloneSegment<T>(this ArraySegment<T> data)
        {
            return data.Array.SubArray<T>(data.Offset, data.Count);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] tArray = new T[length];
            Buffer.BlockCopy(data, index, tArray, 0, length);
            return tArray;
        }
    }
}
