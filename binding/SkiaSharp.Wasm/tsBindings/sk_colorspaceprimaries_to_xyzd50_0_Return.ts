/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspaceprimaries_to_xyzd50_0_Return
	{
		/* Pack=4 */
		primaries : SkiaSharp.SKColorSpacePrimaries;
		public constructor()
		{
			this.primaries = new SkiaSharp.SKColorSpacePrimaries();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(32);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.primaries.marshal(pData + 0);
		}
	}
}
