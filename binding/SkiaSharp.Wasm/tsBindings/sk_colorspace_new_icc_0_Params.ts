/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspace_new_icc_0_Params
	{
		/* Pack=4 */
		input : number;
		len : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorspace_new_icc_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorspace_new_icc_0_Params();
			
			{
				ret.input = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.len = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
