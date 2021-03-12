
// ReSharper disable InconsistentNaming

using System.Linq;
using System.Reflection;
using System.Text;
using Hai.ExpressionsEditor.Scripts.Editor.Internal;

namespace Hai.ExpressionsEditor.Scripts.Editor.EditorUI
{
    public class EeLocale
    {
        private static string EE_Documentation_URL => LocalizeOrElse("EE_Documentation_URL", EeLocaleDefaults.EE_Documentation_URL);
        //
        internal static string EEA_Open_editor => LocalizeOrElse("EEA_Open_editor", EeLocaleDefaults.EEA_Open_editor);

        private static string LocalizeOrElse(string key, string defaultCultureLocalization)
        {
            return EeLocalization.LocalizeOrElse(key, defaultCultureLocalization);
        }

        public static string DocumentationUrl()
        {
            var localizedUrl = EE_Documentation_URL;
            return localizedUrl.StartsWith(EeLocaleDefaults.OfficialDocumentationUrlAsPrefix) ? localizedUrl : EeLocaleDefaults.EE_Documentation_URL;
        }

        public static string CompileDefaultLocaleJson()
        {
            var fields = typeof(EeLocaleDefaults).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            var jsonObject = new JSONObject();
            foreach (var field in fields.Where(info => info.Name.StartsWith("EE")))
            {
                jsonObject[field.Name] = new JSONString((string) field.GetValue(null));
            }

            var sb = new StringBuilder();
            jsonObject.WriteToStringBuilder(sb, 0, 0, JSONTextMode.Indent);
            return sb.ToString();
        }
    }
}
