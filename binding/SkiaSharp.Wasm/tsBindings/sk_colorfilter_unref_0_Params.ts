/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_unref_0_Params
	{
		/* Pack=4 */
		filter : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorfilter_unref_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorfilter_unref_0_Params();
			
			{
				ret.filter = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
