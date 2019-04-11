/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_reset_0_Params
	{
		/* Pack=4 */
		cpixmap : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pixmap_reset_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pixmap_reset_0_Params();
			
			{
				ret.cpixmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
