namespace HarfBuzzSharp
{
	public struct Script
	{
		/*1.1*/
		public static readonly Script COMMON = new Script (new Tag ('Z', 'y', 'y', 'y'));
		/*1.1*/
		public static readonly Script INHERITED = new Script (new Tag ('Z', 'i', 'n', 'h'));
		/*5.0*/
		public static readonly Script UNKNOWN = new Script (new Tag ('Z', 'z', 'z', 'z'));
		/*1.1*/
		public static readonly Script ARABIC = new Script (new Tag ('A', 'r', 'a', 'b'));
		/*1.1*/
		public static readonly Script ARMENIAN = new Script (new Tag ('A', 'r', 'm', 'n'));
		/*1.1*/
		public static readonly Script BENGALI = new Script (new Tag ('B', 'e', 'n', 'g'));
		/*1.1*/
		public static readonly Script CYRILLIC = new Script (new Tag ('C', 'y', 'r', 'l'));
		/*1.1*/
		public static readonly Script DEVANAGARI = new Script (new Tag ('D', 'e', 'v', 'a'));
		/*1.1*/
		public static readonly Script GEORGIAN = new Script (new Tag ('G', 'e', 'o', 'r'));
		/*1.1*/
		public static readonly Script GREEK = new Script (new Tag ('G', 'r', 'e', 'k'));
		/*1.1*/
		public static readonly Script GUJARATI = new Script (new Tag ('G', 'u', 'j', 'r'));
		/*1.1*/
		public static readonly Script GURMUKHI = new Script (new Tag ('G', 'u', 'r', 'u'));
		/*1.1*/
		public static readonly Script HANGUL = new Script (new Tag ('H', 'a', 'n', 'g'));
		/*1.1*/
		public static readonly Script HAN = new Script (new Tag ('H', 'a', 'n', 'i'));
		/*1.1*/
		public static readonly Script HEBREW = new Script (new Tag ('H', 'e', 'b', 'r'));
		/*1.1*/
		public static readonly Script HIRAGANA = new Script (new Tag ('H', 'i', 'r', 'a'));
		/*1.1*/
		public static readonly Script KANNADA = new Script (new Tag ('K', 'n', 'd', 'a'));
		/*1.1*/
		public static readonly Script KATAKANA = new Script (new Tag ('K', 'a', 'n', 'a'));
		/*1.1*/
		public static readonly Script LAO = new Script (new Tag ('L', 'a', 'o', 'o'));
		/*1.1*/
		public static readonly Script LATIN = new Script (new Tag ('L', 'a', 't', 'n'));
		/*1.1*/
		public static readonly Script MALAYALAM = new Script (new Tag ('M', 'l', 'y', 'm'));
		/*1.1*/
		public static readonly Script ORIYA = new Script (new Tag ('O', 'r', 'y', 'a'));
		/*1.1*/
		public static readonly Script TAMIL = new Script (new Tag ('T', 'a', 'm', 'l'));
		/*1.1*/
		public static readonly Script TELUGU = new Script (new Tag ('T', 'e', 'l', 'u'));
		/*1.1*/
		public static readonly Script THAI = new Script (new Tag ('T', 'h', 'a', 'i'));
		/*2.0*/
		public static readonly Script TIBETAN = new Script (new Tag ('T', 'i', 'b', 't'));
		/*3.0*/
		public static readonly Script BOPOMOFO = new Script (new Tag ('B', 'o', 'p', 'o'));
		/*3.0*/
		public static readonly Script BRAILLE = new Script (new Tag ('B', 'r', 'a', 'i'));
		/*3.0*/
		public static readonly Script CANADIAN_SYLLABICS = new Script (new Tag ('C', 'a', 'n', 's'));
		/*3.0*/
		public static readonly Script CHEROKEE = new Script (new Tag ('C', 'h', 'e', 'r'));
		/*3.0*/
		public static readonly Script ETHIOPIC = new Script (new Tag ('E', 't', 'h', 'i'));
		/*3.0*/
		public static readonly Script KHMER = new Script (new Tag ('K', 'h', 'm', 'r'));
		/*3.0*/
		public static readonly Script MONGOLIAN = new Script (new Tag ('M', 'o', 'n', 'g'));
		/*3.0*/
		public static readonly Script MYANMAR = new Script (new Tag ('M', 'y', 'm', 'r'));
		/*3.0*/
		public static readonly Script OGHAM = new Script (new Tag ('O', 'g', 'a', 'm'));
		/*3.0*/
		public static readonly Script RUNIC = new Script (new Tag ('R', 'u', 'n', 'r'));
		/*3.0*/
		public static readonly Script SINHALA = new Script (new Tag ('S', 'i', 'n', 'h'));
		/*3.0*/
		public static readonly Script SYRIAC = new Script (new Tag ('S', 'y', 'r', 'c'));
		/*3.0*/
		public static readonly Script THAANA = new Script (new Tag ('T', 'h', 'a', 'a'));
		/*3.0*/
		public static readonly Script YI = new Script (new Tag ('Y', 'i', 'i', 'i'));
		/*3.1*/
		public static readonly Script DESERET = new Script (new Tag ('D', 's', 'r', 't'));
		/*3.1*/
		public static readonly Script GOTHIC = new Script (new Tag ('G', 'o', 't', 'h'));
		/*3.1*/
		public static readonly Script OLD_ITALIC = new Script (new Tag ('I', 't', 'a', 'l'));
		/*3.2*/
		public static readonly Script BUHID = new Script (new Tag ('B', 'u', 'h', 'd'));
		/*3.2*/
		public static readonly Script HANUNOO = new Script (new Tag ('H', 'a', 'n', 'o'));
		/*3.2*/
		public static readonly Script TAGALOG = new Script (new Tag ('T', 'g', 'l', 'g'));
		/*3.2*/
		public static readonly Script TAGBANWA = new Script (new Tag ('T', 'a', 'g', 'b'));
		/*4.0*/
		public static readonly Script CYPRIOT = new Script (new Tag ('C', 'p', 'r', 't'));
		/*4.0*/
		public static readonly Script LIMBU = new Script (new Tag ('L', 'i', 'm', 'b'));
		/*4.0*/
		public static readonly Script LINEAR_B = new Script (new Tag ('L', 'i', 'n', 'b'));
		/*4.0*/
		public static readonly Script OSMANYA = new Script (new Tag ('O', 's', 'm', 'a'));
		/*4.0*/
		public static readonly Script SHAVIAN = new Script (new Tag ('S', 'h', 'a', 'w'));
		/*4.0*/
		public static readonly Script TAI_LE = new Script (new Tag ('T', 'a', 'l', 'e'));
		/*4.0*/
		public static readonly Script UGARITIC = new Script (new Tag ('U', 'g', 'a', 'r'));
		/*4.1*/
		public static readonly Script BUGINESE = new Script (new Tag ('B', 'u', 'g', 'i'));
		/*4.1*/
		public static readonly Script COPTIC = new Script (new Tag ('C', 'o', 'p', 't'));
		/*4.1*/
		public static readonly Script GLAGOLITIC = new Script (new Tag ('G', 'l', 'a', 'g'));
		/*4.1*/
		public static readonly Script KHAROSHTHI = new Script (new Tag ('K', 'h', 'a', 'r'));
		/*4.1*/
		public static readonly Script NEW_TAI_LUE = new Script (new Tag ('T', 'a', 'l', 'u'));
		/*4.1*/
		public static readonly Script OLD_PERSIAN = new Script (new Tag ('X', 'p', 'e', 'o'));
		/*4.1*/
		public static readonly Script SYLOTI_NAGRI = new Script (new Tag ('S', 'y', 'l', 'o'));
		/*4.1*/
		public static readonly Script TIFINAGH = new Script (new Tag ('T', 'f', 'n', 'g'));
		/*5.0*/
		public static readonly Script BALINESE = new Script (new Tag ('B', 'a', 'l', 'i'));
		/*5.0*/
		public static readonly Script CUNEIFORM = new Script (new Tag ('X', 's', 'u', 'x'));
		/*5.0*/
		public static readonly Script NKO = new Script (new Tag ('N', 'k', 'o', 'o'));
		/*5.0*/
		public static readonly Script PHAGS_PA = new Script (new Tag ('P', 'h', 'a', 'g'));
		/*5.0*/
		public static readonly Script PHOENICIAN = new Script (new Tag ('P', 'h', 'n', 'x'));
		/*5.1*/
		public static readonly Script CARIAN = new Script (new Tag ('C', 'a', 'r', 'i'));
		/*5.1*/
		public static readonly Script CHAM = new Script (new Tag ('C', 'h', 'a', 'm'));
		/*5.1*/
		public static readonly Script KAYAH_LI = new Script (new Tag ('K', 'a', 'l', 'i'));
		/*5.1*/
		public static readonly Script LEPCHA = new Script (new Tag ('L', 'e', 'p', 'c'));
		/*5.1*/
		public static readonly Script LYCIAN = new Script (new Tag ('L', 'y', 'c', 'i'));
		/*5.1*/
		public static readonly Script LYDIAN = new Script (new Tag ('L', 'y', 'd', 'i'));
		/*5.1*/
		public static readonly Script OL_CHIKI = new Script (new Tag ('O', 'l', 'c', 'k'));
		/*5.1*/
		public static readonly Script REJANG = new Script (new Tag ('R', 'j', 'n', 'g'));
		/*5.1*/
		public static readonly Script SAURASHTRA = new Script (new Tag ('S', 'a', 'u', 'r'));
		/*5.1*/
		public static readonly Script SUNDANESE = new Script (new Tag ('S', 'u', 'n', 'd'));
		/*5.1*/
		public static readonly Script VAI = new Script (new Tag ('V', 'a', 'i', 'i'));
		/*5.2*/
		public static readonly Script AVESTAN = new Script (new Tag ('A', 'v', 's', 't'));
		/*5.2*/
		public static readonly Script BAMUM = new Script (new Tag ('B', 'a', 'm', 'u'));
		/*5.2*/
		public static readonly Script EGYPTIAN_HIEROGLYPHS = new Script (new Tag ('E', 'g', 'y', 'p'));
		/*5.2*/
		public static readonly Script IMPERIAL_ARAMAIC = new Script (new Tag ('A', 'r', 'm', 'i'));
		/*5.2*/
		public static readonly Script INSCRIPTIONAL_PAHLAVI = new Script (new Tag ('P', 'h', 'l', 'i'));
		/*5.2*/
		public static readonly Script INSCRIPTIONAL_PARTHIAN = new Script (new Tag ('P', 'r', 't', 'i'));
		/*5.2*/
		public static readonly Script JAVANESE = new Script (new Tag ('J', 'a', 'v', 'a'));
		/*5.2*/
		public static readonly Script KAITHI = new Script (new Tag ('K', 't', 'h', 'i'));
		/*5.2*/
		public static readonly Script LISU = new Script (new Tag ('L', 'i', 's', 'u'));
		/*5.2*/
		public static readonly Script MEETEI_MAYEK = new Script (new Tag ('M', 't', 'e', 'i'));
		/*5.2*/
		public static readonly Script OLD_SOUTH_ARABIAN = new Script (new Tag ('S', 'a', 'r', 'b'));
		/*5.2*/
		public static readonly Script OLD_TURKIC = new Script (new Tag ('O', 'r', 'k', 'h'));
		/*5.2*/
		public static readonly Script SAMARITAN = new Script (new Tag ('S', 'a', 'm', 'r'));
		/*5.2*/
		public static readonly Script TAI_THAM = new Script (new Tag ('L', 'a', 'n', 'a'));
		/*5.2*/
		public static readonly Script TAI_VIET = new Script (new Tag ('T', 'a', 'v', 't'));
		/*6.0*/
		public static readonly Script BATAK = new Script (new Tag ('B', 'a', 't', 'k'));
		/*6.0*/
		public static readonly Script BRAHMI = new Script (new Tag ('B', 'r', 'a', 'h'));
		/*6.0*/
		public static readonly Script MANDAIC = new Script (new Tag ('M', 'a', 'n', 'd'));
		/*6.1*/
		public static readonly Script CHAKMA = new Script (new Tag ('C', 'a', 'k', 'm'));
		/*6.1*/
		public static readonly Script MEROITIC_CURSIVE = new Script (new Tag ('M', 'e', 'r', 'c'));
		/*6.1*/
		public static readonly Script MEROITIC_HIEROGLYPHS = new Script (new Tag ('M', 'e', 'r', 'o'));
		/*6.1*/
		public static readonly Script MIAO = new Script (new Tag ('P', 'l', 'r', 'd'));
		/*6.1*/
		public static readonly Script SHARADA = new Script (new Tag ('S', 'h', 'r', 'd'));
		/*6.1*/
		public static readonly Script SORA_SOMPENG = new Script (new Tag ('S', 'o', 'r', 'a'));
		/*6.1*/
		public static readonly Script TAKRI = new Script (new Tag ('T', 'a', 'k', 'r'));

