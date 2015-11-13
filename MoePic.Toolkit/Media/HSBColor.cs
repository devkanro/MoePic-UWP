using System;
using Windows.UI;

namespace MoePic.Toolkit.Media
{
    /// <summary>
    /// 表示一个由色相，纯度，亮度表示的颜色
    /// </summary>
    public struct HSBColor
    {
        private static Double RangeIntercept(Double a, Double p)
        {
            if (a < 0)
            {
                return 0;
            }
            return Math.Min(a, p);
        }

        /// <summary>
        /// 实现从 <see cref="HSBColor"/> 到 <see cref="Color"/> 颜色的隐式转换。
        /// </summary>
        /// <param name="hsbColor"></param>
        public static implicit operator Color(HSBColor hsbColor)
        {
            Color result = new Color() {A = (byte) (hsbColor.A*255)};

            double max = hsbColor.B * 255;

            if (Math.Abs(hsbColor.S) < 0.0000001)
            {
                result.R = (Byte)max;
                result.G = result.R;
                result.B = result.R;
            }
            else
            {
                double ρ = 255 * hsbColor.S * hsbColor.B;
                double min = max - ρ;
                double hI = hsbColor.H / 60 * ρ;
                
                result.R = (byte)(RangeIntercept(Math.Abs(hI - 3 * ρ) - ρ, ρ) + min);
                result.G = (byte)(RangeIntercept(2 * ρ - Math.Abs(hI - 2 * ρ), ρ) + min);
                result.B = (byte)(RangeIntercept(2 * ρ - Math.Abs(hI - 4 * ρ), ρ) + min);
            }

            return result;
        }

        /// <summary>
        /// 实现从 <see cref="Color"/> 到 <see cref="HSBColor"/> 颜色的隐式转换。
        /// </summary>
        /// <param name="color"></param>
        public static implicit operator HSBColor(Color color)
        {
            HSBColor result = new HSBColor() { A = 1.0 * color.A / 255 };

            byte[] data = new[] { color.B, color.G, color.R };
            Array.Sort(data);

            if (data[0] == data[2])
            {
                result.H = 0;
                result.S = 0;
                result.B = 1.0 * data[2] / 255;
            }
            else
            {
                result.H = 180 +
                           (2 * data[2] - color.G - color.B + color.R - data[0]) *
                           Math.Sign(color.B - color.G - 0.5) /
                           (data[2] - data[0]) * 60;
                result.S = 1 - (1.0 * data[0] / data[2]);
                result.B = 1.0 * data[2] / 255;
            }
            return result;
        }

        /// <summary>
        /// 测试两个指定的 <see cref="HSBColor"/> 结构是否不同。
        /// </summary>
        public static bool operator !=(HSBColor color1, HSBColor color2)
        {
            return !(color1 == color2);
        }

        /// <summary>
        /// 测试两个指定的 <see cref="HSBColor"/> 结构是否相同。
        /// </summary>
        public static bool operator ==(HSBColor color1, HSBColor color2)
        {
            return Equals(color1, color2);
        }

        /// <summary>
        /// 测试两个指定的 <see cref="HSBColor"/> 结构是否相同。
        /// </summary>
        public static bool Equals(HSBColor color1, HSBColor color2)
        {
            return color1.Equals(color2);
        }

        /// <summary>
        /// 使用指定的 aHSB 值创建一个新的 <see cref="HSBColor"/> 结构。
        /// </summary>
        /// <param name="alpha">透明度</param>
        /// <param name="hue">色相</param>
        /// <param name="saturation">饱和度</param>
        /// <param name="brightness">亮度</param>
        public HSBColor(double alpha, double hue, double saturation, double brightness)
        {
            A = alpha;
            H = hue;
            S = saturation;
            B = brightness;
        }

        /// <summary>
        /// 使用指定的 HSB 值创建一个新的无透明的 <see cref="HSBColor"/> 结构。
        /// </summary>
        /// <param name="hue">色相</param>
        /// <param name="saturation">饱和度</param>
        /// <param name="brightness">亮度</param>
        public HSBColor(double hue, double saturation, double brightness) : this(1, hue, saturation, brightness)
        {
            
        }

        /// <summary>
        /// 表示颜色的透明度，取值为 0 - 1。
        /// </summary>
        public double A { get; set; }
        /// <summary>
        /// 表示颜色的色相，取值为 0 - 360。
        /// </summary>
        public double H { get; set; }
        /// <summary>
        /// 表示颜色的饱和度，取值为 0 - 1。
        /// </summary>
        public double S { get; set; }
        /// <summary>
        /// 表示颜色的亮度，取值为 0 - 1。
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// 测试指定的 <see cref="HSBColor"/> 结构是否与当前颜色相同。
        /// </summary>
        public bool Equals(HSBColor color)
        {
            return Math.Abs(color.A - A) + Math.Abs(color.H - H) + Math.Abs(color.S - S) + Math.Abs(color.B - B) < 0.000001;
        }

        /// <summary>
        /// 测试指定的对象是否为 <see cref="HSBColor"/> 结构并等同于当前颜色。
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is HSBColor)
            {
                HSBColor color = (HSBColor) obj;
                return this.Equals(color);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前 <see cref="HSBColor"/> 结构的哈希代码。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            Color color = this;
            return color.A << 24 | color.R << 16 | color.G << 8 | color.B;
        }
        
        /// <summary>
        /// 获取 <see cref="HSBColor"/> 的字符串表示形式。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"A:{A} H:{H} S:{S} B:{B}";
        }
    }
}
