/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_from_encoded_0_Params
	{
		/* Pack=4 */
		encoded : number;
		subset : SkiaSharp.SKRectI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_new_from_encoded_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_new_from_encoded_0_Params();
			
			{
				ret.encoded = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.subset = SkiaSharp.SKRectI.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
