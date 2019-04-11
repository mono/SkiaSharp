/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_3dview_rotate_x_degrees_0_Params
	{
		/* Pack=4 */
		cview : number;
		degrees : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_3dview_rotate_x_degrees_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_3dview_rotate_x_degrees_0_Params();
			
			{
				ret.cview = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.degrees = Number(memoryContext.getValue(pData + 4, "float"));
			}
			return ret;
		}
	}
}
