#nullable disable

namespace HarfBuzzSharp
{
	public partial struct Script
	{
		/// <summary>
		/// The script used to indicate an invalid or no script.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Invalid = new Script (Tag.None);

		public static readonly Script MaxValue = new Script (Tag.Max);

		public static readonly Script MaxValueSigned = new Script (Tag.MaxSigned);

		// Special scripts

		// 1.1
		/// <summary>
		/// The Common (Zyyy) script used to indicate an undetermined script.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Common = new Script (new Tag ('Z', 'y', 'y', 'y'));
		// 1.1
		/// <summary>
		/// The Inherited (Zinh) script used to indicate an inherited script.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Inherited = new Script (new Tag ('Z', 'i', 'n', 'h'));
		// 5.0
		/// <summary>
		/// The Unknown (Zzzz) script used to indicate an uncoded script.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Unknown = new Script (new Tag ('Z', 'z', 'z', 'z'));

		// Scripts

		// 1.1
		/// <summary>
		/// The Arabic (Arab) script typically used with text in the Arabic (ar) language originating from Saudi Arabia.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Arabic = new Script (new Tag ('A', 'r', 'a', 'b'));
		// 1.1
		/// <summary>
		/// The Armenian (Armn) script typically used with text in the Armenian (hy) language originating from Armenia.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Armenian = new Script (new Tag ('A', 'r', 'm', 'n'));
		// 1.1
		/// <summary>
		/// The Bengali (Beng) script typically used with text in the Bengali (bn) language originating from Bangladesh.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Bengali = new Script (new Tag ('B', 'e', 'n', 'g'));
		// 1.1
		/// <summary>
		/// The Cyrillic (Cyrl) script typically used with text in the Russian (ru) language originating from Bulgaria.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Cyrillic = new Script (new Tag ('C', 'y', 'r', 'l'));
		// 1.1
		/// <summary>
		/// The Devanagari (Deva) script typically used with text in the Hindi (hi) language originating from India.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Devanagari = new Script (new Tag ('D', 'e', 'v', 'a'));
		// 1.1
		/// <summary>
		/// The Georgian (Geor) script typically used with text in the Georgian (ka) language originating from Georgia.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Georgian = new Script (new Tag ('G', 'e', 'o', 'r'));
		// 1.1
		/// <summary>
		/// The Greek (Grek) script typically used with text in the Greek (el) language originating from Greece.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Greek = new Script (new Tag ('G', 'r', 'e', 'k'));
		// 1.1
		/// <summary>
		/// The Gujarati (Gujr) script typically used with text in the Gujarati (gu) language originating from India.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Gujarati = new Script (new Tag ('G', 'u', 'j', 'r'));
		// 1.1
		/// <summary>
		/// The Gurmukhi (Guru) script typically used with text in the Punjabi (pa) language originating from India.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Gurmukhi = new Script (new Tag ('G', 'u', 'r', 'u'));
		// 1.1
		/// <summary>
		/// The Hangul (Hang) script typically used with text in the Korean (ko) language originating from Republic of Korea.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Hangul = new Script (new Tag ('H', 'a', 'n', 'g'));
		// 1.1
		/// <summary>
		/// The Han (Hani) script typically used with text in the Chinese (zh) language originating from China.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Han = new Script (new Tag ('H', 'a', 'n', 'i'));
		// 1.1
		/// <summary>
		/// The Hebrew (Hebr) script typically used with text in the Hebrew (he) language originating from Israel.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Hebrew = new Script (new Tag ('H', 'e', 'b', 'r'));
		// 1.1
		/// <summary>
		/// The Hiragana (Hira) script typically used with text in the Japanese (ja) language originating from Japan.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Hiragana = new Script (new Tag ('H', 'i', 'r', 'a'));
		// 1.1
		/// <summary>
		/// The Kannada (Knda) script typically used with text in the Kannada (kn) language originating from India.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Kannada = new Script (new Tag ('K', 'n', 'd', 'a'));
		// 1.1
		/// <summary>
		/// The Katakana (Kana) script typically used with text in the Japanese (ja) language originating from Japan.
		/// </summary>
		/// <remarks></remarks>
		public static readonly Script Katakana = new Script (new Tag ('K', 'a', 'n', 'a'));
		// 1.1
		public static readonly Script Lao = new Script (new Tag ('L', 'a', 'o', 'o'));
		// 1.1
		public static readonly Script Latin = new Script (new Tag ('L', 'a', 't', 'n'));
		// 1.1
		public static readonly Script Malayalam = new Script (new Tag ('M', 'l', 'y', 'm'));
		// 1.1
		public static readonly Script Oriya = new Script (new Tag ('O', 'r', 'y', 'a'));
		// 1.1
		public static readonly Script Tamil = new Script (new Tag ('T', 'a', 'm', 'l'));
		// 1.1
		public static readonly Script Telugu = new Script (new Tag ('T', 'e', 'l', 'u'));
		// 1.1
		public static readonly Script Thai = new Script (new Tag ('T', 'h', 'a', 'i'));
		// 2.0
		public static readonly Script Tibetan = new Script (new Tag ('T', 'i', 'b', 't'));
		// 3.0
		public static readonly Script Bopomofo = new Script (new Tag ('B', 'o', 'p', 'o'));
		// 3.0
		public static readonly Script Braille = new Script (new Tag ('B', 'r', 'a', 'i'));
		// 3.0
		public static readonly Script CanadianSyllabics = new Script (new Tag ('C', 'a', 'n', 's'));
		// 3.0
		public static readonly Script Cherokee = new Script (new Tag ('C', 'h', 'e', 'r'));
		// 3.0
		public static readonly Script Ethiopic = new Script (new Tag ('E', 't', 'h', 'i'));
		// 3.0
		public static readonly Script Khmer = new Script (new Tag ('K', 'h', 'm', 'r'));
		// 3.0
		public static readonly Script Mongolian = new Script (new Tag ('M', 'o', 'n', 'g'));
		// 3.0
		public static readonly Script Myanmar = new Script (new Tag ('M', 'y', 'm', 'r'));
		// 3.0
		public static readonly Script Ogham = new Script (new Tag ('O', 'g', 'a', 'm'));
		// 3.0
		public static readonly Script Runic = new Script (new Tag ('R', 'u', 'n', 'r'));
		// 3.0
		public static readonly Script Sinhala = new Script (new Tag ('S', 'i', 'n', 'h'));
		// 3.0
		public static readonly Script Syriac = new Script (new Tag ('S', 'y', 'r', 'c'));
		// 3.0
		public static readonly Script Thaana = new Script (new Tag ('T', 'h', 'a', 'a'));
		// 3.0
		public static readonly Script Yi = new Script (new Tag ('Y', 'i', 'i', 'i'));
		// 3.1
		public static readonly Script Deseret = new Script (new Tag ('D', 's', 'r', 't'));
		// 3.1
		public static readonly Script Gothic = new Script (new Tag ('G', 'o', 't', 'h'));
		// 3.1
		public static readonly Script OldItalic = new Script (new Tag ('I', 't', 'a', 'l'));
		// 3.2
		public static readonly Script Buhid = new Script (new Tag ('B', 'u', 'h', 'd'));
		// 3.2
		public static readonly Script Hanunoo = new Script (new Tag ('H', 'a', 'n', 'o'));
		// 3.2
		public static readonly Script Tagalog = new Script (new Tag ('T', 'g', 'l', 'g'));
		// 3.2
		public static readonly Script Tagbanwa = new Script (new Tag ('T', 'a', 'g', 'b'));
		// 4.0
		public static readonly Script Cypriot = new Script (new Tag ('C', 'p', 'r', 't'));
		// 4.0
		public static readonly Script Limbu = new Script (new Tag ('L', 'i', 'm', 'b'));
		// 4.0
		public static readonly Script LinearB = new Script (new Tag ('L', 'i', 'n', 'b'));
		// 4.0
		public static readonly Script Osmanya = new Script (new Tag ('O', 's', 'm', 'a'));
		// 4.0
		public static readonly Script Shavian = new Script (new Tag ('S', 'h', 'a', 'w'));
		// 4.0
		public static readonly Script TaiLe = new Script (new Tag ('T', 'a', 'l', 'e'));
		// 4.0
		public static readonly Script Ugaritic = new Script (new Tag ('U', 'g', 'a', 'r'));
		// 4.1
		public static readonly Script Buginese = new Script (new Tag ('B', 'u', 'g', 'i'));
		// 4.1
		public static readonly Script Coptic = new Script (new Tag ('C', 'o', 'p', 't'));
		// 4.1
		public static readonly Script Glagolitic = new Script (new Tag ('G', 'l', 'a', 'g'));
		// 4.1
		public static readonly Script Kharoshthi = new Script (new Tag ('K', 'h', 'a', 'r'));
		// 4.1
		public static readonly Script NewTaiLue = new Script (new Tag ('T', 'a', 'l', 'u'));
		// 4.1
		public static readonly Script OldPersian = new Script (new Tag ('X', 'p', 'e', 'o'));
		// 4.1
		public static readonly Script SylotiNagri = new Script (new Tag ('S', 'y', 'l', 'o'));
		// 4.1
		public static readonly Script Tifinagh = new Script (new Tag ('T', 'f', 'n', 'g'));
		// 5.0
		public static readonly Script Balinese = new Script (new Tag ('B', 'a', 'l', 'i'));
		// 5.0
		public static readonly Script Cuneiform = new Script (new Tag ('X', 's', 'u', 'x'));
		// 5.0
		public static readonly Script Nko = new Script (new Tag ('N', 'k', 'o', 'o'));
		// 5.0
		public static readonly Script PhagsPa = new Script (new Tag ('P', 'h', 'a', 'g'));
		// 5.0
		public static readonly Script Phoenician = new Script (new Tag ('P', 'h', 'n', 'x'));
		// 5.1
		public static readonly Script Carian = new Script (new Tag ('C', 'a', 'r', 'i'));
		// 5.1
		public static readonly Script Cham = new Script (new Tag ('C', 'h', 'a', 'm'));
		// 5.1
		public static readonly Script KayahLi = new Script (new Tag ('K', 'a', 'l', 'i'));
		// 5.1
		public static readonly Script Lepcha = new Script (new Tag ('L', 'e', 'p', 'c'));
		// 5.1
		public static readonly Script Lycian = new Script (new Tag ('L', 'y', 'c', 'i'));
		// 5.1
		public static readonly Script Lydian = new Script (new Tag ('L', 'y', 'd', 'i'));
		// 5.1
		public static readonly Script OlChiki = new Script (new Tag ('O', 'l', 'c', 'k'));
		// 5.1
		public static readonly Script Rejang = new Script (new Tag ('R', 'j', 'n', 'g'));
		// 5.1
		public static readonly Script Saurashtra = new Script (new Tag ('S', 'a', 'u', 'r'));
		// 5.1
		public static readonly Script Sundanese = new Script (new Tag ('S', 'u', 'n', 'd'));
		// 5.1
		public static readonly Script Vai = new Script (new Tag ('V', 'a', 'i', 'i'));
		// 5.2
		public static readonly Script Avestan = new Script (new Tag ('A', 'v', 's', 't'));
		// 5.2
		public static readonly Script Bamum = new Script (new Tag ('B', 'a', 'm', 'u'));
		// 5.2
		public static readonly Script EgyptianHieroglyphs = new Script (new Tag ('E', 'g', 'y', 'p'));
		// 5.2
		public static readonly Script ImperialAramaic = new Script (new Tag ('A', 'r', 'm', 'i'));
		// 5.2
		public static readonly Script InscriptionalPahlavi = new Script (new Tag ('P', 'h', 'l', 'i'));
		// 5.2
		public static readonly Script InscriptionalParthian = new Script (new Tag ('P', 'r', 't', 'i'));
		// 5.2
		public static readonly Script Javanese = new Script (new Tag ('J', 'a', 'v', 'a'));
		// 5.2
		public static readonly Script Kaithi = new Script (new Tag ('K', 't', 'h', 'i'));
		// 5.2
		public static readonly Script Lisu = new Script (new Tag ('L', 'i', 's', 'u'));
		// 5.2
		public static readonly Script MeeteiMayek = new Script (new Tag ('M', 't', 'e', 'i'));
		// 5.2
		public static readonly Script OldSouthArabian = new Script (new Tag ('S', 'a', 'r', 'b'));
		// 5.2
		public static readonly Script OldTurkic = new Script (new Tag ('O', 'r', 'k', 'h'));
		// 5.2
		public static readonly Script Samaritan = new Script (new Tag ('S', 'a', 'm', 'r'));
		// 5.2
		public static readonly Script TaiTham = new Script (new Tag ('L', 'a', 'n', 'a'));
		// 5.2
		public static readonly Script TaiViet = new Script (new Tag ('T', 'a', 'v', 't'));
		// 6.0
		public static readonly Script Batak = new Script (new Tag ('B', 'a', 't', 'k'));
		// 6.0
		public static readonly Script Brahmi = new Script (new Tag ('B', 'r', 'a', 'h'));
		// 6.0
		public static readonly Script Mandaic = new Script (new Tag ('M', 'a', 'n', 'd'));
		// 6.1
		public static readonly Script Chakma = new Script (new Tag ('C', 'a', 'k', 'm'));
		// 6.1
		public static readonly Script MeroiticCursive = new Script (new Tag ('M', 'e', 'r', 'c'));
		// 6.1
		public static readonly Script MeroiticHieroglyphs = new Script (new Tag ('M', 'e', 'r', 'o'));
		// 6.1
		public static readonly Script Miao = new Script (new Tag ('P', 'l', 'r', 'd'));
		// 6.1
		public static readonly Script Sharada = new Script (new Tag ('S', 'h', 'r', 'd'));
		// 6.1
		public static readonly Script SoraSompeng = new Script (new Tag ('S', 'o', 'r', 'a'));
		// 6.1
		public static readonly Script Takri = new Script (new Tag ('T', 'a', 'k', 'r'));

