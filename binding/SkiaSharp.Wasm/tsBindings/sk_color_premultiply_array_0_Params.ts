/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_color_premultiply_array_0_Params
	{
		/* Pack=4 */
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		size : number;
		pmcolors_Length : number;
		pmcolors : Array<SkiaSharp.SKPMColor>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_color_premultiply_array_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_color_premultiply_array_0_Params();
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*"); /*SkiaSharp.SKColor 4 False*/
				if(pArray !== 0)
				{
					ret.colors = new Array<SkiaSharp.SKColor>();
					for(var i=0; i<ret.colors_Length; i++)
					{
						ret.colors.push(SkiaSharp.SKColor.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.colors = null;
				}
			}
			
			{
				ret.size = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.pmcolors_Length = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 16, "*"); /*SkiaSharp.SKPMColor 4 False*/
				if(pArray !== 0)
				{
					ret.pmcolors = new Array<SkiaSharp.SKPMColor>();
					for(var i=0; i<ret.pmcolors_Length; i++)
					{
						ret.pmcolors.push(SkiaSharp.SKPMColor.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.pmcolors = null;
				}
			}
			return ret;
		}
	}
}
