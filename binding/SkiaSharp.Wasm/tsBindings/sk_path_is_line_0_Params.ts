/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_is_line_0_Params
	{
		/* Pack=4 */
		cpath : number;
		line_Length : number;
		line : Array<SkiaSharp.SKPoint>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_is_line_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_is_line_0_Params();
			
			{
				ret.cpath = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.line_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*");
				if(pArray !== 0)
				{
					ret.line = new Array<SkiaSharp.SKPoint>();
					for(var i=0; i<ret.line_Length; i++)
					{
						ret.line.push(SkiaSharp.SKPoint.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.line = null;
				}
			}
			return ret;
		}
	}
}
