/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_new_with_params_0_Params
	{
		/* Pack=4 */
		cinfo : SkiaSharp.SKImageInfoNative;
		addr : number;
		rowBytes : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pixmap_new_with_params_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pixmap_new_with_params_0_Params();
			
			{
				ret.cinfo = SkiaSharp.SKImageInfoNative.unmarshal(pData + 0);
			}
			
			{
				ret.addr = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
