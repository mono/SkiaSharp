/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_data_new_with_proc_0_Params
	{
		/* Pack=4 */
		ptr : number;
		length : number;
		proc : number;
		ctx : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_data_new_with_proc_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_data_new_with_proc_0_Params();
			
			{
				ret.ptr = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.proc = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.ctx = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
