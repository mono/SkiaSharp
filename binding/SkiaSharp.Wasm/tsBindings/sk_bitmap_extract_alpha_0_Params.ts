/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_extract_alpha_0_Params
	{
		/* Pack=4 */
		cbitmap : number;
		dst : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_extract_alpha_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_extract_alpha_0_Params();
			
			{
				ret.cbitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
