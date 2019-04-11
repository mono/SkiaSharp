/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_rrect_contains_0_Params
	{
		/* Pack=4 */
		rrect : number;
		rect : SkiaSharp.SKRect;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_rrect_contains_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_rrect_contains_0_Params();
			
			{
				ret.rrect = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
