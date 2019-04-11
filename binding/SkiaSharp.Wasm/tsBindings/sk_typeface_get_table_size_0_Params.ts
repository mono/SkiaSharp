/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_typeface_get_table_size_0_Params
	{
		/* Pack=4 */
		typeface : number;
		tag : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_typeface_get_table_size_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_typeface_get_table_size_0_Params();
			
			{
				ret.typeface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.tag = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
