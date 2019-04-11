/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_xmlstreamwriter_delete_0_Params
	{
		/* Pack=4 */
		writer : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_xmlstreamwriter_delete_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_xmlstreamwriter_delete_0_Params();
			
			{
				ret.writer = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
