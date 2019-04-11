/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_color_unpremultiply_array_0_Params
	{
		/* Pack=4 */
		pmcolors_Length : number;
		pmcolors : Array<SkiaSharp.SKPMColor>;
		size : number;
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_color_unpremultiply_array_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_color_unpremultiply_array_0_Params();
			
			{
				ret.pmcolors_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
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
			
			{
				ret.size = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 16, "*");
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
			return ret;
		}
	}
}