		/*
		 * Since: 0.9.30
		 */

		/*7.0*/
		public static readonly Script BASSA_VAH = new Script (new Tag ('B', 'a', 's', 's'));
		/*7.0*/
		public static readonly Script CAUCASIAN_ALBANIAN = new Script (new Tag ('A', 'g', 'h', 'b'));
		/*7.0*/
		public static readonly Script DUPLOYAN = new Script (new Tag ('D', 'u', 'p', 'l'));
		/*7.0*/
		public static readonly Script ELBASAN = new Script (new Tag ('E', 'l', 'b', 'a'));
		/*7.0*/
		public static readonly Script GRANTHA = new Script (new Tag ('G', 'r', 'a', 'n'));
		/*7.0*/
		public static readonly Script KHOJKI = new Script (new Tag ('K', 'h', 'o', 'j'));
		/*7.0*/
		public static readonly Script KHUDAWADI = new Script (new Tag ('S', 'i', 'n', 'd'));
		/*7.0*/
		public static readonly Script LINEAR_A = new Script (new Tag ('L', 'i', 'n', 'a'));
		/*7.0*/
		public static readonly Script MAHAJANI = new Script (new Tag ('M', 'a', 'h', 'j'));
		/*7.0*/
		public static readonly Script MANICHAEAN = new Script (new Tag ('M', 'a', 'n', 'i'));
		/*7.0*/
		public static readonly Script MENDE_KIKAKUI = new Script (new Tag ('M', 'e', 'n', 'd'));
		/*7.0*/
		public static readonly Script MODI = new Script (new Tag ('M', 'o', 'd', 'i'));
		/*7.0*/
		public static readonly Script MRO = new Script (new Tag ('M', 'r', 'o', 'o'));
		/*7.0*/
		public static readonly Script NABATAEAN = new Script (new Tag ('N', 'b', 'a', 't'));
		/*7.0*/
		public static readonly Script OLD_NORTH_ARABIAN = new Script (new Tag ('N', 'a', 'r', 'b'));
		/*7.0*/
		public static readonly Script OLD_PERMIC = new Script (new Tag ('P', 'e', 'r', 'm'));
		/*7.0*/
		public static readonly Script PAHAWH_HMONG = new Script (new Tag ('H', 'm', 'n', 'g'));
		/*7.0*/
		public static readonly Script PALMYRENE = new Script (new Tag ('P', 'a', 'l', 'm'));
		/*7.0*/
		public static readonly Script PAU_CIN_HAU = new Script (new Tag ('P', 'a', 'u', 'c'));
		/*7.0*/
		public static readonly Script PSALTER_PAHLAVI = new Script (new Tag ('P', 'h', 'l', 'p'));
		/*7.0*/
		public static readonly Script SIDDHAM = new Script (new Tag ('S', 'i', 'd', 'd'));
		/*7.0*/
		public static readonly Script TIRHUTA = new Script (new Tag ('T', 'i', 'r', 'h'));
		/*7.0*/
		public static readonly Script WARANG_CITI = new Script (new Tag ('W', 'a', 'r', 'a'));
		/*8.0*/
		public static readonly Script AHOM = new Script (new Tag ('A', 'h', 'o', 'm'));
		/*8.0*/
		public static readonly Script ANATOLIAN_HIEROGLYPHS = new Script (new Tag ('H', 'l', 'u', 'w'));
		/*8.0*/
		public static readonly Script HATRAN = new Script (new Tag ('H', 'a', 't', 'r'));
		/*8.0*/
		public static readonly Script MULTANI = new Script (new Tag ('M', 'u', 'l', 't'));
		/*8.0*/
		public static readonly Script OLD_HUNGARIAN = new Script (new Tag ('H', 'u', 'n', 'g'));
		/*8.0*/
		public static readonly Script SIGNWRITING = new Script (new Tag ('S', 'g', 'n', 'w'));

