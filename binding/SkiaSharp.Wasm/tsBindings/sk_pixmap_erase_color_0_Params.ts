/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_erase_color_0_Params
	{
		/* Pack=4 */
		cpixmap : number;
		color : SkiaSharp.SKColor;
		subset : SkiaSharp.SKRectI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pixmap_erase_color_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pixmap_erase_color_0_Params();
			
			{
				ret.cpixmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.color = SkiaSharp.SKColor.unmarshal(pData + 4);
			}
			
			{
				ret.subset = SkiaSharp.SKRectI.unmarshal(pData + 8);
			}
			return ret;
		}
	}
}
