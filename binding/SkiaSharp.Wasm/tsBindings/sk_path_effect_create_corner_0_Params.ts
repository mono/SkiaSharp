/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_corner_0_Params
	{
		/* Pack=4 */
		radius : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_corner_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_corner_0_Params();
			
			{
				ret.radius = Number(memoryContext.getValue(pData + 0, "float"));
			}
			return ret;
		}
	}
}
