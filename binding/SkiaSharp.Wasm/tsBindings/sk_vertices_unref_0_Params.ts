/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_vertices_unref_0_Params
	{
		/* Pack=4 */
		cvertices : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_vertices_unref_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_vertices_unref_0_Params();
			
			{
				ret.cvertices = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
