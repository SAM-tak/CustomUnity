using System;
using System.Text;
using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Rich text.
    /// </summary>
    /// <code>
    /// Example :
    /// Debug.Log(RichText.Sb.Bold(sb => sb.Red("blahblah")).Ln().Small(sb => sb.Grey("foobar").Space().Color(0x443322FF, "hogehoge")), gameObject);
    /// or
    /// Debug.Log("blahblah".Red().Bold() + "\n" + ("foobar".Grey() + " " + "hogehoge".Color(0x443322FF)).Small(), gameObject);
    /// both generate a same string like below :
    /// \<b\>\<color=red\>blahblah\</color\>\</b\>
    /// \<size=9\>\<color=grey\>foobar\</color\> \<color=#443322FF\>hogehoge\</color\>\</size\>
    /// </code>
    public static class RichText
    {
        #region String Extention
        public static string Color(this string str, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + str + "</color>";
        }

        public static string Color(this string str, Color32 color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + str + "</color>";
        }

        public static string Color(this string str, uint color)
        {
            return "<color=#" + color.ToString("X8") + ">" + str + "</color>";
        }

        public static string Color(this string str, string color)
        {
            return "<color=" + color + ">" + str + "</color>";
        }

        // aqua (same as cyan)	#00ffffff
        public static string Aqua(this string str)
        {
            return "<color=aqua>" + str + "</color>";
        }

        // black	#000000ff
        public static string Black(this string str)
        {
            return "<color=black>" + str + "</color>";
        }

        // blue	#0000ffff
        public static string Blue(this string str)
        {
            return "<color=blue>" + str + "</color>";
        }

        // brown	#a52a2aff
        public static string Brown(this string str)
        {
            return "<color=brown>" + str + "</color>";
        }

        // cyan (same as aqua)	#00ffffff
        public static string Cyan(this string str)
        {
            return "<color=cyan>" + str + "</color>";
        }

        // darkblue	#0000a0ff
        public static string DarkBlue(this string str)
        {
            return "<color=darkblue>" + str + "</color>";
        }

        // fuchsia (same as magenta)	#ff00ffff
        public static string Fuchsia(this string str)
        {
            return "<color=fuchsia>" + str + "</color>";
        }

        // green	#008000ff
        public static string Green(this string str)
        {
            return "<color=green>" + str + "</color>";
        }

        // grey	#808080ff
        public static string Grey(this string str)
        {
            return "<color=grey>" + str + "</color>";
        }

        // lightblue	#add8e6ff
        public static string LightBlue(this string str)
        {
            return "<color=lightblue>" + str + "</color>";
        }

        // lime	#00ff00ff
        public static string Lime(this string str)
        {
            return "<color=lime>" + str + "</color>";
        }

        // magenta (same as fuchsia)	#ff00ffff	
        public static string Magenta(this string str)
        {
            return "<color=magenta>" + str + "</color>";
        }

        // maroon	#800000ff	
        public static string Maroon(this string str)
        {
            //return "<color=maroon>" + str + "</color>";
            return "<color=#800000ff>" + str + "</color>"; // MeshTextProがmaroonに対応してない
        }

        // navy	#000080ff	
        public static string Navy(this string str)
        {
            return "<color=navy>" + str + "</color>";
        }

        // olive	#808000ff
        public static string Olive(this string str)
        {
            return "<color=olive>" + str + "</color>";
        }

        // orange	#ffa500ff
        public static string Orange(this string str)
        {
            return "<color=orange>" + str + "</color>";
        }

        // purple	#800080ff
        public static string Purple(this string str)
        {
            return "<color=purple>" + str + "</color>";
        }

        // red	#ff0000ff
        public static string Red(this string str)
        {
            return "<color=red>" + str + "</color>";
        }

        // silver	#c0c0c0ff
        public static string Silver(this string str)
        {
            return "<color=silver>" + str + "</color>";
        }

        // teal	#008080ff
        public static string Teal(this string str)
        {
            return "<color=teal>" + str + "</color>";
        }

        // white	#ffffffff
        public static string White(this string str)
        {
            return "<color=white>" + str + "</color>";
        }

        // yellow	#ffff00ff
        public static string Yellow(this string str)
        {
            return "<color=yellow>" + str + "</color>";
        }

        public static string Size(this string str, int size)
        {
            return "<size=" + size + ">" + str + "</size>";
        }

        public static string Small(this string str)
        {
            return str.Size(9);
        }

        public static string Big(this string str)
        {
            return str.Size(16);
        }

        public static string Bold(this string str)
        {
            return "<b>" + str + "</b>";
        }

        public static string Italic(this string str)
        {
            return "<i>" + str + "</i>";
        }

        public static string Material(this string str, int index)
        {
            return "<material=" + index + ">" + str + "</material>";
        }

        public static string Quad(int material, int size, float x, float y, float width, float height)
        {
            return "<quad material=" + material + " size=" + size + " x=" + x + " y=" + y + " width=" + width + " height=" + height + "/>";
            //return string.Format("<quad material={0} size={1} x={2} y={3} width={4} height={5}>{6}</quad>", material, size, x, y, width, height, str);
        }

        public static string Quad(int material, int size, Rect rect)
        {
            return "<quad material=" + material + " size=" + size + " x=" + rect.x + " y=" + rect.y + " width=" + rect.width + " height=" + rect.height + "/>";
            //return string.Format("<quad material={0} size={1} x={2} y={3} width={4} height={5}>{6}</quad>", material, fontsize, rect.x, rect.y, rect.width, rect.height, str);
        }

        #endregion

        #region StringBuilder Extention

        static readonly Lazy<StringBuilder> sb = new Lazy<StringBuilder>();
        public static StringBuilder Sb { get { return sb.Value; } }

        public static StringBuilder Clear(this StringBuilder sb)
        {
            sb.Length = 0;
            return sb;
        }

        public static StringBuilder Ln(this StringBuilder sb)
        {
            return sb.AppendLine();
        }

        public static StringBuilder Space(this StringBuilder sb)
        {
            return sb.Append(' ');
        }

        public static StringBuilder Space(this StringBuilder sb, int count)
        {
            return sb.Append(' ', count);
        }

        public static StringBuilder Comma(this StringBuilder sb)
        {
            return sb.Append(", ");
        }

        public static StringBuilder Action(this StringBuilder sb, Action<StringBuilder> action)
        {
            action(sb);
            return sb;
        }

        public static StringBuilder Color(this StringBuilder sb, Color color, object message)
        {
            return sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(color)).Append(">").Append(message).Append("</color>");
        }

        public static StringBuilder Color(this StringBuilder sb, Color color, Action<StringBuilder> action)
        {
            return sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(color)).Append(">").Action(action).Append("</color>");
        }

        public static StringBuilder Color(this StringBuilder sb, Color32 color, object message)
        {
            return sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(color)).Append(">").Append(message).Append("</color>");
        }

        public static StringBuilder Color(this StringBuilder sb, Color32 color, Action<StringBuilder> action)
        {
            return sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(color)).Append(">").Action(action).Append("</color>");
        }

        public static StringBuilder Color(this StringBuilder sb, uint color, object message)
        {
            return sb.Append("<color=#").Append(color.ToString("X8")).Append(">").Append(message).Append("</color>");
        }

        public static StringBuilder Color(this StringBuilder sb, uint color, Action<StringBuilder> action)
        {
            return sb.Append("<color=#").Append(color.ToString("X8")).Append(">").Action(action).Append("</color>");
        }

        public static StringBuilder Color(this StringBuilder sb, string color, object message)
        {
            return sb.Append("<color=").Append(color).Append(">").Append(message).Append("</color>");
        }

        public static StringBuilder Color(this StringBuilder sb, string color, Action<StringBuilder> action)
        {
            return sb.Append("<color=").Append(color).Append(">").Action(action).Append("</color>");
        }

        // aqua (same as cyan)	#00ffffff
        public static StringBuilder Aqua(this StringBuilder sb, object message)
        {
            return sb.Append("<color=aqua>").Append(message).Append("</color>");
        }

        public static StringBuilder Aqua(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=aqua>").Action(action).Append("</color>");
        }

        // black	#000000ff
        public static StringBuilder Black(this StringBuilder sb, object message)
        {
            return sb.Append("<color=black>").Append(message).Append("</color>");
        }

        public static StringBuilder Black(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=black>").Action(action).Append("</color>");
        }

        // blue	#0000ffff
        public static StringBuilder Blue(this StringBuilder sb, object message)
        {
            return sb.Append("<color=blue>").Append(message).Append("</color>");
        }

        public static StringBuilder Blue(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=blue>").Action(action).Append("</color>");
        }

        // brown	#a52a2aff
        public static StringBuilder Brown(this StringBuilder sb, object message)
        {
            return sb.Append("<color=brown>").Append(message).Append("</color>");
        }

        public static StringBuilder Brown(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=brown>").Action(action).Append("</color>");
        }

        // cyan (same as aqua)	#00ffffff
        public static StringBuilder Cyan(this StringBuilder sb, object message)
        {
            return sb.Append("<color=cyan>").Append(message).Append("</color>");
        }

        public static StringBuilder Cyan(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=cyan>").Action(action).Append("</color>");
        }

        // darkblue	#0000a0ff
        public static StringBuilder DarkBlue(this StringBuilder sb, object message)
        {
            return sb.Append("<color=darkblue>").Append(message).Append("</color>");
        }

        public static StringBuilder DarkBlue(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=darkblue>").Action(action).Append("</color>");
        }

        // fuchsia (same as magenta)	#ff00ffff
        public static StringBuilder Fuchsia(this StringBuilder sb, object message)
        {
            return sb.Append("<color=fuchsia>").Append(message).Append("</color>");
        }

        public static StringBuilder Fuchsia(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=fuchsia>").Action(action).Append("</color>");
        }

        // green	#008000ff
        public static StringBuilder Green(this StringBuilder sb, object message)
        {
            return sb.Append("<color=green>").Append(message).Append("</color>");
        }

        public static StringBuilder Green(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=green>").Action(action).Append("</color>");
        }

        // grey	#808080ff
        public static StringBuilder Grey(this StringBuilder sb, object message)
        {
            return sb.Append("<color=grey>").Append(message).Append("</color>");
        }

        public static StringBuilder Grey(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=grey>").Action(action).Append("</color>");
        }

        // lightblue	#add8e6ff
        public static StringBuilder LightBlue(this StringBuilder sb, object message)
        {
            return sb.Append("<color=lightblue>").Append(message).Append("</color>");
        }

        public static StringBuilder LightBlue(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=lightblue>").Action(action).Append("</color>");
        }

        // lime	#00ff00ff
        public static StringBuilder Lime(this StringBuilder sb, object message)
        {
            return sb.Append("<color=lime>").Append(message).Append("</color>");
        }

        public static StringBuilder Lime(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=lime>").Action(action).Append("</color>");
        }

        // magenta (same as fuchsia)	#ff00ffff	
        public static StringBuilder Magenta(this StringBuilder sb, object message)
        {
            return sb.Append("<color=magenta>").Append(message).Append("</color>");
        }

        public static StringBuilder Magenta(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=magenta>").Action(action).Append("</color>");
        }

        // maroon	#800000ff	
        public static StringBuilder Maroon(this StringBuilder sb, object message)
        {
            return sb.Append("<color=maroon>").Append(message).Append("</color>");
        }

        public static StringBuilder Maroon(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=maroon>").Action(action).Append("</color>");
        }

        // navy	#000080ff	
        public static StringBuilder Navy(this StringBuilder sb, object message)
        {
            return sb.Append("<color=navy>").Append(message).Append("</color>");
        }

        public static StringBuilder Navy(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=navy>").Action(action).Append("</color>");
        }

        // olive	#808000ff
        public static StringBuilder Olive(this StringBuilder sb, object message)
        {
            return sb.Append("<color=olive>").Append(message).Append("</color>");
        }

        public static StringBuilder Olive(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=olive>").Action(action).Append("</color>");
        }

        // orange	#ffa500ff
        public static StringBuilder Orange(this StringBuilder sb, object message)
        {
            return sb.Append("<color=orange>").Append(message).Append("</color>");
        }

        public static StringBuilder Orange(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=orange>").Action(action).Append("</color>");
        }

        // purple	#800080ff
        public static StringBuilder Purple(this StringBuilder sb, object message)
        {
            return sb.Append("<color=purple>").Append(message).Append("</color>");
        }

        public static StringBuilder Purple(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=purple>").Action(action).Append("</color>");
        }

        // red	#ff0000ff
        public static StringBuilder Red(this StringBuilder sb, object message)
        {
            return sb.Append("<color=red>").Append(message).Append("</color>");
        }

        public static StringBuilder Red(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=red>").Action(action).Append("</color>");
        }

        // silver	#c0c0c0ff
        public static StringBuilder Silver(this StringBuilder sb, object message)
        {
            return sb.Append("<color=silver>").Append(message).Append("</color>");
        }

        public static StringBuilder Silver(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=silver>").Action(action).Append("</color>");
        }

        // teal	#008080ff
        public static StringBuilder Teal(this StringBuilder sb, object message)
        {
            return sb.Append("<color=teal>").Append(message).Append("</color>");
        }

        public static StringBuilder Teal(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=teal>").Action(action).Append("</color>");
        }

        // white	#ffffffff
        public static StringBuilder White(this StringBuilder sb, object message)
        {
            return sb.Append("<color=white>").Append(message).Append("</color>");
        }

        public static StringBuilder White(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=white>").Action(action).Append("</color>");
        }

        // yellow	#ffff00ff
        public static StringBuilder Yellow(this StringBuilder sb, object message)
        {
            return sb.Append("<color=yellow>").Append(message).Append("</color>");
        }

        public static StringBuilder Yellow(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<color=yellow>").Action(action).Append("</color>");
        }

        public static StringBuilder Size(this StringBuilder sb, int size, object message)
        {
            return sb.Append("<size=").Append(size).Append(">").Append(message).Append("</size>");
        }

        public static StringBuilder Size(this StringBuilder sb, int size, Action<StringBuilder> action)
        {
            return sb.Append("<size=").Append(size).Append(">").Action(action).Append("</size>");
        }

        public static StringBuilder Small(this StringBuilder sb, object message)
        {
            return sb.Size(9, message);
        }

        public static StringBuilder Small(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Size(9, action);
        }

        public static StringBuilder Big(this StringBuilder sb, object message)
        {
            return sb.Size(16, message);
        }

        public static StringBuilder Big(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Size(16, action);
        }

        public static StringBuilder Bold(this StringBuilder sb, object message)
        {
            return sb.Append("<b>").Append(message).Append("</b>");
        }

        public static StringBuilder Bold(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<b>").Action(action).Append("</b>");
        }

        public static StringBuilder Italic(this StringBuilder sb, object message)
        {
            return sb.Append("<i>").Append(message).Append("</i>");
        }

        public static StringBuilder Italic(this StringBuilder sb, Action<StringBuilder> action)
        {
            return sb.Append("<i>").Action(action).Append("</i>");
        }

        public static StringBuilder Material(this StringBuilder sb, int index, object message)
        {
            return sb.Append("<material=").Append(index).Append(">").Append(message).Append("</material>");
        }

        public static StringBuilder Material(this StringBuilder sb, int index, Action<StringBuilder> action)
        {
            return sb.Append("<material=").Append(index).Append(">").Action(action).Append("</material>");
        }

        public static StringBuilder Quad(this StringBuilder sb, int material, int size, float x, float y, float width, float height)
        {
            return sb.Append("<quad material=").Append(material).Append(" size=").Append(size).Append(" x=").Append(x).Append(" y=").Append(y).Append(" width=").Append(width).Append(" height=").Append(height).Append("/>");
        }

        public static StringBuilder Quad(this StringBuilder sb, int material, int size, Rect rect)
        {
            return sb.Append("<quad material=").Append(material).Append(" size=").Append(size).Append(" x=").Append(rect.x).Append(" y=").Append(rect.y).Append(" width=").Append(rect.width).Append(" height=").Append(rect.height).Append("/>");
        }
        #endregion
    }
}