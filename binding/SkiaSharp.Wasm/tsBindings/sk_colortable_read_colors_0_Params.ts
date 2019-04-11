/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colortable_read_colors_0_Params
	{
		/* Pack=4 */
		ctable : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colortable_read_colors_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colortable_read_colors_0_Params();
			
			{
				ret.ctable = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
