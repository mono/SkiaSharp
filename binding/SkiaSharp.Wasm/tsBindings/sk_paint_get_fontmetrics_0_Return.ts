/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_get_fontmetrics_0_Return
	{
		/* Pack=4 */
		fontMetrics : SkiaSharp.SKFontMetrics;
		public constructor()
		{
			this.fontMetrics = new SkiaSharp.SKFontMetrics();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(64);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.fontMetrics.marshal(pData + 0);
		}
	}
}
