/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_textblob_builder_runbuffer_set_utf8_text_0_Return
	{
		/* Pack=4 */
		buffer : SkiaSharp.SKTextBlobBuilderRunBuffer;
		public constructor()
		{
			this.buffer = new SkiaSharp.SKTextBlobBuilderRunBuffer();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(16);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.buffer.marshal(pData + 0);
		}
	}
}
