/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_preserves_2d_axis_alignment_0_Params
	{
		/* Pack=4 */
		matrix : number;
		epsilon : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_preserves_2d_axis_alignment_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_preserves_2d_axis_alignment_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.epsilon = Number(memoryContext.getValue(pData + 4, "float"));
			}
			return ret;
		}
	}
}
