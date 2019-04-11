/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_new_copy_0_Params
	{
		/* Pack=4 */
		src : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_new_copy_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_new_copy_0_Params();
			
			{
				ret.src = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
