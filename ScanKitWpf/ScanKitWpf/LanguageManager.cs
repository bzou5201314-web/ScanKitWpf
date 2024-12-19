using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScanKitWpf
{
    public static class LanguageManager
    {
        public static void SetLanguage(string languageCode)
        {
            var rd = new ResourceDictionary();
            rd.Source = new Uri($"language/Resource.{languageCode}.xaml", UriKind.Relative);
            for (int i=0; i<Application.Current.Resources.MergedDictionaries.Count; i++)
            {
                var item= Application.Current.Resources.MergedDictionaries[i];
                if (item.Source != null && item.Source.OriginalString.Contains("language"))
                {
                    Application.Current.Resources.MergedDictionaries.Remove(item);
                }
            }
            Application.Current.Resources.MergedDictionaries.Add(rd);
        }
    }
}
