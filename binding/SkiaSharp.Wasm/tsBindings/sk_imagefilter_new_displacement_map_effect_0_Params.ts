/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_displacement_map_effect_0_Params
	{
		/* Pack=4 */
		xChannelSelector : number;
		yChannelSelector : number;
		scale : number;
		displacement : number;
		color : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_displacement_map_effect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_displacement_map_effect_0_Params();
			
			{
				ret.xChannelSelector = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.yChannelSelector = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.scale = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.displacement = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.color = Number(memoryContext.getValue(pData + 16, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 20, "*"));
			}
			return ret;
		}
	}
}
