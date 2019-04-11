/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_rrect_set_rect_radii_0_Params
	{
		/* Pack=4 */
		rrect : number;
		rect : SkiaSharp.SKRect;
		radii_Length : number;
		radii : Array<SkiaSharp.SKPoint>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_rrect_set_rect_radii_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_rrect_set_rect_radii_0_Params();
			
			{
				ret.rrect = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			
			{
				ret.radii_Length = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 24, "*"); /*SkiaSharp.SKPoint 4 False*/
				if(pArray !== 0)
				{
					ret.radii = new Array<SkiaSharp.SKPoint>();
					for(var i=0; i<ret.radii_Length; i++)
					{
						ret.radii.push(SkiaSharp.SKPoint.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.radii = null;
				}
			}
			return ret;
		}
	}
}