		// Since: 0.9.30

		// 7.0
		public static readonly Script BassaVah = new Script (new Tag ('B', 'a', 's', 's'));
		// 7.0
		public static readonly Script CaucasianAlbanian = new Script (new Tag ('A', 'g', 'h', 'b'));
		// 7.0
		public static readonly Script Duployan = new Script (new Tag ('D', 'u', 'p', 'l'));
		// 7.0
		public static readonly Script Elbasan = new Script (new Tag ('E', 'l', 'b', 'a'));
		// 7.0
		public static readonly Script Grantha = new Script (new Tag ('G', 'r', 'a', 'n'));
		// 7.0
		public static readonly Script Khojki = new Script (new Tag ('K', 'h', 'o', 'j'));
		// 7.0
		public static readonly Script Khudawadi = new Script (new Tag ('S', 'i', 'n', 'd'));
		// 7.0
		public static readonly Script LinearA = new Script (new Tag ('L', 'i', 'n', 'a'));
		// 7.0
		public static readonly Script Mahajani = new Script (new Tag ('M', 'a', 'h', 'j'));
		// 7.0
		public static readonly Script Manichaean = new Script (new Tag ('M', 'a', 'n', 'i'));
		// 7.0
		public static readonly Script MendeKikakui = new Script (new Tag ('M', 'e', 'n', 'd'));
		// 7.0
		public static readonly Script Modi = new Script (new Tag ('M', 'o', 'd', 'i'));
		// 7.0
		public static readonly Script Mro = new Script (new Tag ('M', 'r', 'o', 'o'));
		// 7.0
		public static readonly Script Nabataean = new Script (new Tag ('N', 'b', 'a', 't'));
		// 7.0
		public static readonly Script OldNorthArabian = new Script (new Tag ('N', 'a', 'r', 'b'));
		// 7.0
		public static readonly Script OldPermic = new Script (new Tag ('P', 'e', 'r', 'm'));
		// 7.0
		public static readonly Script PahawhHmong = new Script (new Tag ('H', 'm', 'n', 'g'));
		// 7.0
		public static readonly Script Palmyrene = new Script (new Tag ('P', 'a', 'l', 'm'));
		// 7.0
		public static readonly Script PauCinHau = new Script (new Tag ('P', 'a', 'u', 'c'));
		// 7.0
		public static readonly Script PsalterPahlavi = new Script (new Tag ('P', 'h', 'l', 'p'));
		// 7.0
		public static readonly Script Siddham = new Script (new Tag ('S', 'i', 'd', 'd'));
		// 7.0
		public static readonly Script Tirhuta = new Script (new Tag ('T', 'i', 'r', 'h'));
		// 7.0
		public static readonly Script WarangCiti = new Script (new Tag ('W', 'a', 'r', 'a'));
		// 8.0
		public static readonly Script Ahom = new Script (new Tag ('A', 'h', 'o', 'm'));
		// 8.0
		public static readonly Script AnatolianHieroglyphs = new Script (new Tag ('H', 'l', 'u', 'w'));
		// 8.0
		public static readonly Script Hatran = new Script (new Tag ('H', 'a', 't', 'r'));
		// 8.0
		public static readonly Script Multani = new Script (new Tag ('M', 'u', 'l', 't'));
		// 8.0
		public static readonly Script OldHungarian = new Script (new Tag ('H', 'u', 'n', 'g'));
		// 8.0
		public static readonly Script Signwriting = new Script (new Tag ('S', 'g', 'n', 'w'));

