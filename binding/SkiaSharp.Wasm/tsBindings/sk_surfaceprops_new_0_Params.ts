/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surfaceprops_new_0_Params
	{
		/* Pack=4 */
		flags : number;
		geometry : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surfaceprops_new_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surfaceprops_new_0_Params();
			
			{
				ret.flags = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.geometry = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
