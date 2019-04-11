/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_new_concat_0_Params
	{
		/* Pack=4 */
		a : number;
		b : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_new_concat_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_new_concat_0_Params();
			
			{
				ret.a = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.b = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
