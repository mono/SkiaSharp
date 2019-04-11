/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_convert_conic_to_quads_0_Params
	{
		/* Pack=4 */
		p0 : SkiaSharp.SKPoint;
		p1 : SkiaSharp.SKPoint;
		p2 : SkiaSharp.SKPoint;
		w : number;
		pts_Length : number;
		pts : Array<SkiaSharp.SKPoint>;
		pow2 : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_convert_conic_to_quads_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_convert_conic_to_quads_0_Params();
			
			{
				ret.p0 = SkiaSharp.SKPoint.unmarshal(pData + 0);
			}
			
			{
				ret.p1 = SkiaSharp.SKPoint.unmarshal(pData + 8);
			}
			
			{
				ret.p2 = SkiaSharp.SKPoint.unmarshal(pData + 16);
			}
			
			{
				ret.w = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.pts_Length = Number(memoryContext.getValue(pData + 28, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 32, "*");
				if(pArray !== 0)
				{
					ret.pts = new Array<SkiaSharp.SKPoint>();
					for(var i=0; i<ret.pts_Length; i++)
					{
						ret.pts.push(SkiaSharp.SKPoint.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.pts = null;
				}
			}
			
			{
				ret.pow2 = Number(memoryContext.getValue(pData + 36, "i32"));
			}
			return ret;
		}
	}
}
