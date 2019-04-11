/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_reset_with_params_0_Params
	{
		/* Pack=4 */
		cpixmap : number;
		cinfo : SkiaSharp.SKImageInfoNative;
		addr : number;
		rowBytes : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pixmap_reset_with_params_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pixmap_reset_with_params_0_Params();
			
			{
				ret.cpixmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.cinfo = SkiaSharp.SKImageInfoNative.unmarshal(pData + 4);
			}
			
			{
				ret.addr = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 28, "*"));
			}
			return ret;
		}
	}
}