		/*
		 * Since 1.3.0
		 */

		/*9.0*/
		public static readonly Script ADLAM = new Script (new Tag ('A', 'd', 'l', 'm'));
		/*9.0*/
		public static readonly Script BHAIKSUKI = new Script (new Tag ('B', 'h', 'k', 's'));
		/*9.0*/
		public static readonly Script MARCHEN = new Script (new Tag ('M', 'a', 'r', 'c'));
		/*9.0*/
		public static readonly Script OSAGE = new Script (new Tag ('O', 's', 'g', 'e'));
		/*9.0*/
		public static readonly Script TANGUT = new Script (new Tag ('T', 'a', 'n', 'g'));
		/*9.0*/
		public static readonly Script NEWA = new Script (new Tag ('N', 'e', 'w', 'a'));


		/*
		 * Since 1.6.0
		 */

		/*10.0*/
		public static readonly Script MASARAM_GONDI = new Script (new Tag ('G', 'o', 'n', 'm'));
		/*10.0*/
		public static readonly Script NUSHU = new Script (new Tag ('N', 's', 'h', 'u'));
		/*10.0*/
		public static readonly Script SOYOMBO = new Script (new Tag ('S', 'o', 'y', 'o'));
		/*10.0*/
		public static readonly Script ZANABAZAR_SQUARE = new Script (new Tag ('Z', 'a', 'n', 'b'));

