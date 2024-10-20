using UnityEngine;

namespace CustomUnity
{
    public static class ColorExtension
    {
        public static Color SetA(this Color self, float a) => new(self.r, self.g, self.b, a);

        public static Color SetR(this Color self, float r) => new(r, self.g, self.b, self.a);

        public static Color SetG(this Color self, float g) => new(self.r, g, self.b, self.a);

        public static Color SetB(this Color self, float b) => new(self.r, self.g, b, self.a);

        public static Color SetRGB(this Color self, float r, float g, float b) => new(r, g, b, self.a);

        public static Color SetRGB(this Color self, Color other) => new(other.r, other.g, other.b, self.a);
    }
}