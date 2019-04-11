/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKFontMetrics
	{
		/* Pack=4 */
		flags : number;
		top : number;
		ascent : number;
		descent : number;
		bottom : number;
		leading : number;
		avgCharWidth : number;
		maxCharWidth : number;
		xMin : number;
		xMax : number;
		xHeight : number;
		capHeight : number;
		underlineThickness : number;
		underlinePosition : number;
		strikeoutThickness : number;
		strikeoutPosition : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKFontMetrics
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKFontMetrics();
			
			{
				ret.flags = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.top = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.ascent = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.descent = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.bottom = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.leading = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.avgCharWidth = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.maxCharWidth = Number(memoryContext.getValue(pData + 28, "float"));
			}
			
			{
				ret.xMin = Number(memoryContext.getValue(pData + 32, "float"));
			}
			
			{
				ret.xMax = Number(memoryContext.getValue(pData + 36, "float"));
			}
			
			{
				ret.xHeight = Number(memoryContext.getValue(pData + 40, "float"));
			}
			
			{
				ret.capHeight = Number(memoryContext.getValue(pData + 44, "float"));
			}
			
			{
				ret.underlineThickness = Number(memoryContext.getValue(pData + 48, "float"));
			}
			
			{
				ret.underlinePosition = Number(memoryContext.getValue(pData + 52, "float"));
			}
			
			{
				ret.strikeoutThickness = Number(memoryContext.getValue(pData + 56, "float"));
			}
			
			{
				ret.strikeoutPosition = Number(memoryContext.getValue(pData + 60, "float"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(64);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.flags, "i32");
			memoryContext.setValue(pData + 4, this.top, "float");
			memoryContext.setValue(pData + 8, this.ascent, "float");
			memoryContext.setValue(pData + 12, this.descent, "float");
			memoryContext.setValue(pData + 16, this.bottom, "float");
			memoryContext.setValue(pData + 20, this.leading, "float");
			memoryContext.setValue(pData + 24, this.avgCharWidth, "float");
			memoryContext.setValue(pData + 28, this.maxCharWidth, "float");
			memoryContext.setValue(pData + 32, this.xMin, "float");
			memoryContext.setValue(pData + 36, this.xMax, "float");
			memoryContext.setValue(pData + 40, this.xHeight, "float");
			memoryContext.setValue(pData + 44, this.capHeight, "float");
			memoryContext.setValue(pData + 48, this.underlineThickness, "float");
			memoryContext.setValue(pData + 52, this.underlinePosition, "float");
			memoryContext.setValue(pData + 56, this.strikeoutThickness, "float");
			memoryContext.setValue(pData + 60, this.strikeoutPosition, "float");
		}
	}
}
