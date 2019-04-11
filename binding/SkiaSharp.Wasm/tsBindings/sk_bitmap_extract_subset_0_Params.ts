/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_extract_subset_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		cdst : number;
		subset : SkiaSharp.SKRectI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_extract_subset_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_extract_subset_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cdst = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.subset = SkiaSharp.SKRectI.unmarshal(pData + 8);
			}
			return ret;
		}
	}
}
