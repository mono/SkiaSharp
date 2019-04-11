/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_croprect_destructor_0_Params
	{
		/* Pack=4 */
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_croprect_destructor_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_croprect_destructor_0_Params();
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
