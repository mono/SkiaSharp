/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colortable_new_0_Params
	{
		/* Pack=4 */
		colors_Length : number;
		colors : Array<SkiaSharp.SKPMColor>;
		count : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colortable_new_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colortable_new_0_Params();
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
				if(pArray !== 0)
				{
					ret.colors = new Array<SkiaSharp.SKPMColor>();
					for(var i=0; i<ret.colors_Length; i++)
					{
						ret.colors.push(SkiaSharp.SKPMColor.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.colors = null;
				}
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
