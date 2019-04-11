/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_bitmap_get_pixel_colors_0_Params
	{
		/* Pack=4 */
		b : number;
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_bitmap_get_pixel_colors_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_bitmap_get_pixel_colors_0_Params();
			
			{
				ret.b = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*");
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
