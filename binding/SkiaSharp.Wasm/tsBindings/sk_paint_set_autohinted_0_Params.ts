/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_set_autohinted_0_Params
	{
		/* Pack=4 */
		cpaint : number;
		useAutohinter : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_set_autohinted_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_set_autohinted_0_Params();
			
			{
				ret.cpaint = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.useAutohinter = Boolean(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
