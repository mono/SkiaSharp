/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_extract_subset_0_Params
	{
		/* Pack=4 */
		cpixmap : number;
		result : number;
		subset : SkiaSharp.SKRectI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pixmap_extract_subset_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pixmap_extract_subset_0_Params();
			
			{
				ret.cpixmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.result = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.subset = SkiaSharp.SKRectI.unmarshal(pData + 8);
			}
			return ret;
		}
	}
}