		// Since 1.3.0

		// 9.0
		public static readonly Script Adlam = new Script (new Tag ('A', 'd', 'l', 'm'));
		// 9.0
		public static readonly Script Bhaiksuki = new Script (new Tag ('B', 'h', 'k', 's'));
		// 9.0
		public static readonly Script Marchen = new Script (new Tag ('M', 'a', 'r', 'c'));
		// 9.0
		public static readonly Script Osage = new Script (new Tag ('O', 's', 'g', 'e'));
		// 9.0
		public static readonly Script Tangut = new Script (new Tag ('T', 'a', 'n', 'g'));
		// 9.0
		public static readonly Script Newa = new Script (new Tag ('N', 'e', 'w', 'a'));

		// Since 1.6.0

		// 10.0
		public static readonly Script MasaramGondi = new Script (new Tag ('G', 'o', 'n', 'm'));
		// 10.0
		public static readonly Script Nushu = new Script (new Tag ('N', 's', 'h', 'u'));
		// 10.0
		public static readonly Script Soyombo = new Script (new Tag ('S', 'o', 'y', 'o'));
		// 10.0
		public static readonly Script ZanabazarSquare = new Script (new Tag ('Z', 'a', 'n', 'b'));

		// Since 1.8.0

		// 11.0
		public static readonly Script Dogra = new Script (new Tag ('D', 'o', 'g', 'r'));
		// 11.0
		public static readonly Script GunjalaGondi = new Script (new Tag ('G', 'o', 'n', 'g'));
		// 11.0
		public static readonly Script HanifiRohingya = new Script (new Tag ('R', 'o', 'h', 'g'));
		// 11.0
		public static readonly Script Makasar = new Script (new Tag ('M', 'a', 'k', 'a'));
		// 11.0
		public static readonly Script Medefaidrin = new Script (new Tag ('M', 'e', 'd', 'f'));
		// 11.0
		public static readonly Script OldSogdian = new Script (new Tag ('S', 'o', 'g', 'o'));
		// 11.0
		public static readonly Script Sogdian = new Script (new Tag ('S', 'o', 'g', 'd'));
	}
}
