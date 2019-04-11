/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_compose_0_Params
	{
		/* Pack=4 */
		outer : number;
		inner : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_compose_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_compose_0_Params();
			
			{
				ret.outer = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.inner = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
