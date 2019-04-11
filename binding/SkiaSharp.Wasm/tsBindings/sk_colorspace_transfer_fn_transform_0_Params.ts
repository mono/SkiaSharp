/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspace_transfer_fn_transform_0_Params
	{
		/* Pack=4 */
		transfer : SkiaSharp.SKColorSpaceTransferFn;
		x : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorspace_transfer_fn_transform_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorspace_transfer_fn_transform_0_Params();
			
			{
				ret.transfer = SkiaSharp.SKColorSpaceTransferFn.unmarshal(pData + 0);
			}
			
			{
				ret.x = Number(memoryContext.getValue(pData + 28, "float"));
			}
			return ret;
		}
	}
}
