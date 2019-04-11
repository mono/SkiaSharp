/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_set_rotate_about_radians_0_Params
	{
		/* Pack=4 */
		matrix : number;
		x : number;
		y : number;
		z : number;
		radians : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_set_rotate_about_radians_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_set_rotate_about_radians_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.z = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.radians = Number(memoryContext.getValue(pData + 16, "float"));
			}
			return ret;
		}
	}
}
