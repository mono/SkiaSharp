/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspaceprimaries_to_xyzd50_0_Params
	{
		/* Pack=4 */
		primaries : SkiaSharp.SKColorSpacePrimaries;
		toXYZD50 : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorspaceprimaries_to_xyzd50_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorspaceprimaries_to_xyzd50_0_Params();
			
			{
				ret.primaries = SkiaSharp.SKColorSpacePrimaries.unmarshal(pData + 0);
			}
			
			{
				ret.toXYZD50 = Number(memoryContext.getValue(pData + 32, "*"));
			}
			return ret;
		}
	}
}
