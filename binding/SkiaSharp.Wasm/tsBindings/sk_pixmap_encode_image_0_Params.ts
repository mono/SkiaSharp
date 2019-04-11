/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_encode_image_0_Params
	{
		/* Pack=4 */
		dst : number;
		src : number;
		encoder : number;
		quality : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pixmap_encode_image_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pixmap_encode_image_0_Params();
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.src = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.encoder = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.quality = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
