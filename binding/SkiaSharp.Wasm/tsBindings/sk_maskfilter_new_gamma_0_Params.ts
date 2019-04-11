/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_maskfilter_new_gamma_0_Params
	{
		/* Pack=4 */
		gamma : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_maskfilter_new_gamma_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_maskfilter_new_gamma_0_Params();
			
			{
				ret.gamma = Number(memoryContext.getValue(pData + 0, "float"));
			}
			return ret;
		}
	}
}
