/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_pre_concat_0_Params
	{
		/* Pack=4 */
		matrix : number;
		m : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_pre_concat_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_pre_concat_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.m = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
