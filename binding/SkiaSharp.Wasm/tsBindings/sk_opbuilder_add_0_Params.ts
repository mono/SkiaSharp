/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_opbuilder_add_0_Params
	{
		/* Pack=4 */
		builder : number;
		path : number;
		op : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_opbuilder_add_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_opbuilder_add_0_Params();
			
			{
				ret.builder = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.path = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.op = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
