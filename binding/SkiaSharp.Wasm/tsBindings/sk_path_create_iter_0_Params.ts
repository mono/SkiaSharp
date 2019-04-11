/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_create_iter_0_Params
	{
		/* Pack=4 */
		path : number;
		forceClose : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_create_iter_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_create_iter_0_Params();
			
			{
				ret.path = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.forceClose = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
