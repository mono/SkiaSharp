/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_mask_compute_image_size_0_Params
	{
		/* Pack=4 */
		cmask : SkiaSharp.SKMask;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_mask_compute_image_size_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_mask_compute_image_size_0_Params();
			
			{
				ret.cmask = SkiaSharp.SKMask.unmarshal(pData + 0);
			}
			return ret;
		}
	}
}
