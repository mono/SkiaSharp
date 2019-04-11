/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_frame_info_for_index_0_Return
	{
		/* Pack=4 */
		frameInfo : SkiaSharp.SKCodecFrameInfo;
		public constructor()
		{
			this.frameInfo = new SkiaSharp.SKCodecFrameInfo();
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
			this.frameInfo.marshal(pData + 0);
		}
	}
}
