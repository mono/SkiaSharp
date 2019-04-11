/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_start_incremental_decode_1_Return
	{
		/* Pack=4 */
		info : SkiaSharp.SKImageInfoNative;
		public constructor()
		{
			this.info = new SkiaSharp.SKImageInfoNative();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(20);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.info.marshal(pData + 0);
		}
	}
}
