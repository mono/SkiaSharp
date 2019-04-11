/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_textblob_builder_runbuffer_set_clusters_0_Params
	{
		/* Pack=4 */
		buffer : SkiaSharp.SKTextBlobBuilderRunBuffer;
		clusters : number;
		count : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_textblob_builder_runbuffer_set_clusters_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_textblob_builder_runbuffer_set_clusters_0_Params();
			
			{
				ret.buffer = SkiaSharp.SKTextBlobBuilderRunBuffer.unmarshal(pData + 0);
			}
			
			{
				ret.clusters = Number(memoryContext.getValue(pData + 16, "*"));
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			return ret;
		}
	}
}
