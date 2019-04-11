/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_new_high_contrast_0_Return
	{
		/* Pack=4 */
		config : SkiaSharp.SKHighContrastConfig;
		public constructor()
		{
			this.config = new SkiaSharp.SKHighContrastConfig();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(12);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.config.marshal(pData + 0);
		}
	}
}
