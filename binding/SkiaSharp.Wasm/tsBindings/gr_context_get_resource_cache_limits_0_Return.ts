/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_context_get_resource_cache_limits_0_Return
	{
		/* Pack=4 */
		maxResources : number;
		maxResourceBytes : number;
		public constructor()
		{
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
			memoryContext.setValue(pData + 0, this.maxResources, "i32");
			memoryContext.setValue(pData + 4, this.maxResourceBytes, "*");
		}
	}
}
