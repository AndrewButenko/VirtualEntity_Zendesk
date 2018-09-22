using System;

namespace D365Saturday.VirtualEntities
{
    internal static class Extensions
    {
        internal static Guid ToGuid(this long value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);

            return new Guid(bytes);
        }

        internal static long ToLong(this Guid value)
        {
            var b = value.ToByteArray();
            return BitConverter.ToInt64(b, 0);
        }
    }
}
