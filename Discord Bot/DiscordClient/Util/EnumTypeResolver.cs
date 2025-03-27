using System;

namespace DiscordClient.Utilities
{
    public static class EnumTypeResolver
    {
        public static bool TryResolve<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
        {
            return Enum.TryParse(value, true, out result);
        }
    }
}