		/*
		 * Since 1.8.0
		 */

		/*11.0*/
		public static readonly Script DOGRA = new Script (new Tag ('D', 'o', 'g', 'r'));
		/*11.0*/
		public static readonly Script GUNJALA_GONDI = new Script (new Tag ('G', 'o', 'n', 'g'));
		/*11.0*/
		public static readonly Script HANIFI_ROHINGYA = new Script (new Tag ('R', 'o', 'h', 'g'));
		/*11.0*/
		public static readonly Script MAKASAR = new Script (new Tag ('M', 'a', 'k', 'a'));
		/*11.0*/
		public static readonly Script MEDEFAIDRIN = new Script (new Tag ('M', 'e', 'd', 'f'));
		/*11.0*/
		public static readonly Script OLD_SOGDIAN = new Script (new Tag ('S', 'o', 'g', 'o'));
		/*11.0*/
		public static readonly Script SOGDIAN = new Script (new Tag ('S', 'o', 'g', 'd'));

		/* No script set. */

		public static readonly Script INVALID = new Script (Tag.None);

		public static readonly Script MAX_VALUE = new Script (Tag.MaxSigned);

		public static readonly Script MAX_VALUE_SIGNED = new Script (Tag.Max);

		private readonly Tag _tag;

		private Script (Tag tag)
		{
			_tag = tag;
		}

		public Script (string script)
		{
			_tag = new Tag (script);
		}

		public Direction HorizontalDirection {
			get {
				return HarfBuzzApi.hb_script_get_horizontal_direction (_tag);
			}
		}

		public override string ToString () => _tag.ToString();

		public static implicit operator uint (Script script) => script._tag;

		public static implicit operator Script (uint tag) => new Script (tag);
	}
}
