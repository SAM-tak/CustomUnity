using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUnity
{
    public static class ColorExtension
    {
        public static Color SetA(this Color self, float a)
        {
            return new Color(self.r, self.g, self.b, a);
        }

        public static Color SetR(this Color self, float r)
        {
            return new Color(r, self.g, self.b, self.a);
        }

        public static Color SetG(this Color self, float g)
        {
            return new Color(self.r, g, self.b, self.a);
        }

        public static Color SetB(this Color self, float b)
        {
            return new Color(self.r, self.g, b, self.a);
        }

        public static Color SetRGB(this Color self, float r, float g, float b)
        {
            return new Color(r, g, b, self.a);
        }

        public static Color SetRGB(this Color self, Color other)
        {
            return new Color(other.r, other.g, other.b, self.a);
        }
    }
}