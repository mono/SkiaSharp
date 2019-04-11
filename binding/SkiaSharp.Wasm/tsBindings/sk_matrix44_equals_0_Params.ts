/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_equals_0_Params
	{
		/* Pack=4 */
		matrix : number;
		other : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_equals_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_equals_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.other = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
