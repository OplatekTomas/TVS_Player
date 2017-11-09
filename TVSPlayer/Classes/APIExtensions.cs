using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVS.API;

namespace TVSPlayer
{
    public static class APIExtensions {
        public static bool Compare(this Episode current, Episode episode) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(episode);
            var result = currentDictionary.Where(x => dictionary[x.Key] != x.Value && dictionary[x.Key] != null && dictionary["siteRating"] != "0").ToDictionary(x=>x.Key,x=>x.Value);
            return result.Count > 0 ? true : false;
        }

        public static bool Compare(this Series current, Series series) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(series);
            var result = currentDictionary.Where(x => dictionary[x.Key] != x.Value).ToDictionary(x => x.Key, x => x.Value);
            return result.Count > 0 ? true : false;
        }

        public static bool Compare(this Actor current, Actor actor) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(actor);
            var result = currentDictionary.Where(x => dictionary[x.Key] != x.Value).ToDictionary(x => x.Key, x => x.Value);
            return result.Count > 0 ? true : false;
        }
        public static bool Compare(this Poster current, Poster poster) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(poster);
            var result = currentDictionary.Where(x => dictionary[x.Key] != x.Value).ToDictionary(x => x.Key, x => x.Value);
            return result.Count > 0 ? true : false;
        }

        private static Dictionary<string,string> GenerateDictionary(object obj) {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties) {
                dictionary.Add(property.Name, property.GetValue(obj) != null ? property.GetValue(obj).ToString() : null);
            }
            var fields = obj.GetType().GetFields();
            foreach (var field in fields) {
                dictionary.Add(field.Name, field.GetValue(obj) != null ? field.GetValue(obj).ToString() : null);
            }
            var values = new List<string>() { "files", "continueAt", "thumbnail", "finished", "autoDownload", "defaultPoster", "libraryPath" };
            dictionary = dictionary.Except(dictionary.Where(x => values.Contains(x.Key)).ToList()).ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }

    }
}
