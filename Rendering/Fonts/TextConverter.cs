using System;
using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Extensions;

namespace DeltaEngine.Rendering.Fonts
{
	/// <summary>
	/// Takes a string of text and returns an array of graphical glyph data for rendering.
	/// </summary>
	public class TextConverter
	{
		public TextConverter(Dictionary<char, Glyph> glyphDictionary, int pixelLineHeight)
		{
			this.glyphDictionary = glyphDictionary;
			wrapper = new TextWrapper(glyphDictionary, FallbackCharForUnsupportedCharacters,
				pixelLineHeight);
			MaxTextPixelSize = Size.Zero;
		}

		private readonly Dictionary<char, Glyph> glyphDictionary;
		private readonly TextWrapper wrapper;
		private const char FallbackCharForUnsupportedCharacters = '?';
		public Size MaxTextPixelSize { get; private set; }

		public GlyphDrawData[] GetRenderableGlyphs(string text, HorizontalAlignment alignment)
		{
			var glyphs = new List<GlyphDrawData>();
			lastDrawData = CreateFirstGlyphDrawData(wrapper.GetFontHeight());
			List<List<char>> textlines = wrapper.SplitTextIntoLines(text, MaxTextPixelSize, false);

			for (int lineIndex = 0; lineIndex < textlines.Count; lineIndex++)
			{
				List<char> textLine = textlines[lineIndex];
				if (!IsTextLineEmpty(textLine))
				{
					AlignTextLineHorizontally(textLine, lineIndex, alignment);
					float totalGlyphWidth = 0.0f;
					float lineStartX = lastDrawData.DrawArea.Left;
					Glyph lastGlyph = null;
					foreach (char lineCharacter in textLine)
					{
						Glyph characterGlyph = GetGlyphFromDictionary(lineCharacter);
						totalGlyphWidth += GetKerningFromDictionary(lineCharacter, lastGlyph);
						var newDrawInfo = PlaceGlyphInLine(characterGlyph, lineStartX, totalGlyphWidth);
						glyphs.Add(newDrawInfo);
						lastDrawData = newDrawInfo;
						totalGlyphWidth += (float)Math.Round(characterGlyph.AdvanceWidth);
						lastGlyph = characterGlyph;
					}
				}
				lastDrawData.DrawArea.Top += wrapper.GetFontHeight();
			}

			MaxTextPixelSize = new Size(wrapper.MaxTextLineWidth, lastDrawData.DrawArea.Top);
			return glyphs.ToArray();
		}

		private GlyphDrawData lastDrawData;

		private static GlyphDrawData CreateFirstGlyphDrawData(float totalLineHeight)
		{
			return new GlyphDrawData
			{
				DrawArea = new Rectangle(Point.Zero, new Size(0, totalLineHeight)),
				UV = new Rectangle(Point.Zero, new Size(0, totalLineHeight))
			};
		}

		private static bool IsTextLineEmpty(List<char> textLine)
		{
			return textLine.Count <= 0;
		}

		private void AlignTextLineHorizontally(List<char> textLine, int lineIndex,
			HorizontalAlignment alignment)
		{
			if (alignment == HorizontalAlignment.Center)
				CenterTextLine(textLine, lineIndex);
			else if (alignment == HorizontalAlignment.Left)
				LeftAlignTextLine(textLine);
			else
				RightAlignTextLine(textLine, lineIndex);
		}

		private void CenterTextLine(List<char> textLine, int lineIndex)
		{
			char firstChar = textLine[0];
			lastDrawData.DrawArea.Left =
				MathExtensions.Round((wrapper.MaxTextLineWidth - wrapper.TextLineWidths[lineIndex]) * 0.5f -
					glyphDictionary[firstChar].LeftSideBearing);
		}

		private void LeftAlignTextLine(List<char> textLine)
		{
			char firstChar = textLine[0];
			lastDrawData.DrawArea.Left =
				- MathExtensions.Round(glyphDictionary[firstChar].LeftSideBearing);
		}

		private void RightAlignTextLine(List<char> textLine, int lineIndex)
		{
			char lastChar = textLine[textLine.Count - 1];
			lastDrawData.DrawArea.Left =
				MathExtensions.Round((wrapper.MaxTextLineWidth - wrapper.TextLineWidths[lineIndex]) -
					glyphDictionary[lastChar].RightSideBearing);
		}

		private Glyph GetGlyphFromDictionary(char textChar)
		{
			Glyph characterGlyph;
			return glyphDictionary.TryGetValue(textChar, out characterGlyph)
				? characterGlyph : glyphDictionary[FallbackCharForUnsupportedCharacters];
		}

		private static int GetKerningFromDictionary(char textChar, Glyph lastGlyph)
		{
			int characterKerning;
			return lastGlyph != null && lastGlyph.Kernings != null &&
				lastGlyph.Kernings.TryGetValue(textChar, out characterKerning) ? characterKerning : 0;
		}

		private GlyphDrawData PlaceGlyphInLine(Glyph characterGlyph, float lineStartX,
			float totalGlyphWidth)
		{
			var glyph = new GlyphDrawData();
			var position =
				new Point(
					MathExtensions.Round(lineStartX + totalGlyphWidth + characterGlyph.LeftSideBearing),
					lastDrawData.DrawArea.Top);
			glyph.DrawArea = new Rectangle(position, characterGlyph.UV.Size);
			glyph.UV = characterGlyph.PrecomputedFontMapUV;
			return glyph;
		}
	}
}