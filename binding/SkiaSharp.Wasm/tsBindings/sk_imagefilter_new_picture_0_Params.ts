/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_picture_0_Params
	{
		/* Pack=4 */
		picture : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_picture_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_picture_0_Params();
			
			{
				ret.picture = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
