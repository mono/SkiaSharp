/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pixmap_get_info_0_Return
	{
		/* Pack=4 */
		cinfo : SkiaSharp.SKImageInfoNative;
		public constructor()
		{
			this.cinfo = new SkiaSharp.SKImageInfoNative();
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
			this.cinfo.marshal(pData + 0);
		}
	}
}
