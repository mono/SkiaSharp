/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_is_rect_1_Params
	{
		/* Pack=4 */
		cpath : number;
		rectZero : number;
		isClosedZero : number;
		directionZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_is_rect_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_is_rect_1_Params();
			
			{
				ret.cpath = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rectZero = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.isClosedZero = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.directionZero = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
