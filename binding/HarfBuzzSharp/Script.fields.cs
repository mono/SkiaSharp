#nullable disable

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a particular Unicode script.
	/// </summary>
	public partial struct Script
	{
		/// <summary>
		/// The script used to indicate an invalid or no script.
		/// </summary>
		public static readonly Script Invalid = new Script (Tag.None);

		/// <summary>
		/// The dummy script used to prevent undefined behavior.
		/// </summary>
		public static readonly Script MaxValue = new Script (Tag.Max);

		/// <summary>
		/// The dummy script used to prevent undefined behavior.
		/// </summary>
		public static readonly Script MaxValueSigned = new Script (Tag.MaxSigned);

		// Special scripts

		// 1.1
		/// <summary>
		/// The Common (Zyyy) script used to indicate an undetermined script.
		/// </summary>
		public static readonly Script Common = new Script (new Tag ('Z', 'y', 'y', 'y'));
		// 1.1
		/// <summary>
		/// The Inherited (Zinh) script used to indicate an inherited script.
		/// </summary>
		public static readonly Script Inherited = new Script (new Tag ('Z', 'i', 'n', 'h'));
		// 5.0
		/// <summary>
		/// The Unknown (Zzzz) script used to indicate an uncoded script.
		/// </summary>
		public static readonly Script Unknown = new Script (new Tag ('Z', 'z', 'z', 'z'));

		// Scripts

		// 1.1
		/// <summary>
		/// The Arabic (Arab) script typically used with text in the Arabic (ar) language originating from Saudi Arabia.
		/// </summary>
		public static readonly Script Arabic = new Script (new Tag ('A', 'r', 'a', 'b'));
		// 1.1
		/// <summary>
		/// The Armenian (Armn) script typically used with text in the Armenian (hy) language originating from Armenia.
		/// </summary>
		public static readonly Script Armenian = new Script (new Tag ('A', 'r', 'm', 'n'));
		// 1.1
		/// <summary>
		/// The Bengali (Beng) script typically used with text in the Bengali (bn) language originating from Bangladesh.
		/// </summary>
		public static readonly Script Bengali = new Script (new Tag ('B', 'e', 'n', 'g'));
		// 1.1
		/// <summary>
		/// The Cyrillic (Cyrl) script typically used with text in the Russian (ru) language originating from Bulgaria.
		/// </summary>
		public static readonly Script Cyrillic = new Script (new Tag ('C', 'y', 'r', 'l'));
		// 1.1
		/// <summary>
		/// The Devanagari (Deva) script typically used with text in the Hindi (hi) language originating from India.
		/// </summary>
		public static readonly Script Devanagari = new Script (new Tag ('D', 'e', 'v', 'a'));
		// 1.1
		/// <summary>
		/// The Georgian (Geor) script typically used with text in the Georgian (ka) language originating from Georgia.
		/// </summary>
		public static readonly Script Georgian = new Script (new Tag ('G', 'e', 'o', 'r'));
		// 1.1
		/// <summary>
		/// The Greek (Grek) script typically used with text in the Greek (el) language originating from Greece.
		/// </summary>
		public static readonly Script Greek = new Script (new Tag ('G', 'r', 'e', 'k'));
		// 1.1
		/// <summary>
		/// The Gujarati (Gujr) script typically used with text in the Gujarati (gu) language originating from India.
		/// </summary>
		public static readonly Script Gujarati = new Script (new Tag ('G', 'u', 'j', 'r'));
		// 1.1
		/// <summary>
		/// The Gurmukhi (Guru) script typically used with text in the Punjabi (pa) language originating from India.
		/// </summary>
		public static readonly Script Gurmukhi = new Script (new Tag ('G', 'u', 'r', 'u'));
		// 1.1
		/// <summary>
		/// The Hangul (Hang) script typically used with text in the Korean (ko) language originating from Republic of Korea.
		/// </summary>
		public static readonly Script Hangul = new Script (new Tag ('H', 'a', 'n', 'g'));
		// 1.1
		/// <summary>
		/// The Han (Hani) script typically used with text in the Chinese (zh) language originating from China.
		/// </summary>
		public static readonly Script Han = new Script (new Tag ('H', 'a', 'n', 'i'));
		// 1.1
		/// <summary>
		/// The Hebrew (Hebr) script typically used with text in the Hebrew (he) language originating from Israel.
		/// </summary>
		public static readonly Script Hebrew = new Script (new Tag ('H', 'e', 'b', 'r'));
		// 1.1
		/// <summary>
		/// The Hiragana (Hira) script typically used with text in the Japanese (ja) language originating from Japan.
		/// </summary>
		public static readonly Script Hiragana = new Script (new Tag ('H', 'i', 'r', 'a'));
		// 1.1
		/// <summary>
		/// The Kannada (Knda) script typically used with text in the Kannada (kn) language originating from India.
		/// </summary>
		public static readonly Script Kannada = new Script (new Tag ('K', 'n', 'd', 'a'));
		// 1.1
		/// <summary>
		/// The Katakana (Kana) script typically used with text in the Japanese (ja) language originating from Japan.
		/// </summary>
		public static readonly Script Katakana = new Script (new Tag ('K', 'a', 'n', 'a'));
		// 1.1
		/// <summary>
		/// The Lao (Laoo) script typically used with text in the Lao (lo) language originating from Laos.
		/// </summary>
		public static readonly Script Lao = new Script (new Tag ('L', 'a', 'o', 'o'));
		// 1.1
		/// <summary>
		/// The Latin (Latn) script typically used with text in the English (en) language originating from Italy.
		/// </summary>
		public static readonly Script Latin = new Script (new Tag ('L', 'a', 't', 'n'));
		// 1.1
		/// <summary>
		/// The Malayalam (Mlym) script typically used with text in the Malayalam (ml) language originating from India.
		/// </summary>
		public static readonly Script Malayalam = new Script (new Tag ('M', 'l', 'y', 'm'));
		// 1.1
		/// <summary>
		/// The Oriya (Orya) script typically used with text in the Oriya (or) language originating from India.
		/// </summary>
		public static readonly Script Oriya = new Script (new Tag ('O', 'r', 'y', 'a'));
		// 1.1
		/// <summary>
		/// The Tamil (Taml) script typically used with text in the Tamil (ta) language originating from India.
		/// </summary>
		public static readonly Script Tamil = new Script (new Tag ('T', 'a', 'm', 'l'));
		// 1.1
		/// <summary>
		/// The Telugu (Telu) script typically used with text in the Telugu (te) language originating from India.
		/// </summary>
		public static readonly Script Telugu = new Script (new Tag ('T', 'e', 'l', 'u'));
		// 1.1
		/// <summary>
		/// The Thai (Thai) script typically used with text in the Thai (th) language originating from Thailand.
		/// </summary>
		public static readonly Script Thai = new Script (new Tag ('T', 'h', 'a', 'i'));
		// 2.0
		/// <summary>
		/// The Tibetan (Tibt) script typically used with text in the Tibetan (bo) language originating from China.
		/// </summary>
		public static readonly Script Tibetan = new Script (new Tag ('T', 'i', 'b', 't'));
		// 3.0
		/// <summary>
		/// The Bopomofo (Bopo) script typically used with text in the Chinese (zh) language originating from China.
		/// </summary>
		public static readonly Script Bopomofo = new Script (new Tag ('B', 'o', 'p', 'o'));
		// 3.0
		/// <summary>
		/// The Braille (Brai) script typically used with text in the Braille language originating from France.
		/// </summary>
		public static readonly Script Braille = new Script (new Tag ('B', 'r', 'a', 'i'));
		// 3.0
		/// <summary>
		/// The Unified Canadian Aboriginal Syllabics (Cans) script typically used with text in the Cree (cr) language originating from Canada.
		/// </summary>
		public static readonly Script CanadianSyllabics = new Script (new Tag ('C', 'a', 'n', 's'));
		// 3.0
		/// <summary>
		/// The Cherokee (Cher) script typically used with text in the Cherokee (chr) language originating from United States.
		/// </summary>
		public static readonly Script Cherokee = new Script (new Tag ('C', 'h', 'e', 'r'));
		// 3.0
		/// <summary>
		/// The Ethiopic (Ethi) script typically used with text in the Amharic (am) language originating from Ethiopia.
		/// </summary>
		public static readonly Script Ethiopic = new Script (new Tag ('E', 't', 'h', 'i'));
		// 3.0
		/// <summary>
		/// The Khmer (Khmr) script typically used with text in the Khmer (km) language originating from Cambodia.
		/// </summary>
		public static readonly Script Khmer = new Script (new Tag ('K', 'h', 'm', 'r'));
		// 3.0
		/// <summary>
		/// The Mongolian (Mong) script typically used with text in the Mongolian (mn) language originating from Mongolia.
		/// </summary>
		public static readonly Script Mongolian = new Script (new Tag ('M', 'o', 'n', 'g'));
		// 3.0
		/// <summary>
		/// The Myanmar (Mymr) script typically used with text in the Burmese (my) language originating from Myanmar.
		/// </summary>
		public static readonly Script Myanmar = new Script (new Tag ('M', 'y', 'm', 'r'));
		// 3.0
		/// <summary>
		/// The Ogham (Ogam) script typically used with text in the Old Irish (sga) language originating from Ireland.
		/// </summary>
		public static readonly Script Ogham = new Script (new Tag ('O', 'g', 'a', 'm'));
		// 3.0
		/// <summary>
		/// The Runic (Runr) script typically used with text in the Old Norse (non) language originating from Sweden.
		/// </summary>
		public static readonly Script Runic = new Script (new Tag ('R', 'u', 'n', 'r'));
		// 3.0
		/// <summary>
		/// The Sinhala (Sinh) script typically used with text in the Sinhala (si) language originating from Sri Lanka.
		/// </summary>
		public static readonly Script Sinhala = new Script (new Tag ('S', 'i', 'n', 'h'));
		// 3.0
		/// <summary>
		/// The Syriac (Syrc) script typically used with text in the Syriac (syr) language originating from Syria.
		/// </summary>
		public static readonly Script Syriac = new Script (new Tag ('S', 'y', 'r', 'c'));
		// 3.0
		/// <summary>
		/// The Thaana (Thaa) script typically used with text in the Divehi (dv) language originating from Maldives.
		/// </summary>
		public static readonly Script Thaana = new Script (new Tag ('T', 'h', 'a', 'a'));
		// 3.0
		/// <summary>
		/// The Yi (Yiii) script typically used with text in the Sichuan Yi (ii) language originating from China.
		/// </summary>
		public static readonly Script Yi = new Script (new Tag ('Y', 'i', 'i', 'i'));
		// 3.1
		/// <summary>
		/// The Deseret (Dsrt) script typically used with text in the English (en) language originating from United States.
		/// </summary>
		public static readonly Script Deseret = new Script (new Tag ('D', 's', 'r', 't'));
		// 3.1
		/// <summary>
		/// The Gothic (Goth) script typically used with text in the Gothic (got) language originating from Ukraine.
		/// </summary>
		public static readonly Script Gothic = new Script (new Tag ('G', 'o', 't', 'h'));
		// 3.1
		/// <summary>
		/// The Old Italic (Ital) script typically used with text in the Etruscan (ett) language originating from Italy.
		/// </summary>
		public static readonly Script OldItalic = new Script (new Tag ('I', 't', 'a', 'l'));
		// 3.2
		/// <summary>
		/// The Buhid (Buhd) script typically used with text in the Buhid (bku) language originating from Philippines.
		/// </summary>
		public static readonly Script Buhid = new Script (new Tag ('B', 'u', 'h', 'd'));
		// 3.2
		/// <summary>
		/// The Hanunoo (Hano) script typically used with text in the Hanunoo (hnn) language originating from Philippines.
		/// </summary>
		public static readonly Script Hanunoo = new Script (new Tag ('H', 'a', 'n', 'o'));
		// 3.2
		/// <summary>
		/// The Tagalog (Tglg) script typically used with text in the Filipino (fil) language originating from Philippines.
		/// </summary>
		public static readonly Script Tagalog = new Script (new Tag ('T', 'g', 'l', 'g'));
		// 3.2
		/// <summary>
		/// The Tagbanwa (Tagb) script typically used with text in the Tagbanwa (tbw) language originating from Philippines.
		/// </summary>
		public static readonly Script Tagbanwa = new Script (new Tag ('T', 'a', 'g', 'b'));
		// 4.0
		/// <summary>
		/// The Cypriot (Cprt) script typically used with text in the Ancient Greek (grc) language originating from Cyprus.
		/// </summary>
		public static readonly Script Cypriot = new Script (new Tag ('C', 'p', 'r', 't'));
		// 4.0
		/// <summary>
		/// The Limbu (Limb) script typically used with text in the Limbu (lif) language originating from India.
		/// </summary>
		public static readonly Script Limbu = new Script (new Tag ('L', 'i', 'm', 'b'));
		// 4.0
		/// <summary>
		/// The Linear B (Linb) script typically used with text in the Ancient Greek (grc) language originating from Greece.
		/// </summary>
		public static readonly Script LinearB = new Script (new Tag ('L', 'i', 'n', 'b'));
		// 4.0
		/// <summary>
		/// The Osmanya (Osma) script typically used with text in the Somali (so) language originating from Somalia.
		/// </summary>
		public static readonly Script Osmanya = new Script (new Tag ('O', 's', 'm', 'a'));
		// 4.0
		/// <summary>
		/// The Shavian (Shaw) script typically used with text in the English (en) language originating from United Kingdom.
		/// </summary>
		public static readonly Script Shavian = new Script (new Tag ('S', 'h', 'a', 'w'));
		// 4.0
		/// <summary>
		/// The Tai Le (Tale) script typically used with text in the Tai Nüa (tdd) language originating from China.
		/// </summary>
		public static readonly Script TaiLe = new Script (new Tag ('T', 'a', 'l', 'e'));
		// 4.0
		/// <summary>
		/// The Ugaritic (Ugar) script typically used with text in the Ugaritic (uga) language originating from Syria.
		/// </summary>
		public static readonly Script Ugaritic = new Script (new Tag ('U', 'g', 'a', 'r'));
		// 4.1
		/// <summary>
		/// The Buginese (Bugi) script typically used with text in the Buginese (bug) language originating from Indonesia.
		/// </summary>
		public static readonly Script Buginese = new Script (new Tag ('B', 'u', 'g', 'i'));
		// 4.1
		/// <summary>
		/// The Coptic (Copt) script typically used with text in the Coptic (cop) language originating from Egypt.
		/// </summary>
		public static readonly Script Coptic = new Script (new Tag ('C', 'o', 'p', 't'));
		// 4.1
		/// <summary>
		/// The Glagolitic (Glag) script typically used with text in the Church Slavic (cu) language originating from Bulgaria.
		/// </summary>
		public static readonly Script Glagolitic = new Script (new Tag ('G', 'l', 'a', 'g'));
		// 4.1
		/// <summary>
		/// The Kharoshthi (Khar) script typically used with text in the Gandhari (pra) language originating from Pakistan.
		/// </summary>
		public static readonly Script Kharoshthi = new Script (new Tag ('K', 'h', 'a', 'r'));
		// 4.1
		/// <summary>
		/// The New Tai Lue (Talu) script typically used with text in the Lü (khb) language originating from China.
		/// </summary>
		public static readonly Script NewTaiLue = new Script (new Tag ('T', 'a', 'l', 'u'));
		// 4.1
		/// <summary>
		/// The Old Persian (Xpeo) script typically used with text in the Old Persian (peo) language originating from Iran.
		/// </summary>
		public static readonly Script OldPersian = new Script (new Tag ('X', 'p', 'e', 'o'));
		// 4.1
		/// <summary>
		/// The Syloti Nagri (Sylo) script typically used with text in the Sylheti (syl) language originating from Bangladesh.
		/// </summary>
		public static readonly Script SylotiNagri = new Script (new Tag ('S', 'y', 'l', 'o'));
		// 4.1
		/// <summary>
		/// The Tifinagh (Tfng) script typically used with text in the Standard Moroccan Tamazight (zgh) language originating from Morocco.
		/// </summary>
		public static readonly Script Tifinagh = new Script (new Tag ('T', 'f', 'n', 'g'));
		// 5.0
		/// <summary>
		/// The Balinese (Bali) script typically used with text in the Balinese (ban) language originating from Indonesia.
		/// </summary>
		public static readonly Script Balinese = new Script (new Tag ('B', 'a', 'l', 'i'));
		// 5.0
		/// <summary>
		/// The Cuneiform (Xsux) script typically used with text in the Akkadian (akk) language originating from Iraq.
		/// </summary>
		public static readonly Script Cuneiform = new Script (new Tag ('X', 's', 'u', 'x'));
		// 5.0
		/// <summary>
		/// The Nko (Nkoo) script typically used with text in the Manding (man) language originating from Guinea.
		/// </summary>
		public static readonly Script Nko = new Script (new Tag ('N', 'k', 'o', 'o'));
		// 5.0
		/// <summary>
		/// The Phags Pa (Phag) script typically used with text in the Literary Chinese (lzh) language originating from China.
		/// </summary>
		public static readonly Script PhagsPa = new Script (new Tag ('P', 'h', 'a', 'g'));
		// 5.0
		/// <summary>
		/// The Phoenician (Phnx) script typically used with text in the Phoenician (phn) language originating from Lebanon.
		/// </summary>
		public static readonly Script Phoenician = new Script (new Tag ('P', 'h', 'n', 'x'));
		// 5.1
		/// <summary>
		/// The Carian (Cari) script typically used with text in the Carian (xcr) language originating from Turkey.
		/// </summary>
		public static readonly Script Carian = new Script (new Tag ('C', 'a', 'r', 'i'));
		// 5.1
		/// <summary>
		/// The Cham (Cham) script typically used with text in the Eastern Cham (cjm) language originating from Vietnam.
		/// </summary>
		public static readonly Script Cham = new Script (new Tag ('C', 'h', 'a', 'm'));
		// 5.1
		/// <summary>
		/// The Kayah Li (Kali) script typically used with text in the Eastern Kayah (eky) language originating from Myanmar.
		/// </summary>
		public static readonly Script KayahLi = new Script (new Tag ('K', 'a', 'l', 'i'));
		// 5.1
		/// <summary>
		/// The Lepcha (Lepc) script typically used with text in the Lepcha (lep) language originating from India.
		/// </summary>
		public static readonly Script Lepcha = new Script (new Tag ('L', 'e', 'p', 'c'));
		// 5.1
		/// <summary>
		/// The Lycian (Lyci) script typically used with text in the Lycian (xlc) language originating from Turkey.
		/// </summary>
		public static readonly Script Lycian = new Script (new Tag ('L', 'y', 'c', 'i'));
		// 5.1
		/// <summary>
		/// The Lydian (Lydi) script typically used with text in the Lydian (xld) language originating from Turkey.
		/// </summary>
		public static readonly Script Lydian = new Script (new Tag ('L', 'y', 'd', 'i'));
		// 5.1
		/// <summary>
		/// The Ol Chiki (Olck) script typically used with text in the Santali (sat) language originating from India.
		/// </summary>
		public static readonly Script OlChiki = new Script (new Tag ('O', 'l', 'c', 'k'));
		// 5.1
		/// <summary>
		/// The Rejang (Rjng) script typically used with text in the Rejang (rej) language originating from Indonesia.
		/// </summary>
		public static readonly Script Rejang = new Script (new Tag ('R', 'j', 'n', 'g'));
		// 5.1
		/// <summary>
		/// The Saurashtra (Saur) script typically used with text in the Saurashtra (saz) language originating from India.
		/// </summary>
		public static readonly Script Saurashtra = new Script (new Tag ('S', 'a', 'u', 'r'));
		// 5.1
		/// <summary>
		/// The Sundanese (Sund) script typically used with text in the Sundanese (su) language originating from Indonesia.
		/// </summary>
		public static readonly Script Sundanese = new Script (new Tag ('S', 'u', 'n', 'd'));
		// 5.1
		/// <summary>
		/// The Vai (Vaii) script typically used with text in the Vai (vai) language originating from Liberia.
		/// </summary>
		public static readonly Script Vai = new Script (new Tag ('V', 'a', 'i', 'i'));
		// 5.2
		/// <summary>
		/// The Avestan (Avst) script typically used with text in the Avestan (ae) language originating from Iran.
		/// </summary>
		public static readonly Script Avestan = new Script (new Tag ('A', 'v', 's', 't'));
		// 5.2
		/// <summary>
		/// The Bamum (Bamu) script typically used with text in the Bamun (bax) language originating from Cameroon.
		/// </summary>
		public static readonly Script Bamum = new Script (new Tag ('B', 'a', 'm', 'u'));
		// 5.2
		/// <summary>
		/// The Egyptian Hieroglyphs (Egyp) script typically used with text in the Ancient Egyptian (egy) language originating from Egypt.
		/// </summary>
		public static readonly Script EgyptianHieroglyphs = new Script (new Tag ('E', 'g', 'y', 'p'));
		// 5.2
		/// <summary>
		/// The Imperial Aramaic (Armi) script typically used with text in the Aramaic (arc) language originating from Iran.
		/// </summary>
		public static readonly Script ImperialAramaic = new Script (new Tag ('A', 'r', 'm', 'i'));
		// 5.2
		/// <summary>
		/// The Inscriptional Pahlavi (Phli) script typically used with text in the Pahlavi (pal) language originating from Iran.
		/// </summary>
		public static readonly Script InscriptionalPahlavi = new Script (new Tag ('P', 'h', 'l', 'i'));
		// 5.2
		/// <summary>
		/// The Inscriptional Parthian (Prti) script typically used with text in the Parthian (xpr) language originating from Iran.
		/// </summary>
		public static readonly Script InscriptionalParthian = new Script (new Tag ('P', 'r', 't', 'i'));
		// 5.2
		/// <summary>
		/// The Javanese (Java) script typically used with text in the Javanese (jv) language originating from Indonesia.
		/// </summary>
		public static readonly Script Javanese = new Script (new Tag ('J', 'a', 'v', 'a'));
		// 5.2
		/// <summary>
		/// The Kaithi (Kthi) script typically used with text in the Bhojpuri (bho) language originating from India.
		/// </summary>
		public static readonly Script Kaithi = new Script (new Tag ('K', 't', 'h', 'i'));
		// 5.2
		/// <summary>
		/// The Lisu (Lisu) script typically used with text in the Lisu (lis) language originating from China.
		/// </summary>
		public static readonly Script Lisu = new Script (new Tag ('L', 'i', 's', 'u'));
		// 5.2
		/// <summary>
		/// The Meetei Mayek (Mtei) script typically used with text in the Manipuri (mni) language originating from India.
		/// </summary>
		public static readonly Script MeeteiMayek = new Script (new Tag ('M', 't', 'e', 'i'));
		// 5.2
		/// <summary>
		/// The Old South Arabian (Sarb) script typically used with text in the Sabaean (xsa) language originating from Yemen.
		/// </summary>
		public static readonly Script OldSouthArabian = new Script (new Tag ('S', 'a', 'r', 'b'));
		// 5.2
		/// <summary>
		/// The Old Turkic (Orkh) script typically used with text in the Old Turkish (otk) language originating from Mongolia.
		/// </summary>
		public static readonly Script OldTurkic = new Script (new Tag ('O', 'r', 'k', 'h'));
		// 5.2
		/// <summary>
		/// The Samaritan (Samr) script typically used with text in the Samaritan Hebrew (smp) language originating from Israel.
		/// </summary>
		public static readonly Script Samaritan = new Script (new Tag ('S', 'a', 'm', 'r'));
		// 5.2
		/// <summary>
		/// The Tai Tham (Lana) script typically used with text in the Northern Thai (nod) language originating from Thailand.
		/// </summary>
		public static readonly Script TaiTham = new Script (new Tag ('L', 'a', 'n', 'a'));
		// 5.2
		/// <summary>
		/// The Tai Viet (Tavt) script typically used with text in the Tai Dam (blt) language originating from Vietnam.
		/// </summary>
		public static readonly Script TaiViet = new Script (new Tag ('T', 'a', 'v', 't'));
		// 6.0
		/// <summary>
		/// The Batak (Batk) script typically used with text in the Batak Toba (bbc) language originating from Indonesia.
		/// </summary>
		public static readonly Script Batak = new Script (new Tag ('B', 'a', 't', 'k'));
		// 6.0
		/// <summary>
		/// The Brahmi (Brah) script typically used with text in the Ardhamāgadhī Prākrit (pka) language originating from India.
		/// </summary>
		public static readonly Script Brahmi = new Script (new Tag ('B', 'r', 'a', 'h'));
		// 6.0
		/// <summary>
		/// The Mandaic (Mand) script typically used with text in the Classical Mandaic (myz) language originating from Iran.
		/// </summary>
		public static readonly Script Mandaic = new Script (new Tag ('M', 'a', 'n', 'd'));
		// 6.1
		/// <summary>
		/// The Chakma (Cakm) script typically used with text in the Chakma (ccp) language originating from Bangladesh.
		/// </summary>
		public static readonly Script Chakma = new Script (new Tag ('C', 'a', 'k', 'm'));
		// 6.1
		/// <summary>
		/// The Meroitic Cursive (Merc) script typically used with text in the Meroitic (xmr) language originating from Sudan.
		/// </summary>
		public static readonly Script MeroiticCursive = new Script (new Tag ('M', 'e', 'r', 'c'));
		// 6.1
		/// <summary>
		/// The Meroitic Hieroglyphs (Mero) script typically used with text in the Meroitic (xmr) language originating from Sudan.
		/// </summary>
		public static readonly Script MeroiticHieroglyphs = new Script (new Tag ('M', 'e', 'r', 'o'));
		// 6.1
		/// <summary>
		/// The Miao (Plrd) script typically used with text in the Large Flowery Miao (hmd) language originating from China.
		/// </summary>
		public static readonly Script Miao = new Script (new Tag ('P', 'l', 'r', 'd'));
		// 6.1
		/// <summary>
		/// The Sharada (Shrd) script typically used with text in the Sanskrit (sa) language originating from India.
		/// </summary>
		public static readonly Script Sharada = new Script (new Tag ('S', 'h', 'r', 'd'));
		// 6.1
		/// <summary>
		/// The Sora Sompeng (Sora) script typically used with text in the Sora (srb) language originating from India.
		/// </summary>
		public static readonly Script SoraSompeng = new Script (new Tag ('S', 'o', 'r', 'a'));
		// 6.1
		/// <summary>
		/// The Takri (Takr) script typically used with text in the Dogri (doi) language originating from India.
		/// </summary>
		public static readonly Script Takri = new Script (new Tag ('T', 'a', 'k', 'r'));

		// Since: 0.9.30

		// 7.0
		/// <summary>
		/// The Bassa Vah (Bass) script typically used with text in the Bassa (bsq) language originating from Liberia.
		/// </summary>
		public static readonly Script BassaVah = new Script (new Tag ('B', 'a', 's', 's'));
		// 7.0
		/// <summary>
		/// The Caucasian Albanian (Aghb) script typically used with text in the Lezgian (lez) language originating from Russia.
		/// </summary>
		public static readonly Script CaucasianAlbanian = new Script (new Tag ('A', 'g', 'h', 'b'));
		// 7.0
		/// <summary>
		/// The Duployan (Dupl) script typically used with text in the French (fr) language originating from France.
		/// </summary>
		public static readonly Script Duployan = new Script (new Tag ('D', 'u', 'p', 'l'));
		// 7.0
		/// <summary>
		/// The Elbasan (Elba) script typically used with text in the Albanian (sq) language originating from Albania.
		/// </summary>
		public static readonly Script Elbasan = new Script (new Tag ('E', 'l', 'b', 'a'));
		// 7.0
		/// <summary>
		/// The Grantha (Gran) script typically used with text in the Sanskrit (sa) language originating from India.
		/// </summary>
		public static readonly Script Grantha = new Script (new Tag ('G', 'r', 'a', 'n'));
		// 7.0
		/// <summary>
		/// The Khojki (Khoj) script typically used with text in the Sindhi (sd) language originating from India.
		/// </summary>
		public static readonly Script Khojki = new Script (new Tag ('K', 'h', 'o', 'j'));
		// 7.0
		/// <summary>
		/// The Khudawadi (Sind) script typically used with text in the Sindhi (sd) language originating from India.
		/// </summary>
		public static readonly Script Khudawadi = new Script (new Tag ('S', 'i', 'n', 'd'));
		// 7.0
		/// <summary>
		/// The Linear A (Lina) script typically used with text in the Linear A (lab) language originating from Greece.
		/// </summary>
		public static readonly Script LinearA = new Script (new Tag ('L', 'i', 'n', 'a'));
		// 7.0
		/// <summary>
		/// The Mahajani (Mahj) script typically used with text in the Hindi (hi) language originating from India.
		/// </summary>
		public static readonly Script Mahajani = new Script (new Tag ('M', 'a', 'h', 'j'));
		// 7.0
		/// <summary>
		/// The Manichaean (Mani) script typically used with text in the Manichaean Middle Persian (xmn) language originating from China.
		/// </summary>
		public static readonly Script Manichaean = new Script (new Tag ('M', 'a', 'n', 'i'));
		// 7.0
		/// <summary>
		/// The Mende Kikakui (Mend) script typically used with text in the Mende (men) language originating from Sierra Leone.
		/// </summary>
		public static readonly Script MendeKikakui = new Script (new Tag ('M', 'e', 'n', 'd'));
		// 7.0
		/// <summary>
		/// The Modi (Modi) script typically used with text in the Marathi (mr) language originating from India.
		/// </summary>
		public static readonly Script Modi = new Script (new Tag ('M', 'o', 'd', 'i'));
		// 7.0
		/// <summary>
		/// The Mro (Mroo) script typically used with text in the Mru (mro) language originating from Bangladesh.
		/// </summary>
		public static readonly Script Mro = new Script (new Tag ('M', 'r', 'o', 'o'));
		// 7.0
		/// <summary>
		/// The Nabataean (Nbat) script typically used with text in the Official Aramaic (700-300 BCE) (arc) language originating from Jordan.
		/// </summary>
		public static readonly Script Nabataean = new Script (new Tag ('N', 'b', 'a', 't'));
		// 7.0
		/// <summary>
		/// The Old North Arabian (Narb) script typically used with text in the Ancient North Arabian (xna) language originating from Saudi Arabia.
		/// </summary>
		public static readonly Script OldNorthArabian = new Script (new Tag ('N', 'a', 'r', 'b'));
		// 7.0
		/// <summary>
		/// The Old Permic (Perm) script typically used with text in the Komi (kv) language originating from Russia.
		/// </summary>
		public static readonly Script OldPermic = new Script (new Tag ('P', 'e', 'r', 'm'));
		// 7.0
		/// <summary>
		/// The Pahawh Hmong (Hmng) script typically used with text in the Hmong Njua (hnj) language originating from Laos.
		/// </summary>
		public static readonly Script PahawhHmong = new Script (new Tag ('H', 'm', 'n', 'g'));
		// 7.0
		/// <summary>
		/// The Palmyrene (Palm) script typically used with text in the Official Aramaic (700-300 BCE) (arc) language originating from Syria.
		/// </summary>
		public static readonly Script Palmyrene = new Script (new Tag ('P', 'a', 'l', 'm'));
		// 7.0
		/// <summary>
		/// The Pau Cin Hau (Pauc) script typically used with text in the Tedim Chin (ctd) language originating from Myanmar.
		/// </summary>
		public static readonly Script PauCinHau = new Script (new Tag ('P', 'a', 'u', 'c'));
		// 7.0
		/// <summary>
		/// The Psalter Pahlavi (Phlp) script typically used with text in the Pahlavi (pal) language originating from China.
		/// </summary>
		public static readonly Script PsalterPahlavi = new Script (new Tag ('P', 'h', 'l', 'p'));
		// 7.0
		/// <summary>
		/// The Siddham (Sidd) script typically used with text in the Sanskrit (sa) language originating from India.
		/// </summary>
		public static readonly Script Siddham = new Script (new Tag ('S', 'i', 'd', 'd'));
		// 7.0
		/// <summary>
		/// The Tirhuta (Tirh) script typically used with text in the Maithili (mai) language originating from India.
		/// </summary>
		public static readonly Script Tirhuta = new Script (new Tag ('T', 'i', 'r', 'h'));
		// 7.0
		/// <summary>
		/// The Warang Citi (Wara) script typically used with text in the Ho (hoc) language originating from India.
		/// </summary>
		public static readonly Script WarangCiti = new Script (new Tag ('W', 'a', 'r', 'a'));
		// 8.0
		/// <summary>
		/// The Ahom (Ahom) script typically used with text in the Ahom (aho) language originating from India.
		/// </summary>
		public static readonly Script Ahom = new Script (new Tag ('A', 'h', 'o', 'm'));
		// 8.0
		/// <summary>
		/// The Anatolian Hieroglyphs (Hluw) script typically used with text in the Hieroglyphic Luwian (hlu) language originating from Turkey.
		/// </summary>
		public static readonly Script AnatolianHieroglyphs = new Script (new Tag ('H', 'l', 'u', 'w'));
		// 8.0
		/// <summary>
		/// The Hatran (Hatr) script typically used with text in the Uncoded Languages (mis) language originating from Iraq.
		/// </summary>
		public static readonly Script Hatran = new Script (new Tag ('H', 'a', 't', 'r'));
		// 8.0
		/// <summary>
		/// The Multani (Mult) script typically used with text in the Seraiki (skr) language originating from Pakistan.
		/// </summary>
		public static readonly Script Multani = new Script (new Tag ('M', 'u', 'l', 't'));
		// 8.0
		/// <summary>
		/// The Old Hungarian (Hung) script typically used with text in the Hungarian (hu) language originating from Hungary.
		/// </summary>
		public static readonly Script OldHungarian = new Script (new Tag ('H', 'u', 'n', 'g'));
		// 8.0
		/// <summary>
		/// The Sign Writing (Sgnw) script typically used with text in the American Sign Language (ase) language originating from United States.
		/// </summary>
		public static readonly Script Signwriting = new Script (new Tag ('S', 'g', 'n', 'w'));

		// Since 1.3.0

		// 9.0
		/// <summary>
		/// The Adlam (Adlm) script typically used with text in the Fulah (ff) language originating from Guinea.
		/// </summary>
		public static readonly Script Adlam = new Script (new Tag ('A', 'd', 'l', 'm'));
		// 9.0
		/// <summary>
		/// The Bhaiksuki (Bhks) script typically used with text in the Sanskrit (sa) language originating from India.
		/// </summary>
		public static readonly Script Bhaiksuki = new Script (new Tag ('B', 'h', 'k', 's'));
		// 9.0
		/// <summary>
		/// The Marchen (Marc) script typically used with text in the Tibetan (bo) language originating from China.
		/// </summary>
		public static readonly Script Marchen = new Script (new Tag ('M', 'a', 'r', 'c'));
		// 9.0
		/// <summary>
		/// The Osage (Osge) script typically used with text in the Osage (osa) language originating from United States.
		/// </summary>
		public static readonly Script Osage = new Script (new Tag ('O', 's', 'g', 'e'));
		// 9.0
		/// <summary>
		/// The Tangut (Tang) script typically used with text in the Tangut (txg) language originating from China.
		/// </summary>
		public static readonly Script Tangut = new Script (new Tag ('T', 'a', 'n', 'g'));
		// 9.0
		/// <summary>
		/// The Newa (Newa) script typically used with text in the Newari (new) language originating from Nepal.
		/// </summary>
		public static readonly Script Newa = new Script (new Tag ('N', 'e', 'w', 'a'));

		// Since 1.6.0

		// 10.0
		/// <summary>
		/// The Masaram Gondi (Gonm) script typically used with text in the Aheri Gondi (esg) language originating from India.
		/// </summary>
		public static readonly Script MasaramGondi = new Script (new Tag ('G', 'o', 'n', 'm'));
		// 10.0
		/// <summary>
		/// The Nushu (Nshu) script typically used with text in the Chinese language family (zhx) language originating from China.
		/// </summary>
		public static readonly Script Nushu = new Script (new Tag ('N', 's', 'h', 'u'));
		// 10.0
		/// <summary>
		/// The Soyombo (Soyo) script typically used with text in the Classical Mongolian (cmg) language originating from Mongolia.
		/// </summary>
		public static readonly Script Soyombo = new Script (new Tag ('S', 'o', 'y', 'o'));
		// 10.0
		/// <summary>
		/// The Zanabazar Square (Zanb) script typically used with text in the Classical Mongolian (cmg) language originating from Mongolia.
		/// </summary>
		public static readonly Script ZanabazarSquare = new Script (new Tag ('Z', 'a', 'n', 'b'));

		// Since 1.8.0

		// 11.0
		/// <summary>
		/// The Dogra (Dogr) script typically used with text in the Dogri (doi) language originating from India.
		/// </summary>
		public static readonly Script Dogra = new Script (new Tag ('D', 'o', 'g', 'r'));
		// 11.0
		/// <summary>
		/// The Gunjala Gondi (Gong) script typically used with text in the Adilabad Gondi (wsg) language originating from India.
		/// </summary>
		public static readonly Script GunjalaGondi = new Script (new Tag ('G', 'o', 'n', 'g'));
		// 11.0
		/// <summary>
		/// The Hanifi Rohingya (Rohg) script typically used with text in the Rohingya (rhg) language originating from Myanmar.
		/// </summary>
		public static readonly Script HanifiRohingya = new Script (new Tag ('R', 'o', 'h', 'g'));
		// 11.0
		/// <summary>
		/// The Makasar (Maka) script typically used with text in the Makasar (mak) language originating from Indonesia.
		/// </summary>
		public static readonly Script Makasar = new Script (new Tag ('M', 'a', 'k', 'a'));
		// 11.0
		/// <summary>
		/// The Medefaidrin (Medf) script typically used with text in the Medefaidrin (mis) language originating from Nigeria.
		/// </summary>
		public static readonly Script Medefaidrin = new Script (new Tag ('M', 'e', 'd', 'f'));
		// 11.0
		/// <summary>
		/// The Old Sogdian (Sogo) script typically used with text in the Sogdian (sog) language originating from Uzbekistan.
		/// </summary>
		public static readonly Script OldSogdian = new Script (new Tag ('S', 'o', 'g', 'o'));
		// 11.0
		/// <summary>
		/// The Sogdian (Sogd) script typically used with text in the Sogdian (sog) language originating from Uzbekistan.
		/// </summary>
		public static readonly Script Sogdian = new Script (new Tag ('S', 'o', 'g', 'd'));
	}
}
