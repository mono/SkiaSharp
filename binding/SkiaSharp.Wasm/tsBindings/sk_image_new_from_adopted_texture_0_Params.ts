/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_from_adopted_texture_0_Params
	{
		/* Pack=4 */
		context : number;
		texture : number;
		origin : number;
		colorType : number;
		alpha : number;
		colorSpace : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_new_from_adopted_texture_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_new_from_adopted_texture_0_Params();
			
			{
				ret.context = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.texture = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.origin = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.colorType = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.alpha = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				ret.colorSpace = Number(memoryContext.getValue(pData + 20, "*"));
			}
			return ret;
		}
	}
}
