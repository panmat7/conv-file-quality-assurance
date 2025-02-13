using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDraft.Helpers
{    public struct Method
    {
        public string Name { get; }
        public string Description { get; }

        public Method(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }


    /// <summary>
    /// A helper class containing the comparison methods
    /// </summary>
    public static class Methods
    {
        public static Method Fonts => new Method("Fonts", "Check to ensure the documents use the same fonts.");
        public static Method Pages => new Method("Pages", "Check to ensure the documents have the same amount of pages.");
        public static Method Resolution => new Method("Resolution", "Check to ensure the resolution of the two images is the same.");
        public static Method ColorSpace => new Method("Color space", "Check to ensure the documents use the same color space.");
        public static Method Animations => new Method("Animations", "Check if the presentation has animations.");


        /// <summary>
        /// Get all the methods in a list
        /// </summary>
        public static List<Method> GetList()
        {
            var methods = new List<Method>();
            var properties = typeof(Methods).GetProperties();
            foreach (var p in properties)
            {
                var m = p.GetValue(null);
                if (m is Method method) methods.Add(method);
            }

            return methods;
        }
    }

}
