using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Tasks;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Helpers
{
    public static class TranslationHelper
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _translations =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _translationsA =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        public static bool unitTest = false;

        static TranslationHelper()
        {
            loadTranslations();
        }

        private static void loadTranslations()
        {
            try
            {
                using (DataContext db = DataContext.Create())
                {
                    List<Translation> translations = db.Translations.ToList();
                    foreach (Translation translation in translations)
                    {
                        ConcurrentDictionary<string, string> languageTranslations =
                            (translation.Area == TranslationArea.Frontend ? _translations : _translationsA)
                                .GetOrAdd(translation.LanguageCode, new ConcurrentDictionary<string, string>());

                        languageTranslations.TryAdd(translation.Key, translation.Value);
                    }
                }
            }
            catch
            {
            }
        }

        public static string T(this string text)
        {
            string languageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            ConcurrentDictionary<string, string> languageTranslations = _translations.GetOrAdd(languageCode,
                new ConcurrentDictionary<string, string>());

            string translatedText = languageTranslations.GetOrAdd(text, 
                key => saveAndReturnKey(key, languageCode, TranslationArea.Frontend));

            return translatedText;
        }

        public static string TA(this string text)
        {
            string languageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            ConcurrentDictionary<string, string> languageTranslations = _translationsA.GetOrAdd(languageCode,
                new ConcurrentDictionary<string, string>());

            string translatedText = languageTranslations.GetOrAdd(text,
                key => saveAndReturnKey(key, languageCode, TranslationArea.Backend));

            return translatedText;
        }

        private static string saveAndReturnKey(string key, string languageCode, TranslationArea area)
        {
            if (unitTest) return key;

            using (DataContext db = DataContext.Create())
            {
                var translation = new Translation
                                  {
                                      Key = key,
                                      Value = key,
                                      LanguageCode = languageCode,
                                      Area = area
                                  };
                db.Translations.Add(translation);
                db.SaveChanges();
            }

            return key;
        }

        /// <summary>
        ///     Translates a text
        /// </summary>
        public static IHtmlString T(this System.Web.Mvc.HtmlHelper html, string text, params Object[] args)
        {
            return html.Raw(args == null ? text.T() : string.Format(text.T(), args));
        }

        /// <summary>
        ///     Translates a text in the admin tool
        /// </summary>
        public static IHtmlString TA(this System.Web.Mvc.HtmlHelper html, string text, params Object[] args)
        {
            return html.Raw(args == null ? text.TA() : string.Format(text.TA(), args));
        }

        public static void ClearCache()
        {
            _translations.Clear();
            _translationsA.Clear();
            loadTranslations();
        }
    }

    public class CompareTAttribute : CompareAttribute
    {
        public CompareTAttribute(string otherProperty) : base(otherProperty)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(ErrorMessageString.T(), name, OtherPropertyDisplayName ?? OtherProperty);
        }
    }
}