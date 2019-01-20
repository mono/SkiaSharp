namespace HarfBuzzSharp
{
	public static class Script
	{
		/*1.1*/
		public static readonly Tag COMMON = new Tag('Z', 'y', 'y', 'y');
		/*1.1*/
		public static readonly Tag INHERITED = new Tag ('Z', 'i', 'n', 'h');
		/*5.0*/
		public static readonly Tag UNKNOWN = new Tag ('Z', 'z', 'z', 'z');
		/*1.1*/
		public static readonly Tag ARABIC = new Tag ('A', 'r', 'a', 'b');
		/*1.1*/
		public static readonly Tag ARMENIAN = new Tag ('A', 'r', 'm', 'n');
		/*1.1*/
		public static readonly Tag BENGALI = new Tag ('B', 'e', 'n', 'g');
		/*1.1*/
		public static readonly Tag CYRILLIC = new Tag ('C', 'y', 'r', 'l');
		/*1.1*/
		public static readonly Tag DEVANAGARI = new Tag ('D', 'e', 'v', 'a');
		/*1.1*/
		public static readonly Tag GEORGIAN = new Tag ('G', 'e', 'o', 'r');
		/*1.1*/
		public static readonly Tag GREEK = new Tag ('G', 'r', 'e', 'k');
		/*1.1*/
		public static readonly Tag GUJARATI = new Tag ('G', 'u', 'j', 'r');
		/*1.1*/
		public static readonly Tag GURMUKHI = new Tag ('G', 'u', 'r', 'u');
		/*1.1*/
		public static readonly Tag HANGUL = new Tag ('H', 'a', 'n', 'g');
		/*1.1*/
		public static readonly Tag HAN = new Tag ('H', 'a', 'n', 'i');
		/*1.1*/
		public static readonly Tag HEBREW = new Tag ('H', 'e', 'b', 'r');
		/*1.1*/
		public static readonly Tag HIRAGANA = new Tag ('H', 'i', 'r', 'a');
		/*1.1*/
		public static readonly Tag KANNADA = new Tag ('K', 'n', 'd', 'a');
		/*1.1*/
		public static readonly Tag KATAKANA = new Tag ('K', 'a', 'n', 'a');
		/*1.1*/
		public static readonly Tag LAO = new Tag ('L', 'a', 'o', 'o');
		/*1.1*/
		public static readonly Tag LATIN = new Tag ('L', 'a', 't', 'n');
		/*1.1*/
		public static readonly Tag MALAYALAM = new Tag ('M', 'l', 'y', 'm');
		/*1.1*/
		public static readonly Tag ORIYA = new Tag ('O', 'r', 'y', 'a');
		/*1.1*/
		public static readonly Tag TAMIL = new Tag ('T', 'a', 'm', 'l');
		/*1.1*/
		public static readonly Tag TELUGU = new Tag ('T', 'e', 'l', 'u');
		/*1.1*/
		public static readonly Tag THAI = new Tag ('T', 'h', 'a', 'i');
		/*2.0*/
		public static readonly Tag TIBETAN = new Tag ('T', 'i', 'b', 't');
		/*3.0*/
		public static readonly Tag BOPOMOFO = new Tag ('B', 'o', 'p', 'o');
		/*3.0*/
		public static readonly Tag BRAILLE = new Tag ('B', 'r', 'a', 'i');
		/*3.0*/
		public static readonly Tag CANADIAN_SYLLABICS = new Tag ('C', 'a', 'n', 's');
		/*3.0*/
		public static readonly Tag CHEROKEE = new Tag ('C', 'h', 'e', 'r');
		/*3.0*/
		public static readonly Tag ETHIOPIC = new Tag ('E', 't', 'h', 'i');
		/*3.0*/
		public static readonly Tag KHMER = new Tag ('K', 'h', 'm', 'r');
		/*3.0*/
		public static readonly Tag MONGOLIAN = new Tag ('M', 'o', 'n', 'g');
		/*3.0*/
		public static readonly Tag MYANMAR = new Tag ('M', 'y', 'm', 'r');
		/*3.0*/
		public static readonly Tag OGHAM = new Tag ('O', 'g', 'a', 'm');
		/*3.0*/
		public static readonly Tag RUNIC = new Tag ('R', 'u', 'n', 'r');
		/*3.0*/
		public static readonly Tag SINHALA = new Tag ('S', 'i', 'n', 'h');
		/*3.0*/
		public static readonly Tag SYRIAC = new Tag ('S', 'y', 'r', 'c');
		/*3.0*/
		public static readonly Tag THAANA = new Tag ('T', 'h', 'a', 'a');
		/*3.0*/
		public static readonly Tag YI = new Tag ('Y', 'i', 'i', 'i');
		/*3.1*/
		public static readonly Tag DESERET = new Tag ('D', 's', 'r', 't');
		/*3.1*/
		public static readonly Tag GOTHIC = new Tag ('G', 'o', 't', 'h');
		/*3.1*/
		public static readonly Tag OLD_ITALIC = new Tag ('I', 't', 'a', 'l');
		/*3.2*/
		public static readonly Tag BUHID = new Tag ('B', 'u', 'h', 'd');
		/*3.2*/
		public static readonly Tag HANUNOO = new Tag ('H', 'a', 'n', 'o');
		/*3.2*/
		public static readonly Tag TAGALOG = new Tag ('T', 'g', 'l', 'g');
		/*3.2*/
		public static readonly Tag TAGBANWA = new Tag ('T', 'a', 'g', 'b');
		/*4.0*/
		public static readonly Tag CYPRIOT = new Tag ('C', 'p', 'r', 't');
		/*4.0*/
		public static readonly Tag LIMBU = new Tag ('L', 'i', 'm', 'b');
		/*4.0*/
		public static readonly Tag LINEAR_B = new Tag ('L', 'i', 'n', 'b');
		/*4.0*/
		public static readonly Tag OSMANYA = new Tag ('O', 's', 'm', 'a');
		/*4.0*/
		public static readonly Tag SHAVIAN = new Tag ('S', 'h', 'a', 'w');
		/*4.0*/
		public static readonly Tag TAI_LE = new Tag ('T', 'a', 'l', 'e');
		/*4.0*/
		public static readonly Tag UGARITIC = new Tag ('U', 'g', 'a', 'r');
		/*4.1*/
		public static readonly Tag BUGINESE = new Tag ('B', 'u', 'g', 'i');
		/*4.1*/
		public static readonly Tag COPTIC = new Tag ('C', 'o', 'p', 't');
		/*4.1*/
		public static readonly Tag GLAGOLITIC = new Tag ('G', 'l', 'a', 'g');
		/*4.1*/
		public static readonly Tag KHAROSHTHI = new Tag ('K', 'h', 'a', 'r');
		/*4.1*/
		public static readonly Tag NEW_TAI_LUE = new Tag ('T', 'a', 'l', 'u');
		/*4.1*/
		public static readonly Tag OLD_PERSIAN = new Tag ('X', 'p', 'e', 'o');
		/*4.1*/
		public static readonly Tag SYLOTI_NAGRI = new Tag ('S', 'y', 'l', 'o');
		/*4.1*/
		public static readonly Tag TIFINAGH = new Tag ('T', 'f', 'n', 'g');
		/*5.0*/
		public static readonly Tag BALINESE = new Tag ('B', 'a', 'l', 'i');
		/*5.0*/
		public static readonly Tag CUNEIFORM = new Tag ('X', 's', 'u', 'x');
		/*5.0*/
		public static readonly Tag NKO = new Tag ('N', 'k', 'o', 'o');
		/*5.0*/
		public static readonly Tag PHAGS_PA = new Tag ('P', 'h', 'a', 'g');
		/*5.0*/
		public static readonly Tag PHOENICIAN = new Tag ('P', 'h', 'n', 'x');
		/*5.1*/
		public static readonly Tag CARIAN = new Tag ('C', 'a', 'r', 'i');
		/*5.1*/
		public static readonly Tag CHAM = new Tag ('C', 'h', 'a', 'm');
		/*5.1*/
		public static readonly Tag KAYAH_LI = new Tag ('K', 'a', 'l', 'i');
		/*5.1*/
		public static readonly Tag LEPCHA = new Tag ('L', 'e', 'p', 'c');
		/*5.1*/
		public static readonly Tag LYCIAN = new Tag ('L', 'y', 'c', 'i');
		/*5.1*/
		public static readonly Tag LYDIAN = new Tag ('L', 'y', 'd', 'i');
		/*5.1*/
		public static readonly Tag OL_CHIKI = new Tag ('O', 'l', 'c', 'k');
		/*5.1*/
		public static readonly Tag REJANG = new Tag ('R', 'j', 'n', 'g');
		/*5.1*/
		public static readonly Tag SAURASHTRA = new Tag ('S', 'a', 'u', 'r');
		/*5.1*/
		public static readonly Tag SUNDANESE = new Tag ('S', 'u', 'n', 'd');
		/*5.1*/
		public static readonly Tag VAI = new Tag ('V', 'a', 'i', 'i');
		/*5.2*/
		public static readonly Tag AVESTAN = new Tag ('A', 'v', 's', 't');
		/*5.2*/
		public static readonly Tag BAMUM = new Tag ('B', 'a', 'm', 'u');
		/*5.2*/
		public static readonly Tag EGYPTIAN_HIEROGLYPHS = new Tag ('E', 'g', 'y', 'p');
		/*5.2*/
		public static readonly Tag IMPERIAL_ARAMAIC = new Tag ('A', 'r', 'm', 'i');
		/*5.2*/
		public static readonly Tag INSCRIPTIONAL_PAHLAVI = new Tag ('P', 'h', 'l', 'i');
		/*5.2*/
		public static readonly Tag INSCRIPTIONAL_PARTHIAN = new Tag ('P', 'r', 't', 'i');
		/*5.2*/
		public static readonly Tag JAVANESE = new Tag ('J', 'a', 'v', 'a');
		/*5.2*/
		public static readonly Tag KAITHI = new Tag ('K', 't', 'h', 'i');
		/*5.2*/
		public static readonly Tag LISU = new Tag ('L', 'i', 's', 'u');
		/*5.2*/
		public static readonly Tag MEETEI_MAYEK = new Tag ('M', 't', 'e', 'i');
		/*5.2*/
		public static readonly Tag OLD_SOUTH_ARABIAN = new Tag ('S', 'a', 'r', 'b');
		/*5.2*/
		public static readonly Tag OLD_TURKIC = new Tag ('O', 'r', 'k', 'h');
		/*5.2*/
		public static readonly Tag SAMARITAN = new Tag ('S', 'a', 'm', 'r');
		/*5.2*/
		public static readonly Tag TAI_THAM = new Tag ('L', 'a', 'n', 'a');
		/*5.2*/
		public static readonly Tag TAI_VIET = new Tag ('T', 'a', 'v', 't');
		/*6.0*/
		public static readonly Tag BATAK = new Tag ('B', 'a', 't', 'k');
		/*6.0*/
		public static readonly Tag BRAHMI = new Tag ('B', 'r', 'a', 'h');
		/*6.0*/
		public static readonly Tag MANDAIC = new Tag ('M', 'a', 'n', 'd');
		/*6.1*/
		public static readonly Tag CHAKMA = new Tag ('C', 'a', 'k', 'm');
		/*6.1*/
		public static readonly Tag MEROITIC_CURSIVE = new Tag ('M', 'e', 'r', 'c');
		/*6.1*/
		public static readonly Tag MEROITIC_HIEROGLYPHS = new Tag ('M', 'e', 'r', 'o');
		/*6.1*/
		public static readonly Tag MIAO = new Tag ('P', 'l', 'r', 'd');
		/*6.1*/
		public static readonly Tag SHARADA = new Tag ('S', 'h', 'r', 'd');
		/*6.1*/
		public static readonly Tag SORA_SOMPENG = new Tag ('S', 'o', 'r', 'a');
		/*6.1*/
		public static readonly Tag TAKRI = new Tag ('T', 'a', 'k', 'r');

		/*
		 * Since: 0.9.30
		 */

		/*7.0*/
		public static readonly Tag BASSA_VAH = new Tag ('B', 'a', 's', 's');
		/*7.0*/
		public static readonly Tag CAUCASIAN_ALBANIAN = new Tag ('A', 'g', 'h', 'b');
		/*7.0*/
		public static readonly Tag DUPLOYAN = new Tag ('D', 'u', 'p', 'l');
		/*7.0*/
		public static readonly Tag ELBASAN = new Tag ('E', 'l', 'b', 'a');
		/*7.0*/
		public static readonly Tag GRANTHA = new Tag ('G', 'r', 'a', 'n');
		/*7.0*/
		public static readonly Tag KHOJKI = new Tag ('K', 'h', 'o', 'j');
		/*7.0*/
		public static readonly Tag KHUDAWADI = new Tag ('S', 'i', 'n', 'd');
		/*7.0*/
		public static readonly Tag LINEAR_A = new Tag ('L', 'i', 'n', 'a');
		/*7.0*/
		public static readonly Tag MAHAJANI = new Tag ('M', 'a', 'h', 'j');
		/*7.0*/
		public static readonly Tag MANICHAEAN = new Tag ('M', 'a', 'n', 'i');
		/*7.0*/
		public static readonly Tag MENDE_KIKAKUI = new Tag ('M', 'e', 'n', 'd');
		/*7.0*/
		public static readonly Tag MODI = new Tag ('M', 'o', 'd', 'i');
		/*7.0*/
		public static readonly Tag MRO = new Tag ('M', 'r', 'o', 'o');
		/*7.0*/
		public static readonly Tag NABATAEAN = new Tag ('N', 'b', 'a', 't');
		/*7.0*/
		public static readonly Tag OLD_NORTH_ARABIAN = new Tag ('N', 'a', 'r', 'b');
		/*7.0*/
		public static readonly Tag OLD_PERMIC = new Tag ('P', 'e', 'r', 'm');
		/*7.0*/
		public static readonly Tag PAHAWH_HMONG = new Tag ('H', 'm', 'n', 'g');
		/*7.0*/
		public static readonly Tag PALMYRENE = new Tag ('P', 'a', 'l', 'm');
		/*7.0*/
		public static readonly Tag PAU_CIN_HAU = new Tag ('P', 'a', 'u', 'c');
		/*7.0*/
		public static readonly Tag PSALTER_PAHLAVI = new Tag ('P', 'h', 'l', 'p');
		/*7.0*/
		public static readonly Tag SIDDHAM = new Tag ('S', 'i', 'd', 'd');
		/*7.0*/
		public static readonly Tag TIRHUTA = new Tag ('T', 'i', 'r', 'h');
		/*7.0*/
		public static readonly Tag WARANG_CITI = new Tag ('W', 'a', 'r', 'a');
		/*8.0*/
		public static readonly Tag AHOM = new Tag ('A', 'h', 'o', 'm');
		/*8.0*/
		public static readonly Tag ANATOLIAN_HIEROGLYPHS = new Tag ('H', 'l', 'u', 'w');
		/*8.0*/
		public static readonly Tag HATRAN = new Tag ('H', 'a', 't', 'r');
		/*8.0*/
		public static readonly Tag MULTANI = new Tag ('M', 'u', 'l', 't');
		/*8.0*/
		public static readonly Tag OLD_HUNGARIAN = new Tag ('H', 'u', 'n', 'g');
		/*8.0*/
		public static readonly Tag SIGNWRITING = new Tag ('S', 'g', 'n', 'w');

		/*
		 * Since 1.3.0
		 */

		/*9.0*/
		public static readonly Tag ADLAM = new Tag ('A', 'd', 'l', 'm');
		/*9.0*/
		public static readonly Tag BHAIKSUKI = new Tag ('B', 'h', 'k', 's');
		/*9.0*/
		public static readonly Tag MARCHEN = new Tag ('M', 'a', 'r', 'c');
		/*9.0*/
		public static readonly Tag OSAGE = new Tag ('O', 's', 'g', 'e');
		/*9.0*/
		public static readonly Tag TANGUT = new Tag ('T', 'a', 'n', 'g');
		/*9.0*/
		public static readonly Tag NEWA = new Tag ('N', 'e', 'w', 'a');


		/*
		 * Since 1.6.0
		 */

		/*10.0*/
		public static readonly Tag MASARAM_GONDI = new Tag ('G', 'o', 'n', 'm');
		/*10.0*/
		public static readonly Tag NUSHU = new Tag ('N', 's', 'h', 'u');
		/*10.0*/
		public static readonly Tag SOYOMBO = new Tag ('S', 'o', 'y', 'o');
		/*10.0*/
		public static readonly Tag ZANABAZAR_SQUARE = new Tag ('Z', 'a', 'n', 'b');

		/*
		 * Since 1.8.0
		 */

		/*11.0*/
		public static readonly Tag DOGRA = new Tag ('D', 'o', 'g', 'r');
		/*11.0*/
		public static readonly Tag GUNJALA_GONDI = new Tag ('G', 'o', 'n', 'g');
		/*11.0*/
		public static readonly Tag HANIFI_ROHINGYA = new Tag ('R', 'o', 'h', 'g');
		/*11.0*/
		public static readonly Tag MAKASAR = new Tag ('M', 'a', 'k', 'a');
		/*11.0*/
		public static readonly Tag MEDEFAIDRIN = new Tag ('M', 'e', 'd', 'f');
		/*11.0*/
		public static readonly Tag OLD_SOGDIAN = new Tag ('S', 'o', 'g', 'o');
		/*11.0*/
		public static readonly Tag SOGDIAN = new Tag ('S', 'o', 'g', 'd');

		/* No script set. */

		public static readonly Tag INVALID = Tag.None;

		public static readonly Tag MAX_VALUE = Tag.MaxSigned;

		public static readonly Tag MAX_VALUE_SIGNED = Tag.Max;

		public static Direction GetHorizontalDirection (Tag script)
		{
			return HarfBuzzApi.hb_script_get_horizontal_direction (script);
		}
	}
}
