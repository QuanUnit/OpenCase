using System;
using System.Text;

namespace Shared.Identifiers
{
    public static class EntityIds
    {
        public const char Separator = '_';

        public static TId Parse<TId>(string value) where TId : EntityId
        {
            return (TId)Activator.CreateInstance(typeof(TId), value);
        }

        public static TId Compose<TId>(string entityName, string variant) where TId : EntityId
        {
            return Parse<TId>(entityName + Separator + ToVariant(variant));
        }

        public static TId CreateUnique<TId>(string entityName) where TId : EntityId
        {
            return Parse<TId>(entityName + Separator + Guid.NewGuid().ToString("N"));
        }

        public static string ToVariant(string text)
        {
            var builder = new StringBuilder(text.Length);
            var isSeparatorPending = false;

            foreach (var symbol in text.Replace("&", " And "))
            {
                if (char.IsLetterOrDigit(symbol) == false)
                {
                    isSeparatorPending = builder.Length > 0;
                    continue;
                }

                if (isSeparatorPending == true)
                {
                    builder.Append(Separator);
                    isSeparatorPending = false;
                }

                builder.Append(symbol);
            }

            return builder.ToString();
        }
    }
}
