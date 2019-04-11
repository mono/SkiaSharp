/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_pathmeasure_new_with_path_0_Params
	{
		/* Pack=4 */
		path : number;
		forceClosed : boolean;
		resScale : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_pathmeasure_new_with_path_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_pathmeasure_new_with_path_0_Params();
			
			{
				ret.path = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.forceClosed = Boolean(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.resScale = Number(memoryContext.getValue(pData + 8, "float"));
			}
			return ret;
		}
	}
}
