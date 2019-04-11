/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_maskfilter_new_clip_0_Params
	{
		/* Pack=4 */
		min : number;
		max : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_maskfilter_new_clip_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_maskfilter_new_clip_0_Params();
			
			{
				ret.min = Number(memoryContext.getValue(pData + 0, "i8"));
			}
			
			{
				ret.max = Number(memoryContext.getValue(pData + 4, "i8"));
			}
			return ret;
		}
	}
}
