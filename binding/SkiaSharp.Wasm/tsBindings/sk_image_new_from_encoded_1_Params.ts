/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_from_encoded_1_Params
	{
		/* Pack=4 */
		encoded : number;
		subsetZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_new_from_encoded_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_new_from_encoded_1_Params();
			
			{
				ret.encoded = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.subsetZero = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
