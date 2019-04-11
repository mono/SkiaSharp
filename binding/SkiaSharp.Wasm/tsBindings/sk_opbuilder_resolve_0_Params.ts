/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_opbuilder_resolve_0_Params
	{
		/* Pack=4 */
		builder : number;
		result : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_opbuilder_resolve_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_opbuilder_resolve_0_Params();
			
			{
				ret.builder = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.result = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
