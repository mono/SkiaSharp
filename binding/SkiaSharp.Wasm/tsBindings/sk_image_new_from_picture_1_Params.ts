/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_image_new_from_picture_1_Params
	{
		/* Pack=4 */
		picture : number;
		dimensions : SkiaSharp.SKSizeI;
		matrixZero : number;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_image_new_from_picture_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_image_new_from_picture_1_Params();
			
			{
				ret.picture = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dimensions = SkiaSharp.SKSizeI.unmarshal(pData + 4);
			}
			
			{
				ret.matrixZero = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 16, "*"));
			}
			return ret;
		}
	}
}
