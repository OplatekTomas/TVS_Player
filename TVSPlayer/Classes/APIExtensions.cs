using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVS.API;

namespace TVSPlayer
{
    /// <summary>
    /// Extension class for TVS.API.dll
    /// </summary>
    public static class APIExtensions {

        /// <summary>
        /// Compares episode API values (not custom added)
        /// Only those values that are not empty in parameter are compared
        /// </summary>
        /// <param name="current"></param>
        /// <param name="episode"></param>
        /// <returns></returns>
        public static bool Compare(this Episode current, Episode episode) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(episode);
            var result = currentDictionary.Where(x => dictionary[x.Key] != currentDictionary[x.Key] && !String.IsNullOrEmpty(dictionary[x.Key]) && x.Key != "siteRating").ToDictionary(x=>x.Key,x=>x.Value);
            return result.Count > 0 ? true : false;
        }

        /// <summary>
        /// Compares Series API values (not custom values)
        /// </summary>
        /// <param name="current"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        public static bool Compare(this Series current, Series series) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(series);
            var result = currentDictionary.Where(x => dictionary[x.Key] != currentDictionary[x.Key]).ToDictionary(x => x.Key, x => x.Value);
            return result.Count > 0 ? true : false;
        }

        /// <summary>
        /// Compares Actor values from API
        /// </summary>
        /// <param name="current"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static bool Compare(this Actor current, Actor actor) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(actor);
            var result = currentDictionary.Where(x => dictionary[x.Key] != currentDictionary[x.Key]).ToDictionary(x => x.Key, x => x.Value);
            return result.Count > 0 ? true : false;
        }

        /// <summary>
        /// Compares Poster values from API
        /// </summary>
        /// <param name="current"></param>
        /// <param name="poster"></param>
        /// <returns></returns>
        public static bool Compare(this Poster current, Poster poster) {
            var currentDictionary = GenerateDictionary(current);
            var dictionary = GenerateDictionary(poster);
            var result = currentDictionary.Where(x => dictionary[x.Key] != currentDictionary[x.Key]).ToDictionary(x => x.Key, x => x.Value);
            return result.Count > 0 ? true : false;
        }

        /// <summary>
        /// Reads all properties from object and returns them in a dictionary key = name , value = value
        /// </summary>
        /// <param name="obj">Any object</param>
        /// <returns></returns>
        private static Dictionary<string,string> GenerateDictionary(object obj) {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties) {
                dictionary.Add(property.Name, property.GetValue(obj)?.ToString());
            }
            var fields = obj.GetType().GetFields();
            foreach (var field in fields) {
                dictionary.Add(field.Name, field.GetValue(obj)?.ToString());
            }
            var values = new List<string>() { "files", "continueAt", "thumbnail", "finished", "autoDownload", "defaultPoster", "libraryPath" };
            dictionary = dictionary.Except(dictionary.Where(x => values.Contains(x.Key)).ToList()).ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }

    }
}
