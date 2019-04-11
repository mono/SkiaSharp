/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_offset_0_Params
	{
		/* Pack=4 */
		dx : number;
		dy : number;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_offset_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_offset_0_Params();
			
			{
				ret.dx = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.dy = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
