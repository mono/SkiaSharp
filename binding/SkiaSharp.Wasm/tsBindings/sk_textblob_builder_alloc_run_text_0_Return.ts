/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_textblob_builder_alloc_run_text_0_Return
	{
		/* Pack=4 */
		runbuffer : SkiaSharp.SKTextBlobBuilderRunBuffer;
		public constructor()
		{
			this.runbuffer = new SkiaSharp.SKTextBlobBuilderRunBuffer();
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
			this.runbuffer.marshal(pData + 0);
		}
	}
}
