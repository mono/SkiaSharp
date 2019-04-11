/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_nway_canvas_new_0_Params
	{
		/* Pack=4 */
		width : number;
		height : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_nway_canvas_new_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_nway_canvas_new_0_Params();
			
			{
				ret.width = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.height = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
