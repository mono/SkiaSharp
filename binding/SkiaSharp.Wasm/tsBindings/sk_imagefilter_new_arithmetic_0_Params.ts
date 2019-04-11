/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_arithmetic_0_Params
	{
		/* Pack=4 */
		k1 : number;
		k2 : number;
		k3 : number;
		k4 : number;
		enforcePMColor : boolean;
		background : number;
		foreground : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_arithmetic_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_arithmetic_0_Params();
			
			{
				ret.k1 = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.k2 = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.k3 = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.k4 = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.enforcePMColor = Boolean(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				ret.background = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.foreground = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 28, "*"));
			}
			return ret;
		}
	}
}
