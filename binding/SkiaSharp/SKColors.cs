#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Definitions for some common color names.</summary>
	/// <remarks />
	public struct SKColors
	{
		/// <summary>Gets the predefined empty color (black transparent), or #00000000.</summary>
		/// <value>The empty color value.</value>
		/// <remarks />
		public static SKColor Empty => new SKColor (0x00000000);
		/// <summary>Gets the predefined color of alice blue, or #FFF0F8FF.</summary>
		/// <remarks />
		public static SKColor AliceBlue = new SKColor (0xFFF0F8FF);
		/// <summary>Gets the predefined color of antique white, or #FFFAEBD7.</summary>
		/// <remarks />
		public static SKColor AntiqueWhite = new SKColor (0xFFFAEBD7);
		/// <summary>Gets the predefined color of aqua, or #FF00FFFF.</summary>
		/// <remarks />
		public static SKColor Aqua = new SKColor (0xFF00FFFF);
		/// <summary>Gets the predefined color of aquamarine, or #FF7FFFD4.</summary>
		/// <remarks />
		public static SKColor Aquamarine = new SKColor (0xFF7FFFD4);
		/// <summary>Gets the predefined color of azure, or #FFF0FFFF.</summary>
		/// <remarks />
		public static SKColor Azure = new SKColor (0xFFF0FFFF);
		/// <summary>Gets the predefined color of beige, or #FFF5F5DC.</summary>
		/// <remarks />
		public static SKColor Beige = new SKColor (0xFFF5F5DC);
		/// <summary>Gets the predefined color of bisque, or #FFFFE4C4.</summary>
		/// <remarks />
		public static SKColor Bisque = new SKColor (0xFFFFE4C4);
		/// <summary>Gets the predefined color of black, or #FF000000.</summary>
		/// <remarks />
		public static SKColor Black = new SKColor (0xFF000000);
		/// <summary>Gets the predefined color of blanched almond, or #FFFFEBCD.</summary>
		/// <remarks />
		public static SKColor BlanchedAlmond = new SKColor (0xFFFFEBCD);
		/// <summary>Gets the predefined color of blue, or #FF0000FF.</summary>
		/// <remarks />
		public static SKColor Blue = new SKColor (0xFF0000FF);
		/// <summary>Gets the predefined color of blue violet, or #FF8A2BE2.</summary>
		/// <remarks />
		public static SKColor BlueViolet = new SKColor (0xFF8A2BE2);
		/// <summary>Gets the predefined color of brown, or #FFA52A2A.</summary>
		/// <remarks />
		public static SKColor Brown = new SKColor (0xFFA52A2A);
		/// <summary>Gets the predefined color of burly wood, or #FFDEB887.</summary>
		/// <remarks />
		public static SKColor BurlyWood = new SKColor (0xFFDEB887);
		/// <summary>Gets the predefined color of cadet blue, or #FF5F9EA0.</summary>
		/// <remarks />
		public static SKColor CadetBlue = new SKColor (0xFF5F9EA0);
		/// <summary>Gets the predefined color of chartreuse, or #FF7FFF00.</summary>
		/// <remarks />
		public static SKColor Chartreuse = new SKColor (0xFF7FFF00);
		/// <summary>Gets the predefined color of chocolate, or #FFD2691E.</summary>
		/// <remarks />
		public static SKColor Chocolate = new SKColor (0xFFD2691E);
		/// <summary>Gets the predefined color of coral, or #FFFF7F50.</summary>
		/// <remarks />
		public static SKColor Coral = new SKColor (0xFFFF7F50);
		/// <summary>Gets the predefined color of cornflower blue, or #FF6495ED.</summary>
		/// <remarks />
		public static SKColor CornflowerBlue = new SKColor (0xFF6495ED);
		/// <summary>Gets the predefined color of cornsilk, or #FFFFF8DC.</summary>
		/// <remarks />
		public static SKColor Cornsilk = new SKColor (0xFFFFF8DC);
		/// <summary>Gets the predefined color of crimson, or #FFDC143C.</summary>
		/// <remarks />
		public static SKColor Crimson = new SKColor (0xFFDC143C);
		/// <summary>Gets the predefined color of cyan, or #FF00FFFF.</summary>
		/// <remarks />
		public static SKColor Cyan = new SKColor (0xFF00FFFF);
		/// <summary>Gets the predefined color of dark blue, or #FF00008B.</summary>
		/// <remarks />
		public static SKColor DarkBlue = new SKColor (0xFF00008B);
		/// <summary>Gets the predefined color of dark cyan, or #FF008B8B.</summary>
		/// <remarks />
		public static SKColor DarkCyan = new SKColor (0xFF008B8B);
		/// <summary>Gets the predefined color of dark goldenrod, or #FFB8860B.</summary>
		/// <remarks />
		public static SKColor DarkGoldenrod = new SKColor (0xFFB8860B);
		/// <summary>Gets the predefined color of dark gray, or #FFA9A9A9.</summary>
		/// <remarks />
		public static SKColor DarkGray = new SKColor (0xFFA9A9A9);
		/// <summary>Gets the predefined color of dark green, or #FF006400.</summary>
		/// <remarks />
		public static SKColor DarkGreen = new SKColor (0xFF006400);
		/// <summary>Gets the predefined color of dark khaki, or #FFBDB76B.</summary>
		/// <remarks />
		public static SKColor DarkKhaki = new SKColor (0xFFBDB76B);
		/// <summary>Gets the predefined color of dark magenta, or #FF8B008B.</summary>
		/// <remarks />
		public static SKColor DarkMagenta = new SKColor (0xFF8B008B);
		/// <summary>Gets the predefined color of dark olive green, or #FF556B2F.</summary>
		/// <remarks />
		public static SKColor DarkOliveGreen = new SKColor (0xFF556B2F);
		/// <summary>Gets the predefined color of dark orange, or #FFFF8C00.</summary>
		/// <remarks />
		public static SKColor DarkOrange = new SKColor (0xFFFF8C00);
		/// <summary>Gets the predefined color of dark orchid, or #FF9932CC.</summary>
		/// <remarks />
		public static SKColor DarkOrchid = new SKColor (0xFF9932CC);
		/// <summary>Gets the predefined color of dark red, or #FF8B0000.</summary>
		/// <remarks />
		public static SKColor DarkRed = new SKColor (0xFF8B0000);
		/// <summary>Gets the predefined color of dark salmon, or #FFE9967A.</summary>
		/// <remarks />
		public static SKColor DarkSalmon = new SKColor (0xFFE9967A);
		/// <summary>Gets the predefined color of dark sea green, or #FF8FBC8B.</summary>
		/// <remarks />
		public static SKColor DarkSeaGreen = new SKColor (0xFF8FBC8B);
		/// <summary>Gets the predefined color of dark slate blue, or #FF483D8B.</summary>
		/// <remarks />
		public static SKColor DarkSlateBlue = new SKColor (0xFF483D8B);
		/// <summary>Gets the predefined color of dark slate gray, or #FF2F4F4F.</summary>
		/// <remarks />
		public static SKColor DarkSlateGray = new SKColor (0xFF2F4F4F);
		/// <summary>Gets the predefined color of dark turquoise, or #FF00CED1.</summary>
		/// <remarks />
		public static SKColor DarkTurquoise = new SKColor (0xFF00CED1);
		/// <summary>Gets the predefined color of dark violet, or #FF9400D3.</summary>
		/// <remarks />
		public static SKColor DarkViolet = new SKColor (0xFF9400D3);
		/// <summary>Gets the predefined color of deep pink, or #FFFF1493.</summary>
		/// <remarks />
		public static SKColor DeepPink = new SKColor (0xFFFF1493);
		/// <summary>Gets the predefined color of deep sky blue, or #FF00BFFF.</summary>
		/// <remarks />
		public static SKColor DeepSkyBlue = new SKColor (0xFF00BFFF);
		/// <summary>Gets the predefined color of dim gray, or #FF696969.</summary>
		/// <remarks />
		public static SKColor DimGray = new SKColor (0xFF696969);
		/// <summary>Gets the predefined color of dodger blue, or #FF1E90FF.</summary>
		/// <remarks />
		public static SKColor DodgerBlue = new SKColor (0xFF1E90FF);
		/// <summary>Gets the predefined color of firebrick, or #FFB22222.</summary>
		/// <remarks />
		public static SKColor Firebrick = new SKColor (0xFFB22222);
		/// <summary>Gets the predefined color of floral white, or #FFFFFAF0.</summary>
		/// <remarks />
		public static SKColor FloralWhite = new SKColor (0xFFFFFAF0);
		/// <summary>Gets the predefined color of forest green, or #FF228B22.</summary>
		/// <remarks />
		public static SKColor ForestGreen = new SKColor (0xFF228B22);
		/// <summary>Gets the predefined color of fuchsia, or #FFFF00FF.</summary>
		/// <remarks />
		public static SKColor Fuchsia = new SKColor (0xFFFF00FF);
		/// <summary>Gets the predefined color of gainsboro, or #FFDCDCDC.</summary>
		/// <remarks />
		public static SKColor Gainsboro = new SKColor (0xFFDCDCDC);
		/// <summary>Gets the predefined color of ghost white, or #FFF8F8FF.</summary>
		/// <remarks />
		public static SKColor GhostWhite = new SKColor (0xFFF8F8FF);
		/// <summary>Gets the predefined color of gold, or #FFFFD700.</summary>
		/// <remarks />
		public static SKColor Gold = new SKColor (0xFFFFD700);
		/// <summary>Gets the predefined color of goldenrod, or #FFDAA520.</summary>
		/// <remarks />
		public static SKColor Goldenrod = new SKColor (0xFFDAA520);
		/// <summary>Gets the predefined color of gray, or #FF808080.</summary>
		/// <remarks />
		public static SKColor Gray = new SKColor (0xFF808080);
		/// <summary>Gets the predefined color of green, or #FF008000.</summary>
		/// <remarks />
		public static SKColor Green = new SKColor (0xFF008000);
		/// <summary>Gets the predefined color of green yellow, or #FFADFF2F.</summary>
		/// <remarks />
		public static SKColor GreenYellow = new SKColor (0xFFADFF2F);
		/// <summary>Gets the predefined color of honeydew, or #FFF0FFF0.</summary>
		/// <remarks />
		public static SKColor Honeydew = new SKColor (0xFFF0FFF0);
		/// <summary>Gets the predefined color of hot pink, or #FFFF69B4.</summary>
		/// <remarks />
		public static SKColor HotPink = new SKColor (0xFFFF69B4);
		/// <summary>Gets the predefined color of indian red, or #FFCD5C5C.</summary>
		/// <remarks />
		public static SKColor IndianRed = new SKColor (0xFFCD5C5C);
		/// <summary>Gets the predefined color of indigo, or #FF4B0082.</summary>
		/// <remarks />
		public static SKColor Indigo = new SKColor (0xFF4B0082);
		/// <summary>Gets the predefined color of ivory, or #FFFFFFF0.</summary>
		/// <remarks />
		public static SKColor Ivory = new SKColor (0xFFFFFFF0);
		/// <summary>Gets the predefined color of khaki, or #FFF0E68C.</summary>
		/// <remarks />
		public static SKColor Khaki = new SKColor (0xFFF0E68C);
		/// <summary>Gets the predefined color of lavender, or #FFE6E6FA.</summary>
		/// <remarks />
		public static SKColor Lavender = new SKColor (0xFFE6E6FA);
		/// <summary>Gets the predefined color of lavender blush, or #FFFFF0F5.</summary>
		/// <remarks />
		public static SKColor LavenderBlush = new SKColor (0xFFFFF0F5);
		/// <summary>Gets the predefined color of lawn green, or #FF7CFC00.</summary>
		/// <remarks />
		public static SKColor LawnGreen = new SKColor (0xFF7CFC00);
		/// <summary>Gets the predefined color of lemon chiffon, or #FFFFFACD.</summary>
		/// <remarks />
		public static SKColor LemonChiffon = new SKColor (0xFFFFFACD);
		/// <summary>Gets the predefined color of light blue, or #FFADD8E6.</summary>
		/// <remarks />
		public static SKColor LightBlue = new SKColor (0xFFADD8E6);
		/// <summary>Gets the predefined color of light coral, or #FFF08080.</summary>
		/// <remarks />
		public static SKColor LightCoral = new SKColor (0xFFF08080);
		/// <summary>Gets the predefined color of light cyan, or #FFE0FFFF.</summary>
		/// <remarks />
		public static SKColor LightCyan = new SKColor (0xFFE0FFFF);
		/// <summary>Gets the predefined color of light goldenrod yellow, or #FFFAFAD2.</summary>
		/// <remarks />
		public static SKColor LightGoldenrodYellow = new SKColor (0xFFFAFAD2);
		/// <summary>Gets the predefined color of light gray, or #FFD3D3D3.</summary>
		/// <remarks />
		public static SKColor LightGray = new SKColor (0xFFD3D3D3);
		/// <summary>Gets the predefined color of light green, or #FF90EE90.</summary>
		/// <remarks />
		public static SKColor LightGreen = new SKColor (0xFF90EE90);
		/// <summary>Gets the predefined color of light pink, or #FFFFB6C1.</summary>
		/// <remarks />
		public static SKColor LightPink = new SKColor (0xFFFFB6C1);
		/// <summary>Gets the predefined color of light salmon, or #FFFFA07A.</summary>
		/// <remarks />
		public static SKColor LightSalmon = new SKColor (0xFFFFA07A);
		/// <summary>Gets the predefined color of light sea green, or #FF20B2AA.</summary>
		/// <remarks />
		public static SKColor LightSeaGreen = new SKColor (0xFF20B2AA);
		/// <summary>Gets the predefined color of light sky blue, or #FF87CEFA.</summary>
		/// <remarks />
		public static SKColor LightSkyBlue = new SKColor (0xFF87CEFA);
		/// <summary>Gets the predefined color of light slate gray, or #FF778899.</summary>
		/// <remarks />
		public static SKColor LightSlateGray = new SKColor (0xFF778899);
		/// <summary>Gets the predefined color of light steel blue, or #FFB0C4DE.</summary>
		/// <remarks />
		public static SKColor LightSteelBlue = new SKColor (0xFFB0C4DE);
		/// <summary>Gets the predefined color of light yellow, or #FFFFFFE0.</summary>
		/// <remarks />
		public static SKColor LightYellow = new SKColor (0xFFFFFFE0);
		/// <summary>Gets the predefined color of lime, or #FF00FF00.</summary>
		/// <remarks />
		public static SKColor Lime = new SKColor (0xFF00FF00);
		/// <summary>Gets the predefined color of lime green, or #FF32CD32.</summary>
		/// <remarks />
		public static SKColor LimeGreen = new SKColor (0xFF32CD32);
		/// <summary>Gets the predefined color of linen, or #FFFAF0E6.</summary>
		/// <remarks />
		public static SKColor Linen = new SKColor (0xFFFAF0E6);
		/// <summary>Gets the predefined color of magenta, or #FFFF00FF.</summary>
		/// <remarks />
		public static SKColor Magenta = new SKColor (0xFFFF00FF);
		/// <summary>Gets the predefined color of maroon, or #FF800000.</summary>
		/// <remarks />
		public static SKColor Maroon = new SKColor (0xFF800000);
		/// <summary>Gets the predefined color of medium aquamarine, or #FF66CDAA.</summary>
		/// <remarks />
		public static SKColor MediumAquamarine = new SKColor (0xFF66CDAA);
		/// <summary>Gets the predefined color of medium blue, or #FF0000CD.</summary>
		/// <remarks />
		public static SKColor MediumBlue = new SKColor (0xFF0000CD);
		/// <summary>Gets the predefined color of medium orchid, or #FFBA55D3.</summary>
		/// <remarks />
		public static SKColor MediumOrchid = new SKColor (0xFFBA55D3);
		/// <summary>Gets the predefined color of medium purple, or #FF9370DB.</summary>
		/// <remarks />
		public static SKColor MediumPurple = new SKColor (0xFF9370DB);
		/// <summary>Gets the predefined color of medium sea green, or #FF3CB371.</summary>
		/// <remarks />
		public static SKColor MediumSeaGreen = new SKColor (0xFF3CB371);
		/// <summary>Gets the predefined color of medium slate blue, or #FF7B68EE.</summary>
		/// <remarks />
		public static SKColor MediumSlateBlue = new SKColor (0xFF7B68EE);
		/// <summary>Gets the predefined color of medium spring green, or #FF00FA9A.</summary>
		/// <remarks />
		public static SKColor MediumSpringGreen = new SKColor (0xFF00FA9A);
		/// <summary>Gets the predefined color of medium turquoise, or #FF48D1CC.</summary>
		/// <remarks />
		public static SKColor MediumTurquoise = new SKColor (0xFF48D1CC);
		/// <summary>Gets the predefined color of medium violet red, or #FFC71585.</summary>
		/// <remarks />
		public static SKColor MediumVioletRed = new SKColor (0xFFC71585);
		/// <summary>Gets the predefined color of midnight blue, or #FF191970.</summary>
		/// <remarks />
		public static SKColor MidnightBlue = new SKColor (0xFF191970);
		/// <summary>Gets the predefined color of mint cream, or #FFF5FFFA.</summary>
		/// <remarks />
		public static SKColor MintCream = new SKColor (0xFFF5FFFA);
		/// <summary>Gets the predefined color of misty rose, or #FFFFE4E1.</summary>
		/// <remarks />
		public static SKColor MistyRose = new SKColor (0xFFFFE4E1);
		/// <summary>Gets the predefined color of moccasin, or #FFFFE4B5.</summary>
		/// <remarks />
		public static SKColor Moccasin = new SKColor (0xFFFFE4B5);
		/// <summary>Gets the predefined color of navajo white, or #FFFFDEAD.</summary>
		/// <remarks />
		public static SKColor NavajoWhite = new SKColor (0xFFFFDEAD);
		/// <summary>Gets the predefined color of navy, or #FF000080.</summary>
		/// <remarks />
		public static SKColor Navy = new SKColor (0xFF000080);
		/// <summary>Gets the predefined color of old lace, or #FFFDF5E6.</summary>
		/// <remarks />
		public static SKColor OldLace = new SKColor (0xFFFDF5E6);
		/// <summary>Gets the predefined color of olive, or #FF808000.</summary>
		/// <remarks />
		public static SKColor Olive = new SKColor (0xFF808000);
		/// <summary>Gets the predefined color of olive drab, or #FF6B8E23.</summary>
		/// <remarks />
		public static SKColor OliveDrab = new SKColor (0xFF6B8E23);
		/// <summary>Gets the predefined color of orange, or #FFFFA500.</summary>
		/// <remarks />
		public static SKColor Orange = new SKColor (0xFFFFA500);
		/// <summary>Gets the predefined color of orange red, or #FFFF4500.</summary>
		/// <remarks />
		public static SKColor OrangeRed = new SKColor (0xFFFF4500);
		/// <summary>Gets the predefined color of orchid, or #FFDA70D6.</summary>
		/// <remarks />
		public static SKColor Orchid = new SKColor (0xFFDA70D6);
		/// <summary>Gets the predefined color of pale goldenrod, or #FFEEE8AA.</summary>
		/// <remarks />
		public static SKColor PaleGoldenrod = new SKColor (0xFFEEE8AA);
		/// <summary>Gets the predefined color of pale green, or #FF98FB98.</summary>
		/// <remarks />
		public static SKColor PaleGreen = new SKColor (0xFF98FB98);
		/// <summary>Gets the predefined color of pale turquoise, or #FFAFEEEE.</summary>
		/// <remarks />
		public static SKColor PaleTurquoise = new SKColor (0xFFAFEEEE);
		/// <summary>Gets the predefined color of pale violet red, or #FFDB7093.</summary>
		/// <remarks />
		public static SKColor PaleVioletRed = new SKColor (0xFFDB7093);
		/// <summary>Gets the predefined color of papaya whip, or #FFFFEFD5.</summary>
		/// <remarks />
		public static SKColor PapayaWhip = new SKColor (0xFFFFEFD5);
		/// <summary>Gets the predefined color of peach puff, or #FFFFDAB9.</summary>
		/// <remarks />
		public static SKColor PeachPuff = new SKColor (0xFFFFDAB9);
		/// <summary>Gets the predefined color of peru, or #FFCD853F.</summary>
		/// <remarks />
		public static SKColor Peru = new SKColor (0xFFCD853F);
		/// <summary>Gets the predefined color of pink, or #FFFFC0CB.</summary>
		/// <remarks />
		public static SKColor Pink = new SKColor (0xFFFFC0CB);
		/// <summary>Gets the predefined color of plum, or #FFDDA0DD.</summary>
		/// <remarks />
		public static SKColor Plum = new SKColor (0xFFDDA0DD);
		/// <summary>Gets the predefined color of powder blue, or #FFB0E0E6.</summary>
		/// <remarks />
		public static SKColor PowderBlue = new SKColor (0xFFB0E0E6);
		/// <summary>Gets the predefined color of purple, or #FF800080.</summary>
		/// <remarks />
		public static SKColor Purple = new SKColor (0xFF800080);
		/// <summary>Gets the predefined color of red, or #FFFF0000.</summary>
		/// <remarks />
		public static SKColor Red = new SKColor (0xFFFF0000);
		/// <summary>Gets the predefined color of rosy brown, or #FFBC8F8F.</summary>
		/// <remarks />
		public static SKColor RosyBrown = new SKColor (0xFFBC8F8F);
		/// <summary>Gets the predefined color of royal blue, or #FF4169E1.</summary>
		/// <remarks />
		public static SKColor RoyalBlue = new SKColor (0xFF4169E1);
		/// <summary>Gets the predefined color of saddle brown, or #FF8B4513.</summary>
		/// <remarks />
		public static SKColor SaddleBrown = new SKColor (0xFF8B4513);
		/// <summary>Gets the predefined color of salmon, or #FFFA8072.</summary>
		/// <remarks />
		public static SKColor Salmon = new SKColor (0xFFFA8072);
		/// <summary>Gets the predefined color of sandy brown, or #FFF4A460.</summary>
		/// <remarks />
		public static SKColor SandyBrown = new SKColor (0xFFF4A460);
		/// <summary>Gets the predefined color of sea green, or #FF2E8B57.</summary>
		/// <remarks />
		public static SKColor SeaGreen = new SKColor (0xFF2E8B57);
		/// <summary>Gets the predefined color of sea shell, or #FFFFF5EE.</summary>
		/// <remarks />
		public static SKColor SeaShell = new SKColor (0xFFFFF5EE);
		/// <summary>Gets the predefined color of sienna, or #FFA0522D.</summary>
		/// <remarks />
		public static SKColor Sienna = new SKColor (0xFFA0522D);
		/// <summary>Gets the predefined color of silver, or #FFC0C0C0.</summary>
		/// <remarks />
		public static SKColor Silver = new SKColor (0xFFC0C0C0);
		/// <summary>Gets the predefined color of sky blue, or #FF87CEEB.</summary>
		/// <remarks />
		public static SKColor SkyBlue = new SKColor (0xFF87CEEB);
		/// <summary>Gets the predefined color of slate blue, or #FF6A5ACD.</summary>
		/// <remarks />
		public static SKColor SlateBlue = new SKColor (0xFF6A5ACD);
		/// <summary>Gets the predefined color of slate gray, or #FF708090.</summary>
		/// <remarks />
		public static SKColor SlateGray = new SKColor (0xFF708090);
		/// <summary>Gets the predefined color of snow, or #FFFFFAFA.</summary>
		/// <remarks />
		public static SKColor Snow = new SKColor (0xFFFFFAFA);
		/// <summary>Gets the predefined color of spring green, or #FF00FF7F.</summary>
		/// <remarks />
		public static SKColor SpringGreen = new SKColor (0xFF00FF7F);
		/// <summary>Gets the predefined color of steel blue, or #FF4682B4.</summary>
		/// <remarks />
		public static SKColor SteelBlue = new SKColor (0xFF4682B4);
		/// <summary>Gets the predefined color of tan, or #FFD2B48C.</summary>
		/// <remarks />
		public static SKColor Tan = new SKColor (0xFFD2B48C);
		/// <summary>Gets the predefined color of teal, or #FF008080.</summary>
		/// <remarks />
		public static SKColor Teal = new SKColor (0xFF008080);
		/// <summary>Gets the predefined color of thistle, or #FFD8BFD8.</summary>
		/// <remarks />
		public static SKColor Thistle = new SKColor (0xFFD8BFD8);
		/// <summary>Gets the predefined color of tomato, or #FFFF6347.</summary>
		/// <remarks />
		public static SKColor Tomato = new SKColor (0xFFFF6347);
		/// <summary>Gets the predefined color of turquoise, or #FF40E0D0.</summary>
		/// <remarks />
		public static SKColor Turquoise = new SKColor (0xFF40E0D0);
		/// <summary>Gets the predefined color of violet, or #FFEE82EE.</summary>
		/// <remarks />
		public static SKColor Violet = new SKColor (0xFFEE82EE);
		/// <summary>Gets the predefined color of wheat, or #FFF5DEB3.</summary>
		/// <remarks />
		public static SKColor Wheat = new SKColor (0xFFF5DEB3);
		/// <summary>Gets the predefined color of white, or #FFFFFFFF.</summary>
		/// <remarks />
		public static SKColor White = new SKColor (0xFFFFFFFF);
		/// <summary>Gets the predefined color of white smoke, or #FFF5F5F5.</summary>
		/// <remarks />
		public static SKColor WhiteSmoke = new SKColor (0xFFF5F5F5);
		/// <summary>Gets the predefined color of yellow, or #FFFFFF00.</summary>
		/// <remarks />
		public static SKColor Yellow = new SKColor (0xFFFFFF00);
		/// <summary>Gets the predefined color of yellow green, or #FF9ACD32.</summary>
		/// <remarks />
		public static SKColor YellowGreen = new SKColor (0xFF9ACD32);
		/// <summary>Gets the predefined color of white transparent, or #00FFFFFF.</summary>
		/// <remarks />
		public static SKColor Transparent = new SKColor (0x00FFFFFF);
		
	}
}
