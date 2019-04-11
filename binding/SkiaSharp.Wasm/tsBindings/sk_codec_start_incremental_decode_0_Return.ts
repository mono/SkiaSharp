/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_start_incremental_decode_0_Return
	{
		/* Pack=4 */
		info : SkiaSharp.SKImageInfoNative;
		options : SkiaSharp.SKCodecOptionsInternal;
		public constructor()
		{
			this.info = new SkiaSharp.SKImageInfoNative();
			this.options = new SkiaSharp.SKCodecOptionsInternal();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(40);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.info.marshal(pData + 0);
			this.options.marshal(pData + 20);
		}
	}
}
