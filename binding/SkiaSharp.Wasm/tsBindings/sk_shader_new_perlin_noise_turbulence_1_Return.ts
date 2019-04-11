/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_perlin_noise_turbulence_1_Return
	{
		/* Pack=4 */
		tileSize : SkiaSharp.SKPointI;
		public constructor()
		{
			this.tileSize = new SkiaSharp.SKPointI();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(8);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.tileSize.marshal(pData + 0);
		}
	}
}
