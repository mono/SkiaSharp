/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_picture_get_cull_rect_0_Params
	{
		/* Pack=4 */
		p : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_picture_get_cull_rect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_picture_get_cull_rect_0_Params();
			
			{
				ret.p = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
