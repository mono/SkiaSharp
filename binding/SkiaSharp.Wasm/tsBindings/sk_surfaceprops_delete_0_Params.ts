/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surfaceprops_delete_0_Params
	{
		/* Pack=4 */
		props : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surfaceprops_delete_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surfaceprops_delete_0_Params();
			
			{
				ret.props = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
