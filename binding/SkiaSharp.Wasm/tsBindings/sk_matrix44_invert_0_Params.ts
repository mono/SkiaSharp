/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_invert_0_Params
	{
		/* Pack=4 */
		matrix : number;
		inverse : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_invert_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_invert_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.inverse = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
