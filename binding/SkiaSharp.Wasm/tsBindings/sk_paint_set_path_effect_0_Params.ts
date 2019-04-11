/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_set_path_effect_0_Params
	{
		/* Pack=4 */
		cpaint : number;
		effect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_set_path_effect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_set_path_effect_0_Params();
			
			{
				ret.cpaint = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.effect = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
