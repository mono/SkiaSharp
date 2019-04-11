/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_mask_alloc_image_0_Params
	{
		/* Pack=4 */
		bytes : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_mask_alloc_image_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_mask_alloc_image_0_Params();
			
			{
				ret.bytes = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
