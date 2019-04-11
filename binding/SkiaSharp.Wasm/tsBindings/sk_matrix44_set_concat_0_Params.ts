/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_set_concat_0_Params
	{
		/* Pack=4 */
		matrix : number;
		a : number;
		b : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_set_concat_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_set_concat_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.a = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.b = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